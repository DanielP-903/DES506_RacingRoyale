using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// In Game Pause Menu Functionality
/// </summary>
/// <returns></returns>
public class PauseMenu : MonoBehaviour
{
    [Tooltip("The Ui Panel for Credits")]
    [SerializeField]
    private GameObject pausePanel;
    [Tooltip("The Ui Panel for Options")]
    [SerializeField]
    private GameObject optionsPanel;
    [Tooltip("The Ui Panel for Controls")]
    [SerializeField]
    private GameObject controlsPanel;
    [Tooltip("Audio Mixer for the Game")]
    [SerializeField]
    private AudioMixer mixer;
        
    [Tooltip("Toggle for Fullscreen")]
    [SerializeField]
    private Toggle fullScreen;
    [Tooltip("Toggle for VSync")]
    [SerializeField]
    private Toggle vSync;
    [SerializeField] 
    private SO_Connection connection;
    [Tooltip("Dropdown for Resolution")]
    [SerializeField]
    private TMP_Dropdown resolution;
    [SerializeField] private bool debugMode;
    
    private bool escapeKey;
    private bool lastKey;

    private GameManager _gm;
    
    /// <summary>
    /// Setup Pause Menu Elements on Start
    /// </summary>
    /// <returns></returns>
    void Start()
    {
        if (!debugMode)
        {
            _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        Cursor.visible = false;
        transform.GetChild(0).gameObject.SetActive(false);
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(false);

        /*if (PlayerPrefs.HasKey("MasterVol"))
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
        }*/

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
    }

    /// <summary>
    /// Detect inputs based on change, causing pause menu to open or close
    /// </summary>
    /// <returns></returns>
    void Update()
    {

        if (escapeKey && !pausePanel.activeSelf && escapeKey != lastKey)
        {
            GoToMainMenu();
        }
        else if (escapeKey && pausePanel.activeSelf && escapeKey != lastKey)
        {
            Resume();
        }
        escapeKey = lastKey;
    }

    /// <summary>
    /// Setup Pause Menu Elements on Level Loaded
    /// </summary>
    /// <returns></returns>
    void OnLevelWasLoaded()
    {
        PlayerPrefs.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));
        mixer.SetFloat("Master", PlayerPrefs.GetFloat("MasterVol"));
    }
    
    /// <summary>
    /// Sets escape key to true if input is present
    /// </summary>
    /// <param name="input">True when input is pressed</param>
    /// <returns></returns>
    public void SetEscape(bool input)
    {
        escapeKey = input;
    }

    /// <summary>
    /// Return Escape Key
    /// </summary>
    /// <returns>Escape Key</returns>
    public bool GetEscape()
    {
        return escapeKey;
    }
    
    /// <summary>
    /// Resume/Close Pause Menu
    /// </summary>
    /// <returns></returns>
    public void Resume()
    {
        Cursor.visible = false;
        if (_gm._eliminated)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        transform.GetChild(0).gameObject.SetActive(false);
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    /// <summary>
    /// Go to Pause Menu
    /// </summary>
    /// <returns></returns>
    public void GoToMainMenu()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        transform.GetChild(0).gameObject.SetActive(true);
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(pausePanel.transform.GetChild(1).gameObject);
    }
    
    /// <summary>
    /// Go to Options Menu
    /// </summary>
    /// <returns></returns>
    public void GoToOptions()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        transform.GetChild(0).gameObject.SetActive(false);
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
        controlsPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsPanel.transform.GetChild(5).gameObject);
    }

    /// <summary>
    /// Go to Controls Menu
    /// </summary>
    /// <returns></returns>
    public void GoToControls()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        transform.GetChild(0).gameObject.SetActive(false);
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(controlsPanel.transform.GetChild(5).gameObject);
    }
    
    /// <summary>
    /// Quit Application
    /// </summary>
    /// <returns></returns>
    public void QuitGame()
    {
        _gm.LeaveRoom();
    }
    
    /// <summary>
    /// Change Master Mixer Group Volume
    /// </summary>
    /// <param name="newVol">New Volume for mixer group to be set to</param>
    /// <returns></returns>
    public void ChangeMaster(float newVol)
    {
        PlayerPrefs.SetFloat("MasterVol", newVol);
        mixer.SetFloat("Master", newVol);
    }
        
    /// <summary>
    /// Change Music Mixer Group Volume
    /// </summary>
    /// <param name="newVol">New Volume for mixer group to be set to</param>
    /// <returns></returns>
    public void ChangeMusic(float newVol)
    {
        PlayerPrefs.SetFloat("MusicVol", newVol);
        mixer.SetFloat("Music", newVol);
    }
        
    /// <summary>
    /// Change Sound Mixer Group Volume
    /// </summary>
    /// <param name="newVol">New Volume for mixer group to be set to</param>
    /// <returns></returns>
    public void ChangeSound(float newVol)
    {
        PlayerPrefs.SetFloat("SoundVol", newVol);
        mixer.SetFloat("Sound", newVol);
    }
    
    /// <summary>
    /// Apply changes to graphical settings
    /// </summary>
    /// <returns></returns>
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
                Screen.SetResolution(640, 360, fullScreen.isOn);
                PlayerPrefs.SetInt("Resolution", 2);
                break;
        }
    }
}
