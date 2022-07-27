using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BeachBall : MonoBehaviour
{
    [SerializeField] private float startForce = 35;
    private Vector3 startPos;
    private Rigidbody rb;
    private PhotonView pv;
    
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
        if (!pv.Owner.IsMasterClient)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
        GoToSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < 20)
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
        rb.AddForce(Vector3.forward * startForce, ForceMode.Impulse);
    }
}
