using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerManager : MonoBehaviour
{

    // COMPONENTS, INTS AND BOOLS
    #region Private Variables
    private PhotonView _photonView;
    private CarController _cc;
    private MeshRenderer _mRend;
    private MeshFilter _mFilt;
    private Rigidbody _rb;
    private PlayerManager target;
    private GameManager _gm;
    private Transform _spawnLocation;
    private GameObject mainCam;
    private GameObject startBlocker;
    private CheckpointSystem _cs;
    private TextMeshProUGUI _messageText;
    public TextMeshProUGUI startDelayText;

    private int playerNumber = 0;
    private bool completedStage = false;
    private bool eliminated = false;
    private int elimPosition = 0;
    private bool ready = false;
    #endregion

    // PLAYER NAME UI
    #region Serializable Variables
    [SerializeField]
    private TextMeshProUGUI playerNameText;
    [SerializeField]
    private TextMeshProUGUI playerLicenseText;

    [SerializeField] private bool debugMode;
    
    #endregion

    // PRIVATE METHODS: START, LOAD, UPDATE
    #region Private Methods
    public static bool TryGetReadyPlayers(out int readyPlayers, int stageNum)
    {
        readyPlayers = 0;

        object readyPlayersFromProps;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("ReadyPlayers"+stageNum, out readyPlayersFromProps))
        {
            readyPlayers = (int)readyPlayersFromProps;
            return true;
        }
        
        
        return false;
    }
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
    void Start()
    {
        if (debugMode)
        {
            Debug.Log("DEBUG MODE IS ACTIVE! (PlayerManager)");
        }

        if (!debugMode)
        {
            SetReadyPlayers(0, 1);
        }

        //SceneManager.sceneLoaded += LoadPMInLevel;
        _photonView = GetComponent<PhotonView>();
        this.gameObject.name = _photonView.Owner.NickName;
        _mRend = transform.Find("CarMesh").GetComponent<MeshRenderer>();
        _mFilt = transform.Find("CarMesh").GetComponent<MeshFilter>();
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

        if (!debugMode)
        {
            int skinNum = 0;
            if ((int)_photonView.Owner.CustomProperties["Skin"] != null)
            {
                skinNum = (int)_photonView.Owner.CustomProperties["Skin"];
            }

            Debug.Log("FoundSkinNum: " + (int)_photonView.Owner.CustomProperties["Skin"]);
            /*object skinNumFromProps;
            if (_photonView.Owner.CustomProperties.TryGetValue("Skin", out skinNumFromProps))
            {
                skinNum = (int)skinNumFromProps;
                Debug.Log("FoundSkinNum");
            }*/
            _mRend.material = GameObject.Find("DataManager").GetComponent<DataManager>().GetMats()[skinNum];
            _mFilt.mesh = GameObject.Find("DataManager").GetComponent<DataManager>().GetMesh()[skinNum];
        }

        if (_photonView.Owner == null)
        {
            playerNameText.text = "Guest";
            playerLicenseText.text = "Guest";
        }
        else
        {
            playerNameText.text = _photonView.Owner.NickName;
            playerLicenseText.text = _photonView.Owner.NickName;
        }
        if (_photonView != null)
        {
            DontDestroyOnLoad(this.gameObject);
            if (_photonView.IsMine)
            {
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
            playerNumber = _gm.GetPlayerNumber();
            Debug.Log("Photon view NOT DETECTED during start function of PlayerManager" + playerNumber);
        }
        
        //playerNumber = _gm.GetPlayerNumber();
    }
    
    void Update()
    {
       if (transform.position.y < -5 && _photonView.IsMine)
        {
            //Debug.Log("Less than 5");
            GoToSpawn();
        }

       if (ready)
       {
           Debug.Log("Ready!");
           int readyPlayers;
           TryGetReadyPlayers(out readyPlayers, _gm.GetStageNum());
           Debug.Log( (readyPlayers +":"+ _gm.GetTotalPlayers()));
           // && readyPlayers >= _gm.GetTotalPlayers()
           if (_gm.GetStageNum() > 0 && _gm.GetStageNum() < 5 && _gm.halt == false)
           {
               //_photonView.RPC("startDelayTimer", RpcTarget.All);
               startDelayTimer();
           }

           ready = false;
       }
    }

    void OnLevelWasLoaded()
    {
        SetUp();
    }

    public void SetUp()
    {
        //Debug.Log("PlayerManger Loading Level");
        if (_photonView != null)
        {
            _messageText = GameObject.Find("Message").GetComponent<TextMeshProUGUI>();
            _messageText.color = Color.clear;
            SetReadyPlayers(0, _gm.GetStageNum());
            startBlocker = GameObject.Find("StartBlocker");
            _cs = GameObject.Find("CheckpointSystem").GetComponent<CheckpointSystem>();
            startDelayText = GameObject.Find("Start Delay").GetComponent<TextMeshProUGUI>();
            int readyPlayers;
            TryGetReadyPlayers(out readyPlayers, _gm.GetStageNum());
            readyPlayers += 1;
            SetReadyPlayers(readyPlayers, _gm.GetStageNum());
            ready = true;
            
            transform.gameObject.GetComponent<PlayerPowerups>().SetUp();
            transform.gameObject.GetComponent<CarVFXHandler>().SetUp();
            _cc.SetUp();
            
            CinemachineVirtualCamera cvc = mainCam.GetComponent<CinemachineVirtualCamera>();
            Debug.Log("LoadingPlayer: "+cvc);
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
                Debug.Log("PlayerManagerDestroyed");
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
                
                if (SceneManager.GetActiveScene().name == "Stage1")
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
                completedStage = false;
                _spawnLocation = GameObject.Find("SpawnLocation" + playerNumber).transform;
                GoToSpawn();
                //Debug.Log(_spawnLocation + "- Player: " + playerNumber + " Name: " +_photonView.Owner.NickName);

            }
        }
        else
        {
            playerNumber = _gm.GetPlayerNumber();
            //Debug.Log("ERROR: NO PHOTON VIEW DETECTED! On player " + playerNumber);
        }
    }
    #endregion

    // PUBLIC METHODS: SETNAME, GETPLAYERNUM, GOTOSPAWN, CHANGESPAWN, COMPLETESTAGE, ELIMPLAYER
    #region Public Methods
    public int GetPlayerNumber()
    {
        return playerNumber;
    }

    public bool GetCompleted()
    {
        return completedStage;
    }

    public void GoToSpawn(bool pressedButton = false)
    {
        if (pressedButton && !_spawnLocation.name.Contains("SpawnLocation") && _cs.GetCheckpointElimination(_spawnLocation.parent.gameObject))
        {
            _messageText.color = Color.white;
            StartCoroutine(fadeMessage());
        }
        else if (!_spawnLocation.name.Contains("SpawnLocation") && _cs.GetCheckpointElimination(_spawnLocation.parent.gameObject))
        {
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
        }
    }

    public Transform GetSpawnLocation()
    {
        return _spawnLocation;
    }
    
    public void ChangeSpawnLocation(Transform newSpawn)
    {
        _spawnLocation = newSpawn;
    }

    public void CompleteStage()
    {
        if (!completedStage)
        {
            //Debug.Log("Stage Completed Player: " + GetPlayerNumber());
            completedStage = true;
            GameManager.TryGetFinishedPlayers(out int num, _gm.GetStageNum());
            num = num + 1;
            GameManager.SetFinishedPlayers(num,_gm.GetStageNum());
            if (_gm.GetStageNum() == 4)
            {
                elimPosition = num;
                if (elimPosition < 5)
                {
                    //Debug.Log("Finished at:" +elimPosition);
                    //GameManager.SetTop3Players(_photonView.Owner.NickName, elimPosition);
                    GameManager.SetTopPlayers(_photonView.Owner, elimPosition);
                    //string t3;
                    //GameManager.TryGetTop3Players(out t3, elimPosition);
                    //Debug.Log(t3);
                }
            }
            _gm.CompletePlayer();
        }
    }
    
    public void EliminateCurrentPlayer()
    {
        if (!completedStage && !eliminated)
        {
            startDelayText.color = new Color(startDelayText.color.r, startDelayText.color.g, startDelayText.color.b, 0);
            //Debug.Log("Player: "+_photonView.Owner.NickName + " Eliminated.");
         
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                {"Eliminated", true}
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
            //Debug.Log("Player: "+_photonView.Owner.NickName + " Eliminated with Position "+elimPosition + "/"+_gm.GetTotalPlayers());
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
    #endregion
    
    #region RPCs

    [PunRPC]
    void startDelayTimer()
    {
        StartCoroutine(countdownTimer());
    }

    IEnumerator countdownTimer()
    {
        startDelayText.color = Color.white;
        float timeLeft = _gm.GetStartDelay();
        while (timeLeft > 0)
        {
            startDelayText.text = timeLeft.ToString("F2");
            timeLeft -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
        //Start Code Here
        startBlocker.SetActive(false);
        startDelayText.text = "Go!";
        Debug.Log("StartedRACE!");
        while (startDelayText.color.a > 0)
        {
            startDelayText.color = new Color(startDelayText.color.r, startDelayText.color.g, startDelayText.color.b, startDelayText.color.a - 0.01f);
            yield return new WaitForSeconds(0.01f);
        }
        
    }
    #endregion

    #region TimedScripts

    IEnumerator fadeMessage()
    {
        while (_messageText.color.a < 0)
        {
            _messageText.color = new Color(1,1,1,_messageText.color.a - 0.02f);
            yield return new WaitForFixedUpdate();
        }

        _messageText.color = Color.clear;
    }

    #endregion
}
