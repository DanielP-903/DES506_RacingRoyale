using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Syncs visual information and interactions between players
/// </summary>
/// <returns></returns>
public class ServerSyncScript : MonoBehaviourPunCallbacks, IPunObservable
{

    #region IPunObservable implementation

    public bool completed;
    
    /// <summary>
    /// On serialization for the server, send data on whether the current owner has completed the stage
    /// </summary>
    /// <returns></returns>
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
    //private MessageBox _mb;
    private fadeScreen _fs;
    private bool _mbFound = false;
    private DataManager _dm;
    private Mesh[] meshArray;

    /// <summary>
    /// On awake, find relevant components
    /// </summary>
    /// <returns></returns>
    private void Awake()
    {
        if (!debugMode)
        {
            _dm = GameObject.Find("DataManager").GetComponent<DataManager>();
            meshArray = _dm.GetMesh();
            //_mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        }
    }

    /// <summary>
    /// On start, find relevant components
    /// </summary>
    /// <returns></returns>
    private void Start()
    {
        if (!debugMode)
        {
            _fs = GameObject.Find("FadeScreen").GetComponent<fadeScreen>();
            //_mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        }
    }

    /// <summary>
    /// On setup, find relevant components
    /// </summary>
    /// <returns></returns>
    public void SetUp()
    {
        //_mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        _fs = GameObject.Find("FadeScreen").GetComponent<fadeScreen>();
        //Debug.Log("MessageBase: "+ _mb);
        _mbFound = true;
    }

    /// <summary>
    /// RPC method where when called causes the screen to fade out and fade in to the connecting screen
    /// </summary>
    /// <returns></returns>
    [PunRPC]
    void fadeOut()
    {
        //Debug.Log("FadeScreen: " + _fs);
        if (_fs)
            _fs.fadeOut();
    }

    /// <summary>
    /// Old RPC for message boxes (Obselete)
    /// </summary>
    /// /// <summary>
    /// <param name="text">Message to be sent</param>
    /// <returns></returns>
    //[PunRPC]
    void sendComment(string text)
    {
        //_mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        //Debug.Log("MessageToDisplay: " + text);
        //Debug.Log("MessageBox: " + _mb + ":" + text);
        //_mb.sendText(text);
    }
    
    /// <summary>
    /// Called to display powerup usage from another player
    /// </summary>
    /// <param name="id">Player ID of powerup user</param>
    /// <param name="type">Powerup Type</param>
    /// <param name="active">Whether the object for the powerup should be set to active</param>
    /// <returns></returns>
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

    /// <summary>
    /// Updates the effects of the airblast for all nearby players
    /// </summary>
    /// <param name="id">Player ID of air blast user</param>
    /// <param name="radius">The radius affected by the airblast</param>
    /// <returns></returns>
    [PunRPC]
    void UpdateAirBlast(int id, float radius)
    {
        SphereCollider col = PhotonView.Find(id).transform.GetChild(1).GetComponent<SphereCollider>();
        col.radius = Mathf.Lerp(col.radius, radius, Time.deltaTime);
    }

    /// <summary>
    /// Updates the effects of the grapplehook for all nearby players
    /// </summary>
    /// <param name="id">The player ID of the player who used the grapplehook</param>
    /// <param name="positions">An array of the positions of the player using it and the target affected player</param>
    /// <returns></returns>
    [PunRPC]
    void UpdateGrappleHook(int id, Vector3[] positions)
    {
        PhotonView.Find(id).transform.GetChild(2).GetComponent<LineRenderer>().SetPositions(positions);
    }

    /// <summary>
    /// Updates the effects of the boxing glove for all nearby players
    /// </summary>
    /// <param name="id">The player ID of the player who used the boxing glove</param>
    /// <param name="positions">An array of the positions of the player using it and the target affected player</param>
    /// <returns></returns>
    [PunRPC]
    void UpdatePunchingGlove(int id, Vector3[] positions)
    {
        PhotonView.Find(id).transform.GetChild(2).GetComponent<LineRenderer>().SetPositions(positions);
    }
    
    /// <summary>
    /// Updates the effects of the boxing glove hit for all nearby players
    /// </summary>
    /// <param name="id">The player ID of the player who has been hit by the boxing glove</param>
    /// <param name="positions">The hit position of the target affected player</param>
    /// <returns></returns>
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
    
    /// <summary>
    /// Updates the effects of collision for two players who have collided
    /// </summary>
    /// <param name="id">The player ID of the player who hit the other</param>
    /// <param name="direction">Direction of hitting player</param>
    /// <param name="contactPoint">Point of hit occuring</param>
    /// <param name="bounciness">How much force is applied upon collision</param>
    /// <returns></returns>
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

    /// <summary>
    /// Updates the outline of a player for powerup usage based on their skin
    /// </summary>
    /// <param name="id">The player ID of the player who is to have an outline</param>
    /// <param name="meshIndex">Skin Number</param>
    /// <returns></returns>
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
