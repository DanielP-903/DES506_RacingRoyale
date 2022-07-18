using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.SceneManagement;

public class EndStageDisable : MonoBehaviour
{
    private Camera _mainCam;
    // Camera VFX
    private VisualEffect _speedLinesEffect;
    private VisualEffect _speedCircleEffect;
    private VisualEffect _dangerWallEffect;
    private VisualEffect _portalEffect;
    
    void OnLevelWasLoaded()
    {
        _mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _speedLinesEffect = _mainCam.transform.GetChild(2).gameObject.GetComponent<VisualEffect>();
        _speedCircleEffect = _mainCam.transform.GetChild(3).gameObject.GetComponent<VisualEffect>();
        _dangerWallEffect = _mainCam.transform.GetChild(4).gameObject.GetComponent<VisualEffect>();
        _portalEffect = _mainCam.transform.GetChild(5).gameObject.GetComponent<VisualEffect>();
        _portalEffect.Stop();

        if (SceneManager.GetActiveScene().name == "EndStage")
        {
            //_dangerWallEffect.SetVector2("Alpha Values", new Vector2(0,0));
            //_speedCircleEffect.SetVector2("Alpha Values", new Vector2(0,0));
            //_speedLinesEffect.SetVector2("Alpha Values", new Vector2(0,0));
            _speedCircleEffect.Stop();
            _speedLinesEffect.Stop();
            _dangerWallEffect.Stop(); // Note this might stop it from working entirely
            _portalEffect.Stop(); // Note this might stop it from working entirely
        }
    }
}
