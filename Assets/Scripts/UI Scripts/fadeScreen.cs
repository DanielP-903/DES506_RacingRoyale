using System.Collections;
using System.Collections.Generic;
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

    #region IEnumerators

    IEnumerator FadeIn()
    {
        if (PlayerPrefs.HasKey("MasterVol"))
        {
            mixer.SetFloat("Master", 0);
        }
        fadeScreenUI.color = new Color(fadeScreenUI.color.r, fadeScreenUI.color.g, fadeScreenUI.color.b, 1);
        //Debug.Log("Fading In");
        fadedOut = false;
        float vol = -10;
        float volStep = PlayerPrefs.GetFloat("MasterVol") / 25;
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
        float volStep = PlayerPrefs.GetFloat("MasterVol") / 25;
        mixer.SetFloat("Master", vol);
        while (fadeScreenUI.color.a < 1 || vol > 0)
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
                mixer.SetFloat("Master", vol - volStep);
                mixer.GetFloat("Master", out vol);
            }
            yield return new WaitForFixedUpdate();
        }

        fadedOut = true;
    }
    

    #endregion
}
