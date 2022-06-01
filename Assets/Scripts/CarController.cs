using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class CarController : MonoBehaviour
{
    [Header("Car Driving Physics")]
    public float motorForce = 0;
    public float brakeTorque = 1000;
    public float maxSteeringAngle = 0;
    public List<Axle> axles;

    [Header("Forces")]
    public float pushForceAmount = 5.0f;
    
    private bool _moveForward = false;
    private bool _moveBackward = false;
    private bool _moveRight = false;
    private bool _moveLeft = false;
    private bool _pushUp = false;
    private bool _grounded = false;

    private float _pushDelay = 5.0f;
    
    private Rigidbody _rigidbody;
    private bool _HitDetect;
    private Collider _boxCollider;
    private RaycastHit _Hit;
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        foreach (var axle in axles)
        {
            axle.leftWheel.brakeTorque = brakeTorque;
            axle.rightWheel.brakeTorque = brakeTorque;
        }

        _boxCollider = transform.GetComponent<BoxCollider>();
    }

    public void FixedUpdate()
    {
        PhysUpdateDriving();
        PhysUpdateForces();
    }

    // For updating rigidbody forces acting upon the car
    private void PhysUpdateForces()
    {
        _pushDelay = _pushDelay <= 0 ? 0 : _pushDelay - Time.fixedDeltaTime; 
        
        if (_pushUp && !_grounded && _pushDelay <= 0.0f)
        {
            _pushDelay = 5.0f;
            Vector3 pushForce = Vector3.up + new Vector3(.3f,0,0);
            
            _rigidbody.AddForce(pushForce * (pushForceAmount * 100.0f), ForceMode.Force);
        }
    }
    
    // For updating driving physics with wheel colliders
    private void PhysUpdateDriving()
    {
        float motorMultiplier = _moveForward ? 1 : _moveBackward ? -1 : 0;
        float currentMotorValue = motorForce * motorMultiplier;
        
        float steeringMultiplier = _moveLeft ? -1 : _moveRight ? 1 : 0;
        float currentSteeringValue = maxSteeringAngle * steeringMultiplier;

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

        }

        if (!_moveForward && !_moveBackward)
        {
            brakeTorque = 1000.0f;
        }
        else
        {
            brakeTorque = 0.0f;
        }
    }

    private void Update()
    {
        WheelCollider currentWheel;
        currentWheel = axles[0].leftWheel;
        
        float currentTorque = currentWheel.motorTorque;
        float currentSteer = currentWheel.steerAngle;
        float currentBrake = currentWheel.brakeTorque;
        float currentRpm = currentWheel.rpm;
        Debug.DrawRay(currentWheel.transform.position, currentWheel.transform.parent.forward * currentTorque / 100, Color.blue);
        Debug.DrawRay(currentWheel.transform.position, Vector3.up * currentRpm / 100, Color.green);
        Debug.DrawRay(currentWheel.transform.position, -currentWheel.transform.parent.forward * currentBrake / 100, Color.red);
        
        _HitDetect = Physics.BoxCast(transform.position, transform.localScale, -transform.up, out _Hit, transform.rotation, 1);
        if (_HitDetect)
        {
            _grounded = true;
            Debug.Log("Hit : " + _Hit.collider.name);
        }
        else
        {
            _grounded = false;
            Debug.Log("No Hit :(");
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
            Gizmos.DrawRay(transform.position, - transform.up * 1);
            //Draw a cube at the maximum distance
            Gizmos.DrawWireCube((transform.position) - transform.up * 1, transform.localScale);
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
        _pushUp = value>0;
        //Debug.Log("Space detected");
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
