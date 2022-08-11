using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

public class ServerSyncScript : MonoBehaviourPunCallbacks, IPunObservable, IOnEventCallback
{
    #region ServerEventSystem

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == 1)
        {
            string data = (string)photonEvent.CustomData;
            
            sendComment(data);
        }
    }

    #endregion
    
    
    #region IPunObservable implementation

    public bool completed;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            completed = false;
            if (_gm)
            {
                completed = _gm._completed;
            }
            stream.SendNext(completed);
        }
        else
        {
            // Network player, receive data
            this.completed = (bool)stream.ReceiveNext();
        }
    }
    #endregion
    
    [SerializeField] private bool debugMode;
    private GameManager _gm;
    private MessageBox _mb;
    private fadeScreen _fs;
    private bool _mbFound = false;
    private DataManager _dm;
    private Mesh[] meshArray;

    private void Awake()
    {
        if (!debugMode)
        {
            _dm = GameObject.Find("DataManager").GetComponent<DataManager>();
            meshArray = _dm.GetMesh();
            _mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (!debugMode)
        {
            _fs = GameObject.Find("FadeScreen").GetComponent<fadeScreen>();
            _mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        }
    }

    public void SetUp()
    {
        _mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        _fs = GameObject.Find("FadeScreen").GetComponent<fadeScreen>();
        //Debug.Log("MessageBase: "+ _mb);
        _mbFound = true;
    }

    [PunRPC]
    void fadeOut()
    {
        //Debug.Log("FadeScreen: " + _fs);
        if (_fs)
            _fs.fadeOut();
    }

    [PunRPC]
    void sendComment(string text)
    {
        //_mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        //Debug.Log("MessageToDisplay: " + text);
        //Debug.Log("MessageBox: " + _mb + ":" + text);
        _mb.sendText(text);
    }

    [PunRPC]
    void Powerup(int id, PowerupType type, bool active) //GameObject subobj = null) 
    {
        GameObject obj = null; // = PhotonView.Find(id).transform.get;
        GameObject subobj = null; // = PhotonView.Find(id).transform.get;
        switch (type)
        {
            case PowerupType.None:
                break;
            case PowerupType.Superboost:
                break;
            case PowerupType.BouncyWallShield:
                obj = PhotonView.Find(id).transform.GetChild(0).gameObject;
                break;
            case PowerupType.AirBlast:
                obj = PhotonView.Find(id).transform.GetChild(1).gameObject;
                obj.GetComponent<SphereCollider>().radius = 2;
                break;
            case PowerupType.GrapplingHook:
                obj = PhotonView.Find(id).transform.GetChild(2).gameObject;
                break;
            case PowerupType.PunchingGlove:
                obj = PhotonView.Find(id).transform.GetChild(3).gameObject;
                subobj = PhotonView.Find(id).transform.GetChild(2).gameObject;
                break;
            case PowerupType.WarpPortal:
                obj = PhotonView.Find(id).transform.GetChild(4).gameObject;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        if (obj)
            obj.SetActive(active);
        else
            Debug.LogError("obj is null in 'TriggerPowerup'!");

        if (subobj)
            subobj.SetActive(active);
    }

    [PunRPC]
    void UpdateAirBlast(int id, float radius)
    {
        SphereCollider col = PhotonView.Find(id).transform.GetChild(1).GetComponent<SphereCollider>();
        col.radius = Mathf.Lerp(col.radius, radius, Time.deltaTime);
        ;
    }

    [PunRPC]
    void UpdateGrappleHook(int id, Vector3[] positions)
    {
        PhotonView.Find(id).transform.GetChild(2).GetComponent<LineRenderer>().SetPositions(positions);
    }

    [PunRPC]
    void UpdatePunchingGlove(int id, Vector3[] positions)
    {
        PhotonView.Find(id).transform.GetChild(2).GetComponent<LineRenderer>().SetPositions(positions);
    }
    
    [PunRPC]
    void PunchingGloveHit(int id, Vector3 hitPosition)
    {
        // Reset line between them
        Vector3[] positions = new Vector3[2];
        positions[0] = PhotonView.Find(id).transform.position + PhotonView.Find(id).transform.forward;
        positions[1] = PhotonView.Find(id).transform.position + PhotonView.Find(id).transform.forward;
            
        PhotonView.Find(id).transform.GetChild(2).GetComponent<LineRenderer>().SetPositions(positions);
        PhotonView.Find(id).transform.GetChild(3).transform.position = positions[1];
        PhotonView.Find(id).gameObject.GetComponent<CarVFXHandler>().PlayVFXAtPosition("PunchImpact",hitPosition);
        
    }
    
    [PunRPC]
    void PlayerHit(int id, Vector3 direction, Vector3 contactPoint, float bounciness)
    {
        GameObject target = PhotonView.Find(id).gameObject;

        if (!target)
        {
            Debug.Log("Target not found via id! ID: " + id);
        }

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (!rb)
        {
            Debug.Log("Rigidbody not found! ID: " + id);
        }

        rb.velocity = -(direction.normalized * (bounciness / 10));
        Debug.Log("HIT ANOTHER PLAYER WITH RIGIDBODY VELOCITY: " + rb.velocity);
        target.GetComponent<CarVFXHandler>().PlayVFXAtPosition("Impact", contactPoint);
        int rand = Random.Range(1, 5);
    }

    [PunRPC]
    void UpdateOutlineMeshes(int id, int meshIndex)
    {
        GameObject target = PhotonView.Find(id).gameObject;
        GameObject _outlineObject = target.transform.GetChild(6).gameObject;
        GameObject _outlineObjectGrapple = target.transform.GetChild(7).gameObject;
        _outlineObject.GetComponent<MeshFilter>().sharedMesh =
            meshArray[(int)PhotonNetwork.LocalPlayer.CustomProperties["Skin"]];
        _outlineObjectGrapple.GetComponent<MeshFilter>().sharedMesh =
            meshArray[(int)PhotonNetwork.LocalPlayer.CustomProperties["Skin"]];
    }
}
