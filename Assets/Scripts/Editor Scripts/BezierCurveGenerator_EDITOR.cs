using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(BezierCurveGenerator))]
public class BezierCurveGenerator_EDITOR : Editor
{
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
        //EditorGUILayout.HelpBox("This is a help box", MessageType.Info);
    }
}
