using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrackGenerator : MonoBehaviour
{
    public List<TrackPiece> trackPieces = new List<TrackPiece>();
    private readonly List<TrackPiece> _generatedTrack = new List<TrackPiece>();

    public int noOfPieces = 5;

    private TrackPiece _previousTrackPiece;
    private TrackPiece _newTrackPiece;

    private int _rotation;

    private Vector3 directionEnd = Vector3.zero;
    private Vector3 posEnd = Vector3.zero;
    private Vector3 directionStart= Vector3.zero;
    private Vector3 posStart = Vector3.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        _generatedTrack.Clear();
        
        // for (int i = 0; i < noOfPieces; i++)
        // {
        //     int rand = Random.Range(0, trackPieces.Count);
        //     _generatedTrack.Add(Instantiate(trackPieces[rand]));
        //     
        // }
         _generatedTrack.Add(Instantiate(trackPieces[1]));
         _generatedTrack.Add(Instantiate(trackPieces[3]));
         _generatedTrack.Add(Instantiate(trackPieces[3]));
         _generatedTrack.Add(Instantiate(trackPieces[2]));
         _generatedTrack.Add(Instantiate(trackPieces[3]));

        GenerateTrack();
    }

    private void OnDrawGizmos()
    {

        for (int i = 0; i < noOfPieces - 1; i++)
        {
            // Vector3 forwardPrev = _generatedTrack[i].endLocation.transform.forward;
            // Vector3 forwardCurrent = _generatedTrack[i + 1].startLocation.transform.forward;
            
            directionEnd = transform.worldToLocalMatrix.MultiplyVector(_generatedTrack[i].endLocation.transform.forward);
            directionStart =  transform.worldToLocalMatrix.MultiplyVector(_generatedTrack[i+1].startLocation.transform.forward);
            posEnd = _generatedTrack[i].endLocation.transform.position;
            posStart = _generatedTrack[i + 1].startLocation.transform.position;
            
            Gizmos.color = Color.green;
            Gizmos.DrawRay(posStart + Vector3.up, directionStart);
        
            Gizmos.color = Color.red;
            Gizmos.DrawRay(posEnd + Vector3.up, directionEnd);
        }
        
    }

    private void GenerateTrack()
    {
        for (int i = 0; i < noOfPieces - 1; i++)
        {
            _generatedTrack[i + 1].transform.position = _generatedTrack[i].transform.position; //location;

            float xDistBetweenEndAndStart = _generatedTrack[i + 1].startLocation.transform.position.x - _generatedTrack[i].endLocation.transform.position.x;
            float yDistBetweenEndAndStart = _generatedTrack[i + 1].startLocation.transform.position.y - _generatedTrack[i].endLocation.transform.position.y;
            float zDistBetweenEndAndStart = _generatedTrack[i + 1].startLocation.transform.position.z - _generatedTrack[i].endLocation.transform.position.z;

            Vector3 offset = new Vector3(
                _generatedTrack[i + 1].transform.position.x - xDistBetweenEndAndStart, 
                _generatedTrack[i + 1].transform.position.y - yDistBetweenEndAndStart, 
                _generatedTrack[i + 1].transform.position.z - zDistBetweenEndAndStart);

            _generatedTrack[i + 1].transform.position = offset;
            
            // Get Vector3.Angle between two LOCAL forward vectors
            //Vector3 localForward = transform.worldToLocalMatrix.MultiplyVector(transform.forward);

            //Vector3 forwardPrev = _generatedTrack[i].endLocation.transform.forward;
            //Vector3 forwardCurrent = _generatedTrack[i+1].startLocation.transform.forward;            
            
            Vector3 forwardPrev = transform.worldToLocalMatrix.MultiplyVector(_generatedTrack[i].endLocation.transform.forward);
            Vector3 forwardCurrentStart = transform.worldToLocalMatrix.MultiplyVector(_generatedTrack[i+1].startLocation.transform.forward);
            Vector3 forwardCurrentEnd = transform.worldToLocalMatrix.MultiplyVector(_generatedTrack[i+1].endLocation.transform.forward);
            float angle = Vector3.Angle(forwardCurrentStart, forwardPrev);

            if (forwardCurrentStart != forwardCurrentEnd)
            {
                angle -= 90;
            }
            
            
            _generatedTrack[i + 1].transform.RotateAround(_generatedTrack[i+1].startLocation.transform.position, Vector3.up,  angle);
        }
    }
}
