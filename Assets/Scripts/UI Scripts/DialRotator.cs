using UnityEngine;

/// <summary>
/// Handles speed dial rotation
/// </summary>
public class DialRotator : MonoBehaviour
{
    [SerializeField] private PlayerHUDController hudRef;
    [SerializeField] private Vector3 endAngles = new Vector3(0, 0, 90);
    private float _rotationalValue;
    private RectTransform _rectTransform;
    private Quaternion _toAngle;
    
    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _toAngle = Quaternion.Euler(endAngles);
    }

    private void Update()
    {
        // Rotate the dial based on the min and max speed of the vehicle using the angles given
        _rotationalValue = Mathf.Clamp(hudRef.currentSpeed, 0, 120);
        _toAngle = new Quaternion(0, 0, (-(_rotationalValue*1.8f) + 120) * Mathf.Deg2Rad + (endAngles.z * Mathf.Deg2Rad), 1);
        _rectTransform.rotation = Quaternion.Lerp(_rectTransform.rotation, _toAngle, 10*Time.deltaTime);
    }
}