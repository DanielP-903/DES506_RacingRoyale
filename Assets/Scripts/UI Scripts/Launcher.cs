using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        [Tooltip("The UI panel to inform the user that the connection is in progress")]
        [SerializeField]
        private GameObject progressPanel;
    
        [Tooltip("The Ui Panel for Options")]
        [SerializeField]
        private GameObject optionsPanel;
        [Tooltip("The Ui Panel for Controls")]
        [SerializeField]
        private GameObject controlsPanel;
        [Tooltip("The Ui Panel for Credits")]
        [SerializeField]
        private GameObject creditsPanel;       
        [Tooltip("The Ui Panel for Connection Popup")]
        [SerializeField]
        private GameObject timeoutPanel;
        [Tooltip("The Ui 3d Car Display")]
        [SerializeField]
        private GameObject selectorCar;
        [Tooltip("Audio Mixer for the Game")]
        [SerializeField]
        private AudioMixer mixer;
        
        [Tooltip("Toggle for Fullscreen")]
        [SerializeField]
        private Toggle fullScreen;
        [Tooltip("Toggle for VSync")]
        [SerializeField]
        private Toggle vSync;
        [Tooltip("Dropdown for Resolution")]
        [SerializeField]
        private TMP_Dropdown resolution;
        
        [Tooltip("Connection Error Holder Scriptable Object")]
        [SerializeField]
        private SO_Connection connection;
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
        string gameVersion = "0.28.0";


        
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
            progressPanel.SetActive(false);
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
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            progressPanel.SetActive(false);
            optionsPanel.SetActive(false);
            controlsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            controlPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(controlPanel.transform.GetChild(1).GetChild(1).gameObject);

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
            else
            {
                Debug.Log("Setting Master 0");
                mixer.SetFloat("Master", 0);
            }
            if (PlayerPrefs.HasKey("MusicVol"))
            {
                mixer.SetFloat("Music", PlayerPrefs.GetFloat("MusicVol"));
            }
            else
            {
                Debug.Log("Setting Music 0");
                mixer.SetFloat("Music", 0);
            }
            if (PlayerPrefs.HasKey("SoundVol"))
            {
                mixer.SetFloat("Sound", PlayerPrefs.GetFloat("SoundVol"));
            }
            else
            {
                Debug.Log("Setting Sound 0");
                mixer.SetFloat("Sound", 0);
            }

            if (PlayerPrefs.HasKey("FullScreen"))
            {
                if (PlayerPrefs.GetInt("FullScreen") == 0)
                    fullScreen.isOn = false;
                else
                    fullScreen.isOn = true;
            }
            if (PlayerPrefs.HasKey("VSync"))
            {
                if (PlayerPrefs.GetInt("VSync") == 0)
                    vSync.isOn = false;
                else
                    vSync.isOn = true;
            }
            if (PlayerPrefs.HasKey("Resolution"))
            {
                resolution.value = PlayerPrefs.GetInt("Resolution");
            }
            ApplyGraphics();
            
            timeoutPanel.SetActive(connection.cause != "left");
            if (connection.cause == "left")
                connection.cause = "";
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
            //progressPanel.SetActive(true);
            //controlPanel.SetActive(false);
            //progressPanel.transform.GetChild(0).GetChild(0).GetComponent<Slider>().value = 0;
            //StartCoroutine(LoadingBar());
            GameObject.Find("FadeScreen").GetComponent<fadeScreen>().fadeOut();
            
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
            progressPanel.SetActive(false);
            optionsPanel.SetActive(true);
            controlsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            controlPanel.SetActive(false);
            selectorCar.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(optionsPanel.transform.GetChild(0).gameObject);
        }

        public void GoToControls()
        {
            progressPanel.SetActive(false);
            optionsPanel.SetActive(false);
            controlsPanel.SetActive(true);
            creditsPanel.SetActive(false);
            controlPanel.SetActive(false);
            selectorCar.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(controlsPanel.transform.GetChild(1).gameObject);
        }
        public void GoToCredits()
        {
            progressPanel.SetActive(false);
            optionsPanel.SetActive(false);
            controlsPanel.SetActive(false);
            creditsPanel.SetActive(true);
            controlPanel.SetActive(false);
            selectorCar.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(creditsPanel.transform.GetChild(1).gameObject);
        }

        public void GoBackToMenu()
        {
            progressPanel.SetActive(false);
            optionsPanel.SetActive(false);
            controlsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            controlPanel.SetActive(true);
            selectorCar.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(controlPanel.transform.GetChild(1).GetChild(1).gameObject);
        }

        public void ApplyGraphics()
        {
            //Screen.fullScreen = fullScreen.isOn;
            if (vSync.isOn)
            {
                QualitySettings.vSyncCount = 1;
                PlayerPrefs.SetInt("VSync", 1);
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                PlayerPrefs.SetInt("VSync", 0);
            }

            if (fullScreen.isOn)
            {
                PlayerPrefs.SetInt("FullScreen", 1);
            }
            else
            {
                PlayerPrefs.SetInt("FullScreen", 0);
            }
            //Debug.Log(resolution.value);
            switch (resolution.value)
            {
                case 0:
                    Screen.SetResolution(1920, 1080, fullScreen.isOn);
                    PlayerPrefs.SetInt("Resolution", 0);
                    break;
                case 1:
                    Screen.SetResolution(1280, 720, fullScreen.isOn);
                    PlayerPrefs.SetInt("Resolution", 1);
                    break;
                case 2:
                    Screen.SetResolution(640, 480, fullScreen.isOn);
                    PlayerPrefs.SetInt("Resolution", 2);
                    break;
            }
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
        IEnumerator LoadingBar() 
        {
            while (PhotonNetwork.LevelLoadingProgress < 1.0f)
            {
                progressPanel.transform.GetChild(0).GetChild(0).GetComponent<Slider>().value = PhotonNetwork.LevelLoadingProgress;
                progressPanel.transform.GetChild(1).Rotate(Vector3.forward, -Time.deltaTime*500.0f, Space.World);
                yield return null;
            }
            yield return new WaitForEndOfFrame();

        }

}
