using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Theme", menuName = "Theme", order = 0)]
public class Theme : ScriptableObject
{
    public GameObject projectile;
    public GameObject control;
    public GameObject destructor;
    public GameObject interceptor;
}