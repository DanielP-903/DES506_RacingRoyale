using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class Dan_CarController : MonoBehaviour
{
    public float maxTorque = 0;
    public float maxSteeringAngle = 0;
    public List<Axle> axles;
    private bool _moveForward = false;
    private bool _moveBackward = false;
    private bool _moveRight = false;
    private bool _moveLeft = false;

    public void FixedUpdate()
    {
        float motorMultiplier = _moveForward ? 1 : _moveBackward ? -1 : 0;
        float currentMotorValue = maxTorque * motorMultiplier;
        
        float steeringMultiplier = _moveLeft ? 1 : _moveRight ? -1 : 0;
        float currentSteeringValue = maxSteeringAngle * steeringMultiplier;

        foreach (var axle in axles)
        {
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
    
    [Serializable]
    public class Axle
    {
        public WheelCollider leftWheel;
        public WheelCollider rightWheel;
        public bool motor;
        public bool steering;
    }
}
