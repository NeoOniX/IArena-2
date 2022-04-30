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

    public void CompileCodeFromOrigin(int index, string origin, Action<CompiledData> callback)
    {
        if (origin.StartsWith("http") || origin.StartsWith("https"))
        {
            CompileCodeFromURL(index, origin, callback);
        } else
        {
            callback.Invoke(CompileCodeFromFile(index, origin));
        }
    }

    private CompiledData CompileCodeFromFile(int index, string path)
    {
        CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);

        CompilerResults results = provider.CompileAssemblyFromFile(compilerParameters, path);

        if (results.Errors.HasErrors)
        {
            LogManager.Error("Errors while loading file : " + path, file: true);
            foreach (CompilerError error in results.Errors)
            {
                LogManager.Error(error.ErrorText + " at line " + error.Line, false, true);
            }
            return null;
        }
        else
        {
            return FromAssembly(index, results.CompiledAssembly);
        }
    }

    private void CompileCodeFromURL(int index, string URL, Action<CompiledData> callback)
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

            if (results.Errors.HasErrors)
            {
                LogManager.Error("Errors while loading from URL : " + URL, file: true);
                foreach (CompilerError error in results.Errors)
                {
                    LogManager.Error(error.ErrorText + " at line " + error.Line, false, true);
                }
                callback.Invoke(null);
                return;
            }
            else
            {
                callback.Invoke(FromAssembly(index, results.CompiledAssembly));
            }
        }));
    }

    // Extract Types from an Assembly
    // Returns the first type to include the EntityKind in its name
    // Else returns "null" to be checked against
    private CompiledData FromAssembly(int index, Assembly assembly)
    {
        Type control = null;
        Type destructor = null;
        Type interceptor = null;

        foreach (Type type in assembly.GetExportedTypes())
        {
            if (type.Name.Contains("control", StringComparison.OrdinalIgnoreCase) && control == null) control = type;
            if (type.Name.Contains("destructor", StringComparison.OrdinalIgnoreCase) && destructor == null) destructor = type;
            if (type.Name.Contains("interceptor", StringComparison.OrdinalIgnoreCase) && interceptor == null) interceptor = type;
        }

        return new CompiledData(index, control, destructor, interceptor);
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

public class CompiledData
{
    public CompiledData(int index, Type control, Type destructor, Type interceptor)
    {
        this.index = index;
        this.control = control;
        this.destructor = destructor;
        this.interceptor = interceptor;
    }

    public int index;
    public Type control;
    public Type destructor;
    public Type interceptor;
}