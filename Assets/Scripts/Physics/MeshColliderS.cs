using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MeshColliderS : ColliderS
{
    public override int colliderType {get{return 3;}}
    public override MeshColliderS mesh {get{return this;}}

    public Vector3[] verticies = new Vector3[0];
    public Vector3Int[] triangles = new Vector3Int[0];

    [SerializeField] Vector4[] verticies4 = new Vector4[0];
    [SerializeField] Vector4[] verticiesWorld = new Vector4[0];

    [SerializeField] Vector2Int[] edges = new Vector2Int[0];
    Dictionary<Vector2Int,int> edgesDict = new Dictionary<Vector2Int,int>();

    [SerializeField] TriData[] triDatas = new TriData[0];

    [System.Serializable]
    struct TriData
    {
        public MeshColliderS mesh;

        public Vector3Int vertexIndicies;
        public Vector3Int edgeIndicies;
        public Vector4 center; //center of the sphere that connects all points in tri
        public Vector4[] edgeNormals;

        public Vector4 GetVertex(int i) {
            return mesh.verticiesWorld[vertexIndicies[i]];
        }
        public Vector2Int GetEdge(int i) {
            return mesh.edges[edgeIndicies[i]];
        }
        public Vector4 GetEdgeVertex(int i, int j) {
            return mesh.verticiesWorld[mesh.edges[edgeIndicies[i]][j]];
        }

        public void Calc()
        {
            center = UFunc.HyperCross(GetVertex(0),GetVertex(1),GetVertex(2)).normalized;
            edgeNormals = new Vector4[3];
            for (int i = 0; i < 3; i++) {
                edgeNormals[i] = UFunc.HyperCross(GetEdgeVertex(i,0),GetEdgeVertex(i,1),center).normalized;
            }

            if (UFunc.Dot(edgeNormals[0], GetVertex(2)) > 0) {
                edgeNormals[0] *= -1;
                edgeNormals[1] *= -1;
                edgeNormals[2] *= -1;
            }
        }
    }

    [SerializeField] bool calcVertex;
    [SerializeField] bool calcTri;

    void Awake() {
        CalcVertex();
        CalcTriangles();
        transform4.onMatrixMult.AddListener((Matrix4x4 mat) => CalcWorldVertex());
    }

    void OnValidate() {
        if (calcVertex) {
            calcVertex = false;
            CalcVertex();
        }
        if (calcTri) {
            calcTri = false;
            CalcTriangles();
        }
    }

    void CalcVertex()
    {
        verticies4 = new Vector4[verticies.Length];
        verticiesWorld = new Vector4[verticies.Length];

        for (int i = 0; i < verticies.Length; i++) {
            verticies4[i] = UFunc.ProjectLocal3QuickS(verticies[i]);
            verticiesWorld[i] = transform4.matrix * verticies4[i];
        }

        for (int i = 0; i < triDatas.Length; i++) {
            triDatas[i].Calc();
        }
    }
    void CalcWorldVertex()
    {
        verticiesWorld = new Vector4[verticies4.Length];
        for (int i = 0; i < verticies4.Length; i++) {
            verticiesWorld[i] = transform4.matrix * verticies4[i];
        }  
    }
    void CalcTriangles()
    {
        List<Vector2Int> edgeList = new List<Vector2Int>();
        edgesDict.Clear();

        for (int i = 0; i < triangles.Length; i++) {
            Vector3Int tri = triangles[i];
            Vector2Int e1 = new Vector2Int(tri.x,tri.y);
            Vector2Int e2 = new Vector2Int(tri.y,tri.z);
            Vector2Int e3 = new Vector2Int(tri.z,tri.x);
            if (!edgeList.Contains(e1) && !edgeList.Contains(new Vector2Int(e1.y,e1.x))) {
                edgesDict.Add(e1,edgeList.Count);
                edgesDict.Add(new Vector2Int(e1.y,e1.x),edgeList.Count);
                edgeList.Add(e1);
            }
            if (!edgeList.Contains(e2) && !edgeList.Contains(new Vector2Int(e2.y,e2.x))) {
                edgesDict.Add(e2,edgeList.Count);
                edgesDict.Add(new Vector2Int(e2.y,e2.x),edgeList.Count);
                edgeList.Add(e2);
            }
            if (!edgeList.Contains(e3) && !edgeList.Contains(new Vector2Int(e3.y,e3.x))) {
                edgesDict.Add(e3,edgeList.Count);
                edgesDict.Add(new Vector2Int(e3.y,e3.x),edgeList.Count);
                edgeList.Add(e3);
            }
        }
        edges = UFunc.List2Array(edgeList);

        triDatas = new TriData[triangles.Length];

        for (int i = 0; i < triangles.Length; i++) {
            TriData tri = new TriData();
            tri.mesh = this;
            tri.vertexIndicies = triangles[i];
            tri.edgeIndicies = new Vector3Int(
                edgesDict[new Vector2Int(triangles[i].x,triangles[i].y)],
                edgesDict[new Vector2Int(triangles[i].y,triangles[i].z)],
                edgesDict[new Vector2Int(triangles[i].z,triangles[i].x)]
            );
            tri.Calc();
            triDatas[i] = tri;
        }
    }

    public Vector4 PointClose(Vector4 point)
    {
        bool found = false;
        Vector4 outV = new Vector4();
        float outDot = 0;

        for (int i = 0; i < triangles.Length; i++)
        {
            TriData tri = triDatas[i];
            Vector4 v1 = tri.GetVertex(0);
            Vector4 v2 = tri.GetVertex(1);
            Vector4 v3 = tri.GetVertex(2);

            //Vector4 proj = (point - tri.center*UFunc.Dot(point, tri.center)).normalized;
            Vector4 close = TriColliderS.PointCloseTri(point, v1, v2, v3);

            float dot = UFunc.Dot(close,point);
            print(i+" "+dot+" "+close);
            if (!found || dot > outDot) {
                //print("real "+i+" "+dot+" "+close);
                outDot = dot;
                outV = close;
                found = true;
            }
        }

        if (!found) return transform4.positionNorm;
        return outV;
    }
}
 