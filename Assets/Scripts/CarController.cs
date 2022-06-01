using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class CarController : MonoBehaviour
{
    public float motorForce = 0;
    public float brakeTorque = 1000;
    public float maxSteeringAngle = 0;
    public List<Axle> axles;
    private bool _moveForward = false;
    private bool _moveBackward = false;
    private bool _moveRight = false;
    private bool _moveLeft = false;
    private bool _pushUp = false;
    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        foreach (var axle in axles)
        {
            axle.leftWheel.brakeTorque = brakeTorque;
            axle.rightWheel.brakeTorque = brakeTorque;
        }
    }

    public void FixedUpdate()
    {
        PhysUpdateDriving();
    }

    // For updating rigidbody forces acting upon the car
    private void PhysUpdateForces()
    {
        
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
        bool value = context.ReadValue<bool>();
        _pushUp = value;
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
