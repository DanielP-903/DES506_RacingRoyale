using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;



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

    [Header("Collisions")] 
    public float bounciness = 100.0f;
    
    [Header("Cooldowns (in seconds)")]
    public float jumpCooldown = 2.0f;
    public float boostCooldown = 2.0f;
    public float resetCooldown = 2.0f;
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
    private bool _grounded = false;

    private float _currentSteeringMulti;
    
    private bool _passedFinishLine = false;
    private bool _hitEliminationZone = false;
    
    private float _pushDelay = 2.0f;
    private float _boostDelay = 2.0f;
    private float _resetDelay = 2.0f;
    
    private PlayerManager _playerManager;
    
    private Rigidbody _rigidbody;
    private bool _HitDetect;
    private RaycastHit _Hit;

    public Transform lastCheckpoint;
    private Dictionary<GameObject, bool> _passedCheckpoints = new Dictionary<GameObject, bool>();
    private Transform _currentRespawnPoint;

    private void Start()
    {
        _passedFinishLine = false;
        _pushDelay = jumpCooldown;
        _boostDelay = boostCooldown;
        _rigidbody = GetComponent<Rigidbody>();
        _playerManager = GetComponent<PlayerManager>();
        
        foreach (var axle in axles)
        {
            axle.leftWheel.brakeTorque = brakeTorque;
            axle.rightWheel.brakeTorque = brakeTorque;
        }

        _rigidbody.centerOfMass = centreOfMass.transform.localPosition;
    }

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
    }

    private void PhysUpdateAirControl()
    {
        if (!_grounded)
        {
            if (_moveForward) _rigidbody.AddTorque(transform.right * airControlForce, ForceMode.Force);
            if (_moveBackward) _rigidbody.AddTorque(-transform.right * airControlForce, ForceMode.Force);
            if (_moveLeft) _rigidbody.AddTorque(-transform.up/20, ForceMode.VelocityChange);
            if (_moveRight) _rigidbody.AddTorque(transform.up/20, ForceMode.VelocityChange);
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
        }
        else if (_moveRight)
        {
            _currentSteeringMulti = Mathf.Lerp(_currentSteeringMulti, 1, Time.deltaTime * 5.0f);
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
        else
        {
            brakeTorque = 0.0f;
        }
        if (_moveForward) _rigidbody.AddForce(transform.forward * accelerationForce, ForceMode.Acceleration);
        if (_moveBackward) _rigidbody.AddForce(-transform.forward * accelerationForce, ForceMode.Acceleration);
        if (_moveLeft) _rigidbody.AddForce(transform.right * (accelerationForce), ForceMode.Acceleration);
        if (_moveRight) _rigidbody.AddForce(-transform.right * (accelerationForce), ForceMode.Acceleration);
        
        
    }

    public bool GetBoost()
    {
        return _boostDelay <= 0 && _boost;
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
            _playerManager.ChangeSpawnLocation(newSpawnLocation.transform);
        }
        if (collider.transform.CompareTag("EliminationZone") && !_hitEliminationZone)
        {
            // Passed finish line
            Debug.Log("Hit the Elimination Wall");
            _hitEliminationZone = true;
            _playerManager.EliminateCurrentPlayer();
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
        //Debug.Log("Boost detected");
    }
    // Roll Right
    public void RollRight(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _rollRight = value > 0;
        //Debug.Log("Boost detected");
    }
    // Reset
    public void Reset(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _reset = value > 0;
        //Debug.Log("Boost detected");
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