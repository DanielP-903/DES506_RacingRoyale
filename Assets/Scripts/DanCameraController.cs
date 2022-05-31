using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanCameraController : MonoBehaviour
{
    public GameObject target;
    public float xOffset;
    public float yOffset;
    public float zOffset;

    // Update is called once per frame
    void Update()
    {
        Vector3 offset;
        
        transform.position = Vector3.Lerp(transform.position, target.transform.position, Time.deltaTime);
        
    }
}
