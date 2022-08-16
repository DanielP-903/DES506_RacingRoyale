using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the generation of bezier curve paths used by the wall
/// </summary>
public class BezierCurveGenerator : MonoBehaviour
{
    public List<Transform> controlPoints;
    private Vector3 _gizmosPos;
    private float _stepValue = 0.02f;
    private int _direction = 1;
    
    /// <summary>
    /// Add a new curve to the list and define it's initial points
    /// </summary>
    public void AddNewCurve()
    {
        GameObject newControlPoint1 = new GameObject("ControlPoint_" + (controlPoints.Count));
        newControlPoint1.transform.position = controlPoints[controlPoints.Count - 1].transform.position + new Vector3(0, 0, 5 * -controlPoints[controlPoints.Count-1].transform.forward.magnitude);
        GameObject newControlPoint2 = new GameObject("ControlPoint_" + (controlPoints.Count+1));
        newControlPoint2.transform.position = controlPoints[controlPoints.Count - 1].transform.position + new Vector3(0, 0, 10 * -controlPoints[controlPoints.Count-1].transform.forward.magnitude);    
        GameObject newControlPoint3 = new GameObject("ControlPoint_" + (controlPoints.Count+2));
        newControlPoint3.transform.position = controlPoints[controlPoints.Count - 1].transform.position + new Vector3(0, 0, 15 * -controlPoints[controlPoints.Count-1].transform.forward.magnitude);

        newControlPoint1.transform.parent = transform;
        newControlPoint2.transform.parent = transform;
        newControlPoint3.transform.parent = transform;
        
        controlPoints.Add(newControlPoint1.transform);
        controlPoints.Add(newControlPoint2.transform);
        controlPoints.Add(newControlPoint3.transform);
        _direction = -_direction;
    }

    /// <summary>
    /// Remove the last curve on the list
    /// </summary>
    public void RemoveLastCurve()
    {
        if (controlPoints.Count > 4)
        {
            for (int i = 0; i < 3; i++)
            {
                DestroyImmediate(controlPoints[controlPoints.Count - 1].gameObject);
                controlPoints.RemoveAt(controlPoints.Count - 1);
            }
        }
        else
        {
            Debug.LogError("One curve must be present in the bezier curve generator!");
        }
    }
    
    /// <summary>
    /// Draw the bezier curve using gizmos for visualisation and ease of use in editor
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (_stepValue < 0.01f) _stepValue = 0.01f;
        
        for (float t = 0; t <= 1; t += _stepValue)
        {
            // Reference: https://en.wikipedia.org/wiki/B%C3%A9zier_curve
            for (int i = 0; i < controlPoints.Count - 1; i += 3)
            {
                _gizmosPos =
                    Mathf.Pow(1 - t, 3) * controlPoints[i].position +
                    3 * Mathf.Pow(1 - t, 2) * t * controlPoints[i+1].position +
                    3 * (1 - t) * Mathf.Pow(t, 2) * controlPoints[i+2].position +
                    Mathf.Pow(t, 3) * controlPoints[i+3].position;
                Gizmos.DrawSphere(_gizmosPos, 0.2f);
            }

        }

        for (int i = 0; i < controlPoints.Count - 1; i += 3)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                new Vector3(controlPoints[i].position.x, controlPoints[i].position.y, controlPoints[i].position.z),
                new Vector3(controlPoints[i+1].position.x, controlPoints[i+1].position.y, controlPoints[i+1].position.z));

            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                new Vector3(controlPoints[i+2].position.x, controlPoints[i+2].position.y, controlPoints[i+2].position.z),
                new Vector3(controlPoints[i+3].position.x, controlPoints[i+3].position.y, controlPoints[i+3].position.z));
        }

        for (var index = 0; index < controlPoints.Count; index++)
        {
            var point = controlPoints[index];
            Gizmos.color = Color.yellow;
            if (index % 3 == 0)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(point.transform.position, 0.5f);
            }
            else
            {
                Gizmos.DrawSphere(point.transform.position, 0.3f);
            }
        }
    }
}
