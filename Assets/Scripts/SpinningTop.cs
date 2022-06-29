using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningTop : MonoBehaviour
{
    public List<GameObject> pathNodes = new List<GameObject>();
    public float speed;
    private Vector3 _currentNodePos;
    private int _currentNodeIndex;

    private float _currentRotation;
    // Start is called before the first frame update
    void Start()
    {
        _currentNodeIndex = 0;
        _currentNodePos = pathNodes[0].transform.position;
    }

    private void OnDrawGizmos()
    {
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
        _currentRotation += Time.deltaTime*50;
        transform.localRotation = Quaternion.Euler(transform.localRotation.x, _currentRotation, transform.localRotation.z);
        
        transform.position = Vector3.MoveTowards(transform.position, _currentNodePos, Time.deltaTime * speed);
        if (transform.position == _currentNodePos)
        {
            _currentNodeIndex++;
            if (_currentNodeIndex > pathNodes.Count - 1)
            {
                _currentNodeIndex = 0;
            }
            _currentNodePos = pathNodes[_currentNodeIndex].transform.position;
        }
    }
}
