using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableAgent", menuName = "Entity", order = 0)]
public class ScriptableAgent : ScriptableObject {
    
    public EntityKind entityKind;
    public float lifeAmount;
    public float reactivityTime;
    public float speed;
    public float damageOutput;
    public float energyAmount;
    public float energyGainPerTime;
    public float energyCostToShoot;
    public float viewRadius;
    public float viewAngle;

}