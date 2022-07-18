using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TimerCube : MonoBehaviour
{
    [SerializeField] private List<Material> materials = new List<Material>();

    private Material _currentMaterial;

    private PlayerManager _playerRef;
    private bool _hasFoundPlayer;

    private float _currentTimerValue;
    
    // Start is called before the first frame update
    void Start()
    {
        _currentMaterial = GetComponent<MeshRenderer>().materials[1];
        StartCoroutine(WaitForPlayer());
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_hasFoundPlayer && _playerRef)
        {
            if (_playerRef.startDelayText.text != "Go!")
                _currentTimerValue = _playerRef.timer;

            if (_currentTimerValue <= 3 && _currentTimerValue > 2)
            {
                _currentMaterial = materials[0];
                Debug.Log("Changed to material 1");
            }
            else if (_currentTimerValue <= 2 && _currentTimerValue > 1)
            {
                _currentMaterial = materials[1];
                Debug.Log("Changed to material 2");
            }
            else
            {              
                _currentMaterial = materials[2];
                Debug.Log("Changed to material 3");
            }
            
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
            
            if (player.GetComponent<PhotonView>().IsMine && !player.GetComponent<CarController>().bot)
            {
                _playerRef = player.GetComponent<PlayerManager>();
                //Debug.Log("Player Found");
                _hasFoundPlayer = true;
            }
        }
    }
}
