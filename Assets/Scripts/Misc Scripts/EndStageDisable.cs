using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.SceneManagement;

/// <summary>
/// Disables vfx and enables cursor on loading the end stage
/// </summary>
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
            _speedCircleEffect.Stop();
            _speedLinesEffect.Stop();
            _dangerWallEffect.Stop();
            _portalEffect.Stop();
        }
        
        Cursor.lockState = CursorLockMode.None;
    }
}
