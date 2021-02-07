using UnityEngine;
using System.Collections.Generic;

public class ArenaHelper : MonoBehaviour
{
    private static ArenaHelper _instance;
    public static ArenaHelper Instance {
        get { return _instance;}
    }

    void Awake(){
        if (_instance != null){
            DestroyImmediate(this.gameObject);
            return;
        }
        _instance = this;
    }

    [SerializeField]
    private PlayerConfig[] players = new PlayerConfig[0];
    public int PlayerCount {
        get { return players.Length;}
    }

    [SerializeField]
    private GameConfiguration gameConfiguration;

    [SerializeField]
    private Map map;

    [SerializeField]
    private LayerMask agentsLayerMask;
    public LayerMask AgentsLayerMask { get { return agentsLayerMask;}}
    [SerializeField]
    private LayerMask obstaclesLayerMask;
    public LayerMask ObstaclesLayerMask { get { return obstaclesLayerMask;}}
    
    public int poolCount = 10;
    public bool poolWillGrow = true;
    public GameObject laserPrefab;


    [Header("UI")]
    public TMPro.TextMeshProUGUI timeScaleTxt;
    public TMPro.TextMeshProUGUI[] playerNamesTxt = new TMPro.TextMeshProUGUI[0];
    public GameObject gameOverUI;
    public TMPro.TextMeshProUGUI winnerNameTxt;

    private List<GameObject> _laserPool = new List<GameObject>();
    private ControlBase[] controls;
    private List<int> eliminated = new List<int>();

    private int currentTimeScaleIndex = 0;
    private float[] timeScales = new float[4]{ 1, 2, 4, 6};
    private List<Color> colorsUsed = new List<Color>();

    public bool Hardcore {
        get { return gameConfiguration.hardcore; }
    }

    void Start(){
        InstantiatePool();
        InitGame();
    }

    void InitGame(){
        //Check players count
        if (players.Length != gameConfiguration.teamCount){
            Debug.LogError("There isn't enough players to play with this game configuration");
            return;
        }

        controls = new ControlBase[gameConfiguration.teamCount];
        colorsUsed = new List<Color>();
        eliminated = new List<int>();
        
        //load teams
        for (int i = 0; i < gameConfiguration.teamCount ; i++){      
            Transform spawn = map.GetSpawnTransform();     
            //Create control
            GameObject controlGo = Instantiate(players[i].controlPrefab, spawn.position,Quaternion.identity);
            controlGo.name = "Control_"+players[i].name;
            controls[i] = controlGo.GetComponent<ControlBase>();

            if (controls[i] == null){
                Debug.LogError("Missing Control Component for Control prefab with player "+players[i].name);
                return;
            }

            controls[i].SetTeam(i);
            
            Color c = players[i].color;
            if (colorsUsed.Contains(c)){
                //Already used color lets choose another color
                c = Random.ColorHSV();
            }
            controls[i].ChangeColor(c);
            colorsUsed.Add(c);

            //update UI
            if (i < playerNamesTxt.Length){
                if (playerNamesTxt[i] != null){
                    playerNamesTxt[i].text = players[i].name;
                    playerNamesTxt[i].color = c;
                }
            }

            if (gameConfiguration.baseUnits != null){
                //Add units
                foreach (GameConfiguration.Units u in gameConfiguration.baseUnits){
                    GameObject prefab = null;
                    switch (u.kind){
                        case EntityKind.Interceptor:
                            prefab = players[i].interceptorPrefab;
                        break;
                        case EntityKind.Destructor:
                            prefab = players[i].destructorPrefab;
                        break;
                        default:
                            continue;
                    }
                    for (int j = 0; j < u.count ; j++){
                        // instantiate
                        GameObject created = Instantiate(prefab,controlGo.transform, true);
                        created.transform.position = controlGo.transform.position + Vector3.forward - new Vector3(0,controlGo.transform.position.y,0);
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
            timeScaleTxt.text = "x"+timeScales[currentTimeScaleIndex];
    }

    void InstantiatePool(){
        for (int i = 0;i < poolCount;i++){
            GameObject go = Instantiate(laserPrefab);
            go.SetActive(false);
            _laserPool.Add(go);
        }
    }

    public GameObject GetLaserProjectil(){
        for (int i = 0;i<_laserPool.Count;i++){
            if (!_laserPool[i].activeInHierarchy){
                return _laserPool[i];
            }
        }
        if (!poolWillGrow)
            return null;
        
        GameObject go = Instantiate(laserPrefab);
        go.SetActive(false);
        _laserPool.Add(go);
        return go;
    }

    public void ControlIsDestroyed(ControlBase control){
        //Check if it's a controlbase who called this function
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        var method = stackTrace.GetFrame(1).GetMethod();
        if (method.ReflectedType.Name != "ControlBase" || method.Name != "LastWordBeforeToDie"){
            throw new System.Exception("Someone is cheating ! (try to end the game from :"+method.ReflectedType.Name + " with method "+method.Name);
        }

        //Check if control doesn't have life
        if (control.IsAlive){
            Debug.LogError("Control has been destroyed but still had life.");
            return;
        }

        //Remove players from actual players
        eliminated.Add(control.Team);
        Debug.Log("Eliminated "+eliminated.Count + " / "+players.Length);
        if (eliminated.Count == players.Length - 1){
            Debug.Log("Finish");
            //We're done we got our winner
            foreach(ControlBase c in controls){
                if (c.IsAlive && !eliminated.Contains(c.Team)){
                    //Show gameover screen
                    if (winnerNameTxt!= null){
                        winnerNameTxt.text = players[c.Team].name;
                    }
                    gameOverUI.SetActive(true);
                    Debug.Log("Winner is "+players[c.Team].name + "!");
                }
            }
        }
    }

    public void TimeScaleUp(){
        currentTimeScaleIndex++;
        if (currentTimeScaleIndex >= timeScales.Length){
            currentTimeScaleIndex = 0;
        }
        if (timeScaleTxt != null)
            timeScaleTxt.text = "x"+timeScales[currentTimeScaleIndex];
        if (Time.timeScale != 0) Time.timeScale = timeScales[currentTimeScaleIndex];
    }

    public void PauseTimeScale(bool enable){
        Time.timeScale = enable ? 0 : timeScales[currentTimeScaleIndex];
    }

    public void AddPlayer(PlayerConfig p){
        if (!Application.isPlaying){
            List<PlayerConfig> playersList = new List<PlayerConfig>(players);
            playersList.Add(p);
            players = playersList.ToArray();
        }
    }

    public void RemoveAllPlayers(){
        if (!Application.isPlaying){
            players = new PlayerConfig[0];
        }
    }

    public bool CanAddMorePlayers(){
        return players.Length < map.MaxPlayersCount;
    }

    public void ChangeGameConfiguration(GameConfiguration configuration){
        if (!Application.isPlaying){
            gameConfiguration = configuration;
        }
    }
    
}
