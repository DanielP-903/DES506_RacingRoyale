using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Photon.Pun;
using UnityEngine;

public class ServerSyncScript : MonoBehaviour
{
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
    void Trigger
    
}
