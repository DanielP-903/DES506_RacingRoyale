using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Disables this object dependant on whether the current scene is the waiting area
/// </summary>
public class WaitingStageDisable : MonoBehaviour
{
    void Update()
    {
        gameObject.SetActive(SceneManager.GetActiveScene().name != "WaitingArea");
    }
}