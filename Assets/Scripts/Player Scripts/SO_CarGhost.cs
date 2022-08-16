using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the state of the ghost for either replaying or recording
/// </summary>
public enum GhostMode
{
    Replay, Record    
}

/// <summary>
/// Car ghost scriptable object contains recording-specific values and playback functionality along with a reset function
/// </summary>
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