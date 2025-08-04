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
    public bool readTri;
    public bool doubleTri;

    public bool readVertex3;

    public bool subdivide;

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

        if (readTri)
        {
            readTri = false;
            triangles = new Vector3Int[mesh.triangles.Length / 3];
            for (int i = 0; i < triangles.Length; i++) {
                triangles[i] = new Vector3Int(mesh.triangles[i+0],mesh.triangles[i+1],mesh.triangles[i+2]);
            }
        }
        if (doubleTri)
        {
            doubleTri = false;
            Vector3Int[] newTri = new Vector3Int[triangles.Length*2];
            for (int i = 0; i < triangles.Length; i++) {
                newTri[i] = triangles[i];
                newTri[i+triangles.Length] = new Vector3Int(triangles[i].z,triangles[i].y,triangles[i].x);
            }
            triangles = newTri;
        }

        if (readVertex3)
        {
            readVertex3 = false;
            vertices = mesh.vertices;
        }

        if (subdivide)
        {
            subdivide = false;
            SetTriInt();
            SubdivideMesh3(ref vertices, ref trianglesInt);
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

    public void SubdivideMesh3(ref Vector3[] inVertex, ref int[] inTri)
    {
        List<Vector3> newVertList = new List<Vector3>();
        Dictionary<Vector3,int> newVertDict = new Dictionary<Vector3,int>();

        List<int> newTriList = new List<int>();        

        for (int i = 0; i < inTri.Length/3; i++)
        {
            Vector3 vertex1 = inVertex[inTri[3*i]];
            Vector3 vertex2 = inVertex[inTri[3*i + 1]];
            Vector3 vertex3 = inVertex[inTri[3*i + 2]];

            Vector3 newVertex1 = Vector3.Lerp(vertex1,vertex2, 0.5f);
            Vector3 newVertex2 = Vector3.Lerp(vertex2,vertex3, 0.5f);
            Vector3 newVertex3 = Vector3.Lerp(vertex3,vertex1, 0.5f);

            AddNewTris(
                new Vector3[] {
                    vertex1,vertex2,vertex3,
                    newVertex1,newVertex2,newVertex3
                }
            );
        }

        inTri = UFunc.List2Array(newTriList);
        inVertex = UFunc.List2Array(newVertList);

        void AddNewTris(Vector3[] triVertex)
        {
            int[] vertexIndex = new int[6];

            for (int i = 0; i < triVertex.Length; i++)
            {
                Vector3 v = triVertex[i];
                if (!newVertDict.ContainsKey(v))
                {
                    vertexIndex[i] = newVertList.Count;
                    newVertDict.Add(v,newVertList.Count);
                    newVertList.Add(v);

                    print("new "+v);
                }
                else
                {
                    vertexIndex[i] = newVertDict[v];
                }
            }

            newTriList.Add(vertexIndex[0]);
            newTriList.Add(vertexIndex[3]);
            newTriList.Add(vertexIndex[5]);

            newTriList.Add(vertexIndex[1]);
            newTriList.Add(vertexIndex[4]);
            newTriList.Add(vertexIndex[3]);

            newTriList.Add(vertexIndex[2]);
            newTriList.Add(vertexIndex[5]);
            newTriList.Add(vertexIndex[4]);

            newTriList.Add(vertexIndex[3]);
            newTriList.Add(vertexIndex[4]);
            newTriList.Add(vertexIndex[5]);
        }
    }
}
