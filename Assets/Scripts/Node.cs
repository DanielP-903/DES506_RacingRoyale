using UnityEngine;

public class Node : MonoBehaviour
{
    public Node parent;
    public Vector3Int gridPosition;
    public float f;
    public float g;
    public float h;
    public int nodeType;
    
    public Node()
    {
        nodeType = 0;
        f = 0;
        g = 0;
        h = 0;
        gridPosition = new Vector3Int(-1, -1, -1);
    }
}