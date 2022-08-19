using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sets sliders to correct player preference
/// </summary>
/// <returns></returns>
public class SliderSetter : MonoBehaviour
{
    [SerializeField]
    private string playerPref;
    
    // Start is called before the first frame update
    void Start()
    {
        Slider slider = GetComponent<Slider>();
        if (PlayerPrefs.HasKey(playerPref))
        {
            slider.value = PlayerPrefs.GetFloat(playerPref);
        }
    }
}
