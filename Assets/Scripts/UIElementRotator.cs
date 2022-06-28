using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class UIElementRotator : MonoBehaviour
{
    private GameObject _playerRef;
    private bool _hasFoundPlayer = false;

    private void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    // Update is called once per frame
    void Update()
    {
        if (_hasFoundPlayer)
        {
            transform.rotation = Quaternion.Euler(0.0f, _playerRef.transform.eulerAngles.y, 0.0f);
        }
    }
    
    IEnumerator WaitForPlayer()
    {
        yield return new WaitForSeconds(1);
        
        GameObject[] listOfPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in listOfPlayers)
        {
            //Debug.Log("Player: " + player);
            if (!player.GetComponent<PhotonView>())
            {
                continue;
            }
            
            if (player.GetComponent<PhotonView>().IsMine)
            {
                _playerRef = player;
                //Debug.Log("Player Found");
                _hasFoundPlayer = true;
            }
        }
    }
}
