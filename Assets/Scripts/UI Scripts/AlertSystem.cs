using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Display Alerts for finishing, elimination and completion of stages and checkpoints
/// </summary>
/// <returns></returns>
public class AlertSystem : MonoBehaviour
{
    [SerializeField]
    private float displayTime;
    [SerializeField]
    private float fadeInTime;
    [SerializeField]
    private float fadeOutTime;

    private Slider _slider;
    private Image _alertImage;
    [SerializeField]
    private Sprite checkpointImage;
    [SerializeField]
    private Sprite winImage;
    [SerializeField]
    private Sprite qualifiedImage;
    [SerializeField]
    private Sprite eliminatedImage;
    [SerializeField]
    private Sprite checkpointDestroyedImage;

    private IEnumerator storedRoutine;
    
    /// <summary>
    /// Establish UI on Start
    /// </summary>
    /// <returns></returns>
    void Start()
    {
        _slider = transform.Find("Mask").Find("Slider").GetComponent<Slider>();
        _alertImage = _slider.transform.Find("Fill Area").Find("Fill").GetComponent<Image>();
        _slider.value = 0;
        _alertImage.fillOrigin = (int) Image.OriginHorizontal.Left;
    }
    
    /// <summary>
    /// Start Show Alert Coroutine
    /// </summary>
    /// <returns></returns>
    public void displayAlert(string alertName)
    {
        if (storedRoutine != null)
        {
            StopCoroutine(storedRoutine);
        }

        storedRoutine = showAlert(alertName);
        StartCoroutine(storedRoutine);
    }

    /// <summary>
    /// Scroll fade in the alert, wait and scroll fade out the alert
    /// </summary>
    /// <returns></returns>
    IEnumerator showAlert(string alertName)
    {
        float fadeInStep = 1/fadeInTime * 0.02f;
        float fadeOutStep = 1/fadeOutTime * 0.02f;
        switch (alertName)
        {
            case "Checkpoint":
                _alertImage.sprite = checkpointImage;
                break;
            case "Win":
                _alertImage.sprite = winImage;
                break;
            case "Qualified":
                _alertImage.sprite = qualifiedImage;
                break;
            case "Eliminated":
                _alertImage.sprite = eliminatedImage;
                break;
            case "CheckpointDestroyed":
                _alertImage.sprite = checkpointDestroyedImage;
                break;
        }
        _alertImage.fillOrigin = (int) Image.OriginHorizontal.Left;
        while (_slider.value < 1)
        {
            _slider.value += fadeInStep;
            yield return new WaitForFixedUpdate();
        }
        _alertImage.fillOrigin = (int) Image.OriginHorizontal.Right;
        yield return new WaitForSeconds(displayTime);
        while (_slider.value > 0)
        {
            _slider.value -= fadeOutStep;
            yield return new WaitForFixedUpdate();
        }
    }
}
