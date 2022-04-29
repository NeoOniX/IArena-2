using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LogManager : MonoBehaviour
{
    private static LogManager _instance;
    public static LogManager Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (_instance != null)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("UI Prefabs")]
    public GameObject info;
    public GameObject warn;
    public GameObject error;
    [Header("GameObjects")]
    public GameObject canvas;
    public Transform list;

    private List<GameObject> _current = new List<GameObject>();

    public static void Info(string message, bool screen = true, bool file = false, bool debug = true)
    {
        if (screen)
        {
            GameObject log = Instantiate(Instance.info);
            Instance.Log(log, message);
        }
        if (file)
        {
            FileManager.LogInFile("Info : " + message);
        }
        if (debug)
        {
            Debug.Log(message);
        }
    }

    public static void Warn(string message, bool screen = true, bool file = false, bool debug = true)
    {
        if (screen)
        {
            GameObject log = Instantiate(Instance.warn);
            Instance.Log(log, message);
        }
        if (file)
        {
            FileManager.LogInFile("Warning : " + message);
        }
        if (debug)
        {
            Debug.LogWarning(message);
        }
    }

    public static void Error(string message, bool screen = true, bool file = false, bool debug = true)
    {
        if (screen)
        {
            GameObject log = Instantiate(Instance.error);
            Instance.Log(log, message);
        }
        if (file)
        {
            FileManager.LogInFile("Error : " + message);
        }
        if (debug)
        {
            Debug.LogError(message);
        }
    }

    private void Log(GameObject log, string message)
    {
        canvas.SetActive(true);
        log.transform.GetChild(1).GetComponent<TMP_Text>().text += message;
        log.transform.SetParent(list, false);
        log.transform.SetAsFirstSibling();
        _current.Add(log);
        StartCoroutine(waitAndDestroy(10, log));
    }

    private IEnumerator waitAndDestroy(int seconds, GameObject toDestroy)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(toDestroy);
        _current.Remove(toDestroy);
        if (_current.Count == 0)
        {
            canvas.SetActive(false);
        }
    }
}
