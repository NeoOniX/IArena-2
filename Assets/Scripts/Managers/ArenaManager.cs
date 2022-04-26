using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class ArenaManager : MonoBehaviour
{
    private static ArenaManager _instance;
    public static ArenaManager Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (_instance != null)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Prefabs")]
    [SerializeField]
    private GameObject controlBase;
    [SerializeField]
    private GameObject destructorBase;
    [SerializeField]
    private GameObject interceptorBase;

    [Header("Layers")]
    [SerializeField]
    private LayerMask agentsLayerMask;
    public LayerMask AgentsLayerMask { get { return agentsLayerMask; } }
    [SerializeField]
    private LayerMask obstaclesLayerMask;
    public LayerMask ObstaclesLayerMask { get { return obstaclesLayerMask; } }

    [Header("Pool")]
    public int poolCount = 10;
    public bool poolWillGrow = true;

    [Header("UI")]
    public TMPro.TextMeshProUGUI timeScaleTxt;
    public PlayerCard[] playerCards = new PlayerCard[0];
    public GameObject gameOverUI;
    public TMPro.TextMeshProUGUI winnerNameTxt;

    [Header("Debug")]
    public bool ShowState = false;

    private List<PlayerConfig> players = new List<PlayerConfig>();
    private List<PlayerConfig> includedPlayers = new List<PlayerConfig>();

    public bool showHealth = false;

    private List<GameObject> _pool = new List<GameObject>();
    private ControlBase[] controls;
    private List<int> eliminated = new List<int>();

    private int currentTimeScaleIndex = 0;
    private float[] timeScales = new float[4] { 1, 2, 4, 6 };
    private List<Color> colorsUsed = new List<Color>();

    [HideInInspector]
    public List<GameConfiguration> gameConfigurations;
    [HideInInspector]
    public List<Theme> themes;
    [HideInInspector]
    public List<GameObject> maps;

    public bool Hardcore
    {
        get { return gameConfiguration.hardcore; }
        set { gameConfiguration.hardcore = value; }
    }

    public int gameConfigurationIndex
    {
        get { return gameConfigurations.IndexOf(gameConfiguration); }
        set
        {
            gameConfiguration = gameConfigurations[value];
        }
    }

    public int themeIndex
    {
        get { return themes.IndexOf(theme); }
        set
        {
            theme = themes[value];
        }
    }

    private bool _mapRandom = true;

    public bool MapRandom
    {
        get { return _mapRandom; }
        set { _mapRandom = value; }
    }

    public int mapIndex
    {
        get { return maps.IndexOf(map.gameObject); }
        set
        {
            map = maps[value].GetComponent<Map>();
        }
    }

    private Theme theme;
    private GameConfiguration gameConfiguration;
    private Map map;
    private Map terrain;

    void Start()
    {
        GetResources();
        LoadPlayersFromFiles();
        UseDefaults();
    }

    public void UseDefaults()
    {
        gameConfiguration = gameConfigurations[0];
        theme = themes[0];
        map = maps[0].GetComponent<Map>();
    }

    private void GetResources()
    {
        // GameConfigs
        UnityEngine.Object[] tmp;
        tmp = Resources.LoadAll("GameConfigurations/", typeof(GameConfiguration));

        foreach (UnityEngine.Object obj in tmp)
        {
            gameConfigurations.Add(obj as GameConfiguration);
        }

        // Themes
        tmp = Resources.LoadAll("Themes/", typeof(Theme));

        foreach (UnityEngine.Object obj in tmp)
        {
            themes.Add(obj as Theme);
        }

        // Maps
        tmp = Resources.LoadAll("Maps/", typeof(GameObject));

        foreach (UnityEngine.Object obj in tmp)
        {
            maps.Add(obj as GameObject);
        }
    }

    public bool CanInitGame()
    {
        // Check Config
        if (gameConfiguration == null) return false;

        // Check count
        bool count = true;

        if (includedPlayers.Count != gameConfiguration.teamCount)
        {
            LogManager.Error("The player count doesn't match with the current match settings.");
            count = false;
        }

        if (gameConfiguration.teamCount != map.MaxPlayersCount)
        {
            LogManager.Error("The map doesn't match with the current match settings.");
            count = false;
        }

        if (includedPlayers.Count != map.MaxPlayersCount)
        {
            LogManager.Error("The player count doesn't match with the choosen map.");
            count = false;
        }

        if (!count) return false;

        // Check Map
        if (map == null) return false;

        // Everything OK
        return true;
    }

    public void InitGame()
    {
        if (!CanInitGame()) return;

        // Projectiles Pool
        InstantiatePool();

        // Choose map
        SelectMap();

        controls = new ControlBase[gameConfiguration.teamCount];
        colorsUsed = new List<Color>();
        eliminated = new List<int>();

        int currentTeam = 0;
        int currentPlayerCountInTeam = 0;

        // Load teams
        for (int i = 0; i < gameConfiguration.teamCount; i++)
        {
            Transform spawn = terrain.GetSpawnTransform();

            // Get all types from player
            CompileManager.Instance.CompileCodeFromOrigin(includedPlayers[i].source, (CompiledData data) =>
            {
                //Create control
                GameObject controlGo = Instantiate(controlBase, spawn.position, Quaternion.identity);
                GameObject skin = Instantiate(theme.control, controlGo.transform);
                skin.transform.SetAsFirstSibling();
                controlGo.AddComponent(data.control);
                controlGo.name = "Control_" + includedPlayers[i].name;
                controls[i] = controlGo.GetComponent<ControlBase>();

                if (controls[i] == null)
                {
                    Debug.LogError("Missing Control Component for Control prefab with player " + includedPlayers[i].name);
                    return;
                }

                controls[i].SetTeam(currentTeam);
                currentPlayerCountInTeam++;

                if (gameConfiguration.playerPerTeam == currentPlayerCountInTeam)
                {
                    currentTeam++;
                    currentPlayerCountInTeam = 0;
                }

                // Get player color
                Color c;
                ColorUtility.TryParseHtmlString(includedPlayers[i].color, out c);
                if (colorsUsed.Contains(c))
                {
                    c = UnityEngine.Random.ColorHSV();
                }
                controls[i].ChangeColor(c);
                colorsUsed.Add(c);

                // Setup UI
                if (i < playerCards.Length)
                {
                    if (playerCards[i] != null)
                    {
                        playerCards[i].Set(includedPlayers[i].name, c, controls[i].Team);
                    }
                }

                if (gameConfiguration.baseUnits != null)
                {
                    //Add units
                    foreach (GameConfiguration.Units u in gameConfiguration.baseUnits)
                    {
                        switch (u.kind)
                        {
                            case EntityKind.Interceptor:
                                AddUnits(controls[i], c, u.count, interceptorBase, theme.interceptor, data.interceptor);
                                break;
                            case EntityKind.Destructor:
                                AddUnits(controls[i], c, u.count, destructorBase, theme.destructor, data.destructor);
                                break;
                            default:
                                continue;
                        }

                    }
                }
            });
        }

        //map.BakeNavMesh();

        // Set TimeScale
        currentTimeScaleIndex = 0;
        Time.timeScale = timeScales[currentTimeScaleIndex];
        if (timeScaleTxt != null)
            timeScaleTxt.text = "x" + timeScales[currentTimeScaleIndex];

        // UI Show
        transform.GetChild(0).gameObject.SetActive(true);
    }

    void AddUnits(ControlBase control, Color c, int quantity, GameObject baseObj, GameObject prefab, Type script)
    {
        for (int j = 0; j < quantity; j++)
        {
            // Instantiate
            GameObject created = Instantiate(baseObj, control.transform.position, Quaternion.identity);
            GameObject skin = Instantiate(prefab, created.transform);
            skin.transform.SetAsFirstSibling();
            created.AddComponent(script);
            created.transform.position = control.transform.position + Vector3.forward - new Vector3(0, control.transform.position.y, 0);
            Agent a = created.GetComponent<Agent>();
            a.ChangeColor(c);
            control.AddAgent(a);
        }
    }

    void SelectMap()
    {
        // Random map
        if (MapRandom)
        {
            if (maps != null && maps.Count > 0)
            {
                List<GameObject> list = maps.FindAll((map) => map.GetComponent<Map>().MaxPlayersCount == includedPlayers.Count);
                if (list.Count == 0)
                {
                    LogManager.Info("No map available for the selected player count.");
                    return;
                }
                map = list[UnityEngine.Random.Range(0, list.Count)].GetComponent<Map>();
            }
            else
            {
                LogManager.Error("Something went wrong with selection and loading of random map.", file: true);
                return;
            }
        }

        // Instantiate Map
        terrain = Instantiate(map.gameObject).GetComponent<Map>();

        //Setup camera
        Camera.main.fieldOfView = map.CamFOV;
    }

    void InstantiatePool()
    {
        for (int i = 0; i < poolCount; i++)
        {
            GameObject go = Instantiate(theme.projectile);
            go.SetActive(false);
            _pool.Add(go);
        }
    }

    public GameObject GetProjectile()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].activeInHierarchy)
            {
                return _pool[i];
            }
        }
        if (!poolWillGrow)
            return null;

        GameObject go = Instantiate(theme.projectile);
        go.SetActive(false);
        _pool.Add(go);
        return go;
    }

    public void ControlIsDestroyed(ControlBase control)
    {
        //Check if it's a controlbase who called this function
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        var method = stackTrace.GetFrame(1).GetMethod();
        if (method.ReflectedType.Name != "ControlBase" || method.Name != "LastWordBeforeToDie")
        {
            throw new System.Exception("Someone is cheating ! (try to end the game from :" + method.ReflectedType.Name + " with method " + method.Name);
        }

        //Check if control doesn't have life
        if (control.IsAlive)
        {
            Debug.LogError("Control has been destroyed but still had life.");
            return;
        }

        //Remove players from actual players
        eliminated.Add(control.Team);
        Debug.Log("Eliminated " + eliminated.Count + " / " + includedPlayers.Count);
        if (eliminated.Count == includedPlayers.Count - 1)
        {
            Debug.Log("Finish");
            //We're done we got our winner
            foreach (ControlBase c in controls)
            {
                if (c.IsAlive && !eliminated.Contains(c.Team))
                {
                    //Show gameover screen
                    if (winnerNameTxt != null)
                    {
                        winnerNameTxt.text = includedPlayers[c.Team].name;
                        Color color = Color.white;
                        ColorUtility.TryParseHtmlString(includedPlayers[c.Team].color, out color);
                        winnerNameTxt.color = color;
                    }
                    gameOverUI.SetActive(true);
                    Debug.Log("Winner is " + includedPlayers[c.Team].name + "!");
                }
            }
        }
    }

    // Time Scale

    public void TimeScaleUp()
    {
        currentTimeScaleIndex++;
        if (currentTimeScaleIndex >= timeScales.Length)
        {
            currentTimeScaleIndex = 0;
        }
        if (timeScaleTxt != null)
            timeScaleTxt.text = "x" + timeScales[currentTimeScaleIndex];
        if (Time.timeScale != 0) Time.timeScale = timeScales[currentTimeScaleIndex];
    }

    public void PauseTimeScale(bool enable)
    {
        Time.timeScale = enable ? 0 : timeScales[currentTimeScaleIndex];
    }

    // IArena Players

    public void LoadPlayersFromFiles()
    {
        RemoveAllPlayers();
        foreach (string path in FileManager.ReadDirectory("Players"))
        {
            string content = FileManager.ReadFile(path);
            PlayerConfig p = JsonUtility.FromJson<PlayerConfig>(content);
            p.path = path;
            AddPlayer(p);
        }
    }

    public List<PlayerConfig> GetPlayers()
    {
        return players;
    }

    public void AddPlayer(PlayerConfig p)
    {
        
        players.Add(p);
    }

    public void SavePlayer(PlayerConfig p)
    {
        PlayerConfigFile player = new PlayerConfigFile(p);
        if (p.path == null)
        {
            string path = FileManager.SaveFile("Players", p.name, new List<FileManager.Extensions>() { FileManager.Extensions.JSON });
            FileManager.WriteFile(path, JsonUtility.ToJson(player, true));
        } else
        {
            FileManager.WriteFile(p.path, JsonUtility.ToJson(player, true));
        }
    }

    public void RemovePlayer(PlayerConfig p)
    {
        if (players.Contains(p)) players.Remove(p);
    }

    public void RemoveAllPlayers()
    {
        players = new List<PlayerConfig>();
    }

    // Match included Players

    public bool CanAddMorePlayers()
    {
        if (gameConfiguration != null)
            return includedPlayers.Count < gameConfiguration.teamCount;
        else
            return false;
    }

    public List<PlayerConfig> GetIncludedPlayers()
    {
        return includedPlayers;
    }

    public void AddIncludedPlayer(PlayerConfig player)
    {
        includedPlayers.Add(player);
    }

    public void RemoveIncludedPlayer(PlayerConfig player)
    {
        includedPlayers.Remove(player);
    }

    public void RemoveAllIncludedPlayers()
    {
        includedPlayers = new List<PlayerConfig>();
    }

    // Game Config

    public void ChangeGameConfiguration(GameConfiguration configuration)
    {
        if (!Application.isPlaying)
        {
            gameConfiguration = configuration;
        }
    }

    // Back Button

    public void GoBackToMenu()
    {
        gameOverUI.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(false);
        SceneManager.LoadScene(1);
    }
}
