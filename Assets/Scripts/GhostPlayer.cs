using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Adapted from: https://www.youtube.com/watch?v=c5G2jv7YCxM
public class GhostPlayer : MonoBehaviour
{
    public SO_CarGhost ghost;
    private float timeStepValue;
    private int index1;
    private int index2;

    private void Awake()
    {
        timeStepValue = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        timeStepValue += Time.unscaledDeltaTime;

        if (ghost.mode == GhostMode.Replay)
        {
            GetIndex();
            SetTransform();
        }
    }

    private void GetIndex()
    {
        for (int i = 0; i < ghost.timeStamp.Count - 2; i++)
        {
            if (ghost.timeStamp[i] == timeStepValue)
            {
                index1 = i;
                index2 = i;
                return;
            }
            else if (ghost.timeStamp[i] < timeStepValue && timeStepValue < ghost.timeStamp[i + 1])
            {
                index1 = i;
                index2 = i+1;
                return;
            }
        }

        index1 = ghost.timeStamp.Count - 1;
        index2 = ghost.timeStamp.Count - 1;
    }
    
    private void SetTransform()
    {
        if (index1 == index2)
        {
            transform.position = ghost.position[index1];
            transform.rotation = ghost.rotation[index1];
        }
        else
        {
            float lerpFactor = (timeStepValue - ghost.timeStamp[index1]) /
                               (ghost.timeStamp[index2] - ghost.timeStamp[index1]);
            transform.position = Vector3.Lerp(ghost.position[index1], ghost.position[index2], lerpFactor);
            transform.rotation = Quaternion.Slerp(ghost.rotation[index1], ghost.rotation[index2], lerpFactor);

        }
    }
}
