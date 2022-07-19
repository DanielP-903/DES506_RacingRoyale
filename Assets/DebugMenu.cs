using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    public CarVFXHandler carVFXHandler;
    public CarController carController;
    public PlayerPowerups playerPowerups;
    
    private Rect windowRect = new Rect (20, 90, 300, 250);
    private Rect windowToggle = new Rect (20, 20, 250, 40);
    private bool showWindow = false;

    void OnGUI ()
    {
        windowToggle = GUI.Window (0, windowToggle, DebugToggleWindow, "");
        if (showWindow)
        {
            windowRect = GUI.Window (1, windowRect, WindowFunction, "Debug Menu");
        }
    }

    void DebugToggleWindow(int windowID)
    {
        // Make a toggle button for hiding and showing the window
        showWindow = GUI.Toggle(new Rect(10, 10, 200, 20), showWindow, "Enable/Disable Debug Menu");
    }

    void WindowFunction (int windowID) 
    {
        // Draw any Controls inside the window here
        GUI.Label (new Rect (25, 25, 100, 30), "Powerups");

        if (GUI.Button (new Rect (25, 50, 200, 30), "Give Superboost")) 
        {
            // This code is executed when the Button is clicked
            playerPowerups.DebugSetCurrentPowerup(PowerupType.Superboost);
        }
        if (GUI.Button (new Rect (25, 80, 200, 30), "Give Bouncy Wall Shield")) 
        {
            // This code is executed when the Button is clicked
            playerPowerups.DebugSetCurrentPowerup(PowerupType.BouncyWallShield);
        }
        if (GUI.Button (new Rect (25, 110, 200, 30), "Give Air Blast")) 
        {
            // This code is executed when the Button is clicked
            playerPowerups.DebugSetCurrentPowerup(PowerupType.AirBlast);
        }
        if (GUI.Button (new Rect (25, 140, 200, 30), "Give Grappling Hook")) 
        {
            // This code is executed when the Button is clicked
            playerPowerups.DebugSetCurrentPowerup(PowerupType.GrapplingHook);
        }
        if (GUI.Button (new Rect (25, 170, 200, 30), "Give Punching Glove"))
        {
            // This code is executed when the Button is clicked
            playerPowerups.DebugSetCurrentPowerup(PowerupType.PunchingGlove);
        }
        if (GUI.Button (new Rect (25, 200, 200, 30), "Give Warp Portal")) 
        {
            // This code is executed when the Button is clicked
            playerPowerups.DebugSetCurrentPowerup(PowerupType.WarpPortal);
        }
        
        // Make the windows be draggable.
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }
}
