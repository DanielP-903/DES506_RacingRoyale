using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class SetCarName : MonoBehaviour
{
    private TextMeshProUGUI _name;

    private void Start()
    {
        _name = transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        _name.text = PhotonNetwork.NickName;
    }
}
