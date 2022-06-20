using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Cinemachine;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    // --- VARS ---
    // CHANGEABLE GM VARIABLES (WAITING TIME HERE)
    #region Serializable Private Fields

    [Tooltip("Time spent waiting in lobby before game starts")] [SerializeField]
    private float waitingTime;
    
    #endregion
    
    // PRIVATE GM VARIABLES (PV, STAGE, TIMER, ELIM AND NUM VARS HERE)
    #region Private Fields

    [Tooltip("The prefab to use for representing the timer")]
    private TextMeshProUGUI _timer;
    private PhotonView _photonView;
    private int _stage = 1;
    private int _totalPlayers = 0;
    private bool _eliminated = false;
    private int _elimPositon = 0;
    private int _playerNumber = 0 ;
    private Transform spectateTarget;

    #endregion
    
    // PUBLIC GM VARIABLES (PLAYER PREFAB HERE)
    #region Public Fields
    
    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    
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
            //Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            //LoadArena();
        }
    }
    public override void OnPlayerLeftRoom(Player other)
    {
        //Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            //Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            //LoadArena();
        }
    }
    
    #endregion
    
    // PUBLIC METHODS: LEAVE, GETPLAYERNUM, SETPLAYERNUM, RETURNPLAYERNUM, ELIMPLAYER, GETTOTAL, GETSTAGE
    #region Public Methods

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
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

    public int GetTotalPlayers()
    {
        return _totalPlayers;
    }
    
    public int GetStageNum()
    {
        return _stage;
    }
    
    #endregion

    // CUSTOM PROPS METHODS: GET/SET ELIM, TOP3 AND FINISHED
    #region Custom Property Methods

    public static bool TryGetTop3Players(out string top3Players, int posNum)
    {
        top3Players = "";

        object top3PlayersFromProps;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Top"+posNum, out top3PlayersFromProps))
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
            {"Top"+posNum, (string)top3}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        bool wasSet2 = TryGetTop3Players(out top3Players, posNum);

        //Debug.Log("Set Custom Props for Top 3 Players: "+ props.ToStringFull() + " wasSet: "+wasSet+" NewValue: "+top3Players);
    }
    
    public static bool TryGetFinishedPlayers(out int finishedPlayers, int stageNum)
    {
        finishedPlayers = 0;

        object finishedPlayersFromProps;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("FinishedPlayers"+stageNum, out finishedPlayersFromProps))
        {
            finishedPlayers = (int)finishedPlayersFromProps;
            return true;
        }

        return false;
    }
    public static void SetFinishedPlayers(int num, int stageNum)
    {
        int finishedPlayers;
        bool wasSet = TryGetFinishedPlayers(out finishedPlayers, stageNum);

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            {"FinishedPlayers"+stageNum, (int)num}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        bool wasSet2 = TryGetFinishedPlayers(out finishedPlayers, stageNum);

        //Debug.Log("Set Custom Props for Finished Players: "+ props.ToStringFull() + " wasSet: "+wasSet+" NewValue: "+finishedPlayers + " , wasSet2: " + wasSet2);
    }
    
    
    public static bool TryGetElimPlayers(out int elimPlayers)
    {
        elimPlayers = 0;

        object elimPlayersFromProps;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("ElimPlayers", out elimPlayersFromProps))
        {
            elimPlayers = (int)elimPlayersFromProps;
            return true;
        }

        return false;
    }
    public static void SetElimPlayers(int num)
    {
        int elimPlayers = 0;
        bool wasSet = TryGetElimPlayers(out elimPlayers);

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            {"ElimPlayers", (int)num}
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);


        //Debug.Log("Set Custom Props for Eliminated Players: "+ props.ToStringFull() + " wasSet: "+wasSet);
    }

    #endregion
    
    // PRIVATE METHODS: SPECTATE, LOADARENA, LOADPLAYER, START, UPDATE
    #region Private Methods

    void Spectate()
    {
        if (GameObject.Find("Speedometer"))
        {
            GameObject.Find("Speedometer").SetActive(false);
        }

        CinemachineVirtualCamera cvc = Camera.main.gameObject.GetComponent<CinemachineVirtualCamera>();
            
        spectateTarget = transform;
        bool foundView = false;
        foreach (PhotonView pv in PhotonNetwork.PhotonViewCollection)
        {
            if (!pv.Owner.CustomProperties.ContainsKey("Eliminated") && pv.gameObject != null)
            {
                spectateTarget = pv.gameObject.transform;
                foundView = true;
                break;
            }
        }

        if (!foundView)
        {
            if (GameObject.Find("Danger Wall") != null)
            {
                spectateTarget = GameObject.Find("Danger Wall").transform;
            }
        }

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
        yield return new WaitForSeconds(0.1f);
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

        SceneManager.sceneLoaded += LoadPlayerInLevel;
        DontDestroyOnLoad(this.gameObject);
        _timer = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",this);
        }
        else
        {
            //Debug.LogFormat("We are Instantiating LocalPlayer from {0}", Application.loadedLevelName);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            //PhotonNetwork.Instantiate(this.playerPrefab.name, GameObject.Find("SpawnLocation"+PhotonNetwork.CurrentRoom.PlayerCount).transform.position, Quaternion.identity, 0);
            //Debug.Log("Player Number: "+PhotonNetwork.LocalPlayer.GetPlayerNumber()); //GetPlayerNumber()
            GameObject player = PhotonNetwork.Instantiate(this.playerPrefab.name, GameObject.Find("SpawnLocation" + GetPlayerNumber()).transform.position, GameObject.Find("SpawnLocation" + GetPlayerNumber()).transform.rotation, 0);
            _photonView = player.GetComponent<PhotonView>();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            SetFinishedPlayers(0, _stage);
            SetElimPlayers(0);
            CountdownTimer.SetStartTime();
        }
    }

    private void LoadPlayerInLevel(Scene scene, LoadSceneMode loadSceneMode)
    {
        //_timer = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        //Debug.Log("GameManager Loading Level");
        if (scene.name == "Launcher")
        {
            Destroy(this.gameObject);
        }
        //Debug.Log("Loaded Player- Elim?: "+_eliminated+" PlayerNum: "+_playerNumber);
        // IF NOT PEDESTAL STAGE AND NOT ELIMINATED : CREATE PLAYER CAR AND SET PHOTON VIEW
        if (scene.name != "EndStage" && !_eliminated)
        {
            
        }
        // UPON REACHING PEDESTAL STAGE
        else if (scene.name == "EndStage")
        {
            for (int i = 1; i < 5; i++)
            {
                //Top3PlayerData t3;
                //TryGetTop3Players(out t3, i);
                string t3;
                TryGetTop3Players(out t3, i);
                if (t3 != "")
                {
                    string winnerName = t3;
                    GameObject.Find("TopCar" + i).GetComponent<SetWinnerName>().SetName(winnerName);
                }
                else
                {
                    GameObject.Find("TopCar" + i).SetActive(false);
                    GameObject.Find("Podium"+i).SetActive(false);
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
    }

    private void Update()
    {
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
                LoadArena("EndStage");
            }
        }
        int playersCompleted;
        switch (SceneManager.GetActiveScene().name)
        {
            case "WaitingArea":
                CountdownTimer.TryGetStartTime(out var hit);
                if (PhotonNetwork.CurrentRoom.IsOpen &&
                    (PhotonNetwork.ServerTimestamp - hit) / 1000f > waitingTime)
                {
                    _totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
                    //Debug.Log("TotalPlayers: "+_totalPlayers);
                    if (PhotonNetwork.IsMasterClient)
                    {
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
                //Debug.Log("Name: "+SceneManager.GetActiveScene().name + " Stage: " + _stage + " Players Finished: "+playersCompleted+" Goal: " + (_totalPlayers/2));
                
                if (_stage == 1 && playersCompleted >= (float)_totalPlayers/2)
                {
                    _stage++;
                    SetFinishedPlayers(0, _stage);
                    if (PhotonNetwork.IsMasterClient)
                    {
                        LoadArena("Stage2");
                    }
                }

                break;
            case "Stage2":
                //_timer.gameObject.SetActive(false);
                TryGetFinishedPlayers(out playersCompleted, _stage);
                //Debug.Log("Name: "+SceneManager.GetActiveScene().name + " Stage: " + _stage + " Players Finished: "+playersCompleted+" Goal: " + (PhotonNetwork.CurrentRoom.MaxPlayers/4));
                
                if (_stage == 2 && playersCompleted >= (float)_totalPlayers/4)
                {
                    _stage++;
                    SetFinishedPlayers(0, _stage);
                    if (PhotonNetwork.IsMasterClient)
                    {
                        LoadArena("Stage3");
                    }
                }
                
                break;
            case "Stage3":
                //_timer.gameObject.SetActive(false);
                TryGetFinishedPlayers(out playersCompleted, _stage);
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
                //Debug.Log("Name: "+SceneManager.GetActiveScene().name + " Stage: " + _stage + " Players Finished: "+(_totalPlayers - elimPlayers)+" Goal: 0");
                
                if (_stage == 4 && playersCompleted >= (float)_totalPlayers/16)
                {
                    _stage++;
                    if (PhotonNetwork.IsMasterClient)
                    {
                        LoadArena("EndStage");
                    }
                }
                
                break;
            case "EndStage":
                break;
        }
    }

    #endregion
    
}

