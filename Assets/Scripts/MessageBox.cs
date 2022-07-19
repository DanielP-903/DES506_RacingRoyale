using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class MessageBox : MonoBehaviour
{
    private Queue<string> messages;
    private TextMeshProUGUI[] messageBoxes;
    // Start is called before the first frame update
    void Start()
    {
        messageBoxes = transform.GetChild(0).GetChild(0).GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI tmp in messageBoxes)
        {
            tmp.text = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    void sendMessage(string text)
    {
        
    }
}
