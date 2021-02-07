using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Player", order = 0)]
public class PlayerConfig : ScriptableObject {   
    public new string name; 
    public Color color = new Color(1f,1f,1f);
    public GameObject controlPrefab;
    public GameObject interceptorPrefab;
    public GameObject destructorPrefab;
}