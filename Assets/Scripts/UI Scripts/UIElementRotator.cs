using System.Collections;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// Handles the rotation of minimap ui elements
/// </summary>
public class UIElementRotator : MonoBehaviour
{
    private GameObject _playerRef;
    private bool _hasFoundPlayer;

    private void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    // Update is called once per frame
    void Update()
    {
        if (_hasFoundPlayer && _playerRef)
        {
            transform.rotation = Quaternion.Euler(0.0f, _playerRef.transform.eulerAngles.y, 0.0f);
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
            }
        }
    }
}
