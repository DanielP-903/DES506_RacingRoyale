using UnityEngine;

/// <summary>
/// Data Manager keeps all universal data such as meshs and materials of cars stored in one location
/// </summary>
/// <returns></returns>
public class DataManager : MonoBehaviour
{
    [SerializeField] private Mesh[] meshArray;
    [SerializeField] private Material[] matArray;

    private static GameObject instance;

    /// <summary>
    /// Initiliase this data manager as an instance if one doesn't already exist or destroy self
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Get the stored array of meshes
    /// </summary>
    /// <returns>Array of meshes</returns>
    public Mesh[] GetMesh()
    {
        return meshArray;
    }

    /// <summary>
    /// Get the stored array of materials
    /// </summary>
    /// <returns>Array of materials</returns>
    public Material[] GetMats()
    {
        return matArray;
    }
}
