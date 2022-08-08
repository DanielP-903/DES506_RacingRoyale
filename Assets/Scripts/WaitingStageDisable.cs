using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitingStageDisable : MonoBehaviour
{
    void Update()
    {
        gameObject.SetActive(SceneManager.GetActiveScene().name != "WaitingArea");
    }
}