using SFB;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FileManager
{
    [System.Serializable]
    public enum Extensions
    {
        CS,
        TXT,
        JSON,
        ALL
    }

    // IO / FileSystem Options
    static string iarenaFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments).Replace("\\", "/") + "/IArena/";

    public static void VerifyFolder()
    {
        string[] folders = { iarenaFolder, iarenaFolder + "/Scripts/", iarenaFolder + "/Players/" };

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
        VerifyFolder();

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
            return path[0];
        } else
        {
            Debug.Log("No file selected");
            return null;
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
}
