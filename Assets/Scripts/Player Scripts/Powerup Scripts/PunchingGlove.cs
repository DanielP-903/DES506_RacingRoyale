using System.Collections;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// Handles the punching glove individually
/// </summary>
public class PunchingGlove : MonoBehaviour
{
    private GameObject _playerRef;
    private PlayerPowerups _playerPowerups;
    private bool _hasFoundPlayer;

    private void Start()
    {
        _hasFoundPlayer = false;
        StartCoroutine(WaitForPlayer());
    }

    private void OnTriggerEnter(Collider other)
    {
        // Reset glove on collision
        if (!other.CompareTag("Player") && other.gameObject.layer != 7 && _hasFoundPlayer) // Player layer
        {
            _playerPowerups.ResetPunch();
        }
    }
    
    /// <summary>
    /// Wait for player to spawn then get a reference
    /// </summary>
    IEnumerator WaitForPlayer()
    {
        yield return new WaitForSeconds(1);
    
        GameObject[] listOfPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in listOfPlayers)
        {
            if (!player.GetComponent<PhotonView>())
            {
                continue;
            }
        
            if (player.GetComponent<PhotonView>().IsMine && !player.GetComponent<CarController>().bot)
            {
                _playerRef = player;
                _hasFoundPlayer = true;
                _playerPowerups = _playerRef.GetComponent<PlayerPowerups>();
            }
        }
    }
}

