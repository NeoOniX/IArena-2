using UnityEngine;
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
    }

    [Header("Layers")]
    [SerializeField]
    private LayerMask agentsLayerMask;
    public LayerMask AgentsLayerMask { get { return agentsLayerMask; } }
    [SerializeField]
    private LayerMask obstaclesLayerMask;
    public LayerMask ObstaclesLayerMask { get { return obstaclesLayerMask; } }

    [Header("Defaults")]
    [SerializeField]
    private GameConfiguration defaultConfiguration;
    [SerializeField]
    private Theme defaultTheme;
    [SerializeField]
    private Map defaultMap;

    [SerializeField]
    private List<PlayerConfig> players = new List<PlayerConfig>();
    public int PlayerCount
    {
        get { return players.Count; }
    }

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

    private List<GameObject> _pool = new List<GameObject>();
    private ControlBase[] controls;
    private List<int> eliminated = new List<int>();

    private int currentTimeScaleIndex = 0;
    private float[] timeScales = new float[4] { 1, 2, 4, 6 };
    private List<Color> colorsUsed = new List<Color>();

    private Theme theme;
    private GameConfiguration gameConfiguration;
    private Map map;

    [HideInInspector]
    public bool forceMap;
    [HideInInspector]
    public string forcedMapName;

    public bool Hardcore
    {
        get { return gameConfiguration.hardcore; }
    }

    void Start()
    {
        LoadPlayersFromFiles();
        UseDefaults();
    }

    public void UseDefaults()
    {
        gameConfiguration = defaultConfiguration;
        theme = defaultTheme;
        map = defaultMap;
    }

    void InitGame()
    {
        // Projectiles Pool
        InstantiatePool();

        // Check players count
        if (players.Count != gameConfiguration.teamCount)
        {
            Debug.LogError("There isn't enough players to play with this game configuration");
            return;
        }

        // Choose map
        SelectMap();
        if (map == null) return;

        if (gameConfiguration.teamCount > map.MaxPlayersCount)
        {
            Debug.LogError("Please select a bigger map to use this game mode");
            Application.Quit(-1);
        }

        controls = new ControlBase[gameConfiguration.teamCount];
        colorsUsed = new List<Color>();
        eliminated = new List<int>();

        if (players.Count != gameConfiguration.teamCount)
        {
            Debug.LogError("There isn't any player in the arena");
            Application.Quit(-1);
        }

        int currentTeam = 0;
        int currentPlayerCountInTeam = 0;

        //load teams
        for (int i = 0; i < gameConfiguration.teamCount; i++)
        {
            Transform spawn = map.GetSpawnTransform();

            //Create control
            GameObject controlGo = Instantiate(theme.control, spawn.position, Quaternion.identity);
            // Add Script to control
            // ...
            controlGo.name = "Control_" + players[i].name;
            controls[i] = controlGo.GetComponent<ControlBase>();

            if (controls[i] == null)
            {
                Debug.LogError("Missing Control Component for Control prefab with player " + players[i].name);
                return;
            }

            controls[i].SetTeam(currentTeam);
            currentPlayerCountInTeam++;

            if (gameConfiguration.playerPerTeam == currentPlayerCountInTeam)
            {
                currentTeam++;
                currentPlayerCountInTeam = 0;
            }

            Color c;
            ColorUtility.TryParseHtmlString(players[i].color, out c);
            if (colorsUsed.Contains(c))
            {
                //Already used color lets choose another color
                c = Random.ColorHSV();
            }
            controls[i].ChangeColor(c);
            colorsUsed.Add(c);

            //update UI
            if (i < playerCards.Length)
            {
                if (playerCards[i] != null)
                {
                    playerCards[i].Set(players[i].name, c, controls[i].Team);
                }
            }

            if (gameConfiguration.baseUnits != null)
            {
                //Add units
                foreach (GameConfiguration.Units u in gameConfiguration.baseUnits)
                {
                    GameObject prefab = null;
                    // object script = null;
                    switch (u.kind)
                    {
                        case EntityKind.Interceptor:
                            prefab = theme.interceptor;
                            break;
                        case EntityKind.Destructor:
                            prefab = theme.destructor;
                            break;
                        default:
                            continue;
                    }
                    for (int j = 0; j < u.count; j++)
                    {
                        // instantiate
                        GameObject created = Instantiate(prefab, spawn.position, Quaternion.identity);
                        // created.AddComponent(script.GetType());
                        created.transform.position = controlGo.transform.position + Vector3.forward - new Vector3(0, controlGo.transform.position.y, 0);
                        Agent a = created.GetComponent<Agent>();
                        a.ChangeColor(c);
                        controls[i].AddAgent(a);
                    }
                }
            }
        }

        //map.BakeNavMesh();

        currentTimeScaleIndex = 0;
        Time.timeScale = timeScales[currentTimeScaleIndex];
        if (timeScaleTxt != null)
            timeScaleTxt.text = "x" + timeScales[currentTimeScaleIndex];
    }

    void SelectMap()
    {
        if (forceMap)
        {
            GameObject m = Resources.Load("Maps/" + forcedMapName) as GameObject;
            if (m == null)
            {
                Debug.LogError("Trying to force map but can't find it...");
                return;
            }
            map = Instantiate(m).GetComponent<Map>();
        }
        else
        {
            Object[] maps = Resources.LoadAll("Maps/", typeof(GameObject));
            if (maps != null && maps.Length > 0)
            {
                GameObject selectedMap = Instantiate(maps[Random.Range(0, maps.Length)] as GameObject, Vector3.zero, Quaternion.identity);
                map = selectedMap.GetComponent<Map>();
            }
            else
            {
                Debug.LogError("Something went wrong with loading of maps...");
                return;
            }
        }
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
        Debug.Log("Eliminated " + eliminated.Count + " / " + players.Count);
        if (eliminated.Count == players.Count - 1)
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
                        winnerNameTxt.text = players[c.Team].name;
                    }
                    gameOverUI.SetActive(true);
                    Debug.Log("Winner is " + players[c.Team].name + "!");
                }
            }
        }
    }

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

    public void LoadPlayersFromFiles()
    {
        RemoveAllPlayers();
        foreach (string path in FileManager.ReadDirectory("Players"))
        {
            string content = FileManager.ReadFile(path);
            PlayerConfig p = JsonUtility.FromJson<PlayerConfig>(content);
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

    public void RemovePlayer(PlayerConfig p)
    {
        players.Remove(p);
    }

    public void RemoveAllPlayers()
    {
        players = new List<PlayerConfig>();
    }

    public bool CanAddMorePlayers()
    {
        if (gameConfiguration != null)
            return players.Count < gameConfiguration.teamCount;
        else
            return false;
    }

    public void ChangeGameConfiguration(GameConfiguration configuration)
    {
        if (!Application.isPlaying)
        {
            gameConfiguration = configuration;
        }
    }

}
