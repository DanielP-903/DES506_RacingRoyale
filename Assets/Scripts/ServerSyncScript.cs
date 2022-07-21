using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Photon.Pun;
using UnityEngine;

public class ServerSyncScript : MonoBehaviour
{
    private GameManager _gm;
    private MessageBox _mb;
    private fadeScreen _fs;
    private bool _mbFound = false;
    // Start is called before the first frame update
    private void Start()
    {
        _fs = GameObject.Find("FadeScreen").GetComponent<fadeScreen>();
    }

    public void SetUp()
    {
        _mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        _fs = GameObject.Find("FadeScreen").GetComponent<fadeScreen>();
        Debug.Log("MessageBase: "+ _mb);
        _mbFound = true;
    }
    
    [PunRPC]
    void fadeOut()
    {
        Debug.Log("FadeScreen: " + _fs);
        _fs.fadeOut();
    }
    
    [PunRPC]
    void sendMessage(string text)
    {
        //_mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        Debug.Log("MessageBox: " + _mb + ":" + text);
        _mb.sendMessage(text);
    }
    
    [PunRPC]
    void Powerup(int id, PowerupType type, bool active)//GameObject subobj = null) 
    {
        GameObject obj = null;// = PhotonView.Find(id).transform.get;
        GameObject subobj = null;// = PhotonView.Find(id).transform.get;
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
                
                // if (subobj != null)
                //     subobj.SetActive(true);
                // else
                //     Debug.LogError("subobj in PunRPC function 'TriggerPowerup' is missing!");
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
    void UpdateAirBlast(SphereCollider col, float radius)
    {
        col.radius =  Mathf.Lerp(col.radius, radius, Time.deltaTime);;
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
    
    // [PunRPC]
    // void ResetPunchingGlove(GameObject obj, Vector3 pos)
    // {
    //     obj.transform.position = pos;
    // }

    // [PunRPC]
    // void DisablePowerup(GameObject obj, PowerupType type, GameObject subobj = null)
    // {
    //     switch (type)
    //     {
    //         case PowerupType.None:
    //             break;
    //         case PowerupType.Superboost:
    //             break;
    //         case PowerupType.BouncyWallShield:
    //             obj.SetActive(false);
    //             break;
    //         case PowerupType.AirBlast:
    //             obj.SetActive(false);
    //             obj.GetComponent<SphereCollider>().radius = 2;
    //             break;
    //         case PowerupType.GrapplingHook:
    //             obj.SetActive(false);
    //             break;
    //         case PowerupType.PunchingGlove:
    //             obj.SetActive(false);
    //             if (subobj != null)
    //                 subobj.SetActive(false);
    //             else
    //                 Debug.LogError("subobj in PunRPC function 'DisablePowerup' is missing!");
    //             break;
    //         case PowerupType.WarpPortal:
    //             obj.SetActive(false);
    //             break;
    //         default:
    //             throw new ArgumentOutOfRangeException(nameof(type), type, null);
    //     }
    // }

}
