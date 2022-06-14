using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetWinnerName : MonoBehaviour
{
    // WINNER NAME UI
    #region Serializable Variables
    [SerializeField]
    private TextMeshProUGUI playerNameText;
    [SerializeField]
    private TextMeshProUGUI playerLicenseText;
    #endregion

    #region Public Methods
    public void SetName(string inputName)
    {
        playerNameText.text = inputName;
        playerLicenseText.text = inputName;
    }
    #endregion
    
}
