using System.Collections;
using Cinemachine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerManager : MonoBehaviour
{

    // COMPONENTS, INTS AND BOOLS

    #region Non-Serializable Variables

    private PhotonView _photonView;
    private CarController _cc;
    private MeshRenderer _mRend;
    private MeshFilter _mFilt;
    private GameObject _flaps;
    private Rigidbody _rb;
    private PlayerManager target;
    private GameManager _gm;
    private Transform _spawnLocation;
    private GameObject mainCam;
    private GameObject startBlocker;
    private CheckpointSystem _cs;
    private TextMeshProUGUI _messageText;
    private CarVFXHandler _vfx;
    private AlertSystem _as;
    public TextMeshProUGUI startDelayText;

    public float timer = 3;

    private int playerNumber = 0;
    public bool completedStage = false;
    private bool eliminated = false;
    private int elimPosition = 0;
    private bool ready = false;

    #endregion

    // PLAYER NAME UI

    #region Serializable Variables

    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerLicenseText;
    [SerializeField] private TextMeshProUGUI playerFrontLicenseText;

    [SerializeField] private bool debugMode;

    #endregion

    // PRIVATE METHODS

    #region Private Methods

    /// <summary>
    /// Attempt to get the number of total ready players for a stage
    /// </summary>
    /// <param name="readyPlayers">Out number of ready players</param>
    /// <param name="stageNum">Number of stage</param>
    /// <returns>True if found, false if not</returns>
    public static bool TryGetReadyPlayers(out int readyPlayers, int stageNum)
    {
        readyPlayers = 0;

        object readyPlayersFromProps;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("ReadyPlayers" + stageNum,
                out readyPlayersFromProps))
        {
            readyPlayers = (int)readyPlayersFromProps;
            return true;
        }


        return false;
    }

    /// <summary>
    /// Set the number of total ready players for a stage
    /// </summary>
    /// <param name="num">Number of ready players to be set</param>
    /// <param name="stageNum">Number of stage</param>
    /// <returns></returns>
    public static void SetReadyPlayers(int num, int stageNum)
    {
        int readyPlayers;
        bool wasSet = TryGetReadyPlayers(out readyPlayers, stageNum);

        if (!wasSet)
        {
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { "ReadyPlayers" + stageNum, (int)num }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);

            bool wasSet2 = TryGetReadyPlayers(out readyPlayers, stageNum);

            Debug.Log("Set Custom Props for Finished Players: " + props.ToStringFull() + " wasSet: " + wasSet +
                      " NewValue: " + readyPlayers + " , wasSet2: " + wasSet2);
        }
        else
        {
            PhotonNetwork.CurrentRoom.CustomProperties["ReadyPlayers" + stageNum] = num;
        }
    }

    /// <summary>
    /// Attempt to get if a player is ready at a certain stage
    /// </summary>
    /// <param name="readyPlayer">Out true if player is ready</param>
    /// <param name="stageNum">Number of stage</param>
    /// <param name="player">Player to be checked</param>
    /// <returns>True if found, false if not</returns>
    public static bool TryGetReadyPlayer(out bool readyPlayer, int stageNum, Player player)
    {
        readyPlayer = false;

        object readyPlayerFromProps;

        if (player.CustomProperties.TryGetValue("ReadyPlayer" + stageNum, out readyPlayerFromProps))
        {
            readyPlayer = (bool)readyPlayerFromProps;
            return true;
        }


        return false;
    }

    /// <summary>
    /// Set if a player is ready at a certain stage
    /// </summary>
    /// <param name="readyPlayer">Readiness of player</param>
    /// <param name="stageNum">Number of stage</param>
    /// <param name="player">Player to be set</param>
    /// <returns></returns>
    public static void SetReadyPlayer(bool setReady, int stageNum)
    {
        bool readyPlayer;
        bool wasSet = TryGetReadyPlayer(out readyPlayer, stageNum, PhotonNetwork.LocalPlayer);

        /*if (!wasSet)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "ReadyPlayer" + stageNum, (bool)setReady }
        };
        player.SetCustomProperties(props);

        bool wasSet2 = TryGetReadyPlayer(out readyPlayer, stageNum, player);

        Debug.Log("Set Custom Props for Ready Players: " + props.ToStringFull() + " wasSet: " + wasSet +
                  " NewValue: " + readyPlayer + " , wasSet2: " + wasSet2+" Player: "+player);
    }
    else
    {
        player.CustomProperties["ReadyPlayer" + stageNum] = setReady;
        Debug.Log("Set Custom Props for Ready Players: " + setReady+" Player: "+player);
    }*/

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "ReadyPlayer" + stageNum, (bool)setReady }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        bool wasSet2 = TryGetReadyPlayer(out readyPlayer, stageNum, PhotonNetwork.LocalPlayer);

        Debug.Log("Set Custom Props for Ready Players: " + props.ToStringFull() + " wasSet: " + wasSet +
                  " NewValue: " + readyPlayer + " , wasSet2: " + wasSet2 + " Player: " + PhotonNetwork.LocalPlayer);
    }

    /// <summary>
    /// Initialise basics of Player Manager during start occuring in waiting area
    /// </summary>
    /// <returns></returns>
    void Start()
    {
        if (debugMode)
        {
            Debug.Log("DEBUG MODE IS ACTIVE! (PlayerManager)");
            _cc = GetComponent<CarController>();
            _rb = GetComponent<Rigidbody>();
            //_cc.SetUp();
            _cs = GameObject.Find("CheckpointSystem").GetComponent<CheckpointSystem>();
        }

        _vfx = GetComponent<CarVFXHandler>();

        if (!debugMode)
        {
            //SceneManager.sceneLoaded += LoadPMInLevel;
            _photonView = GetComponent<PhotonView>();

            this.gameObject.name = _photonView.Owner.NickName;
            _mRend = transform.Find("CarMesh").GetComponent<MeshRenderer>();
            _mFilt = transform.Find("CarMesh").GetComponent<MeshFilter>();
            _flaps = transform.Find("Flaps").gameObject;
            object skinNumFromProps;
            /*if (_photonView.IsMine && !_photonView.Owner.CustomProperties.TryGetValue("Skin", out skinNumFromProps))
    {
        _photonView.Owner.CustomProperties.Add("Skin", PlayerPrefs.GetInt("Skin"));
    }
    else if (_photonView.IsMine)
    {
        _photonView.Owner.CustomProperties.Remove("Skin");
        _photonView.Owner.CustomProperties.Add("Skin", PlayerPrefs.GetInt("Skin"));
    }*/


            int skinNum = 0;
            if ((int)_photonView.Owner.CustomProperties["Skin"] != null)
            {
                skinNum = (int)_photonView.Owner.CustomProperties["Skin"];
            }

            //Debug.Log("FoundSkinNum: " + (int)_photonView.Owner.CustomProperties["Skin"]);
            /*object skinNumFromProps;
        if (_photonView.Owner.CustomProperties.TryGetValue("Skin", out skinNumFromProps))
        {
            skinNum = (int)skinNumFromProps;
            Debug.Log("FoundSkinNum");
        }*/

            _mRend.material = GameObject.Find("DataManager").GetComponent<DataManager>().GetMats()[skinNum];
            _mFilt.mesh = GameObject.Find("DataManager").GetComponent<DataManager>().GetMesh()[skinNum];
            _flaps.SetActive(skinNum < 3);


            if (_photonView.Owner == null)
            {
                playerNameText.text = "Guest";
                playerLicenseText.text = "Guest";
                playerFrontLicenseText.text = "Guest";
            }
            else
            {
                playerNameText.text = _photonView.Owner.NickName;
                playerLicenseText.text = _photonView.Owner.NickName;
                playerFrontLicenseText.text = _photonView.Owner.NickName;
            }
        }

        if (_photonView != null)
        {
            DontDestroyOnLoad(this.gameObject);
            if (_photonView.IsMine)
            {
                if (!debugMode)
                {
                    //SetReadyPlayers(0, 1);
                    SetReadyPlayer(false, 1);
                }

                _cc = GetComponent<CarController>();
                _rb = GetComponent<Rigidbody>();
                _rb.velocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                if (!_cc.debug)
                {
                    _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
                }

                mainCam = Camera.main.gameObject;
                AudioSource source = mainCam.GetComponent<AudioSource>();
                source.loop = true;
                source.clip = Resources.Load<AudioClip>("Audio/Music/NewMusic");
                source.Play();
                CinemachineVirtualCamera cvc = mainCam.GetComponent<CinemachineVirtualCamera>();
                DontDestroyOnLoad(mainCam);
                var transform1 = transform;
                cvc.m_Follow = transform1;
                cvc.m_LookAt = transform1;

                int spawnNumber = playerNumber;
                if (_gm != null)
                {
                    if (playerNumber == 0)
                    {
                        spawnNumber = _gm.GetPlayerNumber();
                    }
                }
                else
                {
                    Debug.Log("GameManager does not exist!");
                }

                _spawnLocation = GameObject.Find("SpawnLocation" + spawnNumber).transform;
            }
            else
            {
                //parts = GetComponent<CarController>().boostEffects;
                _cc = GetComponent<CarController>();
                Destroy(transform.Find("InputSystem").gameObject);
                Destroy(_cc);
                //Destroy(GetComponent<Rigidbody>());
                Destroy(GetComponent<PlayerPowerups>());
                Destroy(this);
            }
        }
        else
        {
            if (!debugMode)
            {
                playerNumber = _gm.GetPlayerNumber();
                Debug.Log("Photon view NOT DETECTED during start function of PlayerManager" + playerNumber);
            }
        }

        //StartCoroutine(TestScript());
        //playerNumber = _gm.GetPlayerNumber();
    }
    
    /// <summary>
    /// Every frame, check for relevant game checks
    /// </summary>
    /// <returns></returns>
    void Update()
    {
        if (transform.position.y < -5 && _photonView.IsMine)
        {
            //Debug.Log("Less than 5");
            GoToSpawn();
        }

        if (ready)
        {
            //Debug.Log("Ready!");
            //int readyPlayers;
            //TryGetReadyPlayers(out readyPlayers, _gm.GetStageNum());
            //Debug.Log( "Ready Players: "+readyPlayers +":"+ _gm.GetTotalPlayers());
            // && readyPlayers >= _gm.GetTotalPlayers()
            if (_gm.GetStageNum() > 0 && _gm.GetStageNum() < 5 && _gm.halt == false)
            {
                //_photonView.RPC("startDelayTimer", RpcTarget.AllViaServer);
                startDelayTimer();
            }

            ready = false;
        }
    }

    /// <summary>
    /// Initialise basics upon level loaded for Player Manager
    /// </summary>
    /// <returns></returns>
    void OnLevelWasLoaded()
    {
        SetUp();
    }

    /// <summary>
    /// Setup basics of the Player Manager
    /// </summary>
    /// <returns></returns>
    public void SetUp()
    {
        //Debug.Log("PlayerManger Loading Level");
        if (_photonView != null)
        {
            _rb.constraints = RigidbodyConstraints.None;
            CinemachineVirtualCamera cvc = mainCam.GetComponent<CinemachineVirtualCamera>();
            //Debug.Log("LoadingPlayer: "+cvc);
            var transform1 = transform;
            cvc.m_Follow = transform1;
            cvc.m_LookAt = transform1;

            if ((SceneManager.GetActiveScene().name == "Launcher" || SceneManager.GetActiveScene().name == "EndStage"))
            {
                if (mainCam != null)
                {
                    Debug.Log("MainCamDestroyed");
                    Destroy(mainCam.gameObject);
                }

                Debug.Log("PlayerDestroyed");
                PhotonNetwork.Destroy(this.gameObject);
            }
            else
            {
                if (SceneManager.GetActiveScene().name == "WaitingArea")
                {
                    //Debug.Log("In waiting area!");    
                    AudioSource source = Camera.main.GetComponent<AudioSource>();
                    source.loop = true;
                    source.clip = Resources.Load<AudioClip>("Audio/Music/MenuMusic1");
                    source.Play();
                }
                else if (SceneManager.GetActiveScene().name == "Stage1")
                {
                    playerNumber = _gm.setPlayerNumber();
                    AudioSource source = Camera.main.GetComponent<AudioSource>();
                    source.loop = true;
                    source.clip = Resources.Load<AudioClip>("Audio/Music/TrackMusic1");
                    source.Play();
                }
                else
                {
                    EliminateCurrentPlayer();
                }


                transform.gameObject.GetComponent<PlayerPowerups>().SetUp();
                transform.gameObject.GetComponent<CarVFXHandler>().SetUp();
                transform.gameObject.GetComponent<ServerSyncScript>().SetUp();
                _cc.SetUp();
                //SetReadyPlayers(0, _gm.GetStageNum());
                startBlocker = GameObject.Find("StartBlocker");
                _cs = GameObject.Find("CheckpointSystem").GetComponent<CheckpointSystem>();
                _as = GameObject.Find("Alerts").GetComponent<AlertSystem>();
                startDelayText = GameObject.Find("Start Delay").GetComponent<TextMeshProUGUI>();
                completedStage = false;
                _spawnLocation = GameObject.Find("SpawnLocation" + playerNumber).transform;
                GoToSpawn();
                //_photonView.RPC("sendComment", RpcTarget.AllViaServer, "<color=blue>" + _photonView.Owner.NickName + "</color> has loaded.");
                _gm.sendData(("<color=blue>" + _photonView.Owner.NickName + "</color> has loaded.").ToString());
                ready = true;
            }
        }
        else
        {
            playerNumber = _gm.GetPlayerNumber();
            //Debug.Log("ERROR: NO PHOTON VIEW DETECTED! On player " + playerNumber);
        }
    }

    #endregion

    // PUBLIC METHODS

    #region Public Methods

    /// <summary>
    /// Returns the Local Player's Position in Room's Player List
    /// </summary>
    /// <returns>Local Player's Position in Room's Player List</returns>
    public int GetPlayerNumber()
    {
        return playerNumber;
    }

    /// <summary>
    /// Returns if the local player has completed the stage
    /// </summary>
    /// <returns>True if completed in current stage</returns>
    public bool GetCompleted()
    {
        return completedStage;
    }

    /// <summary>
    /// Resets the local player to it's spawn location
    /// </summary>
    /// <param name="pressedButton">Whether or not this was called via button press</param>
    /// <returns></returns>
    public void GoToSpawn(bool pressedButton = false)
    {
        if (pressedButton && _spawnLocation && !_spawnLocation.name.Contains("SpawnLocation") &&
            _cs.GetCheckpointElimination(_spawnLocation.parent.gameObject))
        {
            //_messageText.color = Color.white;
            //StartCoroutine(fadeMessage());
            _as.displayAlert("CheckpointDestroyed");
        }
        else if (pressedButton && _spawnLocation && _spawnLocation.name.Contains("SpawnLocation") && _spawnLocation.parent.GetComponent<Dissolve>().dissolve)
        {
            //_messageText.color = Color.white;
            //StartCoroutine(fadeMessage());
            _as.displayAlert("CheckpointDestroyed");
        }
        else if (_spawnLocation && !_spawnLocation.name.Contains("SpawnLocation") &&
                 _cs.GetCheckpointElimination(_spawnLocation.parent.gameObject))
        {
            _cc.audioManager.PlaySound("CarEliminatedByWall");
            EliminateCurrentPlayer();
        }
        else if (_spawnLocation && _spawnLocation.name.Contains("SpawnLocation") &&
                 _spawnLocation.parent.GetComponent<Dissolve>().dissolve)
        {
            _cc.audioManager.PlaySound("CarEliminatedByWall");
            EliminateCurrentPlayer();
        }
        else
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            Transform spawn = _spawnLocation;
            Transform thisTransform = transform;
            var rotation = spawn.rotation;
            var position = spawn.position;
            thisTransform.rotation = rotation;
            thisTransform.position = position;
            mainCam.transform.rotation = rotation;
            mainCam.transform.position = position + new Vector3(0, 6, -10);
            _cc.audioManager.PlaySound("CarEliminatedOffTrack");
        }
    }

    /// <summary>
    /// Returns the spawn location of the local player
    /// </summary>
    /// <returns>Transform of the local player's spawn</returns>
    public Transform GetSpawnLocation()
    {
        return _spawnLocation;
    }

    /// <summary>
    /// Sets the current player's spawn location
    /// </summary>
    /// <param name="newSpawn">The new spawn location's transform</param>
    /// <returns></returns>
    public void ChangeSpawnLocation(Transform newSpawn)
    {
        _spawnLocation = newSpawn;
    }

    /// <summary>
    /// Alerts the Player Manager that a checkpoint has been passed
    /// </summary>
    /// <returns></returns>
    public void PassCheckpoint()
    {
        if (!debugMode)
            _as.displayAlert("Checkpoint");
        if (_photonView.Owner.IsMasterClient && _gm.GetBots().Length > 0)
        {
            GameObject[] bots = _gm.GetBots();
            int botsToChange = Random.Range(1, 4);
            //int startBot = Random.Range(0, _gm.GetMaxBots() - 4);
            for (int i = 0; i < botsToChange; i++)
            {
                int counter = 0;
                int randBot = Random.Range(0, _gm.GetMaxBots());
                while (bots[randBot] != null && counter < 100)
                {
                    counter++;
                    randBot = Random.Range(0, _gm.GetMaxBots());
                }

                if (bots[randBot] != null)
                {
                    BotCarController bcc = bots[randBot].GetComponent<BotCarController>();
                    bcc.setSpawn(_spawnLocation.parent.GetChild(i + 1));
                    bcc.RandomSpawn();
                }
            }
        }
    }

    /// <summary>
    /// Alerts the Player Manager and Game Manager that the finish point has been passed
    /// </summary>
    /// <returns></returns>
    public void CompleteStage()
    {
        if (!completedStage && !eliminated)
        {
            //Debug.Log("Stage Completed Player: " + GetPlayerNumber());
            completedStage = true;
            GameManager.TryGetFinishedPlayers(out int num, _gm.GetStageNum());
            num = num + 1;
            GameManager.SetFinishedPlayers(num, _gm.GetStageNum());
            if (_gm.GetStageNum() == 2)
            {
                _as.displayAlert("Win");
                elimPosition = num;
                if (elimPosition < 5)
                {
                    //Debug.Log("Finished at:" +elimPosition);
                    //GameManager.SetTop3Players(_photonView.Owner.NickName, elimPosition);
                    GameManager.SetTopPlayers(_photonView.Owner, elimPosition);
                    /*Player t3;
                GameManager.TryGetTopPlayers(out t3, elimPosition);
                Debug.Log(t3.NickName);*/
                }
            }
            else
            {
                _as.displayAlert("Qualified");
            }

            _gm.CompletePlayer();
        }
    }

    /// <summary>
    /// Returns whether the current player has been eliminated
    /// </summary>
    /// <returns>True if player has been eliminated</returns>
    public bool IsEliminated()
    {
        return eliminated;
    }
    
    /// <summary>
    /// Alerts the Player Manager and Game Manager that the local player has been eliminated and deletes the player object
    /// </summary>
    /// <returns></returns>
    public void EliminateCurrentPlayer()
    {
        if (!completedStage && !eliminated)
        {
            _vfx.speedLinesEffect.Stop();
            _vfx.dangerWallEffect.Stop();
            
            _as.displayAlert("Eliminated");
            startDelayText.color = new Color(startDelayText.color.r, startDelayText.color.g, startDelayText.color.b, 0);
            //Debug.Log("Player: "+_photonView.Owner.NickName + " Eliminated.");

            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { "Eliminated", true }
            };
            _photonView.Owner.SetCustomProperties(props);

            eliminated = true;
            GameManager.TryGetElimPlayers(out int num);
            elimPosition = _gm.GetTotalPlayers() - num;

            if (elimPosition < 5)
            {
                //Debug.Log("Finished at:" +elimPosition);
                //GameManager.SetTop3Players(_photonView.Owner.NickName, elimPosition);
                GameManager.SetTopPlayers(_photonView.Owner, elimPosition);
                //string t3;
                //GameManager.TryGetTop3Players(out t3, elimPosition);
                //Debug.Log(t3);
            }

            num = num + 1;
            GameManager.SetElimPlayers(num);
            _gm.EliminatePlayer(elimPosition);
            string messageToBeSent = "<color=blue>" + _photonView.name + "</color> has been <color=red>Eliminated</color>";
            //_photonView.RPC("sendComment", RpcTarget.AllViaServer, messageToBeSent);
            _gm.sendData(messageToBeSent);
            //Debug.Log("Player: "+_photonView.Owner.NickName + " Eliminated with Position "+elimPosition + "/"+_gm.GetTotalPlayers());
            Debug.Log("PlayerDestroyed");
            _vfx.PlayVFXAtPosition("Elimination", transform.position);
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

    #endregion

    #region RPCs

    /// <summary>
    /// Starts the delay timer coroutine
    /// </summary>
    /// <returns></returns>
    [PunRPC]
    void startDelayTimer()
    {
        StartCoroutine(countdownTimer());
    }

    /// <summary>
    /// Changes the timer cubes and the control of the player based on the timer and whether or not all players are ready
    /// </summary>
    /// <returns></returns>
    IEnumerator countdownTimer()
    {
        if (SceneManager.GetActiveScene().name != "WaitingArea" && _photonView.IsMine)
        {
            yield return new WaitForSeconds(1);
            if (GameObject.FindGameObjectWithTag("FlyBy"))
            {
                CameraFlyBy cfb = GameObject.FindGameObjectWithTag("FlyBy").GetComponent<CameraFlyBy>();
                //Debug.Log("CountDown Started");
                //yield return new WaitUntil(() => cfb.activateFlyBy);
                yield return new WaitUntil(() => !cfb.activateFlyBy);
            }
            SetReadyPlayer(true, _gm.GetStageNum());
            bool allPlayersReady = true;
            int counter = 1;
            bool playerReady = false;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                playerReady = false;
                TryGetReadyPlayer(out playerReady, _gm.GetStageNum(), player);
                Debug.Log("Player: " + player + " Ready: " + playerReady);
                if (playerReady)
                {
                    counter++;
                }
                else
                {
                    allPlayersReady = false;
                }
            }

            //_photonView.RPC("sendComment", RpcTarget.AllViaServer, "<color=blue>" + _photonView.name + "</color> is ready. " + counter + "/" + PhotonNetwork.CurrentRoom.PlayerCount);
            _gm.sendData(("<color=blue>" + _photonView.name + "</color> is ready. " + counter + "/" + PhotonNetwork.CurrentRoom.PlayerCount).ToString());
            counter = 0;
            //&& counter < 100000
            /*while (!allPlayersReady && counter < 100)
            {
                allPlayersReady = true;
                //Debug.Log("Running While Loop");
                string str = "Focus: \n";
                int numOfReadyPlayers = 0;
                GameManager.TryGetElimPlayers(out int elimPlayers);
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    playerReady = false;
                    TryGetReadyPlayer(out playerReady, _gm.GetStageNum(), player);
                    str += "Player: " + player + " Ready: " + playerReady + "\n";
                    //Debug.Log("Player: "+player + " Ready: "+playerReady+"\n");
                    if (playerReady)
                    {
                        numOfReadyPlayers++;
                    }
                    else
                    {
                        allPlayersReady = false;
                    }
                }

                if (numOfReadyPlayers + elimPlayers >= _gm.GetTotalPlayers())
                {
                    allPlayersReady = true;
                }
                else
                {
                    allPlayersReady = false;
                }

                Debug.Log("N: "+numOfReadyPlayers +" E: "+ elimPlayers +" T: "+ _gm.GetTotalPlayers());
                //Debug.Log(str);
                counter++;
                yield return new WaitForSeconds(0.1f);
            }

            if (counter >= 100)
            {
                //Debug.LogError("Counter Broke Chain");
            }
            
            if (_photonView.Owner.IsMasterClient)
            {
                int timeSet = PhotonNetwork.ServerTimestamp;
                ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
                hash.Add(("Timer"+_gm.GetStageNum()), timeSet);
                PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
                //PhotonNetwork.CurrentRoom.CustomProperties[("Timer" + _gm.GetStageNum())] = timeSet;
            }*/
            //yield return new WaitUntil(() => allPlayersReady);
            startDelayText.color = Color.clear; // Changed to clear as rubics are in
            Debug.Log("A: "+PhotonNetwork.CurrentRoom);
            Debug.Log("B: "+"Timer"+_gm.GetStageNum());
            Debug.Log("C: "+PhotonNetwork.CurrentRoom.CustomProperties[("Timer"+_gm.GetStageNum())]);
            //int notZero = (int)PhotonNetwork.CurrentRoom.CustomProperties[("Timer" + _gm.GetStageNum())];;
            counter = 0;
            int timerVal = 0;
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Timer" + _gm.GetStageNum()))
            {
                timerVal = (int)PhotonNetwork.CurrentRoom.CustomProperties[("Timer" + _gm.GetStageNum())];
            }
            while (timerVal == 0 && counter < 600)
            {
                counter++;
                yield return new WaitForFixedUpdate();
            }
            Debug.Log("Count: "+counter);
            //yield return new WaitUntil(() => ((int)PhotonNetwork.CurrentRoom.CustomProperties[("Timer"+_gm.GetStageNum())] != 0));
            int hit = 0;
            while (hit == 0)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(("Timer" + _gm.GetStageNum())))
                {
                    hit = (int)PhotonNetwork.CurrentRoom.CustomProperties[("Timer" + _gm.GetStageNum())];
                }

                yield return new WaitForFixedUpdate();
            }

            Debug.Log("Timer: "+hit);
            //object hitFromProps;
            //PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(("Timer" + _gm.GetStageNum()), out hitFromProps);
            //hit = (int)hitFromProps;
            float timeLeft = _gm.GetStartDelay() - ((PhotonNetwork.ServerTimestamp - hit) / 1000f);
            while (timeLeft > 0)
            {
                startDelayText.text = timeLeft.ToString("F2");
                hit = (int)PhotonNetwork.CurrentRoom.CustomProperties[("Timer"+_gm.GetStageNum())];
                timeLeft = _gm.GetStartDelay() - ((PhotonNetwork.ServerTimestamp - hit) / 1000f);
                timer = timeLeft;
                yield return new WaitForFixedUpdate();
            }
            Debug.Log("TIME LEFT IS: " + _gm.GetStartDelay() + ": Server:" +PhotonNetwork.ServerTimestamp + ": TimeSet:" + hit + " Total Timer Value: "+(_gm.GetStartDelay() - ((PhotonNetwork.ServerTimestamp - hit) / 1000f)));

            //Start Code Here
            startDelayText.text = "Go!";
            //Debug.Log("StartedRACE!");
            while (startDelayText.color.a > 0)
            {
                startDelayText.color = new Color(startDelayText.color.r, startDelayText.color.g, startDelayText.color.b,
                    startDelayText.color.a - 0.01f);
                yield return new WaitForSeconds(0.01f);
            }
        }

    }

    #endregion

    #region TimedScripts

    /// <summary>
    /// Causes any alerts to fade
    /// </summary>
    /// <returns></returns>
    IEnumerator fadeMessage()
    {
        while (_messageText.color.a < 0)
        {
            _messageText.color = new Color(1, 1, 1, _messageText.color.a - 0.02f);
            yield return new WaitForFixedUpdate();
        }

        _messageText.color = Color.clear;
    }

    /*IEnumerator TestScript()
    {
        for (int i = 1; i < 10; i++)
        {
            _photonView.RPC("sendComment", RpcTarget.AllViaServer, "MessageBox Setup: " + i);
            yield return new WaitForSeconds(1f);
        }
    }*/

    #endregion
}
