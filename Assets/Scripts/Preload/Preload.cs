using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Preload : MonoBehaviour
{
    void Start()
    {
        //SceneManager.LoadSceneAsync
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(.5f);
        SceneManager.LoadScene(1);
    }
}
