using Photon.Pun;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class BotCarController : MonoBehaviour
{
    [SerializeField] private bool _debugMode = false;
    private CarController _cc;
    private Rigidbody _rb;
    private int _layerMask;
    private Transform _carBody;
    private PhotonView _pv;
    private GameManager _gm;
    private CheckpointSystem _cs;
    private Transform _spawnLocation;
    private int _botNum = -1;
    private bool touchingReset = false;
    private float target;
    private bool grounded;

    private BotPoint[] _botPoints;

    //private Transform[] _passedBotPoints;
    private int currentOrder = 0;
    private int currentChoice = 0;

    private TextMeshProUGUI name;
    private TextMeshProUGUI lis;
    private TextMeshProUGUI flis;


    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
            _botNum = _gm.GetTotalPlayers() + _gm.GetBotNum() + 1;
            _pv = GetComponent<PhotonView>();
            _pv.name = this.gameObject.name;
            _cc = GetComponent<CarController>();
            _cs = GameObject.Find("CheckpointSystem")
                ? GameObject.Find("CheckpointSystem").GetComponent<CheckpointSystem>()
                : null;
            _rb = GetComponent<Rigidbody>();
            _layerMask = LayerMask.GetMask("Player", "Checkpoint");
            _layerMask = ~_layerMask;
            _carBody = transform.Find("CarMesh");
            _spawnLocation = GameObject.Find("SpawnLocation" + _botNum).transform;
            _botPoints = GameObject.Find("BotPoints").GetComponentsInChildren<BotPoint>();
        }
        else if (_debugMode)
        {
            _botNum = 1;
            _pv = GetComponent<PhotonView>();
            _cc = GetComponent<CarController>();
            _cs = GameObject.Find("CheckpointSystem").GetComponent<CheckpointSystem>();
            _rb = GetComponent<Rigidbody>();
            _layerMask = LayerMask.GetMask("Player", "Checkpoint");
            _layerMask = ~_layerMask;
            _carBody = transform.Find("CarMesh");
            _spawnLocation = GameObject.Find("SpawnLocation" + _botNum).transform;
            _botPoints = GameObject.Find("BotPoints").GetComponentsInChildren<BotPoint>();
        }
        else
        {
            Debug.Log("Destroying Bot Control");
            Destroy(GetComponent<CarController>());
            //Destroy(GetComponent<Rigidbody>());
            Destroy(this);
        }

        name = transform.Find("Name").Find("Name").GetComponent<TextMeshProUGUI>();
        lis = transform.Find("License").Find("Name").GetComponent<TextMeshProUGUI>();
        flis = transform.Find("FLicense").Find("Name").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        decideBehaviour();
        checkReset();
        name.text = _pv.name;
        lis.text = _pv.name;
        flis.text = _pv.name;
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

    public void goToSpawn()
    {
        if (!_spawnLocation.name.Contains("SpawnLocation") && _cs != null &&
            _cs.GetCheckpointElimination(_spawnLocation.parent.gameObject))
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
        else
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

            decideChoice();
        }
    }

    public void setName(string newName = "NotSetProperly")
    {
        _pv.name = newName;
    }

    void decideBehaviour()
    {
        //Debug.Log("Deciding Behaviour");
        //decideTarget();
        decideTargetPoint();
        alignToTarget();
    }

    void decideTargetPoint()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 3f,
                _layerMask))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }

        Transform nearestPoint = null;
        foreach (BotPoint bt in _botPoints)
        {
            //Debug.Log(bt.name);
            if (nearestPoint == null && bt.choice == currentChoice)
            {
                nearestPoint = bt.transform;
            }
            else if (Vector3.Distance(transform.position, bt.transform.position) <
                     Vector3.Distance(transform.position, nearestPoint.position) && bt.choice == currentChoice)
            {
                nearestPoint = bt.transform;
            }
        }

        //Debug.Log(nearestPoint.name + ":"+nearestPoint.position+":" +transform.position+"-"+Vector2.SignedAngle(new Vector2(transform.position.x, transform.position.z), new Vector2(nearestPoint.position.x, nearestPoint.position.z)));
        //target = Vector2.SignedAngle(new Vector2(transform.position.x, transform.position.z), new Vector2(nearestPoint.position.x, nearestPoint.position.z));
        target = Mathf.Atan((transform.position.z - nearestPoint.position.z) /
                            (transform.position.x - nearestPoint.position.x));
        //Debug.Log(nearestPoint.name + ":" + nearestPoint.position + ":" + transform.position + " " + target);
        if (target >= 360)
        {
            target -= 360;
        }
        else if (target < 0)
        {
            while (target < 0)
            {
                //Debug.Log("StillInWhileLoop: "+target);
                target += 360;
            }
        }
    }

    void decideChoice()
    {
        Transform nearestPoint = null;
        foreach (BotPoint bt in _botPoints)
        {
            if (nearestPoint == null)
            {
                nearestPoint = bt.transform;
            }
            else if (Vector3.Distance(transform.position, bt.transform.position) <
                     Vector3.Distance(transform.position, nearestPoint.position))
            {
                nearestPoint = bt.transform;
            }
        }

        currentChoice = nearestPoint.GetComponent<BotPoint>().choice;
    }

    void pickChoice(int[] choices)
    {
        currentChoice = choices[Random.Range(0, choices.Length - 1)];
    }

    void decideTarget()
    {
        RaycastHit hit;
        //Debug.DrawRay(transform.position,  transform.TransformDirection(Vector3.down) * 2f, Color.magenta);
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 3f, _layerMask))
        {
            grounded = true;
            switch (hit.collider.name)
            {
                case "scale13":
                case "polySurface14":
                    target = hit.collider.transform.rotation.eulerAngles.y - 90;
                    break;
                case "PV_OpenArea_1":
                case "PV_TFlatRamp":
                    target = hit.collider.transform.rotation.eulerAngles.y + 90;
                    break;
                default:
                    target = hit.collider.transform.rotation.eulerAngles.y;
                    break;
            }

            if (target >= 360)
            {
                target -= 360;
            }
            else if (target < 0)
            {
                while (target < 0)
                {
                    //Debug.Log("StillInWhileLoop: "+target);
                    target += 360;
                }
            }
            //Debug.Log("Ground Detected!: "+hit.collider.name + " Rot: "+target);
            //target = hit.collider.transform.rotation.eulerAngles.y;
        }
        else
        {
            //Debug.Log("No Ground Detected");
            grounded = false;
        }
        //Debug.Log("Decided Target");
    }

    void alignToTarget()
    {
        //Debug.Log("CurrentRot: " + transform.rotation.eulerAngles.y + " TargetRot: "+ target);
        //Vector2.SignedAngle(new Vector2(0,transform.rotation.eulerAngles.y), new Vector2(0,target))
        //Debug.Log(grounded);
        /*if (detectTooClose())
        {
            justBackward();
            Debug.Log("Reversing - CurrentRot: " + transform.rotation.eulerAngles.y + " TargetRot: "+ target);
        }
        else */
        if (Mathf.Abs(Mathf.DeltaAngle(transform.rotation.eulerAngles.y, target)) > 1) //&& grounded
        {
            if (Mathf.DeltaAngle(transform.rotation.eulerAngles.y, target) < 0 && !detectTooClose())
            {
                //Debug.Log("Turning Left - CurrentRot: " + transform.rotation.eulerAngles.y + " TargetRot: "+ target);
                leftTurn();
            }
            else if (Mathf.DeltaAngle(transform.rotation.eulerAngles.y, target) > 0 && !detectTooClose())
            {
                //Debug.Log("Turning Right - CurrentRot: " + transform.rotation.eulerAngles.y + " TargetRot: "+ target);
                rightTurn();
            }
            else if (Mathf.DeltaAngle(transform.rotation.eulerAngles.y, target) < 0 && detectTooClose())
            {
                //Debug.Log("Reversing Right - CurrentRot: " + transform.rotation.eulerAngles.y + " TargetRot: "+ target);
                rightBackward();
            }
            else if (Mathf.DeltaAngle(transform.rotation.eulerAngles.y, target) > 0 && detectTooClose())
            {
                //Debug.Log("Reversing Left - CurrentRot: " + transform.rotation.eulerAngles.y + " TargetRot: "+ target);
                leftBackward();
            }
            else
            {
                //Debug.Log("Moving Forward - CurrentRot: " + transform.rotation.eulerAngles.y + " TargetRot: "+ target + " AngleVal: "+Mathf.DeltaAngle(transform.rotation.eulerAngles.y, target));
                justForward();
            }
        }
        else
        {
            //Debug.Log("Moving Forward Grounded: " +grounded+" - CurrentRot: " + transform.rotation.eulerAngles.y + " TargetRot: "+ target + " AngleVal: "+Mathf.DeltaAngle(transform.rotation.eulerAngles.y, target));
            justForward();
        }
    }

    void leftTurn()
    {
        Debug.Log("TurningToLeft");
        _cc.BotLeft();
        _cc.BotForward();
        _cc.BotNotRight();
        _cc.BotNotBackward();
    }

    void rightTurn()
    {
        Debug.Log("TurningToRight");
        _cc.BotRight();
        _cc.BotForward();
        _cc.BotNotLeft();
        _cc.BotNotBackward();
    }

    void justForward()
    {
        Debug.Log("JustForward");
        _cc.BotForward();
        _cc.BotNotRight();
        _cc.BotNotLeft();
        _cc.BotNotBackward();
    }

    void justBackward()
    {
        Debug.Log("JustBackward");
        _cc.BotNotForward();
        _cc.BotNotRight();
        _cc.BotNotLeft();
        _cc.BotBackward();
    }

    void leftBackward()
    {
        Debug.Log("LeftBackward");
        _cc.BotNotForward();
        _cc.BotNotRight();
        _cc.BotLeft();
        _cc.BotBackward();
    }

    void rightBackward()
    {
        Debug.Log("RightBackward");
        _cc.BotNotForward();
        _cc.BotRight();
        _cc.BotNotLeft();
        _cc.BotBackward();
    }

    #region OldBehaviour

    void decideOldBehaviour()
    {
        int randomVal = Random.Range((int)0, (int)250);
        if ((detectLeft() && detectForward() && !detectTooClose())
            || (!detectLeftPit() && detectRightPit())
            || (!detectForwardLeftPit() && detectForwardRightPit())
            || randomVal == 0)
        {
            rightTurn();
        }

        else if ((detectRight() && detectForward() && !detectTooClose())
                 || (!detectRightPit() && detectLeftPit())
                 || (detectForward() && !detectTooClose())
                 || (detectForwardLeftPit() && !detectForwardRightPit())
                 || randomVal == 1)
        {
            leftTurn();
        }

        else if (detectTooClose()
                 || randomVal == 2)
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



    bool detectForward()
    {
        RaycastHit hit;
        //Debug.DrawRay(transform.position,  transform.TransformDirection(Vector3.forward) * 8f, Color.magenta);
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 5f, _layerMask))
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
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 5f, Color.magenta);
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 5f, _layerMask))
        {
            //Debug.Log("Too Close!: "+hit.collider.gameObject);
            return true;
        }
        else
        {
            return false;
        }
    }

    bool detectLeft()
    {
        //Quaternion.Euler(0, -45, 0) * 
        //Debug.DrawRay(transform.position, Quaternion.Euler(0, -30, 0) * transform.TransformDirection(Vector3.forward) * 5f, Color.magenta);
        if (Physics.Raycast(transform.position,
                Quaternion.Euler(0, -30, 0) * transform.TransformDirection(Vector3.forward), 5f, _layerMask))
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
        if (Physics.Raycast(transform.position,
                Quaternion.Euler(0, 30, 0) * transform.TransformDirection(Vector3.forward), 5f, _layerMask))
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
        if (Physics.Raycast(transform.position - transform.right * 5, transform.TransformDirection(Vector3.down), 5f,
                _layerMask))
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
        if (Physics.Raycast(transform.position + transform.right * 5, transform.TransformDirection(Vector3.down), 5f,
                _layerMask))
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
        if (Physics.Raycast(transform.position - transform.right * 5 + transform.forward * 5,
                transform.TransformDirection(Vector3.down), 5f, _layerMask))
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
        if (Physics.Raycast(transform.position + transform.right * 5 + transform.forward * 5,
                transform.TransformDirection(Vector3.down), 5f, _layerMask))
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
        if (Physics.Raycast(transform.position + transform.forward * 5, transform.TransformDirection(Vector3.down), 5f,
                _layerMask))
        {
            return true;
        }
        else
        {
            ////Debug.Log("NothingToRight");
            return false;
        }
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ResetZone")
        {
            touchingReset = true;
        }
        else if (other.tag == "EliminationZone")
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
        else if (other.gameObject.GetComponent<BotPoint>())
        {
            currentOrder = other.gameObject.GetComponent<BotPoint>().order + 1;
            if (other.gameObject.GetComponent<BotPoint>().choices.Length > 0)
            {
                pickChoice(other.gameObject.GetComponent<BotPoint>().choices);
            }
        }
    }
}
