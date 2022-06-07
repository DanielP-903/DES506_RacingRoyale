using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    //private ArrayList _playerList;
    //private Dictionary<int, string> players;

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
        //_playerList.Add(other);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


            //LoadArena();
        }
    }
    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects
        //_playerList.Remove(other);

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
    
    #endregion
    
    #region Private Methods


    void LoadArena(string arenaName)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        photonView.RPC("SetNumber", RpcTarget.All);
        PhotonNetwork.LoadLevel(arenaName);
    }

    private void Start()
    {
        timer = (TextMeshProUGUI)GameObject.Find("Timer");
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
            PhotonNetwork.Instantiate(this.playerPrefab.name, GameObject.Find("SpawnLocation" + GetPlayerNumber()).transform.position, GameObject.Find("SpawnLocation" + GetPlayerNumber()).transform.rotation, 0);
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            CountdownTimer.SetStartTime();
        }
    }

    private void Update()
    {
        CountdownTimer.TryGetStartTime(out var hit);
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.Time - hit > waitingTime )
        {
            LoadArena("Stage1");
        }

        int Sec = waitingTime;
        int milSec;
        timer.text = "";
    }

    #endregion
    
    #region Serializable Private Fields

    [Tooltip("Time spent waiting in lobby before game starts")] [SerializeField]
    private float waitingTime;
    
    #endregion
    #region Private Fields

    [Tooltip("The prefab to use for representing the timer")]
    private TextMeshProUGUI timer;

    #endregion
    
    #region Public Fields
    
    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    
    #endregion
}