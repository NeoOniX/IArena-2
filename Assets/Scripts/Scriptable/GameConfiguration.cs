using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "GameConfiguration", menuName = "Game configuration", order = 0)]
public class GameConfiguration : ScriptableObject {
    
    [Serializable]
    public struct Units{
        public EntityKind kind;
        public int count;
    }

    [Range(1,4)]    
    public int teamCount = 2;
    public List<Units> baseUnits;

    public bool hardcore = false;
}