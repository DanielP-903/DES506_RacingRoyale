using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [Tooltip("Dropdown for Resolution")]
    [SerializeField]
    private TMP_Dropdown resolution;
    [SerializeField] private bool debugMode;

    private bool escapeKey;
    private bool lastKey;

    private GameManager _gm;
    // Start is called before the first frame update
    void Start()
    {
        if (!debugMode)
        {
            _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        Cursor.visible = false;
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

    // Update is called once per frame
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

    void OnLevelWasLoaded()
    {
        PlayerPrefs.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));
        mixer.SetFloat("Master", PlayerPrefs.GetFloat("MasterVol"));
    }
    
    public void SetEscape(bool input)
    {
        escapeKey = input;
    }
    
    public void Resume()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    public void GoToMainMenu()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(pausePanel.transform.GetChild(0).gameObject);
    }
    
    public void GoToOptions()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
        controlsPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsPanel.transform.GetChild(0).gameObject);
    }

    public void GoToControls()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        controlsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(controlsPanel.transform.GetChild(1).gameObject);
    }
    
    // QUIT APPLICATION
    public void QuitGame()
    {
        _gm.LeaveRoom();
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
}
