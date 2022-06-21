using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    const string playerMeshPrefKey = "PlayerMeshMat";

    [SerializeField]
    private Mesh[] meshArray;
    [SerializeField]
    private Material[] matArray;

    private MeshRenderer carMeshRend;
    private int meshMatNum = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        carMeshRend = GameObject.Find("Selector Car").transform.Find("CarMesh").GetComponent<MeshRenderer>();
        if (PlayerPrefs.HasKey(playerMeshPrefKey))
        {
            meshMatNum = PlayerPrefs.GetInt(playerMeshPrefKey);
        }
        else
        {
            meshMatNum = 0;
        }
        setSkin(meshMatNum);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void nextSkin()
    {
        
    }

    public void previousSkin()
    {
        
    }

    void setSkin(int value)
    {
        PlayerPrefs.SetInt(playerMeshPrefKey, value);
    }
}