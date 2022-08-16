using UnityEngine;

/// <summary>
/// Handles the recording of ghost car playthroughs
/// </summary>
public class GhostRecorder : MonoBehaviour
{
    [SerializeField] private SO_CarGhost ghost;
    private float _timer;
    private float _timeStampValue;

    private void Awake()
    {
        // Reset the recording on play if set to record
        if (ghost.mode == GhostMode.Record)
        {
            ghost.ResetVars();
            _timeStampValue = 0.0f;
            _timer = 0.0f;
        }
    }

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;
        _timeStampValue += Time.unscaledDeltaTime;

        // Add ghost car locational data to the recorded list based on the state and frequency
        if (ghost.mode == GhostMode.Record && _timer >= (1 / ghost.frequency))
        {
            ghost.timeStamp.Add(_timeStampValue);
            ghost.position.Add(transform.position);
            ghost.rotation.Add(transform.rotation);
            _timer = 0;
        }
    }
}
