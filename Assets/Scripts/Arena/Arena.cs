using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    void Start()
    {
        ArenaManager.Instance.InitGame();
    }
}
