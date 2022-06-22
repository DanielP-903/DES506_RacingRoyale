using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUDController : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    public Slider boostSlider;
    
    
    private GameObject _playerRef;
    private CarController _carController;
    private Rigidbody _rigidbodyRef;
    
    private bool hasFoundPlayer = false;

    [HideInInspector] public float currentSpeed = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        hasFoundPlayer = false;
        StartCoroutine(waitTime());
        /*GameObject[] listOfPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in listOfPlayers)
        {
            Debug.Log("Player: " + player);
            if (player.GetComponent<PhotonView>().IsMine)
            {
                _playerRef = player;
                Debug.Log("Player Found");
            }
        }
        //_playerRef = GameObject.FindGameObjectWithTag("Player");
        _carController = _playerRef.GetComponent<CarController>();
        _rigidbodyRef = _carController.GetComponent<Rigidbody>();*/
    }

    // Update is called once per frame
    void Update()
    {
        if (hasFoundPlayer)
        {
            currentSpeed = (Mathf.Round(_rigidbodyRef.velocity.magnitude * 2.2369362912f));
            speedText.text = currentSpeed.ToString();

            boostSlider.value = (_carController.boostCooldown - _carController.GetBoosDelay())/_carController.boostCooldown;
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
        
        //Debug.Log("There is a QUICK FIX at line 65 of PlayerHUDController");
        //_playerRef = listOfPlayers[0];
        
        //_playerRef = GameObject.FindGameObjectWithTag("Player");
        _carController = _playerRef.GetComponent<CarController>();
        _rigidbodyRef = _carController.GetComponent<Rigidbody>();
    }
}
