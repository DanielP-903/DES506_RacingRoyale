using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    private PhotonView _photonView;
    private CarController _dcc;
    private Rigidbody _rb;
    
    [SerializeField]
    private TextMeshProUGUI playerNameText;
    [SerializeField]
    private TextMeshProUGUI playerLicenseText;
    
    private PlayerManager target;
    private GameManager _gm;
    
    // Start is called before the first frame update
    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        _dcc = GetComponent<CarController>();
        _rb = GetComponent<Rigidbody>();
        _gm = GameObject.Find("GameManager").GetComponent<GameManager>();
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
        playerNameText.text = _photonView.Owner.NickName;
        playerLicenseText.text = _photonView.Owner.NickName;
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
        if (transform.position.y < -5)
        {
            //Debug.Log("Less than 5");
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.Euler(Vector3.zero);
            transform.position = GameObject.Find("SpawnLocation" + PhotonNetwork.LocalPlayer.ActorNumber).transform.position;
        }
    }
}
