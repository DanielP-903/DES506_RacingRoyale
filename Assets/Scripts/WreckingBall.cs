using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WreckingBall : MonoBehaviour
{
    public float speed = 2.5f, distance = 10f;
    public bool reverse;
    private Quaternion currentRotation;
    // Start is called before the first frame update
    void Start()
    {
        currentRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localEulerAngles = new Vector3(transform.rotation.x, transform.rotation.y, reverse ? -Mathf.PingPong(Time.time * speed, distance) + currentRotation.z : Mathf.PingPong(Time.time * speed, distance) + currentRotation.z);
    }
}
