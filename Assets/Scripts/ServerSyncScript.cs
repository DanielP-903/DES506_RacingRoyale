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
    private bool _mbFound = false;
    // Start is called before the first frame update
    /*void Start()
    {
        _mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
    }*/

    public void SetUp()
    {
        _mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        Debug.Log("MessageBase: "+ _mb);
        _mbFound = true;
    }

    [PunRPC]
    void sendMessage(string text)
    {
        //_mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        Debug.Log("MessageBox: " + _mb + ":" + text);
        _mb.sendMessage(text);
    }

    [PunRPC]
    void TriggerPowerup(GameObject obj, PowerupType type, GameObject subobj = null)
    {
        switch (type)
        {
            case PowerupType.None:
                break;
            case PowerupType.Superboost:
                break;
            case PowerupType.BouncyWallShield:
                obj.SetActive(true);
                break;
            case PowerupType.AirBlast:
                obj.SetActive(true);
                obj.GetComponent<SphereCollider>().radius = 2;
                break;
            case PowerupType.GrapplingHook:
                obj.SetActive(true);
                break;
            case PowerupType.PunchingGlove:
                obj.SetActive(true);
                if (subobj != null)
                    subobj.SetActive(true);
                else
                    Debug.LogError("subobj in PunRPC function 'TriggerPowerup' is missing!");
                break;
            case PowerupType.WarpPortal:
                obj.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    [PunRPC]
    void UpdateAirBlast(SphereCollider col, float radius)
    {
        col.radius =  Mathf.Lerp(col.radius, radius, Time.deltaTime);;
    }
    
    [PunRPC]
    void UpdateGrappleHook(LineRenderer line, Vector3[] positions)
    {
        line.SetPositions(positions);
    }

    [PunRPC]
    void UpdatePunchingGlove(LineRenderer line, Vector3[] positions)
    {
        line.SetPositions(positions);
    }
    
    [PunRPC]
    void ResetPunchingGlove(GameObject obj, Vector3 pos)
    {
        obj.transform.position = pos;
    }

    [PunRPC]
    void DisablePowerup(GameObject obj, PowerupType type, GameObject subobj = null)
    {
        switch (type)
        {
            case PowerupType.None:
                break;
            case PowerupType.Superboost:
                break;
            case PowerupType.BouncyWallShield:
                obj.SetActive(false);
                break;
            case PowerupType.AirBlast:
                obj.SetActive(false);
                obj.GetComponent<SphereCollider>().radius = 2;
                break;
            case PowerupType.GrapplingHook:
                obj.SetActive(false);
                break;
            case PowerupType.PunchingGlove:
                obj.SetActive(false);
                if (subobj != null)
                    subobj.SetActive(false);
                else
                    Debug.LogError("subobj in PunRPC function 'DisablePowerup' is missing!");
                break;
            case PowerupType.WarpPortal:
                obj.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

}
