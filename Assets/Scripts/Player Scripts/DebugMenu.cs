using System.Collections;
using Photon.Pun;
using Player_Scripts.Powerup_Scripts;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    public CarVFXHandler carVFXHandler;
    private CarController _carController;
    public PlayerPowerups playerPowerups;
    private GameManager _gameManager;
    private GameObject _playerRef;
    private bool _hasFoundPlayer;

    private Rect windowRect = new Rect(20, 90, 300, 250);
    private Rect windowToggle = new Rect(20, 20, 250, 40);
    private bool showWindow = false;

    private void Start()
    {
        StartCoroutine(waitTime());
    }

    void OnGUI()
    {
        windowToggle = GUI.Window(0, windowToggle, DebugToggleWindow, "");
        if (showWindow)
        {
            windowRect = GUI.Window(1, windowRect, WindowFunction, "Debug Menu");
        }
    }

    void DebugToggleWindow(int windowID)
    {
        // Make a toggle button for hiding and showing the window
        showWindow = GUI.Toggle(new Rect(10, 10, 200, 20), showWindow, "Enable/Disable Debug Menu");
    }

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

        // if (GUI.Button (new Rect (25, 120, 200, 30), "Give Bouncy Wall Shield")) 
        // {
        //     if (_hasFoundPlayer)
        //         playerPowerups.DebugSetCurrentPowerup(PowerupType.BouncyWallShield);
        // }
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
        // if (GUI.Button (new Rect (25, 200, 200, 30), "Give Warp Portal"))
        // {
        //     if (_hasFoundPlayer)
        //         playerPowerups.DebugSetCurrentPowerup(PowerupType.WarpPortal);
        // }

        // Make the windows be draggable.
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }

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
}
