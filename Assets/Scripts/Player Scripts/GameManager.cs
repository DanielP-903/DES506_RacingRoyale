using System.Collections;
using Cinemachine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Cursor = UnityEngine.Cursor;
using Random = UnityEngine.Random;
using Slider = UnityEngine.UI.Slider;

public class GameManager : MonoBehaviourPunCallbacks
{
    // --- VARS ---
    // CHANGEABLE GM VARIABLES (WAITING TIME HERE)

    #region Serializable Private Fields

    [Tooltip("Time spent waiting in lobby before game starts")] [SerializeField]
    private float waitingTime;

    [Tooltip("Time spent waiting before a round starts")] [SerializeField]
    private float startDelay = 3.0f;

    [Tooltip("Number of Bots to Spawn")] [SerializeField]
    private int maxBots = 8;

    #endregion

    // PRIVATE GM VARIABLES (PV, STAGE, TIMER, ELIM AND NUM VARS HERE)

    #region Private Fields

    [Tooltip("The prefab to use for representing the timer")]
    private TextMeshProUGUI _timer;

    private TextMeshProUGUI _placeCounter;
    private GameObject progressPanel;
    private PhotonView _photonView;
    private int _stage = 1;
    private int _totalPlayers = 0;
    public bool _eliminated = false;
    public bool _completed = false;
    private int _elimPositon = 0;
    private int _playerNumber = 0;
    private int _totalBots = 0;
    private GameObject[] botsStored;
    private Transform spectateTarget;
    private GameObject spectateMenu;
    private TextMeshProUGUI spectateText;
    private GameObject attachedPlayer;

    #endregion

    // PUBLIC GM VARIABLES (PLAYER PREFAB HERE)

    #region Public Fields

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;

    [Tooltip("The prefab to use for representing bots")]
    public GameObject botPrefab;

    [Tooltip("The master mixer")] public AudioMixer mixer;

    [HideInInspector] public bool halt = false;

    #endregion



    // --- METHODS ---
    // THIS SECTION IS FOR CALLS TO DO WITH CONNECTING AND DISCONNECTING

