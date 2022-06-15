using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public enum PowerupType
{
    None, Superboost, BouncyWallShield, AirBlast, GrapplingHook
}

public class CarController : MonoBehaviour
{
    [Header("Car Driving Physics")]
    public GameObject centreOfMass;
    public float motorForce = 0;
    public float brakeTorque = 1000;
    public float maxSteeringAngle = 0;
    public List<Axle> axles;
    
    [Header("Forces")]
    public float pushForceAmount = 5.0f;
    public float accelerationForce = 5.0f;
    public Vector3 pushForce = Vector3.up;
    public float torqueVectorAmount = 1.0f;
    public float airControlForce = 500.0f;
    public float boostForceAmount = 5.0f;
    public float driftForceAmount = 3000.0f;

    [Header("Environment Pad")] 
    public float jumpPadForce = 5.0f;
    public float boostPadForce = 5.0f;
    
    [Header("Collisions")] 
    public float bounciness = 100.0f;
    
    [Header("Cooldowns (in seconds)")]
    public float jumpCooldown = 2.0f;
    public float boostCooldown = 2.0f;
    public float resetCooldown = 2.0f;
    public float padCooldown = 2.0f;
    
    private bool _moveForward = false;
    private bool _moveBackward = false;
    private bool _moveRight = false;
    private bool _moveLeft = false;
    private bool _pushUp = false;
    private bool _drift = false;
    private bool _boost = false;
    private bool _rollLeft = false;
    private bool _rollRight = false;
    private bool _reset = false;
    private bool _activatePowerup = false;
    private bool _grounded = false;

    private float _currentSteeringMulti;
    
    private bool _passedFinishLine = false;
    private bool _hitEliminationZone = false;
    
    private float _pushDelay = 2.0f;
    private float _boostDelay = 2.0f;
    private float _resetDelay = 2.0f;
    private float _padDelay = 2.0f;
    
    
    private PlayerManager _playerManager;
    
    private Rigidbody _rigidbody;
    private bool _HitDetect;
    private RaycastHit _Hit;

    private Dictionary<GameObject, bool> _passedCheckpoints = new Dictionary<GameObject, bool>();
    private Transform _currentRespawnPoint;
    public CheckpointSystem checkpoints;

    public List<ParticleSystem> boostEffects = new List<ParticleSystem>();

    private PlayerPowerups _playerPowerups;
    
    [Header("DEBUG MODE")]
    public bool debug = false;

