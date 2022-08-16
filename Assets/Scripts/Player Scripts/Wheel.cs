using UnityEngine;

/// <summary>
/// Visual wheel turning functionality
/// </summary>
public class Wheel : MonoBehaviour
{
    private GameObject _parent;
    private WheelCollider _wheelCollider;

    // Start is called before the first frame update
    void Start()
    {
        _parent = transform.parent.gameObject;
        _wheelCollider = _parent.GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Euler(_wheelCollider.rpm, (_wheelCollider.steerAngle), 0);
    }
}
