using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    private GameObject _playerRef;
    private bool hasFoundPlayer = false;
    private CarController _carController;

    public float yOffset = 20;
    // Start is called before the first frame update
    void Start()
    {
        hasFoundPlayer = false;
        StartCoroutine(waitTime());
        GetComponent<Camera>().orthographicSize = yOffset;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasFoundPlayer)
        {
            Vector3 newPos = new Vector3(_playerRef.transform.position.x, _playerRef.transform.position.y, _playerRef.transform.position.z)
                {
                    y = yOffset
                };
            transform.position = newPos;
            transform.rotation = Quaternion.Euler(90.0f, _playerRef.transform.eulerAngles.y, 0.0f);
        }
    }

    IEnumerator waitTime()
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
                hasFoundPlayer = true;
            }
        }
        
        _carController = _playerRef.GetComponent<CarController>();
        _carController.minimapArrow.SetActive(false);

    }
}

