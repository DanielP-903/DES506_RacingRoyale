using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    //private ArrayList _playerList;
    //private Dictionary<int, string> players;
    
    #region Serializable Private Fields

    [Tooltip("Time spent waiting in lobby before game starts")] [SerializeField]
    private float waitingTime;
    
    #endregion
    
    #region Private Fields

    [Tooltip("The prefab to use for representing the timer")]
    private TextMeshProUGUI timer;
    private PhotonView _photonView;
    private int _stage = 1;
    private int _totalPlayers = 0;

    #endregion
    
    #region Public Fields
    
    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    
    #endregion

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
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            //LoadArena();
        }
    }
    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            //LoadArena();
        }
    }
    
    #endregion
    
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
                return counter;
            }

            counter++;
        }

        return counter;
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

        Debug.Log("Set Custom Props for Finished Players: "+ props.ToStringFull() + " wasSet: "+wasSet+" NewValue: "+finishedPlayers);
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


        Debug.Log("Set Custom Props for Eliminated Players: "+ props.ToStringFull() + " wasSet: "+wasSet);
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
    
    #region Private Methods


    void LoadArena(string arenaName)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel(arenaName);
    }
    private void Start()
    {
        
        
        SceneManager.sceneLoaded += LoadPlayerInLevel;
        DontDestroyOnLoad(this.gameObject);
        timer = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",this);
        }
        else
        {
            Debug.LogFormat("We are Instantiating LocalPlayer from {0}", Application.loadedLevelName);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            //PhotonNetwork.Instantiate(this.playerPrefab.name, GameObject.Find("SpawnLocation"+PhotonNetwork.CurrentRoom.PlayerCount).transform.position, Quaternion.identity, 0);
            Debug.Log("Player Number: "+PhotonNetwork.LocalPlayer.GetPlayerNumber()); //GetPlayerNumber()
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
        GameObject player = PhotonNetwork.Instantiate(this.playerPrefab.name, GameObject.Find("SpawnLocation" + GetPlayerNumber()).transform.position, GameObject.Find("SpawnLocation" + GetPlayerNumber()).transform.rotation, 0);
        _photonView = player.GetComponent<PhotonView>();
    }

    private void Update()
    {
        int playersCompleted;
        switch (SceneManager.GetActiveScene().name)
        {
            case "WaitingArea":
                CountdownTimer.TryGetStartTime(out var hit);
                if (PhotonNetwork.CurrentRoom.IsOpen && PhotonNetwork.IsMasterClient &&
                    (PhotonNetwork.ServerTimestamp - hit) / 1000f > waitingTime)
                {
                    _totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
                    Debug.Log("TotalPlayers: "+_totalPlayers);
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    _photonView.RPC("SetNumber", RpcTarget.All);
                    LoadArena("Stage1");
                }
                else if (!PhotonNetwork.CurrentRoom.IsOpen)
                {
                    timer.gameObject.SetActive(false);
                }

                float tempTime = (float)waitingTime - (float)((PhotonNetwork.ServerTimestamp - hit) / 1000f);
                int sec = Mathf.FloorToInt(tempTime);
                int milSec = Mathf.FloorToInt((tempTime - sec) * 100f);
                timer.text = sec + ":" + milSec;
                break;
            case "Stage1":
                TryGetFinishedPlayers(out playersCompleted, _stage);
                Debug.Log("Name: "+SceneManager.GetActiveScene().name + " Stage: " + _stage + " Players Finished: "+playersCompleted+" Goal: " + (PhotonNetwork.CurrentRoom.MaxPlayers/2));
                
                if (_stage == 1 && playersCompleted >= PhotonNetwork.CurrentRoom.MaxPlayers/2)
                {
                    Debug.Log("Running Level Change");
                    _stage++;
                    SetFinishedPlayers(0, _stage);
                    LoadArena("Stage2");
                    _photonView.RPC("ResetCompleted", RpcTarget.All);
                }

                break;
            case "Stage2":
                TryGetFinishedPlayers(out playersCompleted, _stage);
                Debug.Log("Name: "+SceneManager.GetActiveScene().name + " Stage: " + _stage + " Players Finished: "+playersCompleted+" Goal: " + (PhotonNetwork.CurrentRoom.MaxPlayers/4));
                
                if (_stage == 2 && playersCompleted >= PhotonNetwork.CurrentRoom.MaxPlayers/4)
                {
                    _stage++;
                    SetFinishedPlayers(0, _stage);
                    LoadArena("Stage3");
                    _photonView.RPC("ResetCompleted", RpcTarget.All);
                }
                
                break;
            case "Stage3":
                TryGetFinishedPlayers(out playersCompleted, _stage);
                Debug.Log("Name: "+SceneManager.GetActiveScene().name + " Stage: " + _stage + " Players Finished: "+playersCompleted+" Goal: " + (PhotonNetwork.CurrentRoom.MaxPlayers/8));
                
                if (_stage == 3 && playersCompleted >= PhotonNetwork.CurrentRoom.MaxPlayers/8)
                {
                    _stage++;
                    SetFinishedPlayers(0, _stage);
                    LoadArena("Stage4");
                    _photonView.RPC("ResetCompleted", RpcTarget.All);
                }
                
                break;
            case "Stage4":
                int elimPlayers;
                TryGetElimPlayers(out elimPlayers);
                Debug.Log("Name: "+SceneManager.GetActiveScene().name + " Stage: " + _stage + " Players Finished: "+(_totalPlayers - elimPlayers)+" Goal: 1");
                
                if (_totalPlayers - elimPlayers == 1)
                {
                    LoadArena("EndStage");
                }
                
                break;
            case "EndStage":
                break;
        }
    }
    
    

    /*Debug.Log("Name: "+SceneManager.GetActiveScene().name);
                string str = SceneManager.GetActiveScene().name;
                int stageNumber = Convert.ToInt32(str[str.Length - 1].ToString());
                int playersCompleted;
                TryGetFinishedPlayers(out playersCompleted);
                Debug.Log("CompletionGoal: "+PhotonNetwork.CurrentRoom.MaxPlayers/Mathf.Pow(2,stageNumber));
                Debug.Log("PCompleted: " + playersCompleted);
                if (_stage == stageNumber && playersCompleted >= PhotonNetwork.CurrentRoom.MaxPlayers/Mathf.Pow(2,stageNumber))
                {
                    _stage++;
                    SetFinishedPlayers(0);
                    LoadArena("Stage"+(stageNumber+1));
                    _photonView.RPC("ResetCompleted", RpcTarget.All);
                }*/
    
    #endregion
    
}