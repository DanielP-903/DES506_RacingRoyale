using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NicholasCarController : MonoBehaviour
{
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // maximum steer angle the wheel can have
    private float _forwardInput; // forward input value of keys pressed
    private float _backwardInput; // forward input value of keys pressed
    private float _rightInput; // forward input value of keys pressed
    private float _leftInput; // forward input value of keys pressed
        
    public void FixedUpdate()
    {
        float motor = maxMotorTorque * (_forwardInput + _backwardInput);
        float steering = maxSteeringAngle * (_leftInput + _rightInput);
        Debug.Log("Motor: "+motor);
            
        foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.steering) {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
        }
    }

    public void Forward(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _forwardInput = value;
        Debug.Log("Input: "+_forwardInput);
    }
    
    public void Backward(InputAction.CallbackContext context)
    {
        float value = -context.ReadValue<float>();
        _backwardInput = value;
    }
    
    public void Left(InputAction.CallbackContext context)
    {
        float value = -context.ReadValue<float>();
        _leftInput = value;
    }
    
    public void Right(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        _rightInput = value;
    }
}
    
[Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
}
