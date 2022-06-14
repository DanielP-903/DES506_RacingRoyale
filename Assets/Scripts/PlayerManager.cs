using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    // COMPONENTS, INTS AND BOOLS
    #region Private Variables
    private PhotonView _photonView;
    private CarController _dcc;
    private Rigidbody _rb;
    private PlayerManager target;
    private GameManager _gm;
    private Transform _spawnLocation;
    private GameObject mainCam;

    private int playerNumber = 0;
    private bool completedStage = false;
    private bool eliminated = false;
    private int elimPosition = 0;
    #endregion

    // PLAYER NAME UI
    #region Serializable Variables
    [SerializeField]
    private TextMeshProUGUI playerNameText;
    [SerializeField]
    private TextMeshProUGUI playerLicenseText;
    #endregion

    // PRIVATE METHODS: START, LOAD, UPDATE
    #region Private Methods
    void Start()
    {
        //SceneManager.sceneLoaded += LoadPMInLevel;
        DontDestroyOnLoad(this.gameObject);
        _photonView = GetComponent<PhotonView>();
        _dcc = GetComponent<CarController>();
        _rb = GetComponent<Rigidbody>();
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (_photonView != null)
        {
            if (_photonView.IsMine)
            {
                mainCam = Camera.main.gameObject;
                CinemachineVirtualCamera cvc = mainCam.GetComponent<CinemachineVirtualCamera>();
                DontDestroyOnLoad(mainCam);
                var transform1 = transform;
                cvc.m_Follow = transform1;
                cvc.m_LookAt = transform1;
            }
            else
            {
                Destroy(this);
                Destroy(_dcc);
                Destroy(GetComponent<Rigidbody>());
            }
        }

        int spawnNumber = playerNumber;
        if (playerNumber == 0)
        {
            spawnNumber = _gm.GetPlayerNumber();
        }
        _spawnLocation = GameObject.Find("SpawnLocation" + spawnNumber).transform;
        
        playerNameText.text = _photonView.Owner.NickName;
        playerLicenseText.text = _photonView.Owner.NickName;
    }
    
    void Update()
    {
        if (transform.position.y < -5)
        {
            //Debug.Log("Less than 5");
            GoToSpawn();
        }
    }

    void OnLevelWasLoaded()
    {
        Debug.Log("PlayerManger Loading Level");
        if (_photonView != null)
        {
            if ((SceneManager.GetActiveScene().name == "Launcher" || SceneManager.GetActiveScene().name == "EndStage"))
            {
                Destroy(mainCam.gameObject);
                PhotonNetwork.Destroy(this.gameObject);
            }
            else
            {
                
                if (SceneManager.GetActiveScene().name == "Stage1")
                {
                    playerNumber = _gm.setPlayerNumber();
                }
                else
                {
                    EliminateCurrentPlayer();
                }
                completedStage = false;
                _spawnLocation = GameObject.Find("SpawnLocation" + playerNumber).transform;
                GoToSpawn();
                Debug.Log(_spawnLocation + "- Player: " + playerNumber + " Name: " +_photonView.Owner.NickName);

            }
        }
    }
    #endregion

    // PUBLIC METHODS: SETNAME, GETPLAYERNUM, GOTOSPAWN, CHANGESPAWN, COMPLETESTAGE, ELIMPLAYER
    #region Public Methods
    public int GetPlayerNumber()
    {
        return playerNumber;
    }

    public void GoToSpawn()
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
        mainCam.transform.position = position+new Vector3(0,6,-10);
        
    }

    public void ChangeSpawnLocation(Transform newSpawn)
    {
        _spawnLocation = newSpawn;
    }

    public void CompleteStage()
    {
        if (!completedStage)
        {
            Debug.Log("Stage Completed Player: " + GetPlayerNumber());
            completedStage = true;
            GameManager.TryGetFinishedPlayers(out int num, _gm.GetStageNum());
            num = num + 1;
            GameManager.SetFinishedPlayers(num,_gm.GetStageNum());
            if (_gm.GetStageNum() == 4)
            {
                elimPosition = num;
                if (elimPosition < 5)
                {
                    Debug.Log("Finished at:" +elimPosition);
                    GameManager.SetTop3Players(_photonView.Owner.NickName, elimPosition);
                    string t3;
                    GameManager.TryGetTop3Players(out t3, elimPosition);
                    Debug.Log(t3);
                }
            }
        }
    }
    
    public void EliminateCurrentPlayer()
    {
        if (!completedStage && !eliminated)
        {
            Debug.Log("Player: "+_photonView.Owner.NickName + " Eliminated.");
         
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
                Debug.Log("Finished at:" +elimPosition);
                GameManager.SetTop3Players(_photonView.Owner.NickName, elimPosition);
                string t3;
                GameManager.TryGetTop3Players(out t3, elimPosition);
                Debug.Log(t3);
            }
            
            num = num + 1;
            GameManager.SetElimPlayers(num);
            _gm.EliminatePlayer(elimPosition);
            Debug.Log("Player: "+_photonView.Owner.NickName + " Eliminated with Position "+elimPosition + "/"+_gm.GetTotalPlayers());
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
    #endregion
}
