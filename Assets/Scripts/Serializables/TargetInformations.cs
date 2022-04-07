using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TargetInformations
{
    public EntityKind kind;
    public int team;
    public float life;
    public Vector3 position;

    public TargetInformations(EntityKind k, int t, float l, Vector3 p)
    {
        kind = k;
        team = t;
        life = l;
        position = p;
    }
}