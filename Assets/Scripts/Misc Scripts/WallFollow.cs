using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles how the wall/monster moves through the level
/// </summary>
public class WallFollow : MonoBehaviour
{
    [SerializeField] private GameObject path;
    [SerializeField] private float playerBarStartPos;
    [SerializeField] private float playerBarOffset;
    [SerializeField] private float startDelay = 3.0f;
    [SerializeField] private float chaseSpeed = 1.0f;
    
    private GameObject _wallOMeter;
    private BezierCurveGenerator _bezierCurveGenerator;
    private Slider _wallOMeterSlider;
    private GameObject _wallOMeterPlayer;
    private float _startDelayTimer;
    private int _routeNumber;
    private Vector3 _endPos;
    private Vector3 _startPos;
    private float _tValue;
    private float _tValuePersistant;
    private float _tValueMax;
    private bool _begin;
    private GameObject _playerRef;
    private CarController _carController;
    private bool _hasFoundPlayer;
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

        _hasFoundPlayer = false;
        StartCoroutine(WaitTime());
        _bezierCurveGenerator = path.GetComponent<BezierCurveGenerator>();
        _checkpointSystem = GameObject.FindGameObjectWithTag("CheckpointSystem").GetComponent<CheckpointSystem>();
        _routeNumber = 0;
        _tValue = 0.0f;
        _startDelayTimer = startDelay;
        _begin = false;
        
        _wallOMeter = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Wallometer").gameObject;
        _wallOMeterSlider = _wallOMeter.GetComponent<Slider>();
        _wallOMeterPlayer = _wallOMeter.transform.GetChild(2).gameObject;

        _endPos = _bezierCurveGenerator.controlPoints[_bezierCurveGenerator.controlPoints.Count - 1].transform.position;
        _startPos = _bezierCurveGenerator.controlPoints[0].transform.position;

        _tValueMax = Mathf.Floor(_bezierCurveGenerator.controlPoints.Count / 3);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (_hasFoundPlayer && _playerRef)
        {
            float distanceToStart = Vector3.Distance(_playerRef.transform.position, _startPos) - 5.0f;
            float distanceToEnd = Vector3.Distance(_playerRef.transform.position, _endPos) - 5.0f;
            float distance = Vector3.Distance(_startPos, _endPos) - 5.0f;

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

            var newPosition = Mathf.Pow(1 - _tValue, 3) * _bezierCurveGenerator.controlPoints[_routeNumber].position +
                              3 * Mathf.Pow(1 - _tValue, 2) * _tValue * _bezierCurveGenerator.controlPoints[_routeNumber + 1].position +
                              3 * (1 - _tValue) * Mathf.Pow(_tValue, 2) * _bezierCurveGenerator.controlPoints[_routeNumber + 2].position +
                              Mathf.Pow(_tValue, 3) * _bezierCurveGenerator.controlPoints[_routeNumber + 3].position;
            
            if (_hasFoundPlayer && _playerRef)
            {
                _wallOMeterSlider.value = Mathf.Lerp(1, 0, (_tValueMax - _tValuePersistant) / _tValueMax);
            }
            
            transform.position = newPosition;

            Vector3 difference = newPosition - oldPosition;

            float rotationAngle = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;
            
            transform.rotation = Quaternion.Euler(0, rotationAngle, 0);

            if (_tValue > 1)
            {
                _tValue = 0.0f;
                _routeNumber += 3;
                if (_routeNumber >= _bezierCurveGenerator.controlPoints.Count - 1)
                {
                    _routeNumber = 0;
                    _tValuePersistant = 0.0f;
                }
            }
        }
        else
        {
            _startDelayTimer = _startDelayTimer <= 0 ? 0 : _startDelayTimer - Time.deltaTime; 

            int serverTimer = 0;
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Timer" + GameObject.Find("GameManager").GetComponent<GameManager>().GetStageNum()))
            {
                serverTimer = (int)PhotonNetwork.CurrentRoom.CustomProperties[("Timer" + GameObject.Find("GameManager").GetComponent<GameManager>().GetStageNum())];
            }
            
            if (serverTimer != 0 && PhotonNetwork.ServerTimestamp - serverTimer > startDelay * 1000)
            {
                _startDelayTimer = startDelay;
                _begin = true;
            }
        }
    }
    
    /// <summary>
    /// Wait for player to spawn then get a reference
    /// </summary>
    IEnumerator WaitTime()
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
                _carController = _playerRef.GetComponent<CarController>();
                _carController.GetComponent<Rigidbody>();
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Destroy a checkpoint when collided
        if (other.CompareTag("Checkpoint"))
        {
            _checkpointSystem.EliminateCheckpoint(other.gameObject);
        }
    }
}
