using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpectateButtons : MonoBehaviour
{
    private GameManager gm;
    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.SetSpectateMenu(this.gameObject);
        //this.gameObject.SetActive(false);
    }

    public void SpectateNext()
    {
        gm.ChangeSpectateTarget(true);
    }

    public void SpectatePrevious()
    {
        gm.ChangeSpectateTarget(false);
    }
}
