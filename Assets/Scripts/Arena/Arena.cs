using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    void Start()
    {
        List<AudioClip> bgms = ArenaManager.Instance.theme.bgms;
        AudioSource source = GetComponent<AudioSource>();
        if (bgms != null && bgms.Count > 0)
        {
            source.clip = bgms[Random.Range(0, bgms.Count-1)];
            source.Play();
        }
        ArenaManager.Instance.InitGame();
    }
}
