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
    public int[] trianglesInt;

    public bool makeNew;
    public string makeNewName = "NewMesh";

    public Mesh mesh;

    public bool saveMesh;
    public bool readMesh;

    public bool saveTri;
    public bool readTri;
    public bool doubleTri;

    public bool readVertex3;

    public bool subdivide3;
    //public bool subdivide4;

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

        if (readMesh)
        {
            readMesh = false;
            ReadTri();
            ReadVertex3();
            ReadUV();
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
            ReadTri();
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
            ReadVertex3();
        }

        if (subdivide3)
        {
            subdivide3 = false;
            //SetTriInt();
            SubdivideMesh3(ref vertices, ref uv, ref triangles);
        }

        void ReadTri()
        {
            triangles = new Vector3Int[mesh.triangles.Length / 3];
            for (int i = 0; i < triangles.Length; i++) {
                triangles[i] = new Vector3Int(mesh.triangles[3*i+0],mesh.triangles[3*i+1],mesh.triangles[3*i+2]);
            }
        }
        void ReadVertex3()
        {
            vertices = mesh.vertices;
        }
        void ReadUV()
        {
            uv = mesh.uv;
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

    public static void SubdivideMesh3(ref Vector3[] inVertex, ref Vector2[] inUV, ref int[] inTri)
    {
        Vector3Int[] triVec = GetTriVector(inTri);
        SubdivideMesh3(ref inVertex, ref inUV, ref triVec);
        inTri = GetTriInt(triVec);
    }
    public static void SubdivideMesh3(ref Vector3[] inVertex, ref Vector2[] inUV, ref Vector3Int[] inTri)
    {
        List<Vector3> newVertList = new List<Vector3>();
        Dictionary<Vector3,int> newVertDict = new Dictionary<Vector3,int>();

        List<Vector2> newUVList = new List<Vector2>();

        List<Vector3Int> newTriList = new List<Vector3Int>();        

        for (int i = 0; i < inTri.Length; i++)
        {
            Vector3 vertex1 = inVertex[inTri[i].x];
            Vector3 vertex2 = inVertex[inTri[i].y];
            Vector3 vertex3 = inVertex[inTri[i].z];

            Vector2 uv1 = inUV[inTri[i].x];
            Vector2 uv2 = inUV[inTri[i].y];
            Vector2 uv3 = inUV[inTri[i].z];

            Vector3 newVertex1 = Vector3.Lerp(vertex1,vertex2, 0.5f);
            Vector3 newVertex2 = Vector3.Lerp(vertex2,vertex3, 0.5f);
            Vector3 newVertex3 = Vector3.Lerp(vertex3,vertex1, 0.5f);

            Vector2 newUV1 = Vector2.Lerp(uv1,uv2, 0.5f);
            Vector2 newUV2 = Vector2.Lerp(uv2,uv3, 0.5f);
            Vector2 newUV3 = Vector2.Lerp(uv3,uv1, 0.5f);

            AddNewTris(
                new Vector3[] {
                    vertex1,vertex2,vertex3,
                    newVertex1,newVertex2,newVertex3
                },
                new Vector2[] {
                    uv1,uv2,uv3,
                    newUV1,newUV2,newUV3
                }
            );
        }

        inTri = UFunc.List2Array(newTriList);
        inUV = UFunc.List2Array(newUVList);
        inVertex = UFunc.List2Array(newVertList);

        void AddNewTris(Vector3[] triVertex, Vector2[] triUV)
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
                    newUVList.Add(triUV[i]);
                }
                else
                {
                    vertexIndex[i] = newVertDict[v];
                }
            }

            newTriList.Add(new Vector3Int(vertexIndex[0],vertexIndex[3],vertexIndex[5]));
            newTriList.Add(new Vector3Int(vertexIndex[1],vertexIndex[4],vertexIndex[3]));
            newTriList.Add(new Vector3Int(vertexIndex[2],vertexIndex[5],vertexIndex[4]));
            newTriList.Add(new Vector3Int(vertexIndex[3],vertexIndex[4],vertexIndex[5]));
        }
    }
    public static void SubdivideMesh4(ref Vector4[] inVertex, ref Vector2[] inUV, ref Vector3Int[] inTri)
    {
        List<Vector4> newVertList = new List<Vector4>();
        Dictionary<Vector4,int> newVertDict = new Dictionary<Vector4,int>();

        List<Vector2> newUVList = new List<Vector2>();

        List<Vector3Int> newTriList = new List<Vector3Int>();        

        for (int i = 0; i < inTri.Length; i++)
        {
            Vector4 vertex1 = inVertex[inTri[i].x];
            Vector4 vertex2 = inVertex[inTri[i].y];
            Vector4 vertex3 = inVertex[inTri[i].z];

            Vector2 uv1 = inUV[inTri[i].x];
            Vector2 uv2 = inUV[inTri[i].y];
            Vector2 uv3 = inUV[inTri[i].z];

            Vector4 newVertex1 = Vector4.Lerp(vertex1,vertex2, 0.5f);
            Vector4 newVertex2 = Vector4.Lerp(vertex2,vertex3, 0.5f);
            Vector4 newVertex3 = Vector4.Lerp(vertex3,vertex1, 0.5f);

            Vector2 newUV1 = Vector2.Lerp(uv1,uv2, 0.5f);
            Vector2 newUV2 = Vector2.Lerp(uv2,uv3, 0.5f);
            Vector2 newUV3 = Vector2.Lerp(uv3,uv1, 0.5f);

            AddNewTris(
                new Vector4[] {
                    vertex1,vertex2,vertex3,
                    newVertex1,newVertex2,newVertex3
                },
                new Vector2[] {
                    uv1,uv2,uv3,
                    newUV1,newUV2,newUV3
                }
            );
        }

        inTri = UFunc.List2Array(newTriList);
        inUV = UFunc.List2Array(newUVList);
        inVertex = UFunc.List2Array(newVertList);

        void AddNewTris(Vector4[] triVertex, Vector2[] triUV)
        {
            int[] vertexIndex = new int[6];

            for (int i = 0; i < triVertex.Length; i++)
            {
                Vector4 v = triVertex[i];

                if (!newVertDict.ContainsKey(v))
                {
                    vertexIndex[i] = newVertList.Count;
                    newVertDict.Add(v,newVertList.Count);
                    newVertList.Add(v);
                    newUVList.Add(triUV[i]);
                }
                else
                {
                    vertexIndex[i] = newVertDict[v];
                }
            }

            newTriList.Add(new Vector3Int(vertexIndex[0],vertexIndex[3],vertexIndex[5]));
            newTriList.Add(new Vector3Int(vertexIndex[1],vertexIndex[4],vertexIndex[3]));
            newTriList.Add(new Vector3Int(vertexIndex[2],vertexIndex[5],vertexIndex[4]));
            newTriList.Add(new Vector3Int(vertexIndex[3],vertexIndex[4],vertexIndex[5]));
        }
    }

    public static Vector3Int[] GetTriVector(int[] triangles)
    {
        Vector3Int[] newTri = new Vector3Int[triangles.Length / 3];
        for (int i = 0; i < newTri.Length; i++) {
            newTri[i] = new Vector3Int(triangles[3*i+0],triangles[3*i+1],triangles[3*i+2]);
        }
        return newTri;
    }
    public static int[] GetTriInt(Vector3Int[] triangles)
    {
        int[] newTri = new int[triangles.Length*3];
        for (int i = 0; i < triangles.Length; i++) {
            newTri[3*i] = triangles[i].x;
            newTri[3*i+1] = triangles[i].y;
            newTri[3*i+2] = triangles[i].z;
        }
        return newTri;
    }
}