    #region Photon Callbacks

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        //Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            _photonView.RPC("sendMessage", RpcTarget.AllViaServer,
                other.NickName + " has joined. " + PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                PhotonNetwork.CurrentRoom.MaxPlayers);
            PhotonNetwork.Destroy(botsStored[PhotonNetwork.CurrentRoom.PlayerCount - 2]);
            //Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            //LoadArena();
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        //Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            _photonView.RPC("sendMessage", RpcTarget.AllViaServer,
                other.NickName + " has left. " + PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                PhotonNetwork.CurrentRoom.MaxPlayers);
            //Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            //LoadArena();

        }
    }

    #endregion

    // PUBLIC METHODS: LEAVE, GETPLAYERNUM, SETPLAYERNUM, RETURNPLAYERNUM, ELIMPLAYER, GETTOTAL, GETSTAGE

    #region Public Methods

    public void LeaveRoom()
    {
        //Debug.Log("Player: "+_photonView.Owner.NickName + " Eliminated.");
        if (_photonView)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { "Eliminated", true }
            };
            _photonView.Owner.SetCustomProperties(props);

            TryGetElimPlayers(out int num);
            int elimPosition = GetTotalPlayers() - num;

            if (elimPosition < 5)
            {
                //Debug.Log("Finished at:" +elimPosition);
                //GameManager.SetTop3Players(_photonView.Owner.NickName, elimPosition);
                SetTopPlayers(_photonView.Owner, elimPosition);
                //string t3;
                //GameManager.TryGetTop3Players(out t3, elimPosition);
                //Debug.Log(t3);
            }

            num = num + 1;
            SetElimPlayers(num);
            //EliminatePlayer(elimPosition);
        }

        PhotonNetwork.LeaveRoom();
    }

    public void SetDelayTimer()
    {
        startDelay = 3.0f;
    }

    public int GetPlayerNumber()
    {
        int counter = 1;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.Equals(PhotonNetwork.LocalPlayer))
            {
                _playerNumber = counter;
                return counter;
            }

            counter++;
        }

        _playerNumber = counter;
        return counter;
    }

    public void SetWaitingTimer(float time)
    {
        waitingTime = time;
    }

    public int setPlayerNumber()
    {
        int counter = 1;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber < PhotonNetwork.LocalPlayer.ActorNumber)
            {
                counter++;
            }
        }

        _playerNumber = counter;
        //Debug.Log("CurrentPlayerNumberAfterSet: "+_playerNumber);
        return counter;
    }

    public int ReturnPlayerNumber()
    {
        return _playerNumber;
    }

    public void EliminatePlayer(int elimPos)
    {
        _elimPositon = elimPos;
        _eliminated = true;
        Spectate();
    }

    public void CompletePlayer()
    {
        _completed = true;
        Spectate();
        _photonView.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        //PhotonNetwork.Destroy(_photonView.gameObject);
        //_photonView.gameObject.SetActive(false);
        _photonView.gameObject.transform.position = new Vector3(1000,1000,1000);
    }

    public int GetTotalPlayers()
    {
        return _totalPlayers;
    }

    public int GetStageNum()
    {
        return _stage;
    }

    public float GetStartDelay()
    {
        return startDelay;
    }

    public void SetSpectateMenu(GameObject newMenu)
    {
        spectateMenu = newMenu;
    }

    public void ChangeSpectateTarget(bool next = true)
    {
        int index = 0;
        ArrayList spectateTargets = new ArrayList();
        spectateTargets.Add(GameObject.Find("Danger Wall").transform);
        CinemachineVirtualCamera cvc = Camera.main.gameObject.GetComponent<CinemachineVirtualCamera>();
        Debug.Log(spectateTargets[0]);
        foreach (PhotonView pv in PhotonNetwork.PhotonViewCollection)
        {
            bool notCompleted = true;
            if (pv.gameObject.GetComponent<PlayerManager>())
            {
                notCompleted = !pv.gameObject.GetComponent<PlayerManager>().completedStage;
            }
            if (!pv.Owner.CustomProperties.ContainsKey("Eliminated") && pv.gameObject != null && pv.gameObject.tag == "Player" && pv.gameObject.name != _photonView.Owner.NickName && notCompleted)
            {
                spectateTargets.Add(pv.gameObject.transform);
                Debug.Log("AddedToSpec");
                if (spectateTarget == pv.gameObject.transform)
                {
                    index = spectateTargets.IndexOf(spectateTarget);
                    Debug.Log("IndexSet: " + index);
                }
            }
        }

        foreach (Transform t in spectateTargets)
        {
            Debug.Log("Item: " + t);
        }

        if (next)
        {
            index++;
            Debug.Log("IndexAfter: " + index);
            if (index >= spectateTargets.Count)
            {
                Debug.Log("IndexReset");
                index = 0;
            }
        }
        else
        {
            index--;
            if (index < 0)
            {
                index = spectateTargets.Count - 1;
            }
        }

        Debug.Log("Index: " + index);
        spectateTarget = (Transform)spectateTargets[index];
        string part1 = "";
        string part2 = "";
        if (!_eliminated)
        {
            part1 = "<color=green>Completed!</color>";
        }
        else
        {
            part1 = "<color=red>Eliminated!</color>";
        }

        if (spectateTarget.CompareTag("EliminationZone"))
        {
            part2 = "Spectating... The Wall";
        }
        else
        {
            part2 = "Spectating... " + spectateTarget.GetComponent<PhotonView>().name;
        }

        spectateText.text = part1 + "\n" + part2;
        if (spectateTarget != null && cvc != null)
        {
            cvc.m_Follow = spectateTarget;
            cvc.m_LookAt = spectateTarget;
        }
    }

    public void IncreaseBotNum()
    {
        _totalBots++;
    }

    public int GetBotNum()
    {
        _totalBots++;
        return (_totalBots - 1);
    }

    #endregion

    // CUSTOM PROPS METHODS: GET/SET ELIM, TOP3 AND FINISHED

    #region Custom Property Methods

    public static bool TryGetTop3Players(out string top3Players, int posNum)
    {
        top3Players = "";

        object top3PlayersFromProps;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Top" + posNum, out top3PlayersFromProps))
        {
            top3Players = (string)top3PlayersFromProps;
            return true;
        }

        return false;
    }

    public static void SetTop3Players(string top3, int posNum)
    {
        string top3Players;
        bool wasSet = TryGetTop3Players(out top3Players, posNum);

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "Top" + posNum, (string)top3 }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        bool wasSet2 = TryGetTop3Players(out top3Players, posNum);

        //Debug.Log("Set Custom Props for Top 3 Players: "+ props.ToStringFull() + " wasSet: "+wasSet+" NewValue: "+top3Players);
    }

    public static bool TryGetTopPlayers(out Player player, int posNum)
    {
        object topPlayersFromProps;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("TopPlayer" + posNum, out topPlayersFromProps))
        {
            player = (Player)topPlayersFromProps;
            return true;
        }

        player = null;
        return false;
    }

    public static void SetTopPlayers(Player player, int posNum)
    {
        Player topPlayer;
        bool wasSet = TryGetTopPlayers(out topPlayer, posNum);

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "TopPlayer" + posNum, (Player)player }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        bool wasSet2 = TryGetTopPlayers(out topPlayer, posNum);

        //Debug.Log("Set Custom Props for Top 3 Players: "+ props.ToStringFull() + " wasSet: "+wasSet+" NewValue: "+top3Players);
    }

    public static bool TryGetFinishedPlayers(out int finishedPlayers, int stageNum)
    {
        finishedPlayers = 0;

        object finishedPlayersFromProps;

        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties != null)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("FinishedPlayers" + stageNum,
                    out finishedPlayersFromProps))
            {
                finishedPlayers = (int)finishedPlayersFromProps;
                return true;
            }
        }
        else
        {
            Debug.Log("TryGetFinishedPlayers: CurrentRoom and/or CurrentRoom.CustomProperties is null!");
        }

        return false;
    }

    public static void SetFinishedPlayers(int num, int stageNum)
    {
        int finishedPlayers;
        bool wasSet = TryGetFinishedPlayers(out finishedPlayers, stageNum);

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "FinishedPlayers" + stageNum, (int)num }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        bool wasSet2 = TryGetFinishedPlayers(out finishedPlayers, stageNum);

        //Debug.Log("Set Custom Props for Finished Players: "+ props.ToStringFull() + " wasSet: "+wasSet+" NewValue: "+finishedPlayers + " , wasSet2: " + wasSet2);
    }


    public static bool TryGetElimPlayers(out int elimPlayers)
    {
        elimPlayers = 0;

        object elimPlayersFromProps;

        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties != null)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("ElimPlayers", out elimPlayersFromProps))
            {
                elimPlayers = (int)elimPlayersFromProps;
                return true;
            }
        }
        else
        {
            Debug.Log("TryGetElimPlayers: CurrentRoom and/or CurrentRoom.CustomProperties is null!");
        }

        return false;
    }

    public static void SetElimPlayers(int num)
    {
        int elimPlayers = 0;
        bool wasSet = TryGetElimPlayers(out elimPlayers);

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "ElimPlayers", (int)num }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);


        //Debug.Log("Set Custom Props for Eliminated Players: "+ props.ToStringFull() + " wasSet: "+wasSet);
    }

    #endregion

    // PRIVATE METHODS: SPECTATE, LOADARENA, LOADPLAYER, START, UPDATE

    #region Private Methods

    void Spectate()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (spectateText != null)
        {
            spectateText.gameObject.SetActive(true);
        }

        if (GameObject.Find("Speedometer"))
        {
            GameObject.Find("Speedometer").SetActive(false);
        }

        if (spectateMenu != null)
        {
            spectateMenu.SetActive(true);
        }

        CinemachineVirtualCamera cvc = Camera.main.gameObject.GetComponent<CinemachineVirtualCamera>();

        spectateTarget = transform;
        bool foundView = false;
        foreach (PhotonView pv in PhotonNetwork.PhotonViewCollection)
        {
            if (!pv.Owner.CustomProperties.ContainsKey("Eliminated") && pv.gameObject != null && pv.gameObject.tag == "Player" && pv.gameObject.name != _photonView.Owner.NickName)
            {
                spectateTarget = pv.gameObject.transform;
                foundView = true;
                break;
            }
        }

        string part1 = "";
        string part2 = "";
        if (!_eliminated)
        {
            part1 = "<color=green>Completed!</color>";
        }
        else
        {
            part1 = "<color=red>Eliminated!</color>";
        }

        if (!foundView)
        {
            if (GameObject.Find("Danger Wall") != null)
            {
                spectateTarget = GameObject.Find("Danger Wall").transform;
                part2 = "Spectating... The Wall";
            }
        }
        else
        {
            part2 = "Spectating... " + spectateTarget.GetComponent<PhotonView>().name;
        }

        spectateText.text = part1 + "\n" + part2;

        if (spectateTarget != null && cvc != null)
        {
            cvc.m_Follow = spectateTarget;
            cvc.m_LookAt = spectateTarget;
        }
    }

    void LoadArena(string arenaName)
    {
        StartCoroutine(loadingArena(arenaName));
    }

    IEnumerator loadingArena(string arenaName)
    {
        yield return new WaitForSeconds(5f);
        _photonView.RPC("fadeOut", RpcTarget.AllViaServer);
        yield return new WaitForSeconds(2f);
        if (!PhotonNetwork.IsMasterClient)
        {
            //Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        else
        {
            //Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel(arenaName);
        }
    }

    private void Start()
    {
        //Debug.Log("Running!");
        Cursor.visible = false;
        //SceneManager.sceneLoaded -= LoadPlayerInLevel;
        DontDestroyOnLoad(this.gameObject);
        _timer = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        GameObject.Find("PlaceCounter").SetActive(false);
        GameObject.Find("Message").SetActive(false);
        GameObject spectateObject = GameObject.Find("SpectatorText");
        if (spectateObject)
        {
            spectateText = spectateObject.GetComponent<TextMeshProUGUI>();
            spectateText.gameObject.SetActive(false);
            //Debug.Log("Disabled Spectator Text");
        }

        if (playerPrefab == null)
        {
            Debug.LogError(
                "<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",
                this);
        }
        else
        {
            //Debug.LogFormat("We are Instantiating LocalPlayer from {0}", Application.loadedLevelName);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            //PhotonNetwork.Instantiate(this.playerPrefab.name, GameObject.Find("SpawnLocation"+PhotonNetwork.CurrentRoom.PlayerCount).transform.position, Quaternion.identity, 0);
            //Debug.Log("Player Number: "+PhotonNetwork.LocalPlayer.GetPlayerNumber()); //GetPlayerNumber()
            //attachedPlayer = PhotonNetwork.Instantiate(this.playerPrefab.name, GameObject.Find("SpawnLocation" + GetPlayerNumber()).transform.position, GameObject.Find("SpawnLocation" + GetPlayerNumber()).transform.rotation, 0);
            //_photonView = attachedPlayer.GetComponent<PhotonView>();
            //attachedPlayer.name = _photonView.Owner.NickName;
            GameObject player = PhotonNetwork.Instantiate(this.playerPrefab.name,
                GameObject.Find("SpawnLocation" + GetPlayerNumber()).transform.position,
                GameObject.Find("SpawnLocation" + GetPlayerNumber()).transform.rotation, 0);
            _photonView = player.GetComponent<PhotonView>();
            player.name = _photonView.Owner.NickName;
            //Debug.Log("GotPlayerView: "+_photonView);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Max Bots: " + maxBots);
            string s = Resources.Load<TextAsset>("Names").ToString();
            string[] linesInFile = s.Split('\n');
            botsStored = new GameObject[maxBots];
            for (int i = 0; i < maxBots; i++)
            {
                GameObject bot = PhotonNetwork.Instantiate(this.botPrefab.name,
                    GameObject.Find("SpawnLocation" + (i + 2)).transform.position,
                    GameObject.Find("SpawnLocation" + GetPlayerNumber()).transform.rotation, 0);
                bot.name = "Bot " + linesInFile[Random.Range(0, linesInFile.Length - 1)];
                botsStored[i] = bot;
                //bot.GetComponent<BotCarController>().setName(linesInFile[Random.Range(0, linesInFile.Length-1)]);
                
                ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
                hash.Add(("Timer1"), 0);
                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                hash = new ExitGames.Client.Photon.Hashtable();
                hash.Add(("Timer2"), 0);
                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                hash = new ExitGames.Client.Photon.Hashtable();
                hash.Add(("Timer3"), 0);
                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                hash = new ExitGames.Client.Photon.Hashtable();
                hash.Add(("Timer4"), 0);
                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
            }

            SetFinishedPlayers(0, _stage);
            SetElimPlayers(0);
            CountdownTimer.SetStartTime();
        }
    }

    public GameObject[] GetBots()
    {
        return botsStored;
    }

    public int GetMaxBots()
    {
        return maxBots;
    }
    
    void OnLevelWasLoaded()
    {
        if (SceneManager.GetActiveScene().name != "WaitingArea")
        {
            SetUp();
        }
    }

    /*private void LoadPlayerInLevel(Scene scene, LoadSceneMode loadSceneMode)
{
    
    if (PlayerPrefs.HasKey("MasterVol"))
    {
        mixer.SetFloat("Master", PlayerPrefs.GetFloat("MasterVol"));
    }
    if (PlayerPrefs.HasKey("MusicVol"))
    {
        mixer.SetFloat("Music", PlayerPrefs.GetFloat("MusicVol"));
    }
    if (PlayerPrefs.HasKey("SoundVol"))
    {
        mixer.SetFloat("Sound", PlayerPrefs.GetFloat("SoundVol"));
    }
    _totalBots = 0;
    
    // progressPanel = GameObject.FindGameObjectWithTag("LoadingBar");
    // progressPanel.SetActive(false);
    //Debug.Log("GameManager Loading Level");
    if (scene.name == "Launcher")
    {
        Debug.Log("PlayerDestroyed");
        Destroy(this.gameObject);
        Destroy(this);
    }
    //Debug.Log("Loaded Player- Elim?: "+_eliminated+" PlayerNum: "+_playerNumber);
    // IF NOT PEDESTAL STAGE AND NOT ELIMINATED : CREATE PLAYER CAR AND SET PHOTON VIEW
    if (scene.name != "EndStage" && scene.name != "Launcher")
    {
        //_photonView.gameObject.SetActive(true);
        //_photonView.gameObject.GetComponent<PlayerManager>().SetUp();
        if (!_photonView.gameObject.activeInHierarchy)
        {
            _photonView.gameObject.SetActive(true);
            _photonView.gameObject.GetComponent<PlayerManager>().SetUp();
        }
        
        GameObject spectateObject = GameObject.Find("SpectatorText");
        if (spectateObject)
        {
            spectateText = spectateObject.GetComponent<TextMeshProUGUI>();
            spectateText.gameObject.SetActive(false);
            //Debug.Log("Disabled Spectator Text");
        }

        _placeCounter = GameObject.Find("PlaceCounter").GetComponent<TextMeshProUGUI>();
        //_timer = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            int playersInScene = 0;
            int maxBotsInScene = 0;
            switch (scene.name)
            {
                case "Stage1":
                    playersInScene = PhotonNetwork.CurrentRoom.MaxPlayers;
                    maxBotsInScene = maxBots;
                    break;
                case "Stage2":
                    playersInScene = PhotonNetwork.CurrentRoom.MaxPlayers/2;
                    maxBotsInScene = maxBots;
                    break;
                case "Stage3":
                    playersInScene = PhotonNetwork.CurrentRoom.MaxPlayers/4;
                    maxBotsInScene = maxBots;
                    break;
                case "Stage4":
                    playersInScene = PhotonNetwork.CurrentRoom.MaxPlayers/8;
                    maxBotsInScene = maxBots;
                    break;
            }
            
            int elimPlayersTotal = 0;
            TryGetElimPlayers(out elimPlayersTotal);
            string s = Resources.Load<TextAsset>("Names").ToString();
            string[] linesInFile = s.Split('\n');
            for (int i = _totalPlayers - elimPlayersTotal + 1; i < playersInScene && i < maxBotsInScene; i++)
            {
                GameObject bot = PhotonNetwork.Instantiate(this.botPrefab.name,
                    new Vector3(0, -100, 0),
                    quaternion.identity, 0);
                bot.name = "Bot "+ linesInFile[Random.Range(0, linesInFile.Length - 1)];
                //bot.GetComponent<BotCarController>().setName(linesInFile[Random.Range(0, linesInFile.Length-1)]);
            }
        }
    }
    // UPON REACHING PEDESTAL STAGE
    else if (scene.name == "EndStage")
    {
        //PhotonNetwork.Destroy( _photonView.gameObject);
        //spectateMenu.SetActive(false);
        for (int i = 1; i < 5; i++)
        {
            //Top3PlayerData t3;
            //TryGetTop3Players(out t3, i);
            Player p;
            TryGetTopPlayers(out p, i);
            if (p != null)
            {
                string winnerName = p.NickName;
                int mesh = (int)p.CustomProperties["Skin"];
                GameObject.Find("TopCar" + i).GetComponent<SetWinnerName>().SetWinner(winnerName, mesh);
            }
            else
            {
                GameObject.Find("TopCar" + i).SetActive(false);
                //GameObject.Find("Podium"+i).SetActive(false);
            }
        }

        GameObject mainCam = GameObject.Find("Main Camera");
        if (mainCam != null)
        {
            Destroy(mainCam);
        }
    }
    // IF ELIMINATED AND NOT PEDESTAL STAGE : SPECTATE RANDOM PLAYER
    else
    {
        //Debug.Log("SpectateUponLoad");
        Spectate();
    }
}*/

    void SetUp()
    {
        /*foreach (AudioListener al in FindObjectsOfType<AudioListener>())
    {
        Debug.Log("AL: "+al.gameObject);
    }*/
        //Debug.Log("SetUp was called");
        if (this.gameObject != null)
        {
            if (_eliminated)
            {
                Debug.Log("PlayerEliminated");
                Spectate();
            }
            else if (_completed)
            {
                _completed = false;
            }

            _totalPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["TotalPlayerCount"];
            _totalBots = 0;

            //Debug.Log("GameManager Loading Level");
            if (SceneManager.GetActiveScene().name == "Launcher")
            {
                Debug.Log("PlayerDestroyed");
                Destroy(this.gameObject);
                Destroy(this);
            }

            //Debug.Log("Loaded Player- Elim?: "+_eliminated+" PlayerNum: "+_playerNumber);
            // IF NOT PEDESTAL STAGE AND NOT ELIMINATED : CREATE PLAYER CAR AND SET PHOTON VIEW
            else if (SceneManager.GetActiveScene().name != "EndStage" &&
                     SceneManager.GetActiveScene().name != "Launcher")
            {
                //_photonView.gameObject.SetActive(true);
                //_photonView.gameObject.GetComponent<PlayerManager>().SetUp();
                if (!_photonView.gameObject.activeInHierarchy)
                {
                    _photonView.gameObject.SetActive(true);
                    _photonView.gameObject.GetComponent<PlayerManager>().SetUp();
                }

                GameObject spectateObject = GameObject.Find("SpectatorText");
                if (spectateObject)
                {
                    spectateText = spectateObject.GetComponent<TextMeshProUGUI>();
                    spectateText.gameObject.SetActive(false);
                    //Debug.Log("Disabled Spectator Text");
                }

                _placeCounter = GameObject.Find("PlaceCounter").GetComponent<TextMeshProUGUI>();
                //_timer = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
                if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    int playersInScene = 0;
                    int maxBotsInScene = 0;
                    switch (SceneManager.GetActiveScene().name)
                    {
                        case "Stage1":
                            playersInScene = PhotonNetwork.CurrentRoom.MaxPlayers;
                            maxBotsInScene = maxBots;
                            foreach (DictionaryEntry entry in PhotonNetwork.CurrentRoom.CustomProperties)
                            {
                                Debug.Log("K: "+entry.Key + " V: "+entry.Value);
                            }
                            break;
                        case "Stage2":
                            playersInScene = PhotonNetwork.CurrentRoom.MaxPlayers / 2;
                            maxBotsInScene = maxBots;
                            break;
                        case "Stage3":
                            playersInScene = PhotonNetwork.CurrentRoom.MaxPlayers / 4;
                            maxBotsInScene = maxBots;
                            break;
                        case "Stage4":
                            playersInScene = PhotonNetwork.CurrentRoom.MaxPlayers / 8;
                            maxBotsInScene = maxBots;
                            break;
                    }

                    int elimPlayersTotal = 0;
                    TryGetElimPlayers(out elimPlayersTotal);
                    string s = Resources.Load<TextAsset>("Names").ToString();
                    string[] linesInFile = s.Split('\n');
                    botsStored = new GameObject[maxBots];
                    for (int i = _totalPlayers - elimPlayersTotal + 1; i < playersInScene && i < maxBotsInScene; i++)
                    {
                        GameObject bot = PhotonNetwork.Instantiate(this.botPrefab.name,
                            new Vector3(0, -100, 0),
                            quaternion.identity, 0);
                        bot.name = "Bot " + linesInFile[Random.Range(0, linesInFile.Length - 1)];
                        botsStored[i] = bot;
                        //bot.GetComponent<BotCarController>().setName(linesInFile[Random.Range(0, linesInFile.Length-1)]);
                    }
                }
            }
            // UPON REACHING PEDESTAL STAGE
            else if (SceneManager.GetActiveScene().name == "EndStage")
            {
                //PhotonNetwork.Destroy( _photonView.gameObject);
                //spectateMenu.SetActive(false);
                for (int i = 1; i < 5; i++)
                {
                    //Top3PlayerData t3;
                    //TryGetTop3Players(out t3, i);
                    Player p;
                    TryGetTopPlayers(out p, i);
                    if (p != null)
                    {
                        string winnerName = p.NickName;
                        int mesh = (int)p.CustomProperties["Skin"];
                        GameObject.Find("TopCar" + i).GetComponent<SetWinnerName>().SetWinner(winnerName, mesh);
                    }
                    else
                    {
                        GameObject.Find("TopCar" + i).SetActive(false);
                        //GameObject.Find("Podium"+i).SetActive(false);
                    }
                    /*string t3;
                TryGetTop3Players(out t3, i);
                if (t3 != "")
                {
                    string winnerName = t3;
                    GameObject.Find("TopCar" + i).GetComponent<SetWinnerName>().SetName(winnerName);
                }*/

                }

                GameObject mainCam = GameObject.Find("Main Camera");
                if (mainCam != null)
                {
                    Destroy(mainCam);
                }
            }
            // IF ELIMINATED AND NOT PEDESTAL STAGE : SPECTATE RANDOM PLAYER
            else
            {
                //Debug.Log("SpectateUponLoad");
                Spectate();
            }
        }
    }

    private void Update()
    {
        //Debug.Log("TotalPlayers: "+PhotonNetwork.CurrentRoom.Players.Count);
        if (spectateTarget == null && _eliminated)
        {
            Spectate();
        }

        int elimPlayers;
        TryGetElimPlayers(out elimPlayers);
        if (elimPlayers != 0 && elimPlayers == _totalPlayers && _stage > 0 && _stage < 5)
        {
            _stage = 5;
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                progressPanel = GameObject.FindGameObjectWithTag("MainCanvas").transform.GetChild(11).gameObject;
                progressPanel.SetActive(true);
                StartCoroutine(LoadingBar());
                LoadArena("EndStage");
            }
        }

        int playersCompleted;
        switch (SceneManager.GetActiveScene().name)
        {
            case "Launcher":
                Debug.Log("PlayerDestroyed");
                Destroy(this.gameObject);
                Destroy(this);
                break;
            case "WaitingArea":
                if (!progressPanel)
                {
                    Debug.LogWarning("Progress panel does not exist");
                }

                //_placeCounter.gameObject.SetActive(false);
                CountdownTimer.TryGetStartTime(out var hit);
                if (PhotonNetwork.CurrentRoom.IsOpen &&
                    (PhotonNetwork.ServerTimestamp - hit) / 1000f > waitingTime)
                {
                    //_totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
                    //Debug.Log("TotalPlayers: "+_totalPlayers);

                    progressPanel = GameObject.FindGameObjectWithTag("MainCanvas").transform.GetChild(11).gameObject;
                    progressPanel.SetActive(true);
                    StartCoroutine(LoadingBar());
                    if (PhotonNetwork.IsMasterClient)
                    {
                        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
                        hash.Add("TotalPlayerCount", (int)PhotonNetwork.CurrentRoom.PlayerCount);
                        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                        PhotonNetwork.CurrentRoom.IsOpen = false;
                        LoadArena("Stage1");
                    }
                }
                else if (!PhotonNetwork.CurrentRoom.IsOpen)
                {
                    _timer.gameObject.SetActive(false);
                }

                float tempTime = (float)waitingTime - (float)((PhotonNetwork.ServerTimestamp - hit) / 1000f);
                int sec = Mathf.FloorToInt(tempTime);
                int milSec = Mathf.FloorToInt((tempTime - sec) * 100f);
                _timer.text = sec + ":" + milSec;
                break;
            case "Stage1":
                //_timer.gameObject.SetActive(false);
                TryGetFinishedPlayers(out playersCompleted, _stage);
                TryGetElimPlayers(out elimPlayers);
                if (Mathf.Min(Mathf.Ceil((float)_totalPlayers / 2) - playersCompleted, PhotonNetwork.CurrentRoom.PlayerCount-playersCompleted) == 1)
                {
                    _placeCounter.text = "1 place left!";
                }
                else
                {
                    _placeCounter.text = Mathf.Min(Mathf.Ceil((float)_totalPlayers / 2) - playersCompleted,PhotonNetwork.CurrentRoom.PlayerCount-playersCompleted) - playersCompleted + " places left!";
                }
                //Debug.Log("Name: "+SceneManager.GetActiveScene().name + " Stage: " + _stage + " Players Finished: "+playersCompleted+" Goal: " + (_totalPlayers/2));

                if (_stage == 1 && (playersCompleted >= (float)_totalPlayers / 2 || playersCompleted + elimPlayers >= _totalPlayers))
                {
                    _stage = 2;
                    SetFinishedPlayers(0, _stage);
                    if (!_completed && !_eliminated)
                    {
                        _photonView.gameObject.GetComponent<PlayerManager>().EliminateCurrentPlayer();
                    }
                    if (PhotonNetwork.IsMasterClient)
                    {
                        progressPanel = GameObject.FindGameObjectWithTag("MainCanvas").transform.GetChild(11)
                            .gameObject;
                        progressPanel.SetActive(true);
                        StartCoroutine(LoadingBar());
                        LoadArena("Stage2");
                    }
                }

                break;
            case "Stage2":
                //_timer.gameObject.SetActive(false);
                TryGetFinishedPlayers(out playersCompleted, _stage);
                TryGetElimPlayers(out elimPlayers);
                if (Mathf.Min(Mathf.Ceil((float)_totalPlayers / 2) - playersCompleted, PhotonNetwork.CurrentRoom.PlayerCount-playersCompleted) == 1)
                {
                    _placeCounter.text = "1 podium space left!";
                }
                else
                {
                    _placeCounter.text = Mathf.Min(4 - playersCompleted,PhotonNetwork.CurrentRoom.PlayerCount-playersCompleted) + " podium spaces left!";
                }

                //Debug.Log("Name: "+SceneManager.GetActiveScene().name + " Stage: " + _stage + " Players Finished: "+(_totalPlayers - elimPlayers)+" Goal: 0");
                //_stage == 2 && playersCompleted >= (float)_totalPlayers/16
                if (_stage == 2 && (playersCompleted >= (int)4 || elimPlayers + playersCompleted >= _totalPlayers))
                {
                    _stage++;
                    if (PhotonNetwork.IsMasterClient)
                    {
                        progressPanel = GameObject.FindGameObjectWithTag("MainCanvas").transform.GetChild(11)
                            .gameObject;
                        progressPanel.SetActive(true);
                        StartCoroutine(LoadingBar());
                        LoadArena("EndStage");
                    }
                }

                break;
            /*case "Stage2":
            //_timer.gameObject.SetActive(false);
            TryGetFinishedPlayers(out playersCompleted, _stage);
            if (Mathf.Ceil((float)_totalPlayers / 4) - playersCompleted == 1)
            {
                _placeCounter.text = "1 place left!";
            }
            else
            {
                _placeCounter.text = Mathf.Ceil((float)_totalPlayers / 4) - playersCompleted + " places left!";
            }
            //Debug.Log("Name: "+SceneManager.GetActiveScene().name + " Stage: " + _stage + " Players Finished: "+playersCompleted+" Goal: " + (PhotonNetwork.CurrentRoom.MaxPlayers/4));
            
            if (_stage == 2 && playersCompleted >= (float)_totalPlayers/4)
            {
                _stage++;
                SetFinishedPlayers(0, _stage);
                if (PhotonNetwork.IsMasterClient)
                {
                    //LoadArena("Stage3");
                    LoadArena("EndStage");
                }
            }
            
            break;
        case "Stage3":
            //_timer.gameObject.SetActive(false);
            TryGetFinishedPlayers(out playersCompleted, _stage);
            if (Mathf.Ceil((float)_totalPlayers / 8) - playersCompleted == 1)
            {
                _placeCounter.text = "1 place left!";
            }
            else
            {
                _placeCounter.text = Mathf.Ceil((float)_totalPlayers / 8) - playersCompleted + " places left!";
            }
            //Debug.Log("Name: "+SceneManager.GetActiveScene().name + " Stage: " + _stage + " Players Finished: "+playersCompleted+" Goal: " + (PhotonNetwork.CurrentRoom.MaxPlayers/8));
            
            if (_stage == 3 && playersCompleted >= (float)_totalPlayers/8)
            {
                _stage++;
                SetFinishedPlayers(0, _stage);
                if (PhotonNetwork.IsMasterClient)
                {
                    LoadArena("Stage4");
                }
            }
            
            break;
        case "Stage4":
            //_timer.gameObject.SetActive(false);
            TryGetFinishedPlayers(out playersCompleted, _stage);
            TryGetElimPlayers(out elimPlayers);
            _placeCounter.text = Mathf.Ceil(_totalPlayers - elimPlayers) + " players left!";
            
            //Debug.Log("Name: "+SceneManager.GetActiveScene().name + " Stage: " + _stage + " Players Finished: "+(_totalPlayers - elimPlayers)+" Goal: 0");
            
            if (_stage == 4 && playersCompleted >= (float)_totalPlayers/16)
            {
                _stage++;
                if (PhotonNetwork.IsMasterClient)
                {
                    LoadArena("EndStage");
                }
            }
            
            break;*/
            case "EndStage":
                break;
        }
    }

    #endregion

    #region IEnumerators

    // IEnumerator LoadingBar() 
    // {
    //     //Debug.Log("Progress: " + PhotonNetwork.LevelLoadingProgress);
    //     // if (PhotonNetwork._AsyncLevelLoadingOperation != null)
    //     // {
    //     //     while (!PhotonNetwork._AsyncLevelLoadingOperation.isDone)
    //     //     {
    //     //         progressPanel.transform.GetChild(0).GetChild(0).GetComponent<Slider>().value =
    //     //             PhotonNetwork._AsyncLevelLoadingOperation.progress;
    //     //         
    //     //         yield return null;
    //     //     }
    //     //     
    //     // }
    //     // else
    //     // {
    //     //     yield return null;
    //     // }
    //     //
    //     // yield return new WaitForEndOfFrame();
    //
    //     if (progressPanel && progressPanel.transform.childCount > 0)
    //     {
    //         progressPanel.transform.GetChild(0).GetChild(0).GetComponent<Slider>().value = PhotonNetwork.LevelLoadingProgress;
    //         progressPanel.transform.GetChild(1).Rotate(Vector3.forward, -Time.deltaTime * 500.0f, Space.World);
    //         while (PhotonNetwork.LevelLoadingProgress < 1.0f)
    //         {
    //             progressPanel.transform.GetChild(0).GetChild(0).GetComponent<Slider>().value = PhotonNetwork.LevelLoadingProgress;
    //             yield return null;
    //         }
    //
    //         yield return new WaitForEndOfFrame();
    //     }
    //     else
    //     {
    //         yield return new WaitForEndOfFrame();
    //     }
    // }

    IEnumerator LoadingBar()
    {
        while (PhotonNetwork.LevelLoadingProgress < 1.0f)
        {
            progressPanel.transform.GetChild(0).GetChild(0).GetComponent<Slider>().value =
                PhotonNetwork.LevelLoadingProgress;
            progressPanel.transform.GetChild(1).Rotate(Vector3.forward, -Time.deltaTime * 500.0f, Space.World);
            yield return null;
        }

        yield return new WaitForEndOfFrame();

    }

    #endregion

}

