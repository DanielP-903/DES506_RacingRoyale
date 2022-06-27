using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Audio;

public class Launcher : MonoBehaviourPunCallbacks
    {
        // --- VARS ---
        // CHANGEABLE LAUNCHER VARIABLES (MAX PLAYERS HERE)
        #region Private Serializable Fields

        /// <summary>
        /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
        /// </summary>
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        [SerializeField]
        private byte maxPlayersPerRoom = 32;

        #endregion
        
        // PUBLIC LAUNCHER VARIABLES (LAUNCHER UI HERE)
        #region Public Fields

        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        [SerializeField]
        private GameObject controlPanel;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        [SerializeField]
        private GameObject progressLabel;
        [Tooltip("The Ui Panel for Options")]
        [SerializeField]
        private GameObject optionsPanel;
        [Tooltip("The Ui Panel for Credits")]
        [SerializeField]
        private GameObject creditsPanel;
        [Tooltip("The Ui 3d Car Display")]
        [SerializeField]
        private GameObject selectorCar;
        [Tooltip("Audio Mixer for the Game")]
        [SerializeField]
        private AudioMixer mixer;

        #endregion

        // PRIVATE LAUNCHER VARIABLES (VERSION CONTROL HERE)
        #region Private Fields

        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;

        /// <summary>
        /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
        /// </summary>
        string gameVersion = "0.11.2";


        #endregion
        
        
        
        // --- METHODS ---
        // THIS SECTION IS FOR CALLS TO DO WITH CONNECTING AND DISCONNECTING
        #region MonoBehaviourPunCallbacks Callbacks
        
        // IF CONNECTED TO MASTER SERVER : JOIN RANDOM SERVER
        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            if (isConnecting)
            {
                // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
                PhotonNetwork.JoinRandomRoom();
                isConnecting = false;
            }
        }

        // IF DISCONNECTED FROM SERVER : RESET
        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            isConnecting = false;
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        }
        
        // IF FAILED TO JOIN RANDOM SERVER : MAKE NEW SERVER
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            //Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
        }
        
        // IF JOINED SERVER AND MASTER : LOAD WAITING AREA
        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            // #Critical: We only load if we are the master player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("We load the 'WaitingArea' ");
                
                // #Critical
                // Load the Room Level.
                PhotonNetwork.LoadLevel("WaitingArea");
            }
        }
        
        #endregion

        // AWAKE AND START METHODS (SET SYNC SCENE HERE AND SET UI AT START)
        #region MonoBehaviour CallBacks
        
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
        }


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            progressLabel.SetActive(false);
            optionsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            controlPanel.SetActive(true);

            Destroy(GameObject.Find("PlayerCamera"));
            Destroy(GameObject.Find("GameManager"));
            
            AudioSource source = Camera.main.GetComponent<AudioSource>();
            source.loop = true;
            source.clip = Resources.Load<AudioClip>("Audio/Music/MenuMusic1");
            source.Play();

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
        }
        
        
        #endregion

        // CONNECT AND MENU METHODS CALLED BY BUTTONS
        #region Public Methods
        
        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {
            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.IsConnected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }
        
        // QUIT APPLICATION
        public void QuitGame()
        {
            Application.Quit();
        }
        
        public void GoToOptions()
        {
            progressLabel.SetActive(false);
            optionsPanel.SetActive(true);
            creditsPanel.SetActive(false);
            controlPanel.SetActive(false);
            selectorCar.SetActive(false);
        }

        public void GoToCredits()
        {
            progressLabel.SetActive(false);
            optionsPanel.SetActive(false);
            creditsPanel.SetActive(true);
            controlPanel.SetActive(false);
            selectorCar.SetActive(false);
        }

        public void GoBackToMenu()
        {
            progressLabel.SetActive(false);
            optionsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            controlPanel.SetActive(true);
            selectorCar.SetActive(true);
        }

        public void ChangeMaster(float newVol)
        {
            PlayerPrefs.SetFloat("MasterVol", newVol);
            mixer.SetFloat("Master", newVol);
        }
        
        public void ChangeMusic(float newVol)
        {
            PlayerPrefs.SetFloat("MusicVol", newVol);
            mixer.SetFloat("Music", newVol);
        }
        
        public void ChangeSound(float newVol)
        {
            PlayerPrefs.SetFloat("SoundVol", newVol);
            mixer.SetFloat("Sound", newVol);
        }

        #endregion
    
}
