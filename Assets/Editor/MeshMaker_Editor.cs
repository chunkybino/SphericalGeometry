using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(MeshMaker))]
public class MeshMaker_Editor : Editor
{
    MeshMaker meshMaker;

    public override void OnInspectorGUI()
    {
        meshMaker = (MeshMaker)target;

        base.OnInspectorGUI();

        if (GUILayout.Button("MakeNew")) {
            meshMaker.mesh = new Mesh();

            AssetDatabase.CreateAsset(meshMaker.mesh, "Assets/Meshes/"+meshMaker.makeNewName+".asset");
        }

        if (GUILayout.Button("SaveAsset")) {
            AssetDatabase.SaveAssets();
        }
    }
}
