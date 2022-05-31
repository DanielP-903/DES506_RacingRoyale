using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PhotonView _photonView;
    // Start is called before the first frame update
    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        if (_photonView.IsMine)
        {
            CinemachineVirtualCamera cvc = Camera.main.gameObject.GetComponent<CinemachineVirtualCamera>();
            var transform1 = transform;
            cvc.m_Follow = transform1;
            cvc.m_LookAt = transform1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
