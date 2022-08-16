using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the spinning top obstacle functionality
/// </summary>
public class SpinningTop : MonoBehaviour
{
    [SerializeField] private List<GameObject> pathNodes = new List<GameObject>();
    [SerializeField] private float speed;
    
    private Vector3 _currentNodePos;
    private float _currentRotation;
    private int _currentNodeIndex;

    // Start is called before the first frame update
    void Start()
    {
        _currentNodeIndex = 0;
        _currentNodePos = pathNodes[0].transform.position;
    }

    private void OnDrawGizmos()
    {
        // Draw Lines and spheres between path nodes that make up the route for the spinning top to follow
        Gizmos.color = Color.green;
        for (var index = 0; index < pathNodes.Count; index++)
        {
            var pathNode = pathNodes[index];
            Gizmos.DrawSphere(pathNode.transform.position, 0.5f);
            if (index + 1 < pathNodes.Count)
            {
                var nextNode = pathNodes[index+1];
                Gizmos.DrawLine(pathNode.transform.position, nextNode.transform.position);
            }
            else
            {
                var nextNode = pathNodes[0];
                Gizmos.DrawLine(pathNode.transform.position, nextNode.transform.position);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Spin the top
        _currentRotation += Time.deltaTime*1000;
        transform.localRotation = Quaternion.Euler(transform.localRotation.x, _currentRotation, transform.localRotation.z);
        
        // Move the top towards the next node in the path
        transform.position = Vector3.MoveTowards(transform.position, _currentNodePos, Time.deltaTime * speed);

        // Handle node loopback system
        if (transform.position != _currentNodePos) return;
        
        _currentNodeIndex++;
        if (_currentNodeIndex > pathNodes.Count - 1)
        {
            _currentNodeIndex = 0;
        }
        _currentNodePos = pathNodes[_currentNodeIndex].transform.position;
    }
}