    private void Start()
    {
        _passedFinishLine = false;
        _pushDelay = jumpCooldown;
        _boostDelay = boostCooldown;
        //_wallShieldTimer = wallShieldTime;
        _rigidbody = GetComponent<Rigidbody>();
        _playerManager = GetComponent<PlayerManager>();
        _playerPowerups = GetComponent<PlayerPowerups>();
        foreach (var axle in axles)
        {
            axle.leftWheel.brakeTorque = brakeTorque;
            axle.rightWheel.brakeTorque = brakeTorque;
        }
        //SceneManager.sceneLoaded += LoadCCInLevel;
        
        _rigidbody.centerOfMass = centreOfMass.transform.localPosition;
        _playerPowerups.powerupIcon.gameObject.SetActive(false);

        if (debug)
        {
            GameObject checkpointObject = GameObject.Find("CheckpointSystem");

            //_powerupIcon = GameObject.Find("Powerup Icon").gameObject.GetComponent<Image>();

            if (checkpointObject != null)
            {
                checkpoints = checkpointObject.GetComponent<CheckpointSystem>();
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
    }

    void OnLevelWasLoaded()
    {
        _passedFinishLine = false;
        GameObject checkpointObject = GameObject.Find("CheckpointSystem");
        _playerPowerups.powerupIcon = GameObject.Find("Powerup Icon").gameObject.GetComponent<Image>();
        _playerPowerups.powerupIcon.gameObject.SetActive(false);
        if (checkpointObject != null)
        {
            checkpoints = checkpointObject.GetComponent<CheckpointSystem>();
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

    /*private void LoadCCInLevel(Scene scene, LoadSceneMode loadSceneMode)
    {
        checkpoints = GameObject.Find("CheckpointSystem").GetComponent<CheckpointSystem>();
        if (checkpoints != null)
        {
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
    }*/

    public void FixedUpdate()
    {
        PhysUpdateDriving();
        PhysUpdateForces();
        PhysUpdateAirControl();
        PhysUpdateAcceleration();
    }

    private void PhysUpdateAcceleration()
    {
        _boostDelay = _boostDelay <= 0 ? 0 : _boostDelay - Time.fixedDeltaTime;

        // BOOST FUNCTIONALITY
        if (_boost && _boostDelay <= 0)
        {
            StartCoroutine(ActivateBoostEffect());
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
 
        if (_pushUp && !_grounded && _pushDelay <= 0.0f)
        {
            _pushDelay = jumpCooldown;
            Vector3 push = Vector3.up + new Vector3(.5f,0,0);
            _rigidbody.AddForce(push * (pushForceAmount * 500.0f), ForceMode.Force);
            _rigidbody.AddTorque(transform.forward * pushForceAmount, ForceMode.Impulse);
        }
        else if (_pushUp && _grounded && _pushDelay <= 0.0f)
        {
            _pushDelay = jumpCooldown;
            _rigidbody.AddForce(transform.up * (pushForceAmount * 700.0f), ForceMode.Force);
        }
        if (_Hit.transform != null)
        {
            if (_Hit.transform.CompareTag("JumpPad") && _padDelay <= 0)
            {
                _padDelay = padCooldown;
                _rigidbody.AddForce(transform.up * (jumpPadForce * 700.0f * 1.5f), ForceMode.Force);
            }

            if (_Hit.transform.CompareTag("BoostPad") && _padDelay <= 0)
            {
                _padDelay = padCooldown;
                if (_rigidbody.velocity.magnitude * 2.2369362912f < 0.1f)
                {
                    _rigidbody.velocity = -_Hit.transform.forward * boostPadForce;
                }
                else
                {
                    _rigidbody.AddForce(-_Hit.transform.forward * boostPadForce, ForceMode.VelocityChange);
                }
            }
        }
    }

    private void PhysUpdateAirControl()
    {
        if (!_grounded)
        {
            //if (_moveForward) _rigidbody.AddTorque(transform.right * airControlForce, ForceMode.Force);
            //if (_moveBackward) _rigidbody.AddTorque(-transform.right * airControlForce, ForceMode.Force);
            if (_moveLeft) _rigidbody.AddTorque(-transform.up/15, ForceMode.VelocityChange);
            if (_moveRight) _rigidbody.AddTorque(transform.up/15, ForceMode.VelocityChange);
            if (_rollLeft) _rigidbody.AddTorque(transform.forward/15, ForceMode.VelocityChange);
            if (_rollRight) _rigidbody.AddTorque(-transform.forward/15, ForceMode.VelocityChange);
        }
    }
    
    // For updating driving physics with wheel colliders
    private void PhysUpdateDriving()
    {
        float motorMultiplier = _moveForward ? 1 : _moveBackward ? -1 : 0;
  
        float currentMotorValue = motorForce * motorMultiplier;

        maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, _rigidbody.velocity.magnitude * 2.2369362912f > 30 ? 10 : 30, Time.deltaTime * 5);

        if (_moveLeft)
        {
            _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, -1, Time.deltaTime * 5.0f);
            // if (_moveBackward)
            // {
            //     _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, 1, Time.deltaTime * 5.0f);
            // }
            // else
            // {
            //     _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, -1, Time.deltaTime * 5.0f);
            // }
        }
        else if (_moveRight)
        {
            _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, 1, Time.deltaTime * 5.0f);
            // if (_moveBackward)
            // {
            //     _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, -1, Time.deltaTime * 5.0f);
            // }
            // else
            // {
            //     _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, 1, Time.deltaTime * 5.0f);
            // }
        }
        else
        {
            _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, 0, Time.deltaTime * 5.0f);
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
               // frictionCurve.stiffness = 1.5f;
                axle.leftWheel.forwardFriction  = frictionCurve;
                axle.rightWheel.forwardFriction  = frictionCurve;
            }
            else // Default friction
            {
                WheelFrictionCurve frictionCurve = axle.leftWheel.forwardFriction;
                //frictionCurve.stiffness = 1.2f;
                axle.leftWheel.forwardFriction  = frictionCurve;
                axle.rightWheel.forwardFriction  = frictionCurve;  
            }
            
        }

        if (!_moveForward && !_moveBackward)
        {
            brakeTorque = 1000.0f;
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

        if (_grounded)
        {
            if (_moveLeft) _rigidbody.AddForce(transform.right * (accelerationForce), ForceMode.Acceleration);
            if (_moveRight) _rigidbody.AddForce(-transform.right * (accelerationForce), ForceMode.Acceleration);
        }

    }

    public IEnumerator ActivateBoostEffect()
    {
        foreach (var effect in boostEffects)
        {
            effect.Play();
        }

        yield return new WaitForSeconds(2);
        
        foreach (var effect in boostEffects)
        {
            effect.Stop();
        }
    }

    public bool GetBoost()
    {
        return _boostDelay <= 0 && _boost;
    }

    public bool GetActivate()
    {
        return _activatePowerup;
    }
    
    private void Update()
    {
        WheelCollider currentWheel = axles[0].leftWheel;
        
        float currentTorque = currentWheel.motorTorque;
        float currentBrake = currentWheel.brakeTorque;
        float currentRpm = currentWheel.rpm;
        Debug.DrawRay(currentWheel.transform.position, currentWheel.transform.parent.forward * currentTorque / 100, Color.blue);
        Debug.DrawRay(currentWheel.transform.position, Vector3.up * currentRpm / 100, Color.green);
        Debug.DrawRay(currentWheel.transform.position, -currentWheel.transform.parent.forward * currentBrake / 100, Color.red);
        
        _HitDetect = Physics.BoxCast(transform.position + transform.up, transform.localScale, -transform.up, out _Hit, transform.rotation, 1);
        _grounded = _HitDetect;

        // var leftCheck = Physics.Raycast(axles[0].leftWheel.transform.position + axles[0].leftWheel.transform.up, -axles[0].leftWheel.transform.up, out _leftHit, 1.0f);
        // var rightCheck = Physics.Raycast(axles[0].rightWheel.transform.position + axles[0].rightWheel.transform.up, -axles[0].rightWheel.transform.up, out _rightHit, 1.0f);
        // _groundedL = leftCheck;
        // _groundedR = rightCheck;

        _resetDelay = _resetDelay <= 0 ? 0 : _resetDelay - Time.deltaTime;
        if (_reset && _resetDelay <= 0)
        {
            _resetDelay = resetCooldown;
            _playerManager.GoToSpawn();
        }
    }

    void OnDrawGizmos()
    {
        //Check if there has been a hit yet
        if (_HitDetect)
        {
            Gizmos.color = Color.green;
            //Draw a Ray forward from GameObject toward the hit
            Gizmos.DrawRay(transform.position, -transform.up * _Hit.distance);
            //Draw a cube that extends to where the hit exists
            Gizmos.DrawWireCube(transform.position - transform.up * _Hit.distance, transform.localScale);
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
        //Gizmos.DrawSphere(transform.position + (transform.up/4), 0.1f);
        //Gizmos.DrawSphere(GetComponent<BoxCollider>().bounds.center- (transform.up/4), 0.1f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Vector3 direction = collision.contacts[0].point - transform.position;
            _rigidbody.velocity = -(direction.normalized * bounciness);
        }

    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.CompareTag("FinishLine") && !_passedFinishLine)
        {
            // Passed finish line
            Debug.Log("Passed finish line!");
            _passedFinishLine = true;
            _playerManager.CompleteStage();
        }
        
        if (collider.transform.CompareTag("Checkpoint") && _passedCheckpoints.ContainsKey(collider.gameObject) && !_passedCheckpoints[collider.gameObject])
        {
            _passedCheckpoints[collider.gameObject] = true;
            _currentRespawnPoint = collider.gameObject.transform;
            GameObject newSpawnLocation = GameObject.Find(_currentRespawnPoint.name + _playerManager.GetPlayerNumber());
            Debug.Log("Checkpoint passed: " + collider.gameObject.name + " , " + newSpawnLocation + " , " + _currentRespawnPoint.name + " , " + _playerManager.GetPlayerNumber());
            _playerManager.ChangeSpawnLocation(newSpawnLocation.transform);
        }
        
        if (collider.transform.CompareTag("EliminationZone") && !_hitEliminationZone)
        {
            // Passed finish line
            Debug.Log("Hit the Elimination Wall");
            _hitEliminationZone = true;
            _playerManager.EliminateCurrentPlayer();
        }
        if (collider.transform.CompareTag("Blast"))
        {
            Vector3 direction = collider.transform.position - transform.position;
            _rigidbody.velocity = -(direction.normalized * bounciness);
        }
        
    }

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
    public void RollLeft(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _rollLeft = value > 0;
        //Debug.Log("Roll Left detected");
    }
    // Roll Right
    public void RollRight(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _rollRight = value > 0;
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

    [Serializable]
    public class Axle
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public bool motor;
        public bool steering;
    }
}