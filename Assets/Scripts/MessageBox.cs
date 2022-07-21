using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class MessageBox : MonoBehaviour
{
    private Queue<MessageStruct> messages;
    private TextMeshProUGUI[] messageBoxes;
    [SerializeField] private float timeBeforeDecay = 10f;
    
    // Start is called before the first frame update
    void Start()
    {
        //messages.Enqueue("StartMessage");
        //Debug.Log("Messages: "+messages.ToArray());
        if (messageBoxes == null)
        {
            messageBoxes = transform.GetChild(0).GetChild(0).GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI tmp in messageBoxes)
            {
                tmp.text = "";
            }
        }
    }

    private void Update()
    {
        if (messages != null && messages.Count > 0)
        {
            foreach (MessageStruct ms in messages)
            {
                if (ms.timeSet + timeBeforeDecay < Time.time)
                {
                    StartCoroutine(fadeMessage(ms));
                }
            }
        }
    }

    void UpdateMessages()
    {
        int counter = 0;
        foreach (MessageStruct ms in messages)
        {
            Debug.Log("MessageCount: " + counter);
            messageBoxes[counter].text = ms.messageText;
            Debug.Log("MSG BOX: "+messageBoxes[counter].text+" MSG: "+ms.messageText);
            counter++;
        }
    }
    
    public void sendMessage(string text)
    {
        if (messages == null)
        {
            messages = new Queue<MessageStruct>(5);
            messageBoxes = transform.GetChild(0).GetChild(0).GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI tmp in messageBoxes)
            {
                tmp.text = "";
            }
        }
        //Debug.Log("Message to Queue: "+text);
        messages.Enqueue(new MessageStruct(text, Time.time));
        UpdateMessages();
    }

    IEnumerator fadeMessage(MessageStruct trackedMessage)
    {
        if (messageBoxes.Length < 1)
        {
            messageBoxes = transform.GetChild(0).GetChild(0).GetComponentsInChildren<TextMeshProUGUI>();
        }
        int counter = 0;
        TextMeshProUGUI trackedTMP = null;
        foreach (MessageStruct ms in messages)
        {
            if (ms == trackedMessage)
            {
                trackedTMP = messageBoxes[counter];
                break;
            }
            counter++;
        }

        if (trackedTMP != null)
        {
            while (messages.Contains(trackedMessage) && trackedTMP.alpha > 0)
            {
                trackedTMP.alpha = trackedTMP.alpha - 0.01f;
                foreach (MessageStruct ms in messages)
                {
                    if (ms == trackedMessage)
                    {
                        trackedTMP = messageBoxes[counter];
                        break;
                    }

                    counter++;
                }

                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            Debug.LogError("Tracked TMP Not Found");
        }
    }
}

public class MessageStruct
{
    public string messageText;
    public float timeSet;

    public MessageStruct(string newMessage, float newTime)
    {
        messageText = newMessage;
        timeSet = newTime;
    }
}
