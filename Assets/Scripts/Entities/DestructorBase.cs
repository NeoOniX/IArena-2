using UnityEngine;

public abstract class DestructorBase : Agent {
    
    protected override void Start(){
        kind = EntityKind.Destructor;
        configuration = Resources.Load<ScriptableAgent>("Entity/Destructor");
        base.Start();
    }

}