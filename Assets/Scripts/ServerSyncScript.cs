using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ServerSyncScript : MonoBehaviour
{
    private MessageBox _mb;
    // Start is called before the first frame update
    void Start()
    {
        _mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
    }

    void OnLevelWasLoaded()
    {
        _mb = GameObject.Find("MessageBox").GetComponent<MessageBox>();
    }

    [PunRPC]
    void sendMessage(string text)
    {
        Debug.Log("MessageBox: "+_mb+":"+text);
        _mb.sendMessage(text);
    }
}
