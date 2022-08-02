using System.Collections.Generic;
using UnityEngine;

public class CheckpointSystem : MonoBehaviour
{
    [Header("Note: This defines the order of the checkpoints as well!")]
    public List<GameObject> checkpointObjects;

    public List<bool> eliminatedCheckPoints;

    private void Start()
    {
        foreach (GameObject obj in checkpointObjects)
        {
            eliminatedCheckPoints.Add(false);
        }
    }

    public void EliminateCheckpoint(GameObject obj)
    {
        if (checkpointObjects.Contains(obj) && !eliminatedCheckPoints[checkpointObjects.IndexOf(obj)])
        {
            eliminatedCheckPoints[checkpointObjects.IndexOf(obj)] = true;
        }
    }

    public bool GetCheckpointElimination(GameObject obj)
    {
        return eliminatedCheckPoints[checkpointObjects.IndexOf(obj)];
    }
}