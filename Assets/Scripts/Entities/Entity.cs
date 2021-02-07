using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public enum EntityKind{
    Control,
    Interceptor,
    Destructor,
}

public abstract class Entity : MonoBehaviour {

    [Serializable]
    public class TargetInformations{
        public EntityKind kind;
        public int team;
        public Vector3 position;

        public TargetInformations(EntityKind k, int t, Vector3 p){
            kind = k;
            team = t;
            position = p;
        }
    }

    [Header("Debug")]
    public bool printLogs = false;

    protected EntityKind kind;
    /// <summary>
    /// Entity kind
    /// </summary>
    /// <value></value>
    public EntityKind Kind {
        get { return kind; }
    }

    private float lifeAmount;
    /// <summary>
    /// Current life amount of the entity
    /// </summary>
    /// <value></value>
    protected float Life {
        get { return lifeAmount; }
    }

    private float reactivityTime;
    /// <summary>
    /// Reactivity time of the entity
    /// The entity can take an action every ReactivityTime in seconds
    /// </summary>
    /// <value></value>
    protected float ReactivityTime {
        get { return reactivityTime; }
    }    

    /// <summary>
    /// View radius of the entity
    /// </summary>
    /// <value></value>
    protected float ViewRadius {
        get { return fieldOfView.viewRadius; }
    }

    /// <summary>
    /// View angle of the entity
    /// </summary>
    /// <value></value>
    protected float ViewAngle {
        get { return fieldOfView.viewAngle; }
    }

    private int team;
    /// <summary>
    /// Current team id
    /// </summary>
    /// <value></value>
    public int Team { get { return team; }}

    private FieldOfView fieldOfView;
    private MeshRenderer renderer;
    private List<TargetInformations> targets = new List<TargetInformations>();

    private Logs logs = new Logs();

    /// <summary>
    /// Is this entity still alive? (life amount > 0)
    /// </summary>
    /// <value></value>
    public bool IsAlive { 
        get { return lifeAmount > 0f;}
    }

    protected ScriptableAgent configuration;

    protected virtual void Awake(){
        fieldOfView = GetComponent<FieldOfView>();
    }

    protected virtual void Start(){
        LoadConfiguration();
        StartCoroutine(InternRoutine());
    }

    protected virtual void Update(){
        logs.printInConsole = printLogs;
    }

    public void ChangeColor(Color color){
        if (renderer == null){
            renderer = GetComponent<MeshRenderer>();
        }
        if (renderer != null){
            renderer.material.SetColor("_BaseColor",color);
        }
    }

    private IEnumerator InternRoutine(){
        while (IsAlive){
            FindVisibleAgents();
            yield return new WaitForSeconds(0.1f);
        }
    }

    protected virtual void LoadConfiguration(){
        if (configuration == null){
            Debug.LogError("Something's weird, trying to configure an agent without configuration data.");
            return;
        }
        lifeAmount = configuration.lifeAmount;
        reactivityTime = configuration.reactivityTime;
        fieldOfView.viewAngle = configuration.viewAngle;
        fieldOfView.viewRadius = configuration.viewRadius;
    }

    public void SetTeam(int i){
        team = i;
    }

    /// <summary>
    /// Get all targets in sight
    /// </summary>
    /// <returns></returns>
    protected List<TargetInformations> GetTargets(){
        return targets;
    }

    /// <summary>
    /// Get the closest target in sight
    /// This method only returns target from opposite teams 
    /// </summary>
    /// <returns></returns>
    protected TargetInformations GetClosestTarget(){
        float distance = float.MaxValue;
        TargetInformations closest = null;
        foreach (TargetInformations t in targets){
            float d = Vector3.Distance(transform.position, t.position);
            if (d <= distance && t.team != Team){
                distance = d;
                closest = t;
            }   
        }
        return closest;
    }

