using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        transform.localRotation = Quaternion.Euler(0,(90 + _wheelCollider.steerAngle),_wheelCollider.rpm);
    }
}
