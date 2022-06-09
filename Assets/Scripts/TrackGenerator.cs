using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    public List<TrackPiece> trackPieces = new List<TrackPiece>();
    private List<TrackPiece> generatedTrack = new List<TrackPiece>();

    public int noOfPieces = 5;

    private TrackPiece _previousTrackPiece;
    private TrackPiece _newTrackPiece;

    private int _rotation;
    
    // Start is called before the first frame update
    void Start()
    {
        generatedTrack.Clear();
        
        for (int i = 0; i < noOfPieces; i++)
        {
            //int rand = Random.Range(0, trackPieces.Count);
            //generatedTrack.Add(Instantiate(trackPieces[rand]));
        }
        generatedTrack.Add(Instantiate(trackPieces[1]));
        generatedTrack.Add(Instantiate(trackPieces[0]));
        generatedTrack.Add(Instantiate(trackPieces[1]));
        generatedTrack.Add(Instantiate(trackPieces[2]));
        generatedTrack.Add(Instantiate(trackPieces[0]));
        //generatedTrack.Add(Instantiate(trackObjects[1]).transform.GetChild(0).GetComponent<TrackPiece>());

        GenerateTrack();
    }

    private void GenerateTrack()
    {
        for (int i = 0; i < noOfPieces - 1; i++)
        {
            //Vector3 location = GetTrackLocation(generatedTrack[i + 1], generatedTrack[i]);

            //generatedTrack[i + 1].transform.position = generatedTrack[i].startLocation.transform.position;

            generatedTrack[i + 1].transform.position = generatedTrack[i].transform.position; //location;

            float XdistBetweenEndAndStart = generatedTrack[i + 1].startLocation.transform.position.x - generatedTrack[i].endLocation.transform.position.x;
            float ZdistBetweenEndAndStart = generatedTrack[i + 1].startLocation.transform.position.z - generatedTrack[i].endLocation.transform.position.z;

            Vector3 offset = new Vector3(generatedTrack[i + 1].transform.position.x - XdistBetweenEndAndStart, 0, generatedTrack[i + 1].transform.position.z - ZdistBetweenEndAndStart);

            generatedTrack[i + 1].transform.position = offset;
            
            int angle = (int)Vector3.Angle(generatedTrack[i].endLocation.transform.forward, generatedTrack[i + 1].startLocation.transform.forward);
            int previousRotation = _rotation;
 
            _rotation += angle;
            if (_rotation > 360)
            {
                _rotation -= 360;
            }
            if (_rotation < previousRotation)
            {
                angle *= -1;
            }
            //generatedTrack[i + 1].transform.rotation = generatedTrack[i].endLocation.transform.rotation);
            generatedTrack[i + 1].transform.RotateAround(generatedTrack[i+1].startLocation.transform.position, Vector3.up,  angle);
            Debug.Log("Angle " + i + ": " + angle);
            Debug.Log("Rotation " + i + ": " + _rotation);
        }
    }

    private Vector3 GetTrackLocation(TrackPiece newTrackPiece, TrackPiece previousTrackPiece)
    {
        float offsetX = Mathf.Abs(newTrackPiece.startLocation.transform.position.x - previousTrackPiece.endLocation.transform.position.x);// + ((Vector3.Distance(newTrackPiece.startLocation.transform.position, newTrackPiece.endLocation.transform.position)/2));
        float offsetZ = Mathf.Abs(newTrackPiece.startLocation.transform.position.z - previousTrackPiece.endLocation.transform.position.z);// + ((Vector3.Distance(newTrackPiece.startLocation.transform.position, newTrackPiece.endLocation.transform.position)/2));
        
        return new Vector3(newTrackPiece.endLocation.transform.position.x - offsetX, 0, newTrackPiece.endLocation.transform.position.z - offsetZ);
    }
}
