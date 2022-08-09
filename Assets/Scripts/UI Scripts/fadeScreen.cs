using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class fadeScreen : MonoBehaviour
{
    private Image fadeScreenUI;
    [SerializeField]
    private AudioMixer mixer;

    private IEnumerator storedRoutine;
    public bool fadedOut = false;
    private GameObject progressPanel;
    
    // Start is called before the first frame update
    void Start()
    {
        fadeScreenUI = GetComponent<Image>();
        storedRoutine = FadeIn();
        StartCoroutine(storedRoutine);
    }

    public void fadeOut()
    {
        StopCoroutine(storedRoutine);
        storedRoutine = FadeOut();
        StartCoroutine(storedRoutine);
    }

    public void quitFade()
    {
        StopCoroutine(storedRoutine);
        storedRoutine = QuitFade();
        StartCoroutine(storedRoutine);
    }

    #region IEnumerators

    IEnumerator QuitFade()
    {
        float vol = PlayerPrefs.GetFloat("MasterVol");
        float volStep = (PlayerPrefs.GetFloat("MasterVol") + 80) / 25;
        mixer.SetFloat("Master", vol);
        while (fadeScreenUI.color.a < 1 || vol > -80)
        {
            if (fadeScreenUI.color.a < 1)
            {
                fadeScreenUI.color = new Color(fadeScreenUI.color.r, fadeScreenUI.color.g, fadeScreenUI.color.b, fadeScreenUI.color.a + 0.04f);
            }
            else
            {
                fadeScreenUI.color = new Color(fadeScreenUI.color.r, fadeScreenUI.color.g, fadeScreenUI.color.b, 1);
            }

            if (vol > -80)
            {
                mixer.GetFloat("Master", out vol);
                Debug.Log(vol - volStep);
                if (vol - volStep < -80)
                {
                    mixer.SetFloat("Master", -80);
                }
                else
                {
                    mixer.SetFloat("Master", vol - volStep);
                }
                mixer.GetFloat("Master", out vol);
            }
            yield return new WaitForFixedUpdate();
        }

        PhotonNetwork.LeaveRoom();
    }

    IEnumerator FadeIn()
    {
        if (PlayerPrefs.HasKey("MasterVol"))
        {
            mixer.SetFloat("Master", -80);
        }
        fadeScreenUI.color = new Color(fadeScreenUI.color.r, fadeScreenUI.color.g, fadeScreenUI.color.b, 1);
        //Debug.Log("Fading In");
        fadedOut = false;
        float vol = -20;
        float volStep = (PlayerPrefs.GetFloat("MasterVol") + 80) / 25;
        mixer.SetFloat("Master", vol);
        yield return new WaitForSeconds(1);
        while (fadeScreenUI.color.a > 0 || vol < PlayerPrefs.GetFloat("MasterVol"))
        {
            if (fadeScreenUI.color.a > 0)
            {
                fadeScreenUI.color = new Color(fadeScreenUI.color.r, fadeScreenUI.color.g, fadeScreenUI.color.b, fadeScreenUI.color.a - 0.04f);
            }
            else
            {
                fadeScreenUI.color = new Color(fadeScreenUI.color.r, fadeScreenUI.color.g, fadeScreenUI.color.b, 0);
            }

            if (vol < PlayerPrefs.GetFloat("MasterVol"))
            {
                mixer.GetFloat("Master", out vol);
                mixer.SetFloat("Master", vol + volStep);
                mixer.GetFloat("Master", out vol);
            }
            yield return new WaitForFixedUpdate();
        }
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
    
    IEnumerator FadeOut()
    {
        float vol = PlayerPrefs.GetFloat("MasterVol");
        float volStep = (PlayerPrefs.GetFloat("MasterVol") + 80) / 25;
        mixer.SetFloat("Master", vol);
        while (fadeScreenUI.color.a < 1 || vol > -80)
        {
            if (fadeScreenUI.color.a < 1)
            {
                fadeScreenUI.color = new Color(fadeScreenUI.color.r, fadeScreenUI.color.g, fadeScreenUI.color.b, fadeScreenUI.color.a + 0.04f);
            }
            else
            {
                fadeScreenUI.color = new Color(fadeScreenUI.color.r, fadeScreenUI.color.g, fadeScreenUI.color.b, 1);
            }

            if (vol > -80)
            {
                mixer.GetFloat("Master", out vol);
                Debug.Log(vol - volStep);
                if (vol - volStep < -80)
                {
                    mixer.SetFloat("Master", -80);
                }
                else
                {
                    mixer.SetFloat("Master", vol - volStep);
                }
                mixer.GetFloat("Master", out vol);
            }
            yield return new WaitForFixedUpdate();
        }
        mixer.SetFloat("Master", -80);
        StartCoroutine(LoadingBar());
        fadedOut = true;
        while (fadeScreenUI.color.a > 0)
        {
            if (fadeScreenUI.color.a > 0)
            {
                fadeScreenUI.color = new Color(fadeScreenUI.color.r, fadeScreenUI.color.g, fadeScreenUI.color.b, fadeScreenUI.color.a - 0.04f);
            }
            else
            {
                fadeScreenUI.color = new Color(fadeScreenUI.color.r, fadeScreenUI.color.g, fadeScreenUI.color.b, 0);
            }
            
            yield return new WaitForFixedUpdate();
        }
    }
    
    IEnumerator LoadingBar()
    {
        /*if (GameObject.FindGameObjectWithTag("MainCanvas").transform.childCount > 10)
        {
            progressPanel = GameObject.FindGameObjectWithTag("MainCanvas").transform.GetChild(12).gameObject;
        }
        else
        {
            progressPanel = GameObject.FindGameObjectWithTag("MainCanvas").transform.GetChild(6).gameObject;
        }*/
        progressPanel = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Progress BG").gameObject;
        progressPanel.transform.GetChild(0).GetChild(0).GetComponent<Slider>().value = 0;
        progressPanel.SetActive(true);
        while (PhotonNetwork.LevelLoadingProgress < 1.0f)
        {
            progressPanel.transform.GetChild(0).GetChild(0).GetComponent<Slider>().value = PhotonNetwork.LevelLoadingProgress;
            progressPanel.transform.GetChild(1).Rotate(Vector3.forward, -Time.deltaTime * 500.0f, Space.World);
            yield return null;
        }

        yield return new WaitForEndOfFrame();

    }

    #endregion
}
