using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MeshMaker : MonoBehaviour
{
    public Vector3[] vertices;
    public Vector2[] uv;

    public Vector3Int[] triangles;
    int[] trianglesInt;

    public bool makeNew;
    public string makeNewName = "NewMesh";

    public Mesh mesh;

    public bool saveMesh;
    public bool saveTri;

    public bool doVertex4;
    public Vector4[] vertices4;

    void OnValidate()
    {
        if (makeNew) {
            makeNew = false;

            mesh = new Mesh();

            AssetDatabase.CreateAsset(mesh, "Assets/Meshes/"+makeNewName+".asset");
        }

        if (saveMesh) {
            saveMesh = false;
            if (!mesh) return;

            if (!doVertex4)
            {
                mesh.vertices = vertices;
                mesh.uv = uv;
            }
            else
            {
                SetVertex4();
            }

            SetTriInt();
            mesh.triangles = trianglesInt;

            AssetDatabase.SaveAssets();
        }

        if (saveTri)
        {
            saveTri = false;
            SetTriInt();
            mesh.triangles = trianglesInt;

            AssetDatabase.SaveAssets();
        }
    }

    void SetTriInt()
    {
        trianglesInt = new int[triangles.Length * 3];

        for (int i = 0; i < triangles.Length; i++)
        {
            trianglesInt[i*3] = triangles[i].x;
            trianglesInt[i*3 + 1] = triangles[i].y;
            trianglesInt[i*3 + 2] = triangles[i].z;
        }
    }

    public struct Vertex4D
    {
        public Vector4 pos;
        public Vector2 uv;
    }

    void SetVertex4()
    {
        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };

        int vertexCount = vertices4.Length;
        mesh.SetVertexBufferParams(vertexCount, layout);

        Vertex4D[] vertexArray = new Vertex4D[vertexCount];

        for (int i = 0; i < vertexCount; i++) 
        {
            vertexArray[i] = new Vertex4D() {
                pos = vertices4[i],
                uv = uv[i]
            };
        }

        mesh.SetVertexBufferData(vertexArray, 0, 0, vertexCount);
    }
}
