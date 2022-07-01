using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Chat.UtilityScripts;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class BotCarController : MonoBehaviour
{
    [SerializeField] private bool _debugMode = false;
    private CarController _cc;
    private Rigidbody _rb;
    private int _layerMask;
    private Transform _carBody;
    private PhotonView _pv;
    private GameManager _gm;
    private Transform _spawnLocation;
    private int _botNum = -1;
    private bool touchingReset = false;
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
            _botNum = _gm.GetTotalPlayers() + _gm.GetBotNum();
            _pv = GetComponent<PhotonView>();
            _cc = GetComponent<CarController>();
            _rb = GetComponent<Rigidbody>();
            _layerMask = LayerMask.GetMask("Player", "Checkpoint");
            _layerMask = ~_layerMask;
            _carBody = transform.Find("CarMesh");
            _spawnLocation = GameObject.Find("SpawnLocation" + _botNum).transform;
        }
        else if (_debugMode)
        {
            _botNum = 1;
            _pv = GetComponent<PhotonView>();
            _cc = GetComponent<CarController>();
            _rb = GetComponent<Rigidbody>();
            _layerMask = LayerMask.GetMask("Player", "Checkpoint");
            _layerMask = ~_layerMask;
            _carBody = transform.Find("CarMesh");
            _spawnLocation = GameObject.Find("SpawnLocation" + _botNum).transform;
        }
        else
        {
            Destroy(GetComponent<CarController>());
            Destroy(GetComponent<Rigidbody>());
            Destroy(this);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        decideBehaviour();
        checkReset();
    }

    public int GetBotNumber()
    {
        return _botNum;
    }
    
    void checkReset()
    {
        if (transform.position.y < -5 || touchingReset)
        {
            goToSpawn();
        }
    }

    public void setSpawn(Transform newSpawn)
    {
        _spawnLocation = newSpawn;
    }
    void goToSpawn()
    {
        touchingReset = false;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        Transform spawn = _spawnLocation;
        Transform thisTransform = transform;
        var rotation = spawn.rotation;
        var position = spawn.position;
        thisTransform.rotation = rotation;
        thisTransform.position = position;
    }

    void decideBehaviour()
    {
        if ((detectLeft() && detectForward() && !detectTooClose()) 
            || (!detectLeftPit() && detectRightPit())
            || (!detectForwardLeftPit() && detectForwardRightPit()))
        {
            rightTurn();
        }

        else if ((detectRight()  && detectForward() && !detectTooClose()) 
                 || (!detectRightPit() && detectLeftPit()) 
                 || (detectForward() && !detectTooClose())
                 || (detectForwardLeftPit() && !detectForwardRightPit()))
        {
            leftTurn();
        }
        
        else if (detectTooClose())
        {
            justBackward();
        }
        
        else
        {
            justForward();
        }

        if (!_cc.GetGrounded())
        {
            _cc.BotBoost();
        }
        else
        {
            _cc.BotNotBoost();
        }

        if (!detectForwardPit() && !detectForwardRightPit() && !detectForwardLeftPit())
        {
            _cc.BotSpace();
        }
        else
        {
            _cc.BotNotSpace();
        }
    }

    void leftTurn()
    {
        //Debug.Log("TurningToLeft");
        _cc.BotLeft();
        _cc.BotForward();
        _cc.BotNotRight();
        _cc.BotNotBackward();
    }

    void rightTurn()
    {
        //Debug.Log("TurningToRight");
        _cc.BotRight();
        _cc.BotForward();
        _cc.BotNotLeft();
        _cc.BotNotBackward();
    }

    void justForward()
    {
        _cc.BotForward();
        _cc.BotNotRight();
        _cc.BotNotLeft();
        _cc.BotNotBackward();
    }

    void justBackward()
    {
        _cc.BotNotForward();
        _cc.BotNotRight();
        _cc.BotNotLeft();
        _cc.BotBackward();
    }

    bool detectForward()
    {
        RaycastHit hit;
        //Debug.DrawRay(transform.position,  transform.TransformDirection(Vector3.forward) * 8f, Color.magenta);
        if (Physics.Raycast(transform.position,  transform.TransformDirection(Vector3.forward), out hit, 5f, _layerMask))
        {
            //Debug.Log("Forward Detected!: "+hit.collider.gameObject);
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
        //Debug.DrawRay(transform.position,  transform.TransformDirection(Vector3.forward) * 3f, Color.magenta);
        if (Physics.Raycast(transform.position,  transform.TransformDirection(Vector3.forward), out hit, 5f, _layerMask))
        {
            ////Debug.Log("Too Close!: "+hit.collider.gameObject);
            return true;
        }
        else
        {
            return false;
        }
    }
    
    bool detectLeft()
    {//Quaternion.Euler(0, -45, 0) * 
        //Debug.DrawRay(transform.position, Quaternion.Euler(0, -30, 0) * transform.TransformDirection(Vector3.forward) * 5f, Color.magenta);
        if (Physics.Raycast(transform.position,  Quaternion.Euler(0, -30, 0) * transform.TransformDirection(Vector3.forward), 5f, _layerMask))
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
        //Debug.DrawRay(transform.position, Quaternion.Euler(0, 30, 0) * transform.TransformDirection(Vector3.forward) * 5f, Color.magenta);
        if (Physics.Raycast(transform.position,  Quaternion.Euler(0, 30, 0) * transform.TransformDirection(Vector3.forward), 5f, _layerMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    bool detectLeftPit()
    {
        //Debug.DrawRay(transform.position - transform.right * 5, transform.TransformDirection(Vector3.down) * 5f, Color.cyan);
        if (Physics.Raycast(transform.position - transform.right * 5,  transform.TransformDirection(Vector3.down), 5f, _layerMask))
        {
            return true;
        }
        else
        {
            ////Debug.Log("NothingToLeft");
            return false;
        }
    }
    
    bool detectRightPit()
    {
        //Debug.DrawRay(transform.position + transform.right * 5, transform.TransformDirection(Vector3.down) * 5f, Color.cyan);
        if (Physics.Raycast(transform.position + transform.right * 5,  transform.TransformDirection(Vector3.down), 5f, _layerMask))
        {
            return true;
        }
        else
        {
            ////Debug.Log("NothingToRight");
            return false;
        }
    }
    
    bool detectForwardLeftPit()
    {
        //Debug.DrawRay(transform.position - transform.right * 5 + transform.forward * 5, transform.TransformDirection(Vector3.down) * 5f, Color.cyan);
        if (Physics.Raycast(transform.position - transform.right * 5 + transform.forward * 5,  transform.TransformDirection(Vector3.down), 5f, _layerMask))
        {
            return true;
        }
        else
        {
            ////Debug.Log("NothingToLeft");
            return false;
        }
    }
    
    bool detectForwardRightPit()
    {
        //Debug.DrawRay(transform.position + transform.right * 5 + transform.forward * 5, transform.TransformDirection(Vector3.down) * 5f, Color.cyan);
        if (Physics.Raycast(transform.position + transform.right * 5 + transform.forward * 5,  transform.TransformDirection(Vector3.down), 5f, _layerMask))
        {
            return true;
        }
        else
        {
            ////Debug.Log("NothingToRight");
            return false;
        }
    }
    
    bool detectForwardPit()
    {
        //Debug.DrawRay(transform.position + transform.forward * 5, transform.TransformDirection(Vector3.down) * 5f, Color.cyan);
        if (Physics.Raycast(transform.position + transform.forward * 5,  transform.TransformDirection(Vector3.down), 5f, _layerMask))
        {
            return true;
        }
        else
        {
            ////Debug.Log("NothingToRight");
            return false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ResetZone")
        {
            touchingReset = true;
        }
    }
}
