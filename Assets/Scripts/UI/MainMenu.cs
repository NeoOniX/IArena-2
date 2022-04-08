using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("PlayerEditor")]
    public Transform playerList;
    public GameObject playerEditorCardPrefab;
    public GameObject playerEditor;


    // Start is called before the first frame update
    void Start()
    {
        foreach (PlayerConfig p in ArenaManager.Instance.GetPlayers())
        {
            AddPlayerEditorCard(p);
        }
    }

    public void AddPlayerEditorCard(PlayerConfig player)
    {
        GameObject newCard = Instantiate(playerEditorCardPrefab);
        TMP_Text playerText = newCard.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
        playerText.text = player.name;
        Color c = new Color(1f, 1f, 1f);
        ColorUtility.TryParseHtmlString(player.color, out c);
        playerText.color = c;
        newCard.transform.SetParent(playerList);
    }

    public void LoadPlayer()
    {
        string path = FileManager.OpenFile("Players", new List<FileManager.Extensions>() { FileManager.Extensions.JSON });

        if (path != null)
        {
            string content = FileManager.ReadFile(path);

            if (content != null)
            {
                PlayerConfig player = JsonUtility.FromJson<PlayerConfig>(content);
                ArenaManager.Instance.AddPlayer(player);
                AddPlayerEditorCard(player);
            }
        }
    }

    public void NewPlayer()
    {

    }

    public void PlayerEditorReload()
    {
        ArenaManager.Instance.LoadPlayersFromFiles();
    }
}
