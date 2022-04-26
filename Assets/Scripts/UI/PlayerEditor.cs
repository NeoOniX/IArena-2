using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerEditor : MonoBehaviour
{
    [Header("GameObjects")]
    public GameObject playerEditorMenu;
    public MainMenu mainMenu;
    [Header("Name")]
    public TMP_InputField name;
    [Header("Color")]
    public TMP_InputField color;
    public Color defaultColor;
    [Header("Source")]
    public TMP_InputField source;

    private PlayerConfig currentPlayer;
    private PlayerConfig oldCurrentPlayer;

    public string Name
    {
        get { return currentPlayer.name; }
        set { currentPlayer.name = value; }
    }

    public string Color
    {
        get { return currentPlayer.color; }
        set {
            if (value.Length == 7)
            {
                Color c = defaultColor;
                ColorUtility.TryParseHtmlString(value, out c);
                color.textComponent.color = c;
                currentPlayer.color = value;
            } else
            {
                color.textComponent.color = defaultColor;
                currentPlayer.color = "#" + ColorUtility.ToHtmlStringRGB(defaultColor);
            }
        }
    }

    public string Source
    {
        get { return currentPlayer.source; }
        set { currentPlayer.source = value; }
    }

    private string OpenPathCommon(TMP_InputField source)
    {
        string path = FileManager.OpenFile("Scripts", new List<FileManager.Extensions> { FileManager.Extensions.CS, FileManager.Extensions.TXT, FileManager.Extensions.ALL });

        if (path is not null)
        {
            source.text = path;
        }

        return path;
    }

    public void OpenSource()
    {
        Source = OpenPathCommon(source);
    }

    public void SavePlayer()
    {
        ArenaManager.Instance.RemovePlayer(oldCurrentPlayer);
        ArenaManager.Instance.SavePlayer(currentPlayer);
        ArenaManager.Instance.AddPlayer(currentPlayer);
        gameObject.SetActive(false);
        playerEditorMenu.SetActive(true);
        mainMenu.ShowPlayers();
    }

    public void OpenPlayer(PlayerConfig player)
    {
        currentPlayer = player;
        oldCurrentPlayer = player;
        name.text = player.name;
        color.text = player.color;
        Color c = defaultColor;
        ColorUtility.TryParseHtmlString(player.color, out c);
        color.textComponent.color = c;
        source.text = player.source;
    }
}
