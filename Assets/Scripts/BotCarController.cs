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
        layerMask = LayerMask.GetMask("Player") | LayerMask.GetMask("Checkpoint");
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
        if (detectLeft() && detectForward() && !detectTooClose())
        {
            _cc.BotRight();
            _cc.BotForward();
            _cc.BotNotLeft();
            _cc.BotNotBackward();
        }

        else if (detectRight() && detectForward() && !detectTooClose())
        {
            _cc.BotLeft();
            _cc.BotForward();
            _cc.BotNotRight();
            _cc.BotNotBackward();
        }
        
        else if (detectForward() && !detectTooClose())
        {
            _cc.BotLeft();
            _cc.BotForward();
            _cc.BotNotRight();
            _cc.BotNotBackward();
        }
        else if (detectTooClose())
        {
            _cc.BotNotForward();
            _cc.BotBackward();
        }
        else
        {
            _cc.BotForward();
            _cc.BotNotRight();
            _cc.BotNotLeft();
            _cc.BotNotBackward();
        }

        if (!_cc.GetGrounded())
        {
            _cc.BotBoost();
        }
        else
        {
            _cc.BotNotBoost();
        }
    }

    bool detectForward()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position,  transform.TransformDirection(Vector3.forward) * 8f, Color.magenta);
        if (Physics.Raycast(transform.position,  transform.TransformDirection(Vector3.forward), out hit, 5f, layerMask))
        {
            Debug.Log("Forward Detected!: "+hit.collider.gameObject);
            return true;
        }
        else
        {
            return false;
        }
    }
    bool detectTooClose()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position,  transform.TransformDirection(Vector3.forward) * 3f, Color.magenta);
        if (Physics.Raycast(transform.position,  transform.TransformDirection(Vector3.forward), out hit, 5f, layerMask))
        {
            Debug.Log("Too Close!: "+hit.collider.gameObject);
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
}
