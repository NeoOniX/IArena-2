using System.Collections.Generic;
using UnityEngine;

public abstract class ControlBase : Entity
{
    public Vector3 spawnUnitPosition;
    private List<Agent> agents = new List<Agent>();
    protected List<Agent> Agents {  get { return agents;}}

    protected override void Start(){
        kind = EntityKind.Control;
        configuration = Resources.Load<ScriptableAgent>("Entity/Control");
        base.Start();
    }

    public void AddAgent(Agent agent){
        //Check authority
        agent.SetControl(this);
        agent.SetTeam(Team);
        agents.Add(agent);
    }

    protected List<T> GetAgentsOfType<T>() where T : Agent {
        List<T> ret = new List<T>();
        foreach (Agent a in Agents){
            if (a is T){
                ret.Add((T)a);
            }
        }
        return ret;
    }

    protected override void AutoDestruction(){
        foreach (Agent agent in agents){
            if (agent.IsAlive)
                agent.RequestAutoDestruction();
        }
    }

    protected override void LastWordBeforeToDie()
    {
        base.LastWordBeforeToDie();
        AutoDestruction();
        ArenaHelper.Instance.ControlIsDestroyed(this);
    }

    public void UpdateAgents(){
        List<Agent> newAgents = new List<Agent>(Agents);
        foreach (Agent a in Agents){
            if (a == null || a.gameObject == null || !a.IsAlive){
                newAgents.Remove(a);
            }
        }
        agents = newAgents;
    }
}
