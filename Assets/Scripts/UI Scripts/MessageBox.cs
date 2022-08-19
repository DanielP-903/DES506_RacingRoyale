using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

/// <summary>
/// Handler for all messages in the top right of the screen
/// </summary>
/// <returns></returns>
public class MessageBox : MonoBehaviour
{
    private Queue<MessageStruct> messages;
    private TextMeshProUGUI[] messageBoxes;
    [SerializeField] private float timeBeforeDecay = 10f;
    
    /// <summary>
    /// Sets up relevant components and structs
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Updates the opacity of messages and whether the correct text is present
    /// </summary>
    /// <returns></returns>
    private void Update()
    {
        int counter = 0;
        if (messages != null && messages.Count > 0)
        {
            int messagesToDequeue = 0;
            foreach (MessageStruct ms in messages)
            {
                if (ms.timeSet + timeBeforeDecay < Time.time && ms.timeSet + timeBeforeDecay + 1 > Time.time)
                {
                    messageBoxes[counter].alpha = 1 - ((Time.time - (ms.timeSet + 10)) );
                }
                else if (ms.timeSet + timeBeforeDecay + 1 < Time.time)
                {
                    messageBoxes[counter].alpha = 0;
                    messagesToDequeue++;
                }

                counter++;
            }

            while (messagesToDequeue > 0)
            {
                messages.Dequeue();
                messagesToDequeue--;
            }
            UpdateMessages();
        }
    }

    /// <summary>
    /// Update messages to contain the correct text
    /// </summary>
    /// <returns></returns>
    void UpdateMessages()
    {
        int counter = 0;
        foreach (MessageStruct ms in messages)
        {
            //Debug.Log("MessageCount: " + counter);
            messageBoxes[counter].text = ms.messageText;
            //Debug.Log("MSG BOX: "+messageBoxes[counter].text+" MSG: "+ms.messageText);
            counter++;
            
        }

        while (counter < 5)
        {
            messageBoxes[counter].text = "";
            counter++;
        }
    }
    
    /// <summary>
    /// Adds message to the message box, removing one if there are already 5 messages
    /// </summary>
    /// <param name="text">Message to be added to the message box</param>
    /// <returns></returns>
    public void sendText(string text)
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

        if (messages.Count == 5)
        {
            messages.Dequeue();
        }
        //Debug.Log("Message to Queue: "+text);
        messages.Enqueue(new MessageStruct(text, Time.time));
        UpdateMessages();
    }
}

/// <summary>
/// Contains message information
/// </summary>
/// <returns></returns>
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
