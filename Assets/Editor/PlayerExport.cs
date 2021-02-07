using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class PlayerExport : EditorWindow
{
    private static string PLAYERS_PATH = "Assets/Players/";
    private string[] players = new string[0];
    private int selectedPlayerIndex = 0;
    private string studentName = "Name";
    private string studentFirstName = "Firstname";
    private string status = "";

    [MenuItem("iArena/Export your player...")]
    static void OpenWindow(){
        PlayerExport window = (PlayerExport)EditorWindow.GetWindow(typeof(PlayerExport));
        window.Show();
        window.Init();
    }

    void Init(){
        studentName = "Name";
        studentFirstName = "Firstname";
        //Check all players available in this project
        players = Directory.GetDirectories(PLAYERS_PATH);
    }

    private void OnGUI(){
        GUILayout.Label("Player exporter tool:");
        GUILayout.Space(10);
        GUILayout.Label("Choose a player from your project you want to export:");
        selectedPlayerIndex = EditorGUILayout.Popup(selectedPlayerIndex,players);
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Student:");
        studentName = EditorGUILayout.TextField(studentName);
        studentFirstName = EditorGUILayout.TextField(studentFirstName);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Export")){
            Export(players[selectedPlayerIndex]);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(50);
        GUILayout.Label(status,EditorStyles.boldLabel);
    }

    private void Export(string player){
        if (Directory.Exists(player)){
            status = "Export in progress...";
            AssetDatabase.ExportPackage(player,player+"."+studentFirstName+"_"+studentName.ToUpper()+".unitypackage", ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse | ExportPackageOptions.Interactive);
            status = "Export done";
        }else{
            Debug.LogError("Something went wrong with export...");
            Debug.LogError("Trying to export "+player);
            status = "Error, please look in console to get more details";
        }
    }
}
