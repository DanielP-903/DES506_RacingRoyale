using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class BezierCurveGenerator : MonoBehaviour
{
    private Vector3 _gizmosPos;
    public List<Transform> controlPoints;
    
    // [Header("Adjust Speed Between Length of Curve")]
    // [Tooltip("Step size between each generated point in the bezier sphere")]
    // public float stepValue = 0.05f;
    private float stepValue = 0.02f;

    private int direction = 1;
    
    public void AddNewCurve()
    {
        GameObject newControlPoint1 = new GameObject("ControlPoint_" + (controlPoints.Count));
        newControlPoint1.transform.position = controlPoints[controlPoints.Count - 1].transform.position + new Vector3(0, 0, 10 * direction);
        GameObject newControlPoint2 = new GameObject("ControlPoint_" + (controlPoints.Count+1));
        newControlPoint2.transform.position = controlPoints[controlPoints.Count - 1].transform.position + new Vector3(10, 0, 10 * direction);    
        GameObject newControlPoint3 = new GameObject("ControlPoint_" + (controlPoints.Count+2));
        newControlPoint3.transform.position = controlPoints[controlPoints.Count - 1].transform.position + new Vector3(10, 0, 0);

        newControlPoint1.transform.parent = transform;
        newControlPoint2.transform.parent = transform;
        newControlPoint3.transform.parent = transform;
        
        controlPoints.Add(newControlPoint1.transform);
        controlPoints.Add(newControlPoint2.transform);
        controlPoints.Add(newControlPoint3.transform);
        direction = -direction;
    }

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
    
    
    private void OnDrawGizmos()
    {
        if (stepValue < 0.01f) stepValue = 0.01f;
        
        for (float t = 0; t <= 1; t += stepValue)
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
            Gizmos.DrawLine(
                new Vector3(controlPoints[i].position.x, controlPoints[i].position.y, controlPoints[i].position.z),
                new Vector3(controlPoints[i+1].position.x, controlPoints[i+1].position.y, controlPoints[i+1].position.z));

            Gizmos.DrawLine(
                new Vector3(controlPoints[i+2].position.x, controlPoints[i+2].position.y, controlPoints[i+2].position.z),
                new Vector3(controlPoints[i+3].position.x, controlPoints[i+3].position.y, controlPoints[i+3].position.z));
        }
    }
}
