using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrackPieceType
{
    Corner, Straight
}

public class TrackPiece : MonoBehaviour
{
    // Public track vars
    public TrackPieceType type;

    // Two transforms representing the start and end points
    public GameObject startLocation;
    public GameObject endLocation;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startLocation.transform.position, 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endLocation.transform.position, 0.3f);
    }
    
}