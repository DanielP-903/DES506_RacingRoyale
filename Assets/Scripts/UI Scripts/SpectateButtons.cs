using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// Functionality for Spectate Buttons
/// </summary>
/// <returns></returns>
public class SpectateButtons : MonoBehaviour
{
    private GameManager gm;
    /// <summary>
    /// Establish UI Elements on Start
    /// </summary>
    /// <returns></returns>
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.SetSpectateMenu(this.gameObject);
        //this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Spectate Next Target
    /// </summary>
    /// <returns></returns>
    public void SpectateNext()
    {
        gm.ChangeSpectateTarget(true);
    }
    
    /// <summary>
    /// Spectate Previous Target
    /// </summary>
    /// <returns></returns>
    public void SpectatePrevious()
    {
        gm.ChangeSpectateTarget(false);
    }
}
