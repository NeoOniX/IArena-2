using SFB;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FileManager
{

    // -- IO / FileSystem --

    // File Extensions
    [System.Serializable]
    public enum Extensions
    {
        CS,
        TXT,
        JSON,
        ALL
    }

    // Folders
    static string iarenaFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments).Replace("\\", "/") + "/IArena/";

    public static void VerifyFolders()
    {
        string[] folders = { iarenaFolder, iarenaFolder + "/Scripts/", iarenaFolder + "/Players/", iarenaFolder + "/Logs/" };

        foreach (string folder in folders)
        {
            if (!System.IO.File.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
        }
    }

    public static string OpenFile(string folder, List<Extensions> extensions)
    {
        VerifyFolders();

        List<ExtensionFilter> filters = new List<ExtensionFilter>();

        foreach (Extensions ext in extensions)
        {
            switch (ext)
            {
                case Extensions.CS:
                    filters.Add(new ExtensionFilter("CS Scripts", "cs"));
                    break;
                case Extensions.TXT:
                    filters.Add(new ExtensionFilter("Text Files", "txt"));
                    break;
                case Extensions.JSON:
                    filters.Add(new ExtensionFilter("JSON Files", "json"));
                    break;
                case Extensions.ALL:
                    filters.Add(new ExtensionFilter("All Files", "*"));
                    break;
            }
        }

        string[] path = StandaloneFileBrowser.OpenFilePanel("Open File", iarenaFolder + "/" + folder, filters.ToArray(), false);

        if (path.Length > 0)
        {
            return path[0].Replace("\\", "/");
        } else
        {
            Debug.Log("No file selected");
            return null;
        }
    }

    public static string SaveFile(string folder, string defaultName, List<Extensions> extensions)
    {
        VerifyFolders();

        List<ExtensionFilter> filters = new List<ExtensionFilter>();

        foreach (Extensions ext in extensions)
        {
            switch (ext)
            {
                case Extensions.CS:
                    filters.Add(new ExtensionFilter("CS Scripts", "cs"));
                    break;
                case Extensions.TXT:
                    filters.Add(new ExtensionFilter("Text Files", "txt"));
                    break;
                case Extensions.JSON:
                    filters.Add(new ExtensionFilter("JSON Files", "json"));
                    break;
                case Extensions.ALL:
                    filters.Add(new ExtensionFilter("All Files", "*"));
                    break;
            }
        }

        string path = StandaloneFileBrowser.SaveFilePanel("Save File", iarenaFolder + "/" + folder, defaultName, filters.ToArray()).Replace("\\", "/"); ;

        if (string.IsNullOrEmpty(path))
        {
            return null;
        } else
        {
            return path;
        }
    }

    public static string[] ReadDirectory(string directory)
    {
        return System.IO.Directory.GetFiles(iarenaFolder + "/" + directory);
    }

    public static string ReadFile(string path)
    {
        if (System.IO.File.Exists(path))
        {
            return System.IO.File.ReadAllText(path);
        } else
        {
            return null;
        }
    }

    public static void WriteFile(string path, string content)
    {
        System.IO.FileStream fs = System.IO.File.OpenWrite(path);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
        fs.Write(bytes, 0, bytes.Length);
        fs.Close();
    }

    // Logs

    private static string logPath = null;

    public static void LogInFile(string message)
    {
        VerifyFolders();

        if (logPath == null) logPath = iarenaFolder + "/Logs/" + DateTime.Now.ToString("s").Replace(":","-") + ".log";

        using (System.IO.TextWriter tw = new System.IO.StreamWriter(logPath, append: true))
        {
            tw.WriteLine("[" + DateTime.Now.ToString("G") + "] " + message);
        }
    }
}
