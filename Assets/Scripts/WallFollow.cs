using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class WallFollow : MonoBehaviour
{
    public List<GameObject> routes = new List<GameObject>();

    public float chaseSpeed = 1.0f;
    private BezierCurveGenerator _bezierCurveGenerator;

    public int routeNumber = 0;

    private float _tValue;
    
    
    // Start is called before the first frame update
    void Start()
    {
        routeNumber = 0;
        _tValue = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 p0 = routes[0].transform.GetChild(0).position;
        Vector3 p1 = routes[0].transform.GetChild(1).position;
        Vector3 p2 = routes[0].transform.GetChild(2).position;
        Vector3 p3 = routes[0].transform.GetChild(3).position;

 
        _tValue += Time.deltaTime * chaseSpeed;

        Vector3 oldPosition = transform.position;
        Vector3 newPosition = 
            Mathf.Pow(1 - _tValue, 3) * p0 +
            3 * Mathf.Pow(1 - _tValue, 2) * _tValue * p1 +
            3 * (1 - _tValue) * Mathf.Pow(_tValue, 2) * p2 +
            Mathf.Pow(_tValue, 3) * p3;
        
        transform.position = newPosition;
        
        Vector3 difference = newPosition - oldPosition;

        float rotationAngle = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, rotationAngle, 0);
            

        if (_tValue > 1)
        {
            _tValue = 0.0f;
            routeNumber++;
            if (routeNumber > routes.Count - 1)
            {
                routeNumber = 0;
            }
        }
        
    }

}
