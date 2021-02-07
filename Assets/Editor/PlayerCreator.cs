using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class PlayerCreator : EditorWindow
{
    private PlayerConfig player;
    private string playerName;
    private string interceptorName;
    private string destructorName;
    private string controlName;
    private Color playerColor;
    private string playerPath = "";


    [MenuItem("iArena/Create a new player...")]
    static void OpenWindow(){
        PlayerCreator window = (PlayerCreator)EditorWindow.GetWindow(typeof(PlayerCreator));
        window.Show(); 
        window.Init();
    }

    public void Init(){
        player = ScriptableObject.CreateInstance<PlayerConfig>();
        
        playerColor = new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f));
    }

    private void OnGUI() {
        GUILayout.Label("Player's informations:");
        playerName = TextFieldWithLabel("Player name:",playerName);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Color:");
        playerColor = EditorGUILayout.ColorField(playerColor);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Create")){
            player.name = playerName;
            player.color = playerColor;

            playerPath = "Assets/Players/"+playerName+"/";
            if (!Directory.Exists(playerPath)){
                Directory.CreateDirectory(playerPath);
                Directory.CreateDirectory(playerPath+"Scripts/");
            }
    
            interceptorName = playerName + "_Interceptor";
            destructorName = playerName + "_Destructor";
            controlName = playerName + "_Control";

            GameObject interceptorPrefab = CreateBaseInterceptorPrefab();
            interceptorPrefab.name = interceptorName;

            GameObject destructorPrefab = CreateBaseDestructorPrefab();
            destructorPrefab.name = destructorName;

            GameObject controlPrefab = CreateBaseControlPrefab();
            controlPrefab.name = controlName;

            AssetDatabase.Refresh();

            string interceptorPath = playerPath + interceptorName + ".prefab";
            string destructorPath = playerPath + destructorName + ".prefab";
            string controlPath = playerPath + controlName + ".prefab";

            player.interceptorPrefab = PrefabUtility.SaveAsPrefabAsset(interceptorPrefab ,interceptorPath);
            player.destructorPrefab = PrefabUtility.SaveAsPrefabAsset(destructorPrefab,destructorPath);
            player.controlPrefab = PrefabUtility.SaveAsPrefabAsset(controlPrefab,controlPath);

            AssetDatabase.CreateAsset(player, playerPath+playerName+".asset");
            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();

            DestroyImmediate(interceptorPrefab);
            DestroyImmediate(destructorPrefab);
            DestroyImmediate(controlPrefab);
        }

        GUILayout.Space(10);
        Instructions();
    }

    private string TextFieldWithLabel(string label, string text){
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        text = GUILayout.TextField(text);
        EditorGUILayout.EndHorizontal();
        return text;
    }

    private GameObject CreateBaseInterceptorPrefab(){
        GameObject g = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)Resources.Load("Entity/AgentBasePrefab"));
        return g;
    }

    private GameObject CreateBaseDestructorPrefab(){
        GameObject g = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)Resources.Load("Entity/DestructorBasePrefab"));
        return g;
    }

    private GameObject CreateBaseControlPrefab(){
        GameObject g = (GameObject)PrefabUtility.InstantiatePrefab((GameObject)Resources.Load("Entity/ControlBasePrefab"));
        return g;
    }

    private void Instructions(){
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Instructions:");
        EditorGUILayout.LabelField("1. Fill player name text field with your name.");
        EditorGUILayout.LabelField("2. Choose a color to represent your team");
        EditorGUILayout.LabelField("4. Click on Create !");
        EditorGUILayout.LabelField("5. Go to "+(playerPath == "" ? "<Player_Directory>": playerPath)+ " and in Scripts folder create a class for each units type.");
        EditorGUILayout.LabelField("6. Assign each unit component to each unit prefab already create in your folder.");
        EditorGUILayout.LabelField("7. Don't forget to implement base component for each unit. (ControlBase, InterceptorBase...)");
        EditorGUILayout.EndVertical();
    }

}