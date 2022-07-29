using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player_Scripts
{
    public enum GhostMode
    {
        Replay, Record    
    }

    [CreateAssetMenu(fileName = "CarGhost", menuName = "Car Ghost", order = 1)]
    public class SO_CarGhost : ScriptableObject
    {
        public GhostMode mode;
        public float frequency;
    
        public List<float> timeStamp;
        public List<Vector3> position;
        public List<Quaternion> rotation;

        public void ResetVars()
        {
            timeStamp.Clear();
            position.Clear();
            rotation.Clear();
        }
    }
}