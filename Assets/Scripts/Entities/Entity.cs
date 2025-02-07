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

    [SerializeField]
    private float lifeAmount;
    /// <summary>
    /// Current life amount of the entity
    /// </summary>
    /// <value></value>
    public float Life {
        get { return lifeAmount; }
    }

    [SerializeField]
    private float maxLife;
    /// <summary>
    /// Current life amount of the entity
    /// </summary>
    /// <value></value>
    protected float MaxLife {
        get { return maxLife; }
    }

    private float reactivityTime;
    /// <summary>
    /// Reactivity time of the entity
    /// The entity can take an action every ReactivityTime in seconds
    /// </summary>
    /// <value></value>
    public float ReactivityTime {
        get { return reactivityTime; }
    }    

    /// <summary>
    /// View radius of the entity
    /// </summary>
    /// <value></value>
    public float ViewRadius {
        get { return fieldOfView.viewRadius; }
    }

    /// <summary>
    /// View angle of the entity
    /// </summary>
    /// <value></value>
    public float ViewAngle {
        get { return fieldOfView.viewAngle; }
    }

    private int team;
    /// <summary>
    /// Current team id
    /// </summary>
    /// <value></value>
    public int Team { get { return team; }}

    private FieldOfView fieldOfView;
    protected TMPro.TextMeshProUGUI debugTxt;
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
        debugTxt = GetComponentInChildren<TMPro.TextMeshProUGUI>();
    }

    protected virtual void Start(){
        LoadConfiguration();
        StartCoroutine(InternRoutine());
    }

    protected virtual void Update(){
        logs.printInConsole = printLogs;
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
        maxLife = configuration.lifeAmount;
        lifeAmount = maxLife;
        reactivityTime = configuration.reactivityTime;
        fieldOfView.viewAngle = configuration.viewAngle;
        fieldOfView.viewRadius = configuration.viewRadius;
    }

    public void SetTeam(int i){
        team = i;
    }

    /// <summary>
    /// Returns targets currently in sight
    /// </summary>
    /// <param name="onlyOpponent">if true, will only return target from another team</param>
    /// <returns></returns>
    public List<TargetInformations> GetTargets(bool onlyOpponent = true){
        if (!onlyOpponent)
            return targets;
        List<TargetInformations> result = new List<TargetInformations>(targets);
        foreach (TargetInformations t in targets){
            if (t.team == Team){
                result.Remove(t);
            }
        }
        return result;
    }

    /// <summary>
    /// Get closest target currently in sight
    /// </summary>
    /// <param name="onlyOpponent">if true, will only return target from another team</param>
    /// <returns></returns>
    public TargetInformations GetClosestTarget(bool onlyOpponent = true){
        float distance = float.MaxValue;
        TargetInformations closest = null;
        foreach (TargetInformations t in targets){
            float d = Vector3.Distance(transform.position, t.position);
            if (d <= distance && ((onlyOpponent && t.team != Team) || !onlyOpponent)){
                distance = d;
                closest = t;
            }   
        }
        return closest;
    }

    void FindVisibleAgents(){
        targets.Clear();
        Collider[] targetsC = Physics.OverlapSphere(transform.position,ViewRadius,ArenaManager.Instance.AgentsLayerMask, QueryTriggerInteraction.Collide);
        foreach(Collider c in targetsC){
            if (c.gameObject == gameObject) continue;
            Transform target = c.transform;
            Entity targetEntity = target.GetComponentInParent<Entity>();
            if (targetEntity != null){
                //Vector3 dirToTarget = (target.position - transform.position).normalized;
                if (IsInViewAngle(target.position)){ //dans l'angle de vision
                    if (IsInViewRange(target.position)){ //à distance de vue
                        if (!HasObstacleInSight(target.position)) //non caché derrière un obstacle
                            targets.Add(new TargetInformations(targetEntity.Kind, targetEntity.Team, targetEntity.lifeAmount, target.position));
                    }
                }
            }
        }
    }

    private bool IsInViewRange(Vector3 position){
        float distance = Vector3.Distance(transform.position,position);
        return distance <= ViewRadius;
    }

    private bool HasObstacleInSight(Vector3 position){
        Vector3 direction = (position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, position);
        return Physics.Raycast(transform.position, direction, distance, ArenaManager.Instance.ObstaclesLayerMask)
            || Physics.RaycastAll(transform.position, direction, distance, ArenaManager.Instance.AgentsLayerMask).Length > 1;
    }

    private bool IsInViewAngle(Vector3 position){
        Vector3 direction = (position - transform.position).normalized;
        return Vector3.Angle(transform.forward,direction) < ViewAngle / 2;
    }

    protected bool IsThereAWall(Vector3 direction){
        return Physics.Raycast(transform.position, direction, ViewRadius, ArenaManager.Instance.ObstaclesLayerMask);
    }

    /// <summary>
    /// Can view this position from current position? 
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    protected bool CanView(Vector3 position){
        return IsInViewAngle(position) && IsInViewRange(position) && !HasObstacleInSight(position);
    }

    /// <summary>
    /// Don't call this method.
    /// </summary>
    /// <param name="hitDamage"></param>
    /// <param name="hitDirection"></param>
    public void ReceiveHit(float hitDamage, Vector3 hitDirection){
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        var method = stackTrace.GetFrame(1).GetMethod();
        if (method.ReflectedType.Name != "Projectile" || method.Name != "ApplyDamage"){
            throw new System.Exception("Someone is cheating ! (try to apply damage from :"+method.ReflectedType.Name + " with method "+method.Name);
        }

        lifeAmount -= hitDamage;
        if (lifeAmount <= 0){
            Die();
        }else{
            OnHit(hitDirection);
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