    void FindVisibleAgents(){
        targets.Clear();
        Collider[] targetsC = Physics.OverlapSphere(transform.position,ViewRadius,ArenaHelper.Instance.AgentsLayerMask, QueryTriggerInteraction.Collide);
        foreach(Collider c in targetsC){
            if (c.gameObject == gameObject) continue;
            Transform target = c.transform;
            Entity targetEntity = target.GetComponent<Entity>();
            if (targetEntity != null){
                Log("it's an entity");
                //Vector3 dirToTarget = (target.position - transform.position).normalized;
                if (IsInViewAngle(target.position)){
                    Log("Is in view angle ");
                    if (IsInViewRange(target.position)){
                        Log("Is in view range");
                        targets.Add(new TargetInformations(targetEntity.Kind, targetEntity.Team, target.position));
                    }
                }
            }
        }
    }

    private bool IsInViewRange(Vector3 position){
        Vector3 direction = (position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position,position);
        return !Physics.Raycast(transform.position, direction, distance,ArenaHelper.Instance.ObstaclesLayerMask);
    }

    private bool IsInViewAngle(Vector3 position){
        Vector3 direction = (position - transform.position).normalized;
        return Vector3.Angle(transform.forward,direction) < ViewAngle / 2;
    }

    /// <summary>
    /// Can view this position from current position? 
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    protected bool CanView(Vector3 position){
        return IsInViewAngle(position) && IsInViewRange(position);
    }

    /// <summary>
    /// Don't call this method.
    /// </summary>
    /// <param name="hitDamage"></param>
    /// <param name="hitDirection"></param>
    public void ReceiveHit(float hitDamage, Vector3 hitDirection){
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        var method = stackTrace.GetFrame(1).GetMethod();
        if (method.ReflectedType.Name != "Projectil" || method.Name != "ApplyDamage"){
            throw new System.Exception("Someone is cheating ! (try to apply damage from :"+method.ReflectedType.Name + " with method "+method.Name);
        }

        lifeAmount -= hitDamage;
        OnHit(hitDirection);
        if (lifeAmount <= 0){
            Die();
        }
    }

    /// <summary>
    /// You probably want override this method to respond when someone is attacking you
    /// </summary>
    /// <param name="direction">direction from where the projectil came</param>
    protected virtual void OnHit(Vector3 direction){
    }

    /// <summary>
    /// Don't call this method
    /// </summary>
    public void RequestAutoDestruction(){
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        var method = stackTrace.GetFrame(1).GetMethod();
        if (method.ReflectedType.Name != "ControlBase" || method.Name != "AutoDestruction"){
            throw new System.Exception("Someone is cheating ! (try to auto destruct from :"+method.ReflectedType.Name + " with method "+method.Name);
        }

        AutoDestruction();
    }

    protected virtual void AutoDestruction(){
        Die(false);   
    }

    protected virtual void LastWordBeforeToDie(){
    }

    private void Die(bool lastWord = true){
        if (lastWord)
            LastWordBeforeToDie();
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Allow you to send informations to another known entity
    /// </summary>
    /// <param name="entity">receiver of the information</param>
    /// <param name="data">data you need to send</param>
    protected void SendInformationTo(Entity entity, string data){
        //We can only send information to an entity from same team
        if (entity.Team != Team){
            return;
        }
        entity.ReceiveInformation(data);
    }

    private void ReceiveInformation(string data){
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        var method = stackTrace.GetFrame(1).GetMethod();
        if (method.ReflectedType.Name != "Entity" || method.Name != "SendInformationTo"){
            throw new System.Exception("Someone is cheating ! (try to send information to another entity from :"+method.ReflectedType.Name + " with method "+method.Name);
        }
        OnInformationsReceived(data);
    }

    /// <summary>
    /// Called when entity received data from another entity
    /// </summary>
    /// <param name="data">data received</param>
    protected abstract void OnInformationsReceived(string data);

    protected void Log(string message){
        logs.Log(message);
    }
}