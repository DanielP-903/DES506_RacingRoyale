using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugMenu : MonoBehaviour
{
    [SerializeField] private PlayerPowerups playerPowerups;
    private CarController _carController;
    private GameManager _gameManager;
    private GameObject _playerRef;
    private bool _hasFoundPlayer;

    private Rect windowRect = new Rect(20, 90, 300, 250);
    private Rect windowToggle = new Rect(20, 20, 250, 40);
    private bool showWindow;
    private bool _debugEnable;
    private float _debugTimer = 1;

    private void Start()
    {
        StartCoroutine(waitTime());
    }

    private void Update()
    {
        _debugTimer = _debugTimer <= 0 ? 0 : _debugTimer - Time.deltaTime;
    }

    void OnGUI()
    {
        // Draw the appropriate windows when debugging is allowed
        if (_debugEnable)
        {
            windowToggle = GUI.Window(0, windowToggle, DebugToggleWindow, "");
            if (showWindow)
            { 
                // Draw window when toggled on
                windowRect = GUI.Window(1, windowRect, WindowFunction, "Debug Menu");
            }
        }
    }

    /// <summary>
    /// Create a debug toggle window for toggling the debug features
    /// <param name="windowID">The ID of the window to be used</param>
    /// </summary>
    void DebugToggleWindow(int windowID)
    {
        // Make a toggle button for hiding and showing the window
        showWindow = GUI.Toggle(new Rect(10, 10, 200, 20), showWindow, "Enable/Disable Debug Menu");
    }
    
    /// <summary>
    /// Draw a debug window containing buttons that give powerups or skips the waiting timer
    /// <param name="windowID">The ID of the window to be used</param>
    /// </summary>
    void WindowFunction(int windowID)
    {
        GUI.Label(new Rect(25, 25, 200, 30), "Waiting Area Debug");
        if (GUI.Button(new Rect(25, 50, 200, 30), "Skip Waiting Timer"))
        {
            if (_hasFoundPlayer && !_carController.debug)
                _gameManager.SetWaitingTimer(0.1f);
        }

        // Draw any Controls inside the window here
        GUI.Label(new Rect(25, 80, 100, 30), "Powerups");

        if (GUI.Button(new Rect(25, 100, 200, 30), "Give Superboost"))
        {
            if (_hasFoundPlayer)
                playerPowerups.DebugSetCurrentPowerup(PowerupType.Superboost);
        }
        if (GUI.Button(new Rect(25, 130, 200, 30), "Give Air Blast"))
        {
            if (_hasFoundPlayer)
                playerPowerups.DebugSetCurrentPowerup(PowerupType.AirBlast);
        }

        if (GUI.Button(new Rect(25, 160, 200, 30), "Give Grappling Hook"))
        {
            if (_hasFoundPlayer)
                playerPowerups.DebugSetCurrentPowerup(PowerupType.GrapplingHook);
        }

        if (GUI.Button(new Rect(25, 190, 200, 30), "Give Punching Glove"))
        {
            if (_hasFoundPlayer)
                playerPowerups.DebugSetCurrentPowerup(PowerupType.PunchingGlove);
        }
        
        // Make the windows be draggable.
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }
    
    /// <summary>
    /// Wait for player to spawn then get a reference
    /// </summary>
    IEnumerator waitTime()
    {
        yield return new WaitForSeconds(1);

        GameObject[] listOfPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in listOfPlayers)
        {
            if (!player.GetComponent<PhotonView>())
            {
                continue;
            }

            if (player.GetComponent<PhotonView>().IsMine && !player.GetComponent<CarController>().bot)
            {
                _playerRef = player;
                _hasFoundPlayer = true;
            }
        }

        playerPowerups = _playerRef.GetComponent<PlayerPowerups>();
        _carController = _playerRef.GetComponent<CarController>();
        if (!_carController.debug)
        {
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
    }
    
    /// <summary>
    /// Change the debugging state
    /// </summary>
    private void ChangeDebuggingState()
    {
        if (_debugTimer <= 0)
        {
            _debugTimer = 0.1f;
            _debugEnable = !_debugEnable;
        }
    }


    // Enable debugging input
    public void EnableDebugging(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        ChangeDebuggingState();
        //Debug.Log("Enable debugging detected");
    }
}
