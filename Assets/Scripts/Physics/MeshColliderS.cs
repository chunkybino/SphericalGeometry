using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MeshColliderS : ColliderS
{
    public override int colliderType {get{return 3;}}
    public override MeshColliderS mesh {get{return this;}}

    public Vector3 scale = new Vector3(1,1,1);

    public Vector3[] verticies = new Vector3[0];
    public Vector3Int[] triangles = new Vector3Int[0];

    [SerializeField] Vector4[] verticies4 = new Vector4[0];
    [SerializeField] Vector4[] verticiesWorld = new Vector4[0];

    [SerializeField] Vector2Int[] edges = new Vector2Int[0];
    Dictionary<Vector2Int,int> edgesDict = new Dictionary<Vector2Int,int>();

    [SerializeField] TriData[] triDatas = new TriData[0];

    public override float boundingRadius {get{return furthestVertexDistance;}}
    [SerializeField] float furthestVertexDistance;

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

            if (UFunc.Dot(edgeNormals[0], GetVertex(2)) > 0) edgeNormals[0] *= -1;
            if (UFunc.Dot(edgeNormals[1], GetVertex(0)) > 0) edgeNormals[1] *= -1;
            if (UFunc.Dot(edgeNormals[2], GetVertex(1)) > 0) edgeNormals[2] *= -1;
        }
    }

    [SerializeField] bool calcVertex;
    [SerializeField] bool calcTri;

    void Awake() {
        CalcVertex();
        CalcTriangles();
        transform4.onMatrixUpdate.AddListener((Matrix4x4 mat) => CalcWorldVertex());
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

        int furthestDisIndex = 0;
        float furthestDot = 0;

        for (int i = 0; i < verticies.Length; i++) {
            verticies4[i] = UFunc.ProjectLocal3QuickS(Vector3.Scale(verticies[i], transform4.scale*scale));

            float dot = verticies4[0].w;
            if (i == 0 || dot > furthestDot) {
                furthestDisIndex = i;
                furthestDot = dot;
            }
        }

        furthestVertexDistance = Mathf.Acos(furthestDot);

        CalcWorldVertex();
    }
    void CalcWorldVertex()
    {
        verticiesWorld = new Vector4[verticies4.Length];
        for (int i = 0; i < verticies4.Length; i++) {
            verticiesWorld[i] = transform4.matrix * verticies4[i];
        } 

        for (int i = 0; i < triDatas.Length; i++) {
            triDatas[i].Calc();
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

    public override Vector4 PointClose(Vector4 point)
    {
        Vector4 outV = new Vector4();
        float outDot = 0;

        //true = vertex/edge at corosponding index will not be checked
        //while looping through triangles, if ourporjected point has a negative dot product with the edge normal
        //that edge becomes invalid, only check edges that have only positive dot products
        bool[] invalidEdges = new bool[edges.Length];
        //if a a projected point hase a positive dot with an opposite edge to a vertex, that vertex becoms invalid
        bool[] invalidVerticies = new bool[verticiesWorld.Length];

        bool foundFace = false;

        for (int i = 0; i < triangles.Length; i++)
        {
            TriData tri = triDatas[i];
            Vector4 v1 = tri.GetVertex(0);
            Vector4 v2 = tri.GetVertex(1);
            Vector4 v3 = tri.GetVertex(2);

            Vector4 proj = (point - tri.center*UFunc.Dot(point, tri.center)).normalized;

            float[] dots = {
                UFunc.Dot(tri.edgeNormals[0], proj),
                UFunc.Dot(tri.edgeNormals[1], proj),
                UFunc.Dot(tri.edgeNormals[2], proj)
            };

            if  (dots[0] > 0 && dots[1] > 0 && dots[2] > 0)
            {
                print("how " + gameObject.name);
            }

            if (dots[0] < 0 && dots[1] < 0 && dots[2] < 0)
            {
                float dot = UFunc.Dot(point, proj);
                if (!foundFace || dot > outDot) {
                    outV = proj;
                    outDot = dot;
                    foundFace = true;
                }
            }
            else if (!foundFace && false) //if we've already found a face, dont bother checking edges
            {
                //invalidate any edge where the other 2 edges have negative dots
                if (dots[0] < 0) invalidEdges[tri.edgeIndicies.x] = true;
                if (dots[1] < 0) invalidEdges[tri.edgeIndicies.y] = true;
                if (dots[2] < 0) invalidEdges[tri.edgeIndicies.z] = true;

                if (dots[0] < 0 || dots[2] < 0) invalidVerticies[tri.vertexIndicies.x] = true;
                if (dots[1] < 0 || dots[0] < 0) invalidVerticies[tri.vertexIndicies.y] = true;
                if (dots[2] < 0 || dots[1] < 0) invalidVerticies[tri.vertexIndicies.y] = true;
            }
        }

        //if our point projects onto one of the faces, return that point
        if (foundFace) return outV;

        //if not, check edges next
        bool found = false;

        for (int i = 0; i < edges.Length; i++) 
        {
            if (invalidEdges[i]) continue;
            
            Vector4 close = UFunc.SlerpPointClose(verticiesWorld[edges[i].x], verticiesWorld[edges[i].y], point);
            float dot = UFunc.Dot(close,point);
            if (!found || dot > outDot) {
                outV = close;
                outDot = dot;
                found = true;
            }
        }

        for (int i = 0; i < verticiesWorld.Length; i++) 
        {
            if (invalidVerticies[i]) continue;

            float dot = UFunc.Dot(verticiesWorld[i],point);
            if (!found || dot > outDot) {
                outV = verticiesWorld[i];
                outDot = dot;
                found = true;
            }
        }

        return outV;
    }

    public void LineClose(Vector4 v1, Vector4 v2, ref Vector4 outLine, ref Vector4 outTri)
    {
        Vector4[] points = new Vector4[4];

        FindMin(0, ref points[0], ref points[1]);
        FindMin(1, ref points[2], ref points[3]);

        //take the closest
        if (UFunc.Dot(points[0],points[1]) > UFunc.Dot(points[2],points[3])) {
            outLine = points[0];
            outTri = points[1];
        } else {
            outLine = points[2];
            outTri = points[3];
        }

        void FindMin(float startT, ref Vector4 point1, ref Vector4 point2)
        {
            int iterations = 6;

            point1 = UFunc.Slerp4(v1,v2,startT);
            Vector4 prev1 = point1;

            for (int i = 0; i < iterations; i++)
            {
                point2 = PointClose(point1);
                point1 = UFunc.SlerpPointClose(v1,v2,point2);
                if (prev1 == point1) return; //if we get back the same point, stop here
                prev1 = point1;
            }
        }
    }
}
 