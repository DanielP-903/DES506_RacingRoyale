using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BotCarController : MonoBehaviour
{
    private CarController _cc;
    // Start is called before the first frame update
    void Start()
    {
        _cc = GetComponent<CarController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    void decideBehaviour()
    {
        
    }

    void MoveForward()
    {
        InputAction.CallbackContext ctx = new InputAction.CallbackContext();
        _cc.Forward(ctx);
    }
}
