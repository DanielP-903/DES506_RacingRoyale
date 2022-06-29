using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BotCarController : MonoBehaviour
{
    private CarController _cc;
    private int layerMask;
    private Transform carBody;
    
    // Start is called before the first frame update
    void Start()
    {
        _cc = GetComponent<CarController>();
        layerMask = LayerMask.GetMask("Player");
        layerMask = ~layerMask;
        carBody = transform.Find("CarMesh");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        decideBehaviour();
    }

    void decideBehaviour()
    {
        if (detectLeft() && detectForward())
        {
            _cc.BotRight();
            _cc.BotForward();
        }

        else if (detectRight() && detectForward())
        {
            _cc.BotLeft();
            _cc.BotForward();
        }
        
        else if (detectForward())
        {
            _cc.BotLeft();
            _cc.BotForward();
        }
        else
        {
            _cc.BotForward();
        }
    }

    bool detectForward()
    {
        Debug.DrawRay(transform.position,  transform.TransformDirection(Vector3.forward) * 5f, Color.magenta);
        if (Physics.Raycast(transform.position,  transform.TransformDirection(Vector3.forward), 5f, layerMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    bool detectLeft()
    {//Quaternion.Euler(0, -45, 0) * 
        Debug.DrawRay(transform.position, Quaternion.Euler(0, -30, 0) * transform.TransformDirection(Vector3.forward) * 5f, Color.magenta);
        if (Physics.Raycast(transform.position,  Quaternion.Euler(0, -30, 0) * transform.TransformDirection(Vector3.forward), 5f, layerMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool detectRight()
    {
        Debug.DrawRay(transform.position, Quaternion.Euler(0, 30, 0) * transform.TransformDirection(Vector3.forward) * 5f, Color.magenta);
        if (Physics.Raycast(transform.position,  Quaternion.Euler(0, 30, 0) * transform.TransformDirection(Vector3.forward), 5f, layerMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void MoveForward()
    {
        _cc.BotForward();
    }
}
