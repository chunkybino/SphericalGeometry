using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(Rigidbody4D))]
public class Rigidbody4D_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        Rigidbody4D rb = (Rigidbody4D)target;

        base.OnInspectorGUI();

        rb.isStatic = EditorGUILayout.Toggle("Is Static", rb.isStatic);
    }
}
