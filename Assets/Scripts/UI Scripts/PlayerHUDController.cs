using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Player hud element functionality
/// </summary>
public class PlayerHUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Slider boostSlider;
    private bool _hasFoundPlayer = false;
    private GameObject _playerRef;
    private CarController _carController;
    private Rigidbody _rigidbodyRef;

    [HideInInspector] public float currentSpeed = 0.0f;
    
    // Start is called before the first frame update
    void Start()
    {
        _hasFoundPlayer = false;
        StartCoroutine(waitTime());
    }

    // Update is called once per frame
    void Update()
    {
        if (_hasFoundPlayer && _playerRef)
        {
            // Update MPH text
            currentSpeed = (Mathf.Round(_rigidbodyRef.velocity.magnitude * 2.2369362912f));
            speedText.text = currentSpeed.ToString();

            // Update boost meter
            if (_carController.GetNoOfBoostsLeft() == 0)
            {
                boostSlider.value = 0;
            }
            else
            {
                boostSlider.value = (_carController.boostCooldown - _carController.GetBoostDelay())/_carController.boostCooldown;
            }
        }
        
    }
    
    /// <summary>
    /// Wait for player to spawn then get a reference
    /// </summary>
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
            
            if (player.GetComponent<PhotonView>().IsMine && !player.GetComponent<CarController>().bot)
            {
                _playerRef = player;
                //Debug.Log("Player Found");
                _hasFoundPlayer = true;
            }
        }
        
        //Debug.Log("There is a QUICK FIX at line 65 of PlayerHUDController");
        //_playerRef = listOfPlayers[0];
        
        //_playerRef = GameObject.FindGameObjectWithTag("Player");
        _carController = _playerRef.GetComponent<CarController>();
        _rigidbodyRef = _carController.GetComponent<Rigidbody>();
    }
}
