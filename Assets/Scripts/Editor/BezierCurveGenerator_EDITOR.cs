#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script for the bezier curve creation tool
/// </summary>
[CustomEditor(typeof(BezierCurveGenerator))]
public class BezierCurveGenerator_EDITOR : Editor
{
    /// <summary>
    /// Define the inspector UI for the tool
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BezierCurveGenerator generator = (BezierCurveGenerator)target;

        if (GUILayout.Button("Add New Curve"))
        {
            generator.AddNewCurve();
        }
        if (GUILayout.Button("Remove Last Curve"))
        {
            generator.RemoveLastCurve();
        }
    }
}
#endif
