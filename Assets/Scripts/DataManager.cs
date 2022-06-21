using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField]
    private Mesh[] meshArray;
    [SerializeField]
    private Material[] matArray;
    
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public Mesh[] GetMesh()
    {
        return meshArray;
    }
    
    public Material[] GetMats()
    {
        return matArray;
    }
}
