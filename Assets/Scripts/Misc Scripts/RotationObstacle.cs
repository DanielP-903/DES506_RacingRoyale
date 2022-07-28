using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationObstacle : MonoBehaviour
{
    [SerializeField]
    private bool xRotate;
    [SerializeField]
    private bool yRotate;
    [SerializeField]
    private bool zRotate;
    [SerializeField]
    private float rotationVal = 0.5f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 newRot = Vector3.zero;
        if (xRotate)
        {
            newRot = new Vector3(1, newRot.y, newRot.z);
        }
        if (yRotate)
        {
            newRot = new Vector3(newRot.x, 1, newRot.z);
        }
        if (zRotate)
        {
            newRot = new Vector3(newRot.x, newRot.y, 1);
        }
        transform.Rotate(newRot * rotationVal);
    }
}
