using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Player Selection")]
    public TMP_Dropdown playerSelect;
    public Transform playerSelectionList;
    public GameObject playerSelectionCardPrefab;
    [Header("Player Editor")]
    public GameObject playerEditorMenu;
    public Transform playerList;
    public GameObject playerEditorCardPrefab;
    public PlayerEditor playerEditor;
    [Header("Match Settings")]
    public TMP_Dropdown matchTypeDrop;
    public TMP_Dropdown themeDrop;
    public TMP_Dropdown mapDrop;

    // Player Selection Dynamic Var

    private int _selectIndex = 0;
    public int selectIndex
    {
        get { return _selectIndex; }
        set { _selectIndex = value; }
    }

    // Match Settings Dynamic Vars

    public bool matchTypeHardcore {
        get { return ArenaManager.Instance.Hardcore; }
        set { ArenaManager.Instance.Hardcore = value; }
    }

    public int matchTypeIndex
    {
        get { return ArenaManager.Instance.gameConfigurationIndex; }
        set { ArenaManager.Instance.gameConfigurationIndex = value; }
    }

    public int themeIndex
    {
        get { return ArenaManager.Instance.themeIndex; }
        set { ArenaManager.Instance.themeIndex = value; }
    }
    public bool mapRandom
    {
        get { return ArenaManager.Instance.MapRandom; }
        set { ArenaManager.Instance.MapRandom = value; }
    }

    public int mapIndex
    {
        get { return ArenaManager.Instance.mapIndex; }
        set { ArenaManager.Instance.mapIndex = value; }
    }

    // Setup

    void Start()
    {
        ShowPlayers();
        ShowIncludedPlayers();
        SetupMatchSettings();
    }

    public void ShowPlayers()
    {
        foreach (Transform child in playerList)
        {
            Destroy(child.gameObject);
        }
        playerSelect.ClearOptions();
        foreach (PlayerConfig p in ArenaManager.Instance.GetPlayers())
        {
            AddPlayerSelectionOption(p);
            AddPlayerEditorCard(p);
        }
    }

    public void ShowIncludedPlayers()
    {
        foreach (Transform child in playerSelectionList)
        {
            Destroy(child.gameObject);
        }
        foreach (PlayerConfig p in ArenaManager.Instance.GetIncludedPlayers())
        {
            AddPlayerSelectionCard(p);
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
        Button playerBtn = newCard.transform.GetChild(0).GetComponent<Button>();
        playerBtn.onClick.AddListener(() =>
        {
            playerEditorMenu.SetActive(false);
            playerEditor.OpenPlayer(player);
            playerEditor.gameObject.SetActive(true);
        });
    }

    public void AddPlayerSelectionCard(PlayerConfig player)
    {
        GameObject newCard = Instantiate(playerSelectionCardPrefab);
        TMP_Text playerText = newCard.transform.GetChild(0).GetComponent<TMP_Text>();
        playerText.text = player.name;
        Color c = new Color(1f, 1f, 1f);
        ColorUtility.TryParseHtmlString(player.color, out c);
        playerText.color = c;
        newCard.transform.SetParent(playerSelectionList);
        Button playerBtn = newCard.transform.GetChild(1).GetComponent<Button>();
        playerBtn.onClick.AddListener(() =>
        {
            ArenaManager.Instance.RemoveIncludedPlayer(player);
            ShowIncludedPlayers();
        });
    }

    public void AddPlayerSelectionOption(PlayerConfig player)
    {
        playerSelect.AddOptions(new List<string>()
        {
            "<color=" + player.color + ">" + player.name + "</color>"
        });
    }

    public void SelectPlayer()
    {
        ArenaManager.Instance.AddIncludedPlayer(ArenaManager.Instance.GetPlayers()[selectIndex]);
        ShowIncludedPlayers();
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
        PlayerConfig player = new PlayerConfig();
        playerEditor.OpenPlayer(player);
    }

    private void SetupMatchSettings()
    {
        foreach (GameConfiguration gc in ArenaManager.Instance.gameConfigurations)
        {
            matchTypeDrop.AddOptions(new List<string>()
            {
                gc.name
            });
        }
        foreach (Theme theme in ArenaManager.Instance.themes)
        {
            themeDrop.AddOptions(new List<string>()
            {
                theme.name
            });
        }
        foreach (GameObject map in ArenaManager.Instance.maps)
        {
            mapDrop.AddOptions(new List<string>()
            {
                map.name
            });
        }
    }

    public void LaunchMatch()
    {
        if (ArenaManager.Instance.CanInitGame()) SceneManager.LoadScene(2);
    }
}
