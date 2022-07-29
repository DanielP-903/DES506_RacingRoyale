using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Player_Scripts;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class WallFollow : MonoBehaviour
{
    public GameObject path;

    public float playerBarStartPos;
    public float playerBarOffset;
    public float startDelay = 3.0f;
    public float chaseSpeed = 1.0f;
    public TextMeshProUGUI startDelayText;
    private GameObject _wallOMeter;
    private BezierCurveGenerator _bezierCurveGenerator;

    private Slider _wallOMeterSlider;
    private GameObject _wallOMeterPlayer;

    private float _startDelayTimer = 0.0f;
    
    private int routeNumber = 0;
    private Vector3 endPos;
    private Vector3 startPos;

    private float _tValue;
    private float _tValuePersistant;
    private float _tValueMax;

    private bool _begin = false;
    
    private GameObject _playerRef;
    private CarController _carController;
    private Rigidbody _rigidbodyRef;
    
    private bool hasFoundPlayer = false;
    private CheckpointSystem _checkpointSystem;
    
    [Header("Platform Dissolving Functionality")]
    [Tooltip("How much time it takes before a platform starts dissolving (in seconds)")]
    public float timeUntilDissolve = 10;
    [Tooltip("How much time it takes for a platform to fully dissolve (in seconds)")]
    public float dissolveTime = 3;    
    [Tooltip("Value of dissolveTime which destroys the collider")]
    [Range(0.0f, 1.0f)]
    public float colliderDissolveThreshold = 0.5f;
    
    // Start is called before the first frame update
    void Start()
    {

        hasFoundPlayer = false;
        StartCoroutine(waitTime());
        _bezierCurveGenerator = path.GetComponent<BezierCurveGenerator>();
        _checkpointSystem = GameObject.FindGameObjectWithTag("CheckpointSystem").GetComponent<CheckpointSystem>();
        routeNumber = 0;
        _tValue = 0.0f;
        _startDelayTimer = startDelay;
        _begin = false;
        
        _wallOMeter = GameObject.FindGameObjectWithTag("MainCanvas").transform.GetChild(12).gameObject;
        _wallOMeterSlider = _wallOMeter.GetComponent<Slider>();
        _wallOMeterPlayer = _wallOMeter.transform.GetChild(2).gameObject;

        endPos = _bezierCurveGenerator.controlPoints[_bezierCurveGenerator.controlPoints.Count - 1].transform.position;
        startPos = _bezierCurveGenerator.controlPoints[0].transform.position;

        _tValueMax = Mathf.Floor(_bezierCurveGenerator.controlPoints.Count / 3);
    }

    public float GetStartDelayTimer()
    {
        return _startDelayTimer;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (hasFoundPlayer && _playerRef)
        {
            float distanceToStart = Vector3.Distance(_playerRef.transform.position, startPos) - 5.0f;
            float distanceToEnd = Vector3.Distance(_playerRef.transform.position, endPos) - 5.0f;
            float distance = Vector3.Distance(startPos, endPos) - 5.0f;

            Vector3 newValues = _wallOMeterPlayer.transform.localPosition;
            var t = (distance- distanceToStart)/distance;
            t = Mathf.Clamp(t, 0, 1);
            newValues.y = Mathf.Lerp(playerBarStartPos + playerBarOffset, playerBarStartPos, t);
            
            _wallOMeterPlayer.transform.localPosition = newValues;
        }

        if (_begin)
        {
            _tValue += Time.deltaTime * chaseSpeed;
            _tValuePersistant += Time.deltaTime * chaseSpeed;
            Vector3 oldPosition = transform.position;

            var newPosition = Mathf.Pow(1 - _tValue, 3) * _bezierCurveGenerator.controlPoints[routeNumber].position +
                              3 * Mathf.Pow(1 - _tValue, 2) * _tValue * _bezierCurveGenerator.controlPoints[routeNumber + 1].position +
                              3 * (1 - _tValue) * Mathf.Pow(_tValue, 2) * _bezierCurveGenerator.controlPoints[routeNumber + 2].position +
                              Mathf.Pow(_tValue, 3) * _bezierCurveGenerator.controlPoints[routeNumber + 3].position;
            
            if (hasFoundPlayer && _playerRef)
            {
                _wallOMeterSlider.value = Mathf.Lerp(_tValueMax, 0, (_tValueMax - _tValuePersistant)/_tValueMax);
            }
            
            transform.position = newPosition;

            Vector3 difference = newPosition - oldPosition;

            float rotationAngle = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;
            
            transform.rotation = Quaternion.Euler(0, rotationAngle, 0);

            if (_tValue > 1)
            {
                _tValue = 0.0f;
                routeNumber += 3;
                if (routeNumber >= _bezierCurveGenerator.controlPoints.Count - 1)
                {
                    routeNumber = 0;
                    _tValuePersistant = 0.0f;
                }
            }
        }
        else
        {
            _startDelayTimer = _startDelayTimer <= 0 ? 0 : _startDelayTimer - Time.deltaTime;
            if (_startDelayTimer < 1)
            {
                StartCoroutine(RemoveDelayText());
                _startDelayTimer = startDelay;
                _begin = true;
            }
        }

    }

    private IEnumerator RemoveDelayText()
    {
        //startDelayText.text = "GO";
        yield return new WaitForSeconds(2);
        //startDelayText.text = "";
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
            
            if (player.GetComponent<PhotonView>().IsMine && !player.GetComponent<CarController>().bot)
            {
                _playerRef = player;
                //Debug.Log("Player Found");
                hasFoundPlayer = true;
            }
        }
        
        _carController = _playerRef.GetComponent<CarController>();
        _rigidbodyRef = _carController.GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Wall Hit: "+other.gameObject.name+":"+other.tag);
        if (other.CompareTag("Checkpoint"))
        {
            Debug.Log("Hit Checkpoint");
            _checkpointSystem.EliminateCheckpoint(other.gameObject);
        }
    }
}
