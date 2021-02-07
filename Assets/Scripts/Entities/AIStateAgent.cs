using UnityEngine;

public abstract class AIStateAgent : Entity
{
    private string _state;
    public string State {
        get{ return _state; }
        set { 
            OnExitState(_state);
            _state = value;
            OnEnterState(_state);
        }
    }
    
    protected abstract void OnEnterState(string state);
    protected abstract void OnExitState(string state);
    protected abstract void OnStateUpdate();
}
