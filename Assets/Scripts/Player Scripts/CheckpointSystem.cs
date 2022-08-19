using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages Checkpoints
/// </summary>
/// <returns></returns>
public class CheckpointSystem : MonoBehaviour
{
    [Header("Note: This defines the order of the checkpoints as well!")]
    public List<GameObject> checkpointObjects;

    public List<bool> eliminatedCheckPoints;

    /// <summary>
    /// Establish Checkpoints on Start
    /// </summary>
    /// <returns>True if checkpoint has been eliminated</returns>
    private void Start()
    {
        foreach (GameObject obj in checkpointObjects)
        {
            eliminatedCheckPoints.Add(false);
        }
    }

    /// <summary>
    /// Stops Players from using current checkpoint anymore
    /// </summary>
    /// <returns></returns>
    public void EliminateCheckpoint(GameObject obj)
    {
        if (checkpointObjects.Contains(obj) && !eliminatedCheckPoints[checkpointObjects.IndexOf(obj)])
        {
            eliminatedCheckPoints[checkpointObjects.IndexOf(obj)] = true;
        }
    }

    /// <summary>
    /// Check if a checkpoint is eliminated
    /// </summary>
    /// <param name="obj">Checkpoint to be checked</param>
    /// <returns>True if checkpoint has been eliminated</returns>
    public bool GetCheckpointElimination(GameObject obj)
    {
        return eliminatedCheckPoints[checkpointObjects.IndexOf(obj)];
    }
}