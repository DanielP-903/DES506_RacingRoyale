using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurveGenerator : MonoBehaviour
{
    private Vector3 _gizmosPos;
    public Transform[] controlPoints;
    
    [Header("Adjust Speed Between Length of Curve")]
    [Tooltip("Step size between each generated point in the bezier sphere")]
    public float stepValue = 0.05f;
    
    private void OnDrawGizmos()
    {
        for (float t = 0; t <= 1; t += 0.05f)
        {
            // Reference: https://en.wikipedia.org/wiki/B%C3%A9zier_curve
            _gizmosPos =
                Mathf.Pow(1 - t, 3) * controlPoints[0].position +
                3 * Mathf.Pow(1 - t, 2) * t * controlPoints[1].position +
                3 * (1 - t) * Mathf.Pow(t, 2) * controlPoints[2].position +
                Mathf.Pow(t, 3) * controlPoints[3].position;
            
            Gizmos.DrawSphere(_gizmosPos, 0.2f);
        }
        
        
        Gizmos.DrawLine(
            new Vector3(controlPoints[0].position.x, controlPoints[0].position.y, controlPoints[0].position.z),
            new Vector3(controlPoints[1].position.x, controlPoints[1].position.y, controlPoints[1].position.z));
        
        Gizmos.DrawLine(
            new Vector3(controlPoints[2].position.x, controlPoints[2].position.y, controlPoints[2].position.z),
            new Vector3(controlPoints[3].position.x, controlPoints[3].position.y, controlPoints[3].position.z));
    }
}
