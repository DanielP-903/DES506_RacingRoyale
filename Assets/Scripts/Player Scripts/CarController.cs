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

/// <summary>
/// Defines the specific types of powerups by name
/// </summary>
public enum PowerupType
{
    None, Superboost, BouncyWallShield, AirBlast, GrapplingHook, PunchingGlove, WarpPortal
}

/// <summary>
/// Handles all player input and control of the car
/// </summary>
public class CarController : MonoBehaviour
{
    /// <summary>
    /// Represents each axle on the car and their properties
    /// </summary>
    [Serializable]
    public class Axle
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public bool motor;
        public bool steering;
        public bool isFrontWheel;
    }

    #region Editable-Vars

    [Header("Car Driving Physics")] 
    [Tooltip("Object to represent transform of centre of mass")]
    [SerializeField] private GameObject centreOfMass;

    [Tooltip("Amount of force applied to the motor wheels")]
    [SerializeField] private float motorForce;

    [Tooltip("Braking force")] 
    [SerializeField] private float brakeTorque = 1000;

    [Tooltip("Maximum angle for steering")]
    [SerializeField] private float maxSteeringAngle;

    [Tooltip("Maximum angle for back wheel steering")]
    [SerializeField] private float maxBackWheelSteeringAngle;

    [Tooltip("Maximum velocity for accelerating")]
    [SerializeField] private float terminalVelocity = 120;

    [Tooltip("List of axles - DO NOT DELETE")]
    [SerializeField] private List<Axle> axles;

    [Tooltip("Physical wheel reset to idle speed")]
    [SerializeField] private float wheelResetSpeed = 50;

    [Tooltip("Physical wheel turning speed")]
    [SerializeField] private float wheelTurningSpeed = 1;

    [Tooltip("Maximum turn percentage")] 
    [SerializeField] private float maxTurnAmount = 0.3f;

    [Tooltip("Multiplier for forward acceleration")]
    [SerializeField] private float forwardMultiplier = 1;

    [Tooltip("Multiplier for backward acceleration")]
    [SerializeField] private float backwardMultiplier = 0.5f;

    [Tooltip("Idle brake force")]
    [SerializeField] private float idleBrakeForce = 8000;

    [Tooltip("Drift smoke VFX speed threshold")]
    [SerializeField] private float driftSmokeThreshold = 20;

    [Tooltip("Max forward turning angle in the air")]
    [SerializeField] private float maxAirTurnAngle = 180;    
    
    [Header("----------------------------")]     
    [Header("Air Turning Settings")]     
    [Header("----------------------------")]     
    [Tooltip("Sideways move force")]
    [SerializeField] private float sidewaysMoveForce = 100;
    
    [Tooltip("Sideways turn force (YAW)")]
    [SerializeField] private float sidewaysTurnForceYaw = 200;
    
    [Tooltip("Sideways turn force (PITCH)")]
    [SerializeField] private float sidewaysTurnForcePitch = 15;
    
    [Tooltip("Sideways turn force (ROLL)")]
    [SerializeField] private float sidewaysTurnForceRoll = 15;

    [Header("Forces")] 
    [Tooltip("Jumping force")]
    public float pushForceAmount = 5.0f;

    [Tooltip("Main forward speed force")] 
    [SerializeField] private float accelerationForce = 5.0f;
    
    [Tooltip("Boost force")] 
    [SerializeField] private float boostForceAmount = 5.0f;

    [Tooltip("Drifting force (might not be in use currently)")]
    [SerializeField] private float driftForceAmount = 3000.0f;

    [Tooltip("Max number of boosts allowed in the air")]
    [SerializeField] private int maxBoostsInAir = 1;

    [Header("Environmental Pads")] 
    [Tooltip("Jumping pad force")]
    [SerializeField] private float jumpPadForce = 5.0f;

    [Tooltip("Boost pad force")] 
    [SerializeField] private float boostPadForce = 5.0f;

    [Header("Collisions")] 
    [Tooltip("Collision force applied when bouncing off of players and objects")]
    [SerializeField] private float bounciness = 100.0f;

    [Header("Cooldowns (in seconds)")] 
    [Tooltip("Jumping cooldown time")]
    [SerializeField] private float jumpCooldown = 2.0f;

    [Tooltip("Boosting cooldown time")]
    public float boostCooldown = 2.0f;
     
    [Tooltip("Reset cooldown time")] 
    [SerializeField] private float resetCooldown = 2.0f;
    
    [Tooltip("Jump pad cooldown time")] 
    [SerializeField] private float padCooldown = 2.0f;

    [Header("Other")]
    public GameObject minimapArrow;
    [SerializeField] private CheckpointSystem checkpoints;
    [SerializeField] private Camera flybyCam;
    [SerializeField] private float flybyDelay = 0.01f;
    public AudioManager audioManager;

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
    private bool _groundedPreviousFrame;
    private bool _onOil;
    private bool _onOilPreviousFrame;
    private bool _passedFinishLine;
    private bool _hitEliminationZone;
    private bool _hitDetect;
    private bool _lookBehind;
    private bool _tutorialEnabled;
    
    #endregion

    #region Numerical

    private int _boostsInAirLeft = 1;
    private float _currentSteeringMulti;
    private float _currentSteeringAngle;
    private float _pushDelay = 2.0f;
    private float _boostDelay = 2.0f;
    private float _resetDelay = 2.0f;
    private float _padDelay = 2.0f;
    private float _airTime;
    private float _turnAmount = 0.3f;
    private float _hornTimer;
    private float _currentAirAngle;
    private const float MphConstant = 2.2369362912f;    // Value for converting velocity in m/s to MPH
    private Vector3 _savedOilVelocity;
    private Vector3 _groundForward;
    
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
    private Camera _mainCam;
    private CarVFXHandler _vfxHandler;
    private CameraFlyBy _cameraFlyBy;
    private PauseMenu _pm;
    private GameObject _tutorialObject;

    #endregion

    #region Initialisation

    // Initialise values and get components to be used
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

        // Initialise brake torque
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
            // Debugging only: Get a checkpoint system within the current level
            
            GameObject checkpointObject = GameObject.Find("CheckpointSystem");
            checkpointObject.GetComponent<CheckpointSystem>();

            if (checkpointObject != null)
            {
                checkpoints = checkpointObject.GetComponent<CheckpointSystem>();
                _passedCheckpoints.Clear();
                foreach (var checkpoint in checkpoints.checkpointObjects)
                {
                    _passedCheckpoints.Add(checkpoint, false);
                }
            }
            else
            {
                Debug.Log("Error: no CheckpointSystem object in scene");
            }

        }

        _pm = GameObject.Find("PauseMenu").GetComponent<PauseMenu>();
        _tutorialObject = GameObject.Find("TutorialMenu");
        var mainCameraObject = GameObject.Find("PlayerCamera");
        if (mainCameraObject)
        {
            _mainCam = mainCameraObject.GetComponent<Camera>();
            _virtualCamera = _mainCam.GetComponent<CinemachineVirtualCamera>();
            _transposer = _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        }

        if (debug) return;
        
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        _photonView = GetComponent<PhotonView>();

    }
    
    /// <summary>
    /// Initialise values and get components to be used (on each level load)
    /// </summary>
    public void SetUp()
    {
        if (debug)
        {
            Debug.Log("DEBUG MODE IS ACTIVE! (CarController)");
        }

        _passedFinishLine = false;
        GameObject checkpointObject = GameObject.Find("CheckpointSystem");
        
        if (SceneManager.GetActiveScene().name == "Stage1" || SceneManager.GetActiveScene().name == "Stage2")
        {
            _playerPowerups.powerupIcon = GameObject.FindGameObjectWithTag("PowerupIcon").gameObject.GetComponent<Image>();
        }

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

        // Execute if flyby is present
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
        // Disable car controls if tutorial menu is present
        if (_tutorialObject)
            _tutorialEnabled = _tutorialObject.activeInHierarchy;

        if (_tutorialEnabled || _pm.transform.GetChild(0).gameObject.activeInHierarchy) return;
        
        // Update basic movement and turning of the car
        PhysUpdateDriving();
        
        // Update forces given to the car via jumping
        PhysUpdateForces();
        
        // Update air control values
        PhysUpdateAirControl();
        
        // Update boost functionality
        PhysUpdateAcceleration();
        
        // Place restrictions on velocities both linear and angular
        PhysRestrictVelocities();
    }
    
    /// <summary>
    /// Restrictions on angular and linear velocities based on state
    /// </summary>
    private void PhysRestrictVelocities()
    {
        // Store current angular velocity
        Vector3 newValues = new Vector3(_rigidbody.angularVelocity.x, _rigidbody.angularVelocity.y, _rigidbody.angularVelocity.z);

        if (!_grounded)
        {
            // In the air
            // Restrict y angular velocity to -/+3
            newValues.y = Mathf.Clamp(newValues.y, -3, 3);
        }
        else
        {
            // On the ground
            // Restrict x and z angular velocity to -/+1 and y to -/+2
            newValues.x = Mathf.Clamp(newValues.x, -1, 1);
            newValues.y = Mathf.Clamp(newValues.y, -2, 2);
            newValues.z = Mathf.Clamp(newValues.z, -1, 1);
        }

        _rigidbody.angularVelocity = newValues;

        // Restrict linear velocity based on terminal velocity value
        if (!_vfxHandler.boostPlaying)
        {
            Vector3 newLinearValues = _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, _rigidbody.velocity.z);
            newLinearValues.x = Mathf.Clamp(newLinearValues.x, -terminalVelocity / MphConstant, terminalVelocity / MphConstant);
            newLinearValues.y = Mathf.Clamp(newLinearValues.y, -terminalVelocity / MphConstant, terminalVelocity / MphConstant);
            newLinearValues.z = Mathf.Clamp(newLinearValues.z, -terminalVelocity / MphConstant, terminalVelocity / MphConstant);
            _rigidbody.velocity = newLinearValues;
        }
    }
    
    /// <summary>
    /// For updating boost functionality
    /// </summary>
    private void PhysUpdateAcceleration()
    {
        _boostDelay = _boostDelay <= 0 ? 0 : _boostDelay - Time.fixedDeltaTime;

        // Boost functionality
        if (_boost && _boostDelay <= 0)
        {
            if (!_grounded)
            {
                if (_boostsInAirLeft <= 0) return;
                _boostsInAirLeft--;
            }

            if (!bot)
            {
                audioManager.PlaySound("SuperBoostLoop");
            }
            
            _vfxHandler.PlayVFX("BoostEffect");
            _vfxHandler.StartCoroutine(_vfxHandler.ActivateBoostEffect());
            
            _boostDelay = boostCooldown;
            
            if (_rigidbody.velocity.magnitude * MphConstant < 0.1f)
            {
                _rigidbody.velocity = transform.forward * boostForceAmount;
            }
            else
            {
                _rigidbody.AddForce(transform.forward * boostForceAmount, ForceMode.VelocityChange);
            }
        }
    }
    
    /// <summary>
    /// For updating rigidbody forces acting upon the car
    /// </summary>
    private void PhysUpdateForces()
    {
        _pushDelay = _pushDelay <= 0 ? 0 : _pushDelay - Time.fixedDeltaTime;
        _padDelay = _padDelay <= 0 ? 0 : _padDelay - Time.fixedDeltaTime;
        
        if (_pushUp && _grounded && _pushDelay <= 0.0f)
        {
            _pushDelay = jumpCooldown;
            _rigidbody.AddForce(transform.up * (pushForceAmount * 700.0f), ForceMode.Force);
        }

        if (_hit.transform != null)
        {
            // Jump Pad
            if (_hit.transform.CompareTag("JumpPad") && _padDelay <= 0)
            {
                _padDelay = padCooldown;
                _rigidbody.AddForce(transform.up * (jumpPadForce * 700.0f * 1.5f), ForceMode.Force);
            }

            // Boost Pad
            if (_hit.transform.CompareTag("BoostPad") && _padDelay <= 0)
            {
                if (!bot) audioManager.PlaySound("SuperBoostLoop");
                _vfxHandler.StartCoroutine(_vfxHandler.ActivateBoostEffect());
                _padDelay = padCooldown;
                if (_rigidbody.velocity.magnitude * MphConstant < 0.1f)
                {
                    _rigidbody.velocity = -_hit.transform.forward * boostPadForce;
                }
                else
                {
                    _rigidbody.AddForce(-_hit.transform.forward * boostPadForce, ForceMode.VelocityChange);
                }
            }
        }

        // Oil slick functionality
        if (_onOil && !_vfxHandler.boostPlaying && _rigidbody.velocity.magnitude * MphConstant > 30)
        {
            var oilVector = new Vector3(_savedOilVelocity.x, _rigidbody.velocity.y, _savedOilVelocity.z);
            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity,oilVector, Time.deltaTime * 10);
        }
    }

    /// <summary>
    /// Air control functionality
    /// </summary>
    private void PhysUpdateAirControl()
    {
        if (_grounded) return;

        // Pitch
        if (_airDown) _rigidbody.AddTorque(-transform.right / sidewaysTurnForcePitch, ForceMode.VelocityChange);
        if (_airUp) _rigidbody.AddTorque(transform.right / sidewaysTurnForcePitch, ForceMode.VelocityChange);
        
        // Lock pitch by angle
        if (Vector3.Angle(transform.forward, _groundForward) > maxAirTurnAngle)
        {
            _rigidbody.angularVelocity = new Vector3(0,_rigidbody.angularVelocity.y, 0);
        }
        
        // Yaw and Roll
        if (_moveLeft) _rigidbody.AddTorque(-transform.up / sidewaysTurnForceYaw, ForceMode.VelocityChange);
        if (_moveRight) _rigidbody.AddTorque(transform.up / sidewaysTurnForceYaw, ForceMode.VelocityChange);
        if (_airLeft) _rigidbody.AddTorque(transform.forward / sidewaysTurnForceRoll, ForceMode.VelocityChange);
        if (_airRight) _rigidbody.AddTorque(-transform.forward / sidewaysTurnForceRoll, ForceMode.VelocityChange);

        // restrict angular velocity to small value on x and z planes
        if (!_airLeft && !_airRight && !_airUp && !_airDown)
        {
            _rigidbody.angularVelocity = new Vector3(_rigidbody.angularVelocity.x, _rigidbody.angularVelocity.y, Mathf.Clamp(_rigidbody.angularVelocity.z, -0.1f, 0.1f));
            _rigidbody.angularVelocity = new Vector3(Mathf.Clamp(_rigidbody.angularVelocity.x, -0.1f, 0.1f), _rigidbody.angularVelocity.y, _rigidbody.angularVelocity.z);
        }
    }
    
    /// <summary>
    /// For updating driving physics with wheel colliders
    /// </summary>
    private void PhysUpdateDriving()
    {
        // Get amount of motor direction scale to apply to the wheels
        float motorMultiplier = _moveForward ? forwardMultiplier : _moveBackward ? -backwardMultiplier : 0;

        // Apply motor force to multiplier
        float currentMotorValue = motorForce * motorMultiplier;

        // Get current steering angle
        _currentSteeringAngle = Mathf.Lerp(_currentSteeringAngle,
            _rigidbody.velocity.magnitude * MphConstant > 60 ? 10 : maxSteeringAngle,
            Time.deltaTime * wheelResetSpeed);

        // Increase turning amount based on if reversing
        float currentTurnAmount = _moveBackward ? _turnAmount * 2 : _turnAmount;

        // Increase to max turning for bots only (for snappier bot movement)
        if (bot && _moveLeft)
        {
            _turnAmount = maxTurnAmount;
        }
        if (bot && _moveRight)
        {
            _turnAmount = maxTurnAmount;
        }
        
        if (_moveLeft)
        {
            // Steer left
            _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, -currentTurnAmount, Time.deltaTime * wheelTurningSpeed);
        }
        else if (_moveRight)
        {
            // Steer right
            _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, currentTurnAmount, Time.deltaTime * wheelTurningSpeed);
        }
        else
        {
            // No steer if no turn buttons are pressed
            _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, 0, Time.deltaTime * wheelResetSpeed);
        }

        // Apply current steering multiplier to the max steering angle given
        float currentSteeringValue = maxSteeringAngle * _currentSteeringMulti;
        
        // Go through each axle (front and back)
        foreach (var axle in axles)
        {
            // Set all wheels to use correct brake torque
            axle.leftWheel.brakeTorque = brakeTorque;
            axle.rightWheel.brakeTorque = brakeTorque;
            
            // Apply steering values to steering-enabled wheels
            if (axle.steering)
            {
                // Steering for front and back wheels
                if (axle.isFrontWheel)
                {
                    axle.leftWheel.steerAngle = currentSteeringValue;
                    axle.rightWheel.steerAngle = currentSteeringValue;
                }
                else
                {
                    axle.leftWheel.steerAngle = maxBackWheelSteeringAngle * _currentSteeringMulti;
                    axle.rightWheel.steerAngle = maxBackWheelSteeringAngle * _currentSteeringMulti;
                }
            }

            // Apply motor values to motor-enabled wheels
            if (axle.motor)
            {
                axle.leftWheel.motorTorque = currentMotorValue;
                axle.rightWheel.motorTorque = currentMotorValue;
            }

            // New original drift friction
            if (_drift) 
            {
                WheelFrictionCurve frictionCurve = axle.leftWheel.forwardFriction;
                axle.leftWheel.forwardFriction = frictionCurve;
                axle.rightWheel.forwardFriction = frictionCurve;
            }
            else // Default friction
            {
                WheelFrictionCurve frictionCurve = axle.leftWheel.forwardFriction;
                axle.leftWheel.forwardFriction = frictionCurve;
                axle.rightWheel.forwardFriction = frictionCurve;
            }

        }
        
        if (!_moveForward && !_moveBackward)
        {
            // No forward/backward movement, apply an idle brake force to make the car come to a stop
            brakeTorque = idleBrakeForce;
        }
        else if (_drift)
        {
            // Disable motors and drift the car by a defined force
            brakeTorque = driftForceAmount;
            axles[0].motor = false;
        }
        else
        {
            // Reset brake torque and motors if moving
            brakeTorque = 0.0f;
            axles[0].motor = true;
        }

        // Apply an additional acceleration force for moving forward
        if (_moveForward) _rigidbody.AddForce(transform.forward * accelerationForce, ForceMode.Acceleration);

        if (!_grounded)
        {
            // IN AIR
            _vfxHandler.StopDriftEffects();
            audioManager.SetSoundVolume("TireSqueelLoop", 0.0f);
            if (!bot) audioManager.StopSound("TireSqueelLoop");

            if (_moveLeft) _rigidbody.AddForce(-transform.right * (accelerationForce / sidewaysMoveForce), ForceMode.VelocityChange);
            if (_moveRight) _rigidbody.AddForce(transform.right * (accelerationForce / sidewaysMoveForce), ForceMode.VelocityChange);
        }
        else
        {
            // ON GROUND
            if (_moveLeft) _rigidbody.AddForce(transform.right * (accelerationForce / 4), ForceMode.Acceleration);
            if (_moveRight) _rigidbody.AddForce(-transform.right * (accelerationForce / 4), ForceMode.Acceleration);
            
            if (_moveBackward) _rigidbody.AddForce(-transform.forward * accelerationForce, ForceMode.Acceleration);
        }
        
        // EXCLUSIVELY ON THE GROUND
        if (!_grounded) return;
        
        if (_moveLeft || _moveRight)
        {
            if (_rigidbody.velocity.magnitude * MphConstant > driftSmokeThreshold)
            {
                _vfxHandler.PlayVFX("DriftSmoke");
                audioManager.SetSoundVolume("TireSqueelLoop", Mathf.Abs(_currentSteeringMulti));
                if (!bot && !audioManager.IsPlayingSound("TireSqueelLoop")) audioManager.PlaySound("TireSqueelLoop");
            }
        }
        else if (_drift)
        {
            _vfxHandler.PlayVFX("DriftSmoke");
            audioManager.SetSoundVolume("TireSqueelLoop", Mathf.Abs(_currentSteeringMulti));
            if (!bot && !audioManager.IsPlayingSound("TireSqueelLoop")) audioManager.PlaySound("TireSqueelLoop");
        }
        else
        {
            _vfxHandler.StopDriftEffects();
            audioManager.SetSoundVolume("TireSqueelLoop", 0.0f);
            if (!bot) audioManager.StopSound("TireSqueelLoop");
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

    /// <summary>
    /// Delays the flyby by a given amount of time
    /// </summary>
    private IEnumerator DelayFlyBy()
    {
        _gm.halt = true;
        yield return new WaitForSeconds(flybyDelay);
        _mainCam.enabled = false;
        flybyCam.gameObject.SetActive(true);
        flybyCam.GetComponent<CameraFlyBy>().activateFlyBy = true;
    }

    private void Update()
    {
        _hornTimer = _hornTimer <= 0 ? 0 : _hornTimer - Time.deltaTime;
        
        if (_gm && _gm.halt)
        {
            _rigidbody.velocity = new Vector3(0, 0, 0);
            _gm.SetDelayTimer();
        }

        // Draw debug rays for wheel directions
        WheelCollider currentWheel = axles[0].leftWheel;
        float currentTorque = currentWheel.motorTorque;
        float currentBrake = currentWheel.brakeTorque;
        float currentRpm = currentWheel.rpm;
        Debug.DrawRay(currentWheel.transform.position, currentWheel.transform.parent.forward * currentTorque / 100, Color.blue);
        Debug.DrawRay(currentWheel.transform.position, Vector3.up * currentRpm / 100, Color.green);
        Debug.DrawRay(currentWheel.transform.position, -currentWheel.transform.parent.forward * currentBrake / 100, Color.red);
        Debug.DrawRay(currentWheel.transform.position, _groundForward * currentBrake / 100, Color.magenta);
        Debug.DrawRay(currentWheel.transform.position, currentWheel.transform.parent.forward * currentBrake / 100, Color.yellow);
        
        // Ground detection
        _hitDetect = Physics.BoxCast(transform.position + transform.up, transform.localScale, -transform.up, out _hit, transform.rotation, 1);
        _grounded = _hitDetect;
        
        if (_hit.collider && !_hit.collider.isTrigger)
        {
            if (_grounded != _groundedPreviousFrame)
            {
                if (!_groundedPreviousFrame)
                {
                    // Apply a small camera shake, sound and vfx to the car when landing after being in the air for a while
                    if (_airTime > 1)
                    {
                        _vfxHandler.SpawnVFXAtPosition("GroundImpact", transform.position + (transform.forward / 2) - (transform.up / 1.5f), 2, false);
                        if (!bot)
                        {
                            _impulseSource.GenerateImpulseAt(transform.position + Vector3.down, new Vector3(0, -_airTime, 0));
                            audioManager.PlaySound("CarLand");
                        }

                        _airTime = 0;
                    }
                }
            }
        }

        if (_hit.transform != null)
        {
            _onOil = _hit.transform.CompareTag("OilSlip");
        }

        // Oil slick functionality
        if (_onOil != _onOilPreviousFrame)
        {
            _savedOilVelocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);

            if (_savedOilVelocity.magnitude * MphConstant < 30)
            {
                _savedOilVelocity = new Vector3(30 / MphConstant, 0, 30 / MphConstant);
            }
        }

        if (_grounded != _groundedPreviousFrame)
        {
            // Just left the ground!
            _groundForward = transform.forward;
        }
        
        // Player manual resetting
        _resetDelay = _resetDelay <= 0 ? 0 : _resetDelay - Time.deltaTime;
        if (!bot && _reset && _resetDelay <= 0)
        {
            _resetDelay = resetCooldown;
            ResetPlayer(true);
        }

        // Retrieve previous frame vars
        _onOilPreviousFrame = _onOil;
        _groundedPreviousFrame = _grounded;
        
        if (!_grounded)
        {
            _airTime += Time.deltaTime;
        }
        else
        {
            _boostsInAirLeft = maxBoostsInAir;
        }

        // Flyby
        if (!_cameraFlyBy) return;
        
        if (!_cameraFlyBy.activateFlyBy)
        {
            flybyCam.gameObject.SetActive(false); 
            _mainCam.enabled = true;
            _gm.halt = false;
        }
        else
        {
            flybyCam.gameObject.SetActive(true);
            _mainCam.enabled = false;
            _gm.SetDelayTimer();
        }
    }

    /// <summary>
    /// Reset the player based on manual or automatic (collided with reset zone)
    /// <param name="pressedButton">Reset button has been triggered or not</param>
    /// </summary>
    private void ResetPlayer(bool pressedButton = false)
    {
        if (!_photonView)
            Debug.LogError("No photon view!");
        if (_photonView.IsMine)
        {
            if (!bot && pressedButton)
            {
                _playerManager.GoToSpawn(true);
            }
            else if (!bot)
            {
                _playerManager.GoToSpawn();
            }
            else if (_photonView.Owner.IsMasterClient && _photonView.IsMine)
            {
                _botCarController.goToSpawn();
            }
        }
        else if (bot && _photonView.Owner.IsMasterClient && _photonView.IsMine)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Sourced gizmos code for displaying directional debug data for driving
    /// </summary>
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
            Gizmos.DrawRay(transform.position + transform.up, -transform.up * 1);
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
            // Bounce players away from each other on collide
            Vector3 direction = collision.contacts[0].point - transform.position;
            if (!bot)
            {
                _rigidbody.velocity = -(direction.normalized * bounciness);
                Debug.Log("HIT ANOTHER PLAYER WITH RIGIDBODY VELOCITY: " + _rigidbody.velocity);
            }
            else
            {
                GetComponent<Rigidbody>().velocity = -(direction.normalized * bounciness);
            }

            if (bot) return;
            _vfxHandler.PlayVFXAtPosition("Impact", collision.contacts[0].point);
            audioManager.PlaySound("CarHit0" + Random.Range(1, 5));
            _photonView.RPC("PlayerHit", RpcTarget.All, collision.transform.GetComponent<PhotonView>().ViewID, direction, collision.contacts[0].point, bounciness);
        }
        else if (collision.transform.CompareTag("SpinningTop"))
        {
            // Bounce player away from spinning top
            Vector3 direction = collision.contacts[0].point - transform.position;
            _rigidbody.velocity = -(direction.normalized * 30);
            _vfxHandler.PlayVFXAtPosition("SoftImpact", collision.contacts[0].point);
            int rand = Random.Range(1, 5);
            if (!bot) audioManager.PlaySound("CarHit0" + rand);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("FinishLine") && !_passedFinishLine && !bot)
        {
            // Passed finish line
            _vfxHandler.PlayVFX("Confetti");
            audioManager.PlaySound("FinishLine");
            audioManager.PlaySound("Confetti");
            _passedFinishLine = true;
            _playerManager.CompleteStage();
        }

        if (other.transform.CompareTag("Checkpoint") && _passedCheckpoints.ContainsKey(other.gameObject) &&
            !_passedCheckpoints[other.gameObject])
        {
            // Passed an uncleared checkpoint
            _vfxHandler.PlayVFX("Confetti");
            audioManager.PlaySound("Checkpoint");
            audioManager.PlaySound("Confetti");
            _passedCheckpoints[other.gameObject] = true;
            var playerNo = !bot ? _playerManager.GetPlayerNumber() : _botCarController.GetBotNumber();
            GameObject newSpawnLocation = other.gameObject.transform.GetChild(playerNo).gameObject;
            if (!bot)
            {
                _playerManager.ChangeSpawnLocation(newSpawnLocation.transform);
            }
            else
            {
                _botCarController.setSpawn(newSpawnLocation.transform);
            }
            if (!bot)
            {
                _playerManager.PassCheckpoint();
            }

        }

        if (other.transform.CompareTag("EliminationZone") && !bot)
        {
            // Inside the zone, set in vfx handler
            _vfxHandler.SetInZone(true);
        }

        if (other.transform.CompareTag("ResetZone"))
        {
            // Triggered a reset  zone
            ResetPlayer();
            if (!bot) audioManager.PlaySound("CarEliminatedOffTrack");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("EliminationZone") && !bot)
        {
            // No longer in the zone, update vfx handler
            Debug.Log("Exit the Elimination Wall");
            _vfxHandler.SetInZone(false);
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
        if (_moveLeft)
            _turnAmount = maxTurnAmount;
        //Debug.Log("Left detected");
    }

    // D
    public void Right(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _moveRight = value > 0;
        if (_moveRight)
            _turnAmount = maxTurnAmount;
        //Debug.Log("Right detected");
    }

    // Controller Left Stick Left
    public void ControllerLeft(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _moveLeft = value > 0;
        _turnAmount = Mathf.Lerp(0, maxTurnAmount, value);
        //Debug.Log("Left detected");
    }

    // Controller Left Stick Right
    public void ControllerRight(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _moveRight = value > 0;
        _turnAmount = Mathf.Lerp(0, maxTurnAmount, value);
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
        //Debug.Log("Rearview detected");
    }
    
    // Horn
    public void Horn(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        if (value > 0 && _hornTimer <= 0)
        {
            audioManager.PlaySound("CarHorn0" + Random.Range(1, 5));
            _hornTimer = 0.1f;
        }
        //Debug.Log("Horn detected");
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

        Debug.Log("CamPos: " + _transposer.m_FollowOffset + " - Transposer:" + _transposer.gameObject);
    }

    #endregion

    #region AI-Input

    //AI ONLY
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