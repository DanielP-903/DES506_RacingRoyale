using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerHUDController : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    private GameObject _playerRef;
    private Dan_CarController _carController;
    private Rigidbody _rigidbodyRef;

    [HideInInspector] public float currentSpeed = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        _playerRef = GameObject.FindGameObjectWithTag("Player");
        _carController = _playerRef.GetComponent<Dan_CarController>();
        _rigidbodyRef = _carController.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        currentSpeed = (Mathf.Round(_rigidbodyRef.velocity.magnitude * 2.2369362912f));
        speedText.text = "Speed: " + currentSpeed + " MPH";
    }
}
