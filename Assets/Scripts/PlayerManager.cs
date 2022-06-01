using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    private PhotonView _photonView;
    private CarController _dcc;
    
    [SerializeField]
    private TextMeshProUGUI playerNameText;
    [SerializeField]
    private TextMeshProUGUI playerLicenseText;
    
    private PlayerManager target;
    
    // Start is called before the first frame update
    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        _dcc = GetComponent<CarController>();
        if (_photonView.IsMine)
        {
            CinemachineVirtualCamera cvc = Camera.main.gameObject.GetComponent<CinemachineVirtualCamera>();
            var transform1 = transform;
            cvc.m_Follow = transform1;
            cvc.m_LookAt = transform1;
        }
        else
        {
            Destroy(_dcc);
        }
    }
    
    public void SetTarget(PlayerManager _target)
    {
        if (_target == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
            return;
        }
        // Cache references for efficiency
        target = _target;
        if (playerNameText != null)
        {
            playerNameText.text = target._photonView.Owner.NickName;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
