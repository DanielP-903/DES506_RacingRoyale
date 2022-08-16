using UnityEngine;

/// <summary>
/// Handles playing ghost cars from a defined recording
/// Adapted from: https://www.youtube.com/watch?v=c5G2jv7YCxM
/// </summary>
public class GhostPlayer : MonoBehaviour
{
    [SerializeField] private SO_CarGhost ghost;
    private float _timeStepValue;
    private int _index1;
    private int _index2;

    private void Awake()
    {
        _timeStepValue = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        _timeStepValue += Time.unscaledDeltaTime;

        if (ghost.mode == GhostMode.Replay)
        {
            GetIndex();
            SetTransform();
        }
    }

    /// <summary>
    /// Get the current indexes of the location of the ghost car within the recording
    /// </summary>
    private void GetIndex()
    {
        for (int i = 0; i < ghost.timeStamp.Count - 2; i++)
        {
            if (ghost.timeStamp[i] == _timeStepValue)
            {
                _index1 = i;
                _index2 = i;
                return;
            }
            else if (ghost.timeStamp[i] < _timeStepValue && _timeStepValue < ghost.timeStamp[i + 1])
            {
                _index1 = i;
                _index2 = i + 1;
                return;
            }
        }

        _index1 = ghost.timeStamp.Count - 1;
        _index2 = ghost.timeStamp.Count - 1;
    }

    /// <summary>
    /// Set the transform of the ghost car to its current location in the recording
    /// </summary>
    private void SetTransform()
    {
        if (_index1 == _index2)
        {
            transform.position = ghost.position[_index1];
            transform.rotation = ghost.rotation[_index1];
        }
        else
        {
            float lerpFactor = (_timeStepValue - ghost.timeStamp[_index1]) / (ghost.timeStamp[_index2] - ghost.timeStamp[_index1]);
            transform.position = Vector3.Lerp(ghost.position[_index1], ghost.position[_index2], lerpFactor);
            transform.rotation = Quaternion.Slerp(ghost.rotation[_index1], ghost.rotation[_index2], lerpFactor);

        }
    }
}
