using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Theme", menuName = "Theme", order = 0)]
public class Theme : ScriptableObject
{
    [System.Serializable]
    public class ElementClip
    {
        public List<EntityKind> Kinds;
        public AudioClip ShootSound;
    }

    [Header("Sounds")]
    public List<AudioClip> bgms;
    public List<ElementClip> sfxs;
    [Header("Elements")]
    public ThemeElement projectile;
    public ThemeElement control;
    public ThemeElement destructor;
    public ThemeElement interceptor;
}