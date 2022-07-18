using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeachBall : MonoBehaviour
{
    private Vector3 startPos;
    private Rigidbody rb;
    
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
        GoToSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < 35)
        {
            GoToSpawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ResetZone")
        {
            GoToSpawn();
        }
    }

    private void GoToSpawn()
    {
        transform.position = startPos;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(Vector3.right, ForceMode.Impulse);
    }
}
