using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArenaHelper))]
public class ArenaHelperEditor : Editor
{

    PlayerConfig[] allplayers = new PlayerConfig[0];
    string[] allPlayersName = new string[0];
    GameConfiguration[] gameConfigs = new GameConfiguration[0];
    string[] gameConfigsNames = new string[0];
    int selectedPlayerIndex = 0;
    int selectedGameMode = 0;

    void OnEnable(){
        FindAllProjectPlayers();
        FindAllGameMode();
    }

    public override void OnInspectorGUI()
    {
        ArenaHelper arena = (ArenaHelper)target;
        base.OnInspectorGUI();

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Fight configuration", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(15);

        if (GUILayout.Button("Remove all players")){
            arena.RemoveAllPlayers();
        }
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Add player :");
        selectedPlayerIndex = EditorGUILayout.Popup(selectedPlayerIndex,allPlayersName);
        GUI.enabled = arena.CanAddMorePlayers();
        if (GUILayout.Button("Add")){
            arena.AddPlayer(allplayers[selectedPlayerIndex]);
        }
        GUI.enabled = true;
        if (!arena.CanAddMorePlayers()){
            GUILayout.Label("Current map is too small to add more players.");
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Game mode :");
        selectedGameMode = EditorGUILayout.Popup(selectedGameMode,gameConfigsNames);
        if (GUILayout.Button("Choose")){
            arena.ChangeGameConfiguration(gameConfigs[selectedGameMode]);
        }
        EditorGUILayout.EndHorizontal();
    }

    void FindAllProjectPlayers(){
        if (allplayers.Length == 0){
            string[] guids = AssetDatabase.FindAssets("t:PlayerConfig");
            allplayers = new PlayerConfig[guids.Length];
            allPlayersName = new string[allplayers.Length];
            for (int i = 0; i<guids.Length;i++){
                string p = AssetDatabase.GUIDToAssetPath(guids[i]);
                allplayers[i] = (PlayerConfig) AssetDatabase.LoadMainAssetAtPath(p);
                allPlayersName[i] = allplayers[i].name;
            }
        }
    }

    void FindAllGameMode(){
        if (gameConfigs.Length == 0){
            string[] guids = AssetDatabase.FindAssets("t:GameConfiguration");
            gameConfigs = new GameConfiguration[guids.Length];
            gameConfigsNames = new string[guids.Length];
            for (int i = 0;i<guids.Length;i++){
                string p = AssetDatabase.GUIDToAssetPath(guids[i]);
                gameConfigs[i] = (GameConfiguration)AssetDatabase.LoadMainAssetAtPath(p);
                gameConfigsNames[i] = gameConfigs[i].name;
            }
        }
    }
}
