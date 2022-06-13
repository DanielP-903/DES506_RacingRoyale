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
    private PhotonView _photonView;
    private CarController _dcc;
    private Rigidbody _rb;
    private PlayerManager target;
    private GameManager _gm;
    private Transform _spawnLocation;
    private GameObject mainCam;
    
    static private GameObject instance;

    [SerializeField]
    private TextMeshProUGUI playerNameText;
    [SerializeField]
    private TextMeshProUGUI playerLicenseText;
    
    
    private int playerNumber = 0;
    private bool completedStage = false;
    private bool eliminated = false;
    private int elimPosition = 0;

    // Start is called before the first frame update
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
        
        /*if (instance == null)
        {
            instance = this.gameObject;
        }
        else
        {
            Destroy(this.gameObject);
        }*/
    }

    void OnLevelWasLoaded()
    {
        Debug.Log("PlayerManger Loading Level");
        if (_photonView != null)
        {
            if ((SceneManager.GetActiveScene().name == "Launcher" || SceneManager.GetActiveScene().name == "EndStage"))
            {
                PhotonNetwork.Destroy(this.gameObject);
                Destroy(mainCam);
            }
            else
            {
                completedStage = false;
                if (SceneManager.GetActiveScene().name == "Stage1")
                {
                    playerNumber = _gm.setPlayerNumber();
                }

                _spawnLocation = GameObject.Find("SpawnLocation" + playerNumber).transform;
                GoToSpawn();
                Debug.Log(_spawnLocation + "- Player: " + playerNumber + " Name: " +_photonView.Owner.NickName);

            }
        }
    }

/*    private void LoadPMInLevel(Scene scene, LoadSceneMode loadSceneMode)
    {
        Debug.Log("PlayerManger Loading Level");
        if (scene.name == "Launcher" || scene.name == "EndStage")
        {
            PhotonNetwork.Destroy(this.gameObject);
            Destroy(Camera.main.gameObject);
        }
        else
        {
            if (scene.name == "Stage1")
            {
                if (playerNumber == 0)
                {
                    playerNumber = _gm.GetPlayerNumber();
                }
            }
            _spawnLocation = GameObject.Find("SpawnLocation" + playerNumber).transform;
            Debug.Log(_spawnLocation +"- Player: "+playerNumber );
            if (_photonView.IsMine)
            {
                CinemachineVirtualCamera cvc = Camera.main.gameObject.GetComponent<CinemachineVirtualCamera>();
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
    }*/

    public void SetTarget(PlayerManager _target)
    {
        if (_target == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
            return;
        }
        // Cache references for efficiency
        target = _target;
        if (playerNameText != null)
        {
            playerNameText.text = target._photonView.Owner.NickName;
        }
    }

    public void SetName(string inputName)
    {
        playerNameText.text = inputName;
        playerLicenseText.text = inputName;
    }

    public int GetPlayerNumber()
    {
        return playerNumber;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -5)
        {
            //Debug.Log("Less than 5");
            GoToSpawn();
        }
    }

    public void GoToSpawn()
    {
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        Transform spawn = _spawnLocation;
        Transform thisTransform = transform;
        thisTransform.rotation = spawn.rotation;
        thisTransform.position = spawn.position;
        mainCam.transform.position = spawn.position+new Vector3(0,6,-10);
        mainCam.transform.rotation = spawn.rotation;
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
                if (elimPosition < 4)
                {
                    Debug.Log("Finished at:" +elimPosition);
                    GameManager.SetTop3Players(_photonView.Owner.NickName, elimPosition);
                    string t3;
                    GameManager.TryGetTop3Players(out t3, elimPosition);
                    Debug.Log(t3);
                    /*
                    Debug.Log("Finished at:" +elimPosition);
                    GameManager.SetTop3Players(new Top3PlayerData(_photonView.Owner.NickName), elimPosition);
                    Top3PlayerData t3;
                    GameManager.TryGetTop3Players(out t3, 1);
                    Debug.Log(t3.name);
                     */
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
            num = num + 1;
            GameManager.SetElimPlayers(num);
            _gm.EliminatePlayer(elimPosition);
            Debug.Log("Player: "+_photonView.Owner.NickName + " Eliminated with Position "+elimPosition + "/"+_gm.GetTotalPlayers());
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
    
    
    [PunRPC]
    void ResetCompleted()
    {
        Debug.Log("CompletedStage: "+completedStage);
        if (completedStage)
        {
            //completedStage = false;
        }
        else
        {
            EliminateCurrentPlayer();
            Debug.Log("Player: "+playerNumber+" Eliminated. StageCompleted: "+completedStage);
            //_gm.LeaveRoom();
        }
    }
}
