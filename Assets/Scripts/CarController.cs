using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum PowerupType
{
    None, Superboost, BouncyWallShield, AirBlast, GrapplingHook, PunchingGlove, WarpPortal
}

public class CarController : MonoBehaviour
{
    [Serializable]
    public class Axle
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public bool motor;
        public bool steering;
    }
    
    #region Editable-Vars
    
    [Header("Car Driving Physics")] 
    [Tooltip("Object to represent transform of centre of mass")]
    public GameObject centreOfMass;
    [Tooltip("Amount of force applied to the motor wheels")]
    public float motorForce;
    [Tooltip("Braking force")]
    public float brakeTorque = 1000;
    [Tooltip("Maximum angle for steering")]
    public float maxSteeringAngle;
    [Tooltip("Maximum velocity for accelerating")]
    public float terminalVelocity = 120;
    [Tooltip("List of axles - DO NOT DELETE")]
    public List<Axle> axles;
    [Tooltip("Physical wheel turning speed")]
    public float wheelTurningSpeed = 50;
    [Tooltip("Multiplier for forward acceleration")]
    public float forwardMultiplier = 1;
    [Tooltip("Multiplier for backward acceleration")]
    public float backwardMultiplier = 0.5f;
    [Tooltip("Idle brake force")]
    public float idleBrakeForce = 8000;  
    [Tooltip("Drift smoke VFX speed threshold")]
    public float driftSmokeThreshold = 20;

    [Header("Forces")] 
    [Tooltip("Jumping force")]
    public float pushForceAmount = 5.0f;
    [Tooltip("Main forward speed force")]
    public float accelerationForce = 5.0f;
    [Tooltip("Boost force")]
    public float boostForceAmount = 5.0f;
    [Tooltip("Drifting force (might not be in use currently)")]
    public float driftForceAmount = 3000.0f;
    [Tooltip("Max number of boosts allowed in the air")]
    public int maxBoostsInAir = 1;
    
    [Header("Environmental Pads")] 
    [Tooltip("Jumping pad force")]
    public float jumpPadForce = 5.0f;
    [Tooltip("Boost pad force")]
    public float boostPadForce = 5.0f;

    [Header("Collisions")] 
    [Tooltip("Collision force applied when bouncing off of players and objects")]
    public float bounciness = 100.0f;

    [Header("Cooldowns (in seconds)")] 
    [Tooltip("Jumping cooldown time")]
    public float jumpCooldown = 2.0f;
    [Tooltip("Boosting cooldown time")]
    public float boostCooldown = 2.0f;
    [Tooltip("Reset cooldown time")]
    public float resetCooldown = 2.0f;
    [Tooltip("Jump pad cooldown time")]
    public float padCooldown = 2.0f;

    [Header("Other")]
    public GameObject minimapArrow;
    public CheckpointSystem checkpoints;
    public Camera flybyCam;
    public AudioManager audioManager;
    public float flybyDelay = 0.01f;

    [Header("DEBUG MODE")] 
    public bool debug;
    
    [Header("BOT MODE")] 
    public bool bot;
    
    #endregion
    
    #region Bools
    
    private bool _moveForward;
    private bool _moveBackward;
    private bool _moveRight;
    private bool _moveLeft;
    private bool _pushUp;
    private bool _drift;
    private bool _boost;
    private bool _airLeft;
    private bool _airRight;
    private bool _airUp;
    private bool _airDown;
    private bool _reset;
    private bool _activatePowerup;
    private bool _grounded;
    private bool _onOil;
    private bool _onOilPreviousFrame;
    private bool _passedFinishLine;
    private bool _hitEliminationZone;
    private bool _hitDetect;
    private bool _lookBehind;

    #endregion

    #region Floats+Vectors

    private float _currentSteeringMulti;
    private float _currentSteeringAngle;
    private float _pushDelay = 2.0f;
    private float _boostDelay = 2.0f;
    private float _resetDelay = 2.0f;
    private float _padDelay = 2.0f;
    private float _airTime = 0.0f;
    private float _animCamTime = 0.0f;
    private Vector3 _savedOilVelocity;
    
    #endregion
    
    #region Component-References

    private PlayerManager _playerManager;
    private Rigidbody _rigidbody;
    private PlayerPowerups _playerPowerups;
    private BotCarController _botCarController;
    private CinemachineVirtualCamera _virtualCamera;
    private CinemachineTransposer _transposer;
    private CinemachineImpulseSource _impulseSource;
    private GameManager _gm;
    private PhotonView _photonView;

    #endregion

    #region Misc

    private RaycastHit _hit;
    private Dictionary<GameObject, bool> _passedCheckpoints = new Dictionary<GameObject, bool>();
    private Transform _currentRespawnPoint;
    private Camera _mainCam;
    private CarVFXHandler _vfxHandler;
    private bool _delayAirTime;
    private int _boostsInAirLeft = 1;
    private CameraFlyBy _cameraFlyBy;
    private PauseMenu _pm;

    #endregion
   
    #region Initialisation

        private void Start()
        {
            if (debug)
            {
                Debug.Log("DEBUG MODE IS ACTIVE! (CarController)");
            }

            if (bot)
            {
                _botCarController = GetComponent<BotCarController>();
            }
            
            _passedFinishLine = false;
            _pushDelay = jumpCooldown;
            _boostDelay = boostCooldown;
            _rigidbody = GetComponent<Rigidbody>();
            _playerManager = GetComponent<PlayerManager>();
            _playerPowerups = GetComponent<PlayerPowerups>();
            _vfxHandler = GetComponent<CarVFXHandler>();
            GetComponent<Animator>();
            _impulseSource = GetComponent<CinemachineImpulseSource>();
            
            foreach (var axle in axles)
            {
                axle.leftWheel.brakeTorque = brakeTorque;
                axle.rightWheel.brakeTorque = brakeTorque;
            }
    
            _rigidbody.centerOfMass = centreOfMass.transform.localPosition;
            if (!debug)
            {
                GameObject icon = GameObject.Find("Powerup Icon");
                if (icon)
                {
                    _playerPowerups.powerupIcon = icon.gameObject.GetComponent<Image>();
                    _playerPowerups.powerupIcon.gameObject.SetActive(false);
                }
            }
            
            if (debug)
            {
                GameObject checkpointObject = GameObject.Find("CheckpointSystem");
                checkpointObject.GetComponent<CheckpointSystem>();

                if (checkpointObject != null)
                {
                    checkpoints = checkpointObject.GetComponent<CheckpointSystem>();
                    _passedCheckpoints.Clear();
                    foreach (var checkpoint in checkpoints.checkpointObjects)
                    {
                        _passedCheckpoints.Add(checkpoint, false);
                        Debug.Log(_passedCheckpoints[checkpoint] + " : " + checkpoint);
                    }
                }
                else
                {
                    Debug.Log("Error: no CheckpointSystem object in scene");
                }

            }
    
            _pm = GameObject.Find("PauseMenu").GetComponent<PauseMenu>();
            var mainCameraObject = GameObject.Find("PlayerCamera");
            if (mainCameraObject)
            {
                _mainCam = mainCameraObject.GetComponent<Camera>();
                _virtualCamera = _mainCam.GetComponent<CinemachineVirtualCamera>();
                _transposer = _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            }

            if (!debug)
            {
                _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
                _photonView = GetComponent<PhotonView>();
            }

        }
    
        void OnLevelWasLoaded()
        {
            //SetUp();
        }

        public void SetUp()
        {
            if (debug)
            {
                Debug.Log("DEBUG MODE IS ACTIVE! (CarController)");
            }

            _passedFinishLine = false;
            GameObject checkpointObject = GameObject.Find("CheckpointSystem");
            if (SceneManager.GetActiveScene().name == "Stage1" || SceneManager.GetActiveScene().name == "Stage2")
            _playerPowerups.powerupIcon = GameObject.FindGameObjectWithTag("PowerupIcon").gameObject.GetComponent<Image>();
            _playerPowerups.powerupIcon.gameObject.SetActive(false);
            _pm = GameObject.Find("PauseMenu").GetComponent<PauseMenu>();
            if (checkpointObject != null)
            {
                checkpoints = checkpointObject.GetComponent<CheckpointSystem>();
                foreach (var checkpoint in checkpoints.checkpointObjects)
                {
                    _passedCheckpoints.Add(checkpoint, false);
                }
            }
            else
            {
                Debug.Log("Error: no CheckpointSystem object in scene");
            }
            _mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                            
            GameObject flybyCamObject = GameObject.FindGameObjectWithTag("FlyBy");
            
            if (!flybyCamObject) return;
            
            flybyCamObject.GetComponent<Camera>();

            if (flybyCamObject != null)
            {
                flybyCam = flybyCamObject.GetComponent<Camera>();
            }
            else
            {
                Debug.Log("Error: no Flyby Camera object in scene");
            }
            
            _cameraFlyBy = flybyCam.GetComponent<CameraFlyBy>();

            StartCoroutine(DelayFlyBy());
            
            _gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        }

    #endregion

    #region Main-Physics

    public void FixedUpdate()
    {
        PhysUpdateDriving();
        PhysUpdateForces();
        PhysUpdateAirControl();
        PhysUpdateAcceleration();
        PhysRestrictVelocities();
    }

    private void PhysRestrictVelocities()
    {
        Vector3 newValues = new Vector3(_rigidbody.angularVelocity.x,_rigidbody.angularVelocity.y,_rigidbody.angularVelocity.z);
        
        if (!_grounded)
        {
            newValues.x = Mathf.Clamp(newValues.x, -1, 1);
            newValues.y = Mathf.Clamp(newValues.y, -3, 3);
            newValues.z = Mathf.Clamp(newValues.z, -1, 1);
        }
        else
        {
            newValues.x = Mathf.Clamp(newValues.x, -1, 1);
            newValues.y = Mathf.Clamp(newValues.y, -2, 2);
            newValues.z = Mathf.Clamp(newValues.z, -1, 1);
        }
        _rigidbody.angularVelocity = newValues;

        if (!_vfxHandler.boostPlaying)
        {
            Vector3 newLinearValues = _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, _rigidbody.velocity.z);
            newLinearValues.x = Mathf.Clamp(newLinearValues.x, -terminalVelocity / 2.2369362912f, terminalVelocity / 2.2369362912f);
            newLinearValues.y = Mathf.Clamp(newLinearValues.y, -terminalVelocity / 2.2369362912f, terminalVelocity / 2.2369362912f);
            newLinearValues.z = Mathf.Clamp(newLinearValues.z, -terminalVelocity / 2.2369362912f, terminalVelocity / 2.2369362912f);
            _rigidbody.velocity = newLinearValues;
        }

        //float terminalSpeed = (Mathf.Round(_rigidbody.velocity.magnitude ));
    }

    private void PhysUpdateAcceleration()
    {
        _boostDelay = _boostDelay <= 0 ? 0 : _boostDelay - Time.fixedDeltaTime;

        // BOOST FUNCTIONALITY
        if (_boost && _boostDelay <= 0)
        {
            if (!_grounded)
            {
                if ( _boostsInAirLeft <=0) return;
                _boostsInAirLeft--;
            }
            if (!bot) audioManager.PlaySound("SuperBoostLoop");
            _vfxHandler.PlayVFX("BoostEffect");
            _vfxHandler.StartCoroutine(_vfxHandler.ActivateBoostEffect());
            _boostDelay = boostCooldown;
            if (_rigidbody.velocity.magnitude * 2.2369362912f < 0.1f)
            {                
                _rigidbody.velocity = transform.forward * boostForceAmount;
            }
            else
            {
                _rigidbody.AddForce(transform.forward * boostForceAmount, ForceMode.VelocityChange);
            }
        }
    }

    // For updating rigidbody forces acting upon the car
    private void PhysUpdateForces()
    {
        _pushDelay = _pushDelay <= 0 ? 0 : _pushDelay - Time.fixedDeltaTime;
        _padDelay = _padDelay <= 0 ? 0 : _padDelay - Time.fixedDeltaTime;
 
        // if (_pushUp && !_grounded && _pushDelay <= 0.0f)
        // {
        //     _pushDelay = jumpCooldown;
        //     Vector3 push = -Vector3.up + new Vector3(.5f,0,0);
        //     _rigidbody.AddForce(push * (pushForceAmount * 700.0f), ForceMode.Force);
        //     _rigidbody.AddTorque(-transform.right + ((-transform.up + new Vector3(.5f,0,0))  * 10), ForceMode.VelocityChange);
        // }
        //else
        if (_pushUp && _grounded && _pushDelay <= 0.0f)
        {
            _pushDelay = jumpCooldown;
            _rigidbody.AddForce(transform.up * (pushForceAmount * 700.0f), ForceMode.Force);
        }
        
        
        if (_hit.transform != null)
        {
            if (_hit.transform.CompareTag("JumpPad") && _padDelay <= 0)
            {
                _padDelay = padCooldown;
                _rigidbody.AddForce(transform.up * (jumpPadForce * 700.0f * 1.5f), ForceMode.Force);
            }

            if (_hit.transform.CompareTag("BoostPad") && _padDelay <= 0)
            {
                if (!bot) audioManager.PlaySound("SuperBoostLoop");
                _vfxHandler.StartCoroutine(_vfxHandler.ActivateBoostEffect());
                _padDelay = padCooldown;
                if (_rigidbody.velocity.magnitude * 2.2369362912f < 0.1f)
                {
                    _rigidbody.velocity = -_hit.transform.forward * boostPadForce;
                }
                else
                {
                    _rigidbody.AddForce(-_hit.transform.forward * boostPadForce, ForceMode.VelocityChange);
                }
            }
        }

        if (_onOil && !_vfxHandler.boostPlaying && _rigidbody.velocity.magnitude * 2.2369362912f > 30)
        {
            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, new Vector3(_savedOilVelocity.x, _rigidbody.velocity.y, _savedOilVelocity.z), Time.deltaTime* 10);
        }
    }

    private void PhysUpdateAirControl()
    {
        if (_grounded) return;
        
        if (_airDown)   _rigidbody.AddTorque(-transform.right/15, ForceMode.VelocityChange);
        if (_airUp)     _rigidbody.AddTorque(transform.right/15, ForceMode.VelocityChange);
        if (_moveLeft)  _rigidbody.AddTorque(-transform.up/60, ForceMode.VelocityChange);
        if (_moveRight) _rigidbody.AddTorque(transform.up/60, ForceMode.VelocityChange);
        if (_airLeft)   _rigidbody.AddTorque(transform.forward/15, ForceMode.VelocityChange);
        if (_airRight)  _rigidbody.AddTorque(-transform.forward/15, ForceMode.VelocityChange);

        if (!_airLeft && !_airRight && !_airUp && !_airDown)
        {
            _rigidbody.angularVelocity = new Vector3(_rigidbody.angularVelocity.x, _rigidbody.angularVelocity.y, Mathf.Clamp(_rigidbody.angularVelocity.z, -0.1f, 0.1f));
            _rigidbody.angularVelocity = new Vector3(Mathf.Clamp(_rigidbody.angularVelocity.x, -0.1f, 0.1f), _rigidbody.angularVelocity.y, _rigidbody.angularVelocity.z);
        }
    }
    
    // For updating driving physics with wheel colliders
    private void PhysUpdateDriving()
    {
        float motorMultiplier = _moveForward ? forwardMultiplier : _moveBackward ? -backwardMultiplier : 0;
  
        float currentMotorValue = motorForce * motorMultiplier;

        _currentSteeringAngle = Mathf.Lerp(_currentSteeringAngle, _rigidbody.velocity.magnitude * 2.2369362912f > 60 ? 10 : maxSteeringAngle, Time.deltaTime * wheelTurningSpeed);

        if (_moveLeft)
        {
            _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, -1, Time.deltaTime * 1.0f);
        }
        else if (_moveRight)
        {
            _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, 1, Time.deltaTime * 1.0f);
        }
        else
        {
            _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, 0, Time.deltaTime * 50.0f);
        }

        float currentSteeringValue = maxSteeringAngle * _currentSteeringMulti;

        foreach (var axle in axles)
        {
            axle.leftWheel.brakeTorque = brakeTorque;
            axle.rightWheel.brakeTorque = brakeTorque;
            if (axle.steering)
            {
                axle.leftWheel.steerAngle = currentSteeringValue;
                axle.rightWheel.steerAngle = currentSteeringValue;
            }
            if (axle.motor)
            {
                axle.leftWheel.motorTorque = currentMotorValue;
                axle.rightWheel.motorTorque = currentMotorValue;
            }

            if (_drift) // New original friction
            {
                WheelFrictionCurve frictionCurve = axle.leftWheel.forwardFriction;
                axle.leftWheel.forwardFriction  = frictionCurve;
                axle.rightWheel.forwardFriction  = frictionCurve;
            }
            else // Default friction
            {
                WheelFrictionCurve frictionCurve = axle.leftWheel.forwardFriction;
                axle.leftWheel.forwardFriction  = frictionCurve;
                axle.rightWheel.forwardFriction  = frictionCurve;  
            }
            
        }

        if (!_moveForward && !_moveBackward)
        {
            brakeTorque = idleBrakeForce;
        }
        else if (_drift)
        {
            brakeTorque = driftForceAmount;
            axles[0].motor = false; 
        }
        else
        {
            brakeTorque = 0.0f;
            axles[0].motor = true;
        }
        
        if (_moveForward) _rigidbody.AddForce(transform.forward * accelerationForce, ForceMode.Acceleration);
        if (_moveBackward) _rigidbody.AddForce(-transform.forward * accelerationForce, ForceMode.Acceleration);

        if (!_grounded) return;
        
        if (_moveLeft) _rigidbody.AddForce(transform.right * (accelerationForce / 4), ForceMode.Acceleration);
        if (_moveRight) _rigidbody.AddForce(-transform.right * (accelerationForce / 4), ForceMode.Acceleration);
  
        if (_grounded)
        {
            if (_moveLeft || _moveRight)
            {
                if (_rigidbody.velocity.magnitude * 2.2369362912f > driftSmokeThreshold)
                {
                    _vfxHandler.PlayVFX("DriftSmoke");
                }
            }
            else if (_drift)
            {
                _vfxHandler.PlayVFX("DriftSmoke");
            }
            else
            {
                _vfxHandler.StopDriftEffects();
            }
        }
        else
        {
            _vfxHandler.StopDriftEffects();
        }
    }    

    #endregion

    #region Getters

    public float GetBoostDelay()
    {
        return _boostDelay;
    }

    public bool GetGrounded()
    {
        return _grounded;
    }

    public bool GetActivate()
    {
        return _activatePowerup;
    }

    public int GetNoOfBoostsLeft()
    {
        return _boostsInAirLeft;
    }

    public bool GetForward()
    {
        return _moveForward;
    }
    
    public bool GetBackward()
    {
        return _moveBackward;
    }   
    
    public bool GetLeft()
    {
        return _moveLeft;
    }   
    
    public bool GetRight()
    {
        return _moveRight;
    }
    
    #endregion

    private IEnumerator DelayFlyBy()
    {
        _gm.halt = true;
        yield return new WaitForSeconds(flybyDelay);
        _mainCam.gameObject.SetActive(false);
        flybyCam.gameObject.SetActive(true);
        flybyCam.GetComponent<CameraFlyBy>().activateFlyBy = true;
    }
    
    private void Update()
    {
        if (_gm && _gm.halt)
        {
            _rigidbody.velocity = new Vector3(0, 0, 0);
            _gm.SetDelayTimer();
        }

        WheelCollider currentWheel = axles[0].leftWheel;
        float currentTorque = currentWheel.motorTorque;
        float currentBrake = currentWheel.brakeTorque;
        float currentRpm = currentWheel.rpm;
        Debug.DrawRay(currentWheel.transform.position, currentWheel.transform.parent.forward * currentTorque / 100, Color.blue);
        Debug.DrawRay(currentWheel.transform.position, Vector3.up * currentRpm / 100, Color.green);
        Debug.DrawRay(currentWheel.transform.position, -currentWheel.transform.parent.forward * currentBrake / 100, Color.red);
        
        _hitDetect = Physics.BoxCast(transform.position + transform.up, transform.localScale, -transform.up, out _hit, transform.rotation, 1);
        _grounded = _hitDetect;
        
        if (_hit.transform != null)
            _onOil = _hit.transform.CompareTag("OilSlip");

        if (_onOil != _onOilPreviousFrame)
        {
            _savedOilVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            
            if (_savedOilVelocity.magnitude * 2.2369362912f < 30)
            {
                _savedOilVelocity = new Vector3(30 / 2.2369362912f , 0, 30 / 2.2369362912f );
            }
        }
        

        _resetDelay = _resetDelay <= 0 ? 0 : _resetDelay - Time.deltaTime;
        if (_reset && _resetDelay <= 0)
        {
            _resetDelay = resetCooldown;
            ResetPlayer(true);
        }

        _onOilPreviousFrame = _onOil;

        if (!_grounded)
        {
            _airTime += Time.deltaTime;
            _delayAirTime = true;
        }
        else
        {
            _boostsInAirLeft = maxBoostsInAir;

            if (_airTime > 0.5f)
            {
                _vfxHandler.SpawnVFXAtPosition("GroundImpact", transform.position + (transform.forward/2) - (transform.up/1.5f), 2,false);
                if (!bot)
                {
                    _impulseSource.GenerateImpulseAt(transform.position + Vector3.down, new Vector3(0, -_airTime, 0));
                }
                _animCamTime = 1.0f;
                _airTime = 0;
            }
        }

        if (_cameraFlyBy)
        {
            if (!_cameraFlyBy.activateFlyBy)
            {
                flybyCam.gameObject.SetActive(false);
                _mainCam.gameObject.SetActive(true);
                _gm.halt = false;
            }
            else
            {
                flybyCam.gameObject.SetActive(true);
                _mainCam.gameObject.SetActive(false);
                _gm.SetDelayTimer();
            }
        }
    }
    
    public void ResetPlayer(bool pressedButton = false)
    {
        if (!bot && pressedButton) _playerManager.GoToSpawn(true);
        else if (!bot) _playerManager.GoToSpawn();
        else _botCarController.goToSpawn();
    }
    
    void OnDrawGizmos()
    {
        //Check if there has been a hit yet
        if (_hitDetect)
        {
            Gizmos.color = Color.green;
            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay(transform.position, -transform.up * _hit.distance);
            //Draw a cube that extends to where the hit exists
            Gizmos.DrawWireCube(transform.position - transform.up * _hit.distance, transform.localScale);
        }
        //If there hasn't been a hit yet, draw the ray at the maximum distance
        else
        {
            Gizmos.color = Color.red;
            //Draw a Ray forward from GameObject toward the maximum distance
            Gizmos.DrawRay(transform.position+ transform.up, - transform.up * 1);
            //Draw a cube at the maximum distance
            Gizmos.DrawWireCube((transform.position + transform.up) - transform.up * 1, transform.localScale);
        }
        
        Gizmos.color = Color.cyan;
    }

    #region Collisions

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Vector3 direction = collision.contacts[0].point - transform.position;
            if (!bot)
            {
                _rigidbody.velocity = -(direction.normalized * bounciness);
                Debug.Log("HIT ANOTHER PLAYER WITH RIGIDBODY VELOCITY: " + _rigidbody.velocity);
            }
            else
                GetComponent<Rigidbody>().velocity = -(direction.normalized * bounciness);

            if (bot) return;
            _vfxHandler.PlayVFXAtPosition("Impact", collision.contacts[0].point);
            audioManager.PlaySound("CarHit0" + Random.Range(1, 5));
            _photonView.RPC("PlayerHit", RpcTarget.All, collision.transform.GetComponent<PhotonView>().ViewID, direction, collision.contacts[0].point, bounciness);
        }
        else if (collision.transform.CompareTag("SpinningTop"))
        {
            Vector3 direction = collision.contacts[0].point - transform.position;
            _rigidbody.velocity = -(direction.normalized * 30);
            _vfxHandler.PlayVFXAtPosition("SoftImpact", collision.contacts[0].point);
            int rand = Random.Range(1, 5);
            if (!bot) audioManager.PlaySound("CarHit0" + rand);
        }
        else if (collision.contacts[0].point.y > transform.position.y - 4.0f)
        {
            Vector3 direction = collision.contacts[0].point - transform.position;
            _rigidbody.velocity = -(direction.normalized * (bounciness/3));
            _vfxHandler.PlayVFXAtPosition("SoftImpact", collision.contacts[0].point);
            int rand = Random.Range(1, 5);
            if (!bot) audioManager.PlaySound("CarHit0" + rand);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("FinishLine") && !_passedFinishLine && !bot)
        {
            _vfxHandler.PlayVFX("Confetti");
            // Passed finish line
            Debug.Log("Passed finish line!");
            _passedFinishLine = true;
            _playerManager.CompleteStage();
        }
        
        if (other.transform.CompareTag("Checkpoint") && _passedCheckpoints.ContainsKey(other.gameObject) && !_passedCheckpoints[other.gameObject])
        {
            _vfxHandler.PlayVFX("Confetti");
            _passedCheckpoints[other.gameObject] = true;
            _currentRespawnPoint = other.gameObject.transform;
            int playerNo = !bot ? _playerManager.GetPlayerNumber() : _botCarController.GetBotNumber();
            GameObject newSpawnLocation = other.gameObject.transform.GetChild(playerNo).gameObject;
            Debug.Log("Checkpoint passed: " + other.gameObject.name + " , " + newSpawnLocation + " , " + _currentRespawnPoint.name + " , " + (!bot ? _playerManager.GetPlayerNumber() : _botCarController.GetBotNumber()));
            if (!bot) _playerManager.ChangeSpawnLocation(newSpawnLocation.transform);
            else _botCarController.setSpawn(newSpawnLocation.transform);
        }
        
        if (other.transform.CompareTag("EliminationZone") && !_hitEliminationZone)
        {
            // Hit elimination wall
            Debug.Log("Hit the Elimination Wall");
            //_hitEliminationZone = true; 
            //if (!bot) _playerManager.EliminateCurrentPlayer();
            //if (!bot) audioManager.PlaySound("CarEliminatedByWall");
        }

        if (other.transform.CompareTag("ResetZone"))
        {
            ResetPlayer();
            if (!bot) audioManager.PlaySound("CarEliminatedOffTrack");
        }
    }

    #endregion

    #region Input-Detection

    // Input Actions
    // W
    public void Forward(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _moveForward = value > 0;
        //Debug.Log("Forward detected");
    }
    // S
    public void Backward(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _moveBackward = value > 0;
        //Debug.Log("Backward detected");
    }
    // A
    public void Left(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _moveLeft = value > 0;
        //Debug.Log("Left detected");
    }
    // D
    public void Right(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _moveRight = value > 0;
        //Debug.Log("Right detected");
    }
    // Space
    public void Space(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _pushUp = value > 0;
        //Debug.Log("Space detected");
    }
    // Drift
    public void Drift(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _drift = value > 0;
        //Debug.Log("Drift detected");
    }
    // Boost
    public void Boost(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _boost = value > 0;
        //Debug.Log("Boost detected");
    }
    // Roll Left
    public void AirLeft(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _airLeft = value > 0;
        //Debug.Log("Roll Left detected");
    }
    // Roll Right
    public void AirRight(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _airRight = value > 0;
        //Debug.Log("Roll Right detected");
    }
    // Pitch Up
    public void AirUp(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _airUp = value > 0;
        //Debug.Log("Roll Left detected");
    }
    // Pitch Down
    public void AirDown(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _airDown = value > 0;
        //Debug.Log("Roll Right detected");
    }
    // Reset
    public void Reset(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _reset = value > 0;
        //Debug.Log("Reset detected");
    }
    // Activate Power
    public void ActivatePowerup(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _activatePowerup = value > 0;
        //Debug.Log("Activate Powerup detected");
    }
    // Escape
    public void Escape(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _pm.SetEscape(value > 0);
        //Debug.Log("Escape detected");
    }
    // Rearview
    public void Rearview(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        //_lookBehind = value > 0;
        RearviewCamera(value > 0);
        //Debug.Log("Escape detected");
    }

    private void RearviewCamera(bool backCam)
    {
        if (_virtualCamera == null) return;

        _lookBehind = backCam;
        if (_lookBehind)
        {
            Debug.Log("Behind");
            _transposer.m_FollowOffset = new Vector3(0, 5, 8);
        }
        else
        {
            Debug.Log("Forward");
            _transposer.m_FollowOffset = new Vector3(0, 5, -8);
        }

        Debug.Log("CamPos: " +_transposer.m_FollowOffset+ " - Transposer:"+_transposer.gameObject);
    }

    #endregion
    
    #region AI-Input
    
    //AIONLY
    public void BotForward()
    {
        _moveForward = true;
        //Debug.Log("Forward detected");
    }
    // S
    public void BotBackward()
    {
        _moveBackward = true;
        //Debug.Log("Backward detected");
    }
    // A
    public void BotLeft()
    {
        _moveLeft = true;
        //Debug.Log("Left detected");
    }
    // D
    public void BotRight()
    {
        _moveRight = true;
        //Debug.Log("Right detected");
    }
    
    public void BotNotForward()
    {
        _moveForward = false;
        //Debug.Log("Forward detected");
    }
    // S
    public void BotNotBackward()
    {
        _moveBackward = false;
        //Debug.Log("Backward detected");
    }
    // A
    public void BotNotLeft()
    {
        _moveLeft = false;
        //Debug.Log("Left detected");
    }
    // D
    public void BotNotRight()
    {
        _moveRight = false;
        //Debug.Log("Right detected");
    }
    // Space
    public void BotSpace()
    {
        _pushUp = true;
        //Debug.Log("Space detected");
    }
    public void BotNotSpace()
    {
        _pushUp = false;
        //Debug.Log("Space detected");
    }
    
    public void BotBoost()
    {
        _boost = true;
        //Debug.Log("Boost detected");
    }
    public void BotNotBoost()
    {
        _boost = false;
        //Debug.Log("Boost detected");
    }
    
    #endregion
}