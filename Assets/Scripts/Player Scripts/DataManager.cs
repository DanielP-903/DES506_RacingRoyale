using UnityEngine;


public class DataManager : MonoBehaviour
{
    [SerializeField] private Mesh[] meshArray;
    [SerializeField] private Material[] matArray;

    private static GameObject instance;

    // Start is called before the first frame update
    void Start()
    {
        if (!instance)
        {
            instance = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
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
