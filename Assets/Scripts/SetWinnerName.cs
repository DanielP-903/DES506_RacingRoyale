using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetWinnerName : MonoBehaviour
{
    // WINNER NAME UI
    #region Variables
    [SerializeField]
    private TextMeshProUGUI playerNameText;
    [SerializeField]
    private TextMeshProUGUI playerLicenseText;
    [SerializeField]
    private MeshRenderer _mRend;
    [SerializeField]
    private MeshFilter _mFilt;
    #endregion

    private void Start()
    {
        _mRend = transform.Find("CarMesh").GetComponent<MeshRenderer>();
        _mFilt = transform.Find("CarMesh").GetComponent<MeshFilter>();
    }

    #region Public Methods
    public void SetWinner(string inputName, int mesh)
    {
        playerNameText.text = inputName;
        playerLicenseText.text = inputName;
        Debug.Log(this.gameObject.name);
        Debug.Log("MeshSet: "+mesh+":"+GameObject.Find("DataManager").GetComponent<DataManager>());
        Debug.Log(_mRend.material);
        _mRend.material = GameObject.Find("DataManager").GetComponent<DataManager>().GetMats()[mesh];
        _mFilt.mesh = GameObject.Find("DataManager").GetComponent<DataManager>().GetMesh()[mesh];
    }
    #endregion
    
}
