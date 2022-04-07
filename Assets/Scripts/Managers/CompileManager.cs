using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using UnityEngine;
using Microsoft.CSharp;
using UnityEngine.Networking;

public class CompileManager : MonoBehaviour
{
    // Instance setup
    private static CompileManager _instance;
    public static CompileManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        _instance = this;
    }


    // Compiler Options
    Dictionary<string, string> providerOptions = new Dictionary<string, string>
    {
        {"CompilerVersion", "v3.5"}
    };
    CompilerParameters compilerParameters = new CompilerParameters
    {
        GenerateInMemory = true,
        GenerateExecutable = false
    };
    List<string> excludedAssemblies = new List<string> { "Anonymously Hosted DynamicMethods Assembly", "mscorlib" };

    void Start()
    {
        // Setup context transfer
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!excludedAssemblies.Contains(assembly.GetName().Name))
            {
                compilerParameters.ReferencedAssemblies.Add(assembly.Location);
            }
        }
    }

    public void CompileCodeFromOrigin(string origin, Action<object> callback)
    {
        if (origin.StartsWith("http") || origin.StartsWith("https"))
        {
            CompileCodeFromURL(origin, callback);
        }
        callback.Invoke(CompileCodeFromFile(origin));
    }

    private object CompileCodeFromFile(string path)
    {
        CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);

        CompilerResults results = provider.CompileAssemblyFromFile(compilerParameters, path);

        if (results.Errors.Count > 0)
        {
            Debug.LogError("Error while loading file :");
            foreach (CompilerError error in results.Errors)
            {
                Debug.Log(error.ErrorText);
            }
            return null;
        }
        else
        {
            return results.CompiledAssembly.CreateInstance(results.CompiledAssembly.GetExportedTypes()[0].Name);
        }
    }

    private void CompileCodeFromURL(string URL, Action<object> callback)
    {
        StartCoroutine(WebRequest(URL, (string source) =>
        {
            if (source == null)
            {
                callback.Invoke(null);
                return;
            }

            CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);

            CompilerResults results = provider.CompileAssemblyFromSource(compilerParameters, source);

            if (results.Errors.Count > 0)
            {
                Debug.LogError("Error while loading file :");
                foreach (CompilerError error in results.Errors)
                {
                    Debug.Log(error.ErrorText);
                }
                callback.Invoke(null);
            }
            else
            {
                callback.Invoke(results.CompiledAssembly.CreateInstance(results.CompiledAssembly.GetExportedTypes()[0].Name));
            }
        }));
    }

    IEnumerator WebRequest(string URL, Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(URL))
        {
            yield return request.SendWebRequest();

            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    callback.Invoke(request.downloadHandler.text);
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    callback.Invoke("");
                    break;
            }
        }
    }
}
