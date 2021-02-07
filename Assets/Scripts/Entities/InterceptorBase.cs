using UnityEngine;

public abstract class InterceptorBase : Agent 
{
    protected override void Start(){
        kind = EntityKind.Interceptor;
        configuration = Resources.Load<ScriptableAgent>("Entity/Interceptor");
        base.Start();
    }
}