using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// Sets initial movement of the beachball and resets it if it's too low or hits a reset zone.
/// </summary>
public class BeachBall : MonoBehaviour
{
    [SerializeField] private float startForce = 35;
    private Vector3 startPos;
    private Rigidbody rb;
    private PhotonView pv;
    
    /// <summary>
    /// Beachball initialisation upon start
    /// </summary>
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

    /// <summary>
    /// If the beachball is below 20, reset to spawn
    /// </summary>
    void Update()
    {
        if (transform.position.y < 20)
        {
            GoToSpawn();
        }
    }

    /// <summary>
    /// On touching a reset zone, reset to spawn
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ResetZone")
        {
            GoToSpawn();
        }
    }

    /// <summary>
    /// Set beachball at it's initial spawn location and add a force to it to get it spinning
    /// </summary>
    private void GoToSpawn()
    {
        transform.position = startPos;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(Vector3.forward * startForce, ForceMode.Impulse);
    }
}
