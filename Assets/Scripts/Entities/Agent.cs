using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Agent : AIStateAgent
{
    private ControlBase control;
    protected ControlBase Control {
        get { return control;}
    }
    private NavMeshAgent agent;

    private float energyAmount;
    protected float EnergyAmount {
        get { return energyAmount; }
    }

    private float maxEnergy;
    protected float MaxEnergy {
        get { return maxEnergy; }
    }

    private float damageOutput;
    public float DamageOutput{
        get { return damageOutput; }
    }

    private float energyCostToShoot;
    protected float EnergyCostToShoot{
        get { return energyCostToShoot; }
    }

    private float energyGainPerDT;
    protected float EnergyGainPerDeltaTime {
        get { return energyGainPerDT; }
    }
    
    private Vector3 destination;
    private bool canShoot = true;

    protected override void Awake(){
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        if (agent == null){
            Debug.LogError("There is any agent attached to this gameobject",gameObject);
        }
    }

    protected override void LoadConfiguration(){
        base.LoadConfiguration();
        maxEnergy = configuration.energyAmount;
        energyCostToShoot = configuration.energyCostToShoot;
        damageOutput = configuration.damageOutput;
        energyGainPerDT = configuration.energyGainPerTime;
        agent.speed = configuration.speed;
    }

    protected override void Start(){
        base.Start();
        InitializeAgent();
        StartCoroutine(Routine());
    }

    protected override void Update(){
        base.Update();
        if (debugTxt != null && ArenaManager.Instance.ShowState){
            debugTxt.text = State.ToString();
        }else if (debugTxt != null){
            debugTxt.text = "";
        }
        RefillEnergy();
    }

    public void SetControl(ControlBase c){
        control = c;
    }

    private void RefillEnergy(){
        if (energyAmount <= maxEnergy){
            energyAmount += energyGainPerDT * Time.deltaTime;
            if (energyAmount > maxEnergy) energyAmount = maxEnergy;
        }
    }

    protected override void LastWordBeforeToDie()
    {
        Control.UpdateAgents();
    }

    /// <summary>
    /// Go to position given if a valid path exist
    /// </summary>
    /// <param name="position">position where you want to go</param>
    /// <returns>true if a valid path exist, false otherwise</returns>
    public bool GoTo(Vector3 position){
        destination = position;
        agent.isStopped = false;
        return agent.SetDestination(position);
    }

    protected void Stop(){
        if (agent != null && IsAlive)
            agent.isStopped = true;
    }
    
    private IEnumerator Routine(){
        while (IsAlive){
            OnStateUpdate();
            yield return new WaitForSeconds(ReactivityTime);
            canShoot = true;
        }
    }

    /// <summary>
    /// Allow you to move according a normalized direction on some distance.
    /// </summary>
    /// <param name="direction">normalized direction</param>
    /// <param name="distance">distance you want to travel accross direction</param>
    /// <returns>true if a valid path exist, false otherwise</returns>
    public bool GoTowards(Vector3 direction, float distance){
        Vector3 destination = transform.position + direction * distance;
        return GoTo(destination);
    }

    /// <summary>
    /// Returns a random position within current NavMesh from an origin position with some distance
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public Vector3 GetRandomPositionOnNavMesh(Vector3 origin, float distance) {
        Vector3 dir = origin + Random.insideUnitSphere * distance;
        NavMeshHit hit;
        NavMesh.SamplePosition(dir,out hit,distance,-1);
        return hit.position;
    }

    /// <summary>
    /// Can only shoot in the direction we are looking for
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public bool Shoot(Vector3 direction){
        if (energyAmount >= energyCostToShoot && Vector3.Angle(transform.forward,direction) < ViewAngle / 2 && canShoot){
            //Instantiate projectil
            Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up / 4;
            GameObject proj = ArenaManager.Instance.GetProjectile();
            proj.transform.position = spawnPos;
            proj.GetComponent<Projectile>().Setup(this,direction,damageOutput);
            proj.SetActive(true);
            energyAmount -= energyCostToShoot;
            canShoot = false;
            return true;
        }else{
            return false;
        }
    }

    protected abstract void InitializeAgent();
}
