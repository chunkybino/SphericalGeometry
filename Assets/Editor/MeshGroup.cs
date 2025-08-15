using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "ScriptableObject/MeshGroup", fileName = "MeshGroup")]
public class MeshGroup : ScriptableObject
{
    new public string name;

    public Mesh parentMesh;

    public bool pauseSub;

    public int subdivisions;
    public bool updateSub;

    public Mesh[] allMesh;

    public string filePath = "Assets/Meshes/";

    void OnValidate()
    {
        if (subdivisions < 0) subdivisions = 0;

        if (updateSub)
        {
            updateSub = false;

            if (!parentMesh) return;

            Mesh[] newMeshes = new Mesh[subdivisions+1];
            newMeshes[0] = parentMesh; 

            for (int i = 1; i < newMeshes.Length; i++)
            {
                Mesh currentMesh = i < allMesh.Length ? allMesh[i] : MakeMesh(i);
                Vector3[] currentVertex = newMeshes[i-1].vertices; 
                Vector2[] currentUV = newMeshes[i-1].uv; 
                int[] currentTri = newMeshes[i-1].triangles; 

                MeshMaker.SubdivideMesh3(ref currentVertex, ref currentUV, ref currentTri);

                currentMesh.vertices = currentVertex;
                currentMesh.uv = currentUV;
                currentMesh.triangles = currentTri;

                newMeshes[i] = currentMesh;

                AssetDatabase.SaveAssets();
            }

            for (int i = newMeshes.Length; i < allMesh.Length; i++)
            {
                DestroyImmediate(allMesh[i], true);
            }

            allMesh = newMeshes;
        }
    }

    Mesh MakeMesh(int division)
    {
        Mesh mesh = new Mesh();
        AssetDatabase.CreateAsset(mesh, filePath+name+"_Sub"+division+".asset");
        return mesh;
    }

    public Mesh GetMeshFromDistance(float dis)
    {
        if (pauseSub) return parentMesh;

        int index = Mathf.FloorToInt((dis/Mathf.PI)*allMesh.Length);
        if (index >= allMesh.Length) index = allMesh.Length-1; 
        return allMesh[index];
    }
}
