using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class MeshManager : MonoBehaviour
{
    const string playerMeshPrefKey = "Skin";
    
    private Mesh[] meshArray;
    private Material[] matArray;

    private DataManager dm;

    private MeshRenderer carMeshRend;
    private MeshFilter carMeshFilt;
    private int meshMatNum = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        dm = GameObject.Find("DataManager").GetComponent<DataManager>();
        meshArray = dm.GetMesh();
        matArray = dm.GetMats();
        carMeshRend = GameObject.Find("Selector Car").transform.Find("CarMesh").GetComponent<MeshRenderer>();
        carMeshFilt = GameObject.Find("Selector Car").transform.Find("CarMesh").GetComponent<MeshFilter>();
        meshMatNum = PlayerPrefs.HasKey(playerMeshPrefKey) ? PlayerPrefs.GetInt(playerMeshPrefKey) : 0;
        setSkin(meshMatNum);
    }

    public void nextSkin()
    {
        meshMatNum++;
        if (meshMatNum > meshArray.Length-1)
        {
            meshMatNum = 0;
        }
        setSkin(meshMatNum);
    }

    public void previousSkin()
    {
        meshMatNum--;
        if (meshMatNum < 0)
        {
            meshMatNum = meshArray.Length-1;
        }
        setSkin(meshMatNum);
    }

    void setSkin(int value)
    {
        PlayerPrefs.SetInt(playerMeshPrefKey, value);
        carMeshRend.material = matArray[value];
        carMeshFilt.mesh = meshArray[value];
    }
}