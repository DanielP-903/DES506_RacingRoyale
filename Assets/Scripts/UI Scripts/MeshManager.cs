using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    const string playerMeshPrefKey = "Skin";
    private ExitGames.Client.Photon.Hashtable playerCustomProperties;
    
    private Mesh[] meshArray;
    private Material[] matArray;

    private DataManager dm;

    private MeshRenderer carMeshRend;
    private MeshFilter carMeshFilt;
    private GameObject flaps;
    private int meshMatNum = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        dm = GameObject.Find("DataManager").GetComponent<DataManager>();
        meshArray = dm.GetMesh();
        matArray = dm.GetMats();
        carMeshRend = GameObject.Find("Selector Car").transform.Find("CarMesh").GetComponent<MeshRenderer>();
        carMeshFilt = GameObject.Find("Selector Car").transform.Find("CarMesh").GetComponent<MeshFilter>();
        flaps = GameObject.Find("Selector Car").transform.Find("Flaps").gameObject;
        meshMatNum = PlayerPrefs.HasKey(playerMeshPrefKey) ? PlayerPrefs.GetInt(playerMeshPrefKey) : 0;
        playerCustomProperties = new ExitGames.Client.Photon.Hashtable();
        setSkin(meshMatNum);
        flaps.SetActive(meshMatNum < 3);
    }

    public void nextSkin()
    {
        meshMatNum+=4;
        if (meshMatNum > meshArray.Length-1)
        {
            meshMatNum-=8;
        }

        flaps.SetActive(meshMatNum < 3);
        setSkin(meshMatNum);
    }

    public void previousSkin()
    {
        meshMatNum-=4;
        if (meshMatNum < 0)
        {
            meshMatNum+=8;
        }

        flaps.SetActive(meshMatNum < 3);
        setSkin(meshMatNum);
    }
    
    public void nextColor()
    {
        meshMatNum++;
        if (meshMatNum % 4 == 0)
        {
            meshMatNum -= 4;
        }
        setSkin(meshMatNum);
    }

    public void previousColor()
    {
        meshMatNum--;
        if ((meshMatNum+1)%4 == 0)
        {
            meshMatNum += 4;
        }
        setSkin(meshMatNum);
    }


    void setSkin(int value)
    {
        PlayerPrefs.SetInt(playerMeshPrefKey, value);
        playerCustomProperties["Skin"] = value;
        carMeshRend.material = matArray[value];
        carMeshFilt.mesh = meshArray[value];
        PhotonNetwork.LocalPlayer.CustomProperties = playerCustomProperties;
    }
}