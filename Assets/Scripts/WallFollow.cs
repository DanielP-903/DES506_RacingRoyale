using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class WallFollow : MonoBehaviour
{
    public GameObject path;

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
        //Debug.Log("_tValueMax = " + _tValueMax);
    }

    public float GetStartDelayTimer()
    {
        return _startDelayTimer;
    }
    
    // Update is called once per frame
    void Update()
    {
        
        // _wallOMeterSlider.value = Vector3.Lerp(_wallOMeterPlayer.transform.position, endPos, (transform.position-startPos).magnitude).magnitude;
        // _wallOMeterSlider.value = (transform.position - startPos).magnitude /(endPos - transform.position).magnitude;
        //
        // _wallOMeterSlider.value = Mathf.Lerp(_tValueMax, 0, (_tValueMax - _tValuePersistant)/_tValueMax);

      
        
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
                
                float distanceToStart = Vector3.Distance(_playerRef.transform.position, startPos) - 5.0f;
                float distanceToEnd = Vector3.Distance(_playerRef.transform.position, endPos) - 5.0f;

                float maxDistance = 100.0f;
                Vector3 newValues = _wallOMeterPlayer.transform.position;
                newValues.y = Mathf.Lerp(-100, 100,  (distanceToEnd- distanceToStart)/distanceToEnd);
                Debug.Log("newValues.y = " + newValues.y);
                Debug.Log("distanceToEnd = " + distanceToEnd);
                Debug.Log("distanceToStart = " + distanceToStart);
                _wallOMeterPlayer.transform.position = newValues;
                
                _wallOMeterSlider.value = Mathf.Lerp(_tValueMax, 0, (_tValueMax - _tValuePersistant)/_tValueMax);
            
            }
            
            transform.position = newPosition;

            Vector3 difference = newPosition - oldPosition;

            float rotationAngle = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(90, rotationAngle, 0);

            if (_tValue > 1)
            {
                //Debug.Log("_tValue is > 1");
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
            //startDelayText.text = ((int)_startDelayTimer).ToString();
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
            
            if (player.GetComponent<PhotonView>().IsMine)
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
