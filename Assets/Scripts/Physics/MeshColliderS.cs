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
    [SerializeField] Vector4[] normals = new Vector4[0];

    [SerializeField] Vector2Int[] edges = new Vector2Int[0];
    Dictionary<Vector2Int,int> edgesDict = new Dictionary<Vector2Int,int>();

    [SerializeField] List<Vector2Int> edgeTriangles = new List<Vector2Int>(); //the triangles that are connected to each edge

    //[SerializeField] TriData[] triDatas = new TriData[0];

    public override float boundingRadius {get{return furthestVertexDistance;}}
    [SerializeField] float furthestVertexDistance;

    /*
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
    */

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

    void Update()
    {
        if (!rigidbody4.isStatic) {
            CalcWorldVertex();
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

        normals = new Vector4[triangles.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = UFunc.HyperCross(verticiesWorld[triangles[i].x],verticiesWorld[triangles[i].y],verticiesWorld[triangles[i].z]).normalized;
        }

        /*
        for (int i = 0; i < triDatas.Length; i++) {
            triDatas[i].Calc();
        }
        */
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

        edgeTriangles.Clear();
        for (int i = 0; i < triangles.Length; i++)
        {
            Vector3Int tri = triangles[i];
            Vector2Int e1 = new Vector2Int(tri.x,tri.y);
            Vector2Int e2 = new Vector2Int(tri.y,tri.z);
            Vector2Int e3 = new Vector2Int(tri.z,tri.x);

            int edgeInt1 = edgesDict[e1];
            int edgeInt2 = edgesDict[e2];
            int edgeInt3 = edgesDict[e3];

            if (edgeInt1 < edgeTriangles.Count) {
                edgeTriangles[edgeInt1] = new Vector2Int(edgeTriangles[edgeInt1].x,i);
            } else {
                edgeTriangles.Add(new Vector2Int(i,i));
            }

            if (edgeInt2 < edgeTriangles.Count) {
                edgeTriangles[edgeInt2] = new Vector2Int(edgeTriangles[edgeInt2].x,i);
            } else {
                edgeTriangles.Add(new Vector2Int(i,i));
            }

            if (edgeInt3 < edgeTriangles.Count) {
                edgeTriangles[edgeInt3] = new Vector2Int(edgeTriangles[edgeInt3].x,i);
            } else {
                edgeTriangles.Add(new Vector2Int(i,i));
            }
        }
    }

    public override Vector4 PointClose(Vector4 point)
    {
        List<Vector4> checkDirections = new List<Vector4>();
        List<Vector4> directionMeshPoints = new List<Vector4>();

        for (int i = 0; i < triangles.Length; i++)
        {
            Vector4 norm = normals[i];
            checkDirections.Add(norm);
            directionMeshPoints.Add(UFunc.ProjectToVectorNormal(point,norm).normalized);
        }

        for (int i = 0; i < edges.Length; i++)
        {
            Vector4 e1 = verticiesWorld[edges[i].x];
            Vector4 e2 = verticiesWorld[edges[i].y];

            Vector4 edgeClose = UFunc.SlerpPointClose(e1,e2,point);
            Vector4 edgeDir = UFunc.DirectionFromTo(edgeClose,point);

            checkDirections.Add(edgeDir);
            directionMeshPoints.Add(edgeClose);
        }

        for (int i = 0; i < verticiesWorld.Length; i++)
        {
            Vector4 vert = verticiesWorld[i];
            Vector4 dir = UFunc.DirectionFromTo(vert,point);

            checkDirections.Add(dir);
            directionMeshPoints.Add(vert);
        }

        float maxDotSpace = -1;
        Vector4 maxSpaceMesh = new Vector4();

        for (int i = 0; i < checkDirections.Count; i++)
        {
            Vector4 dir = checkDirections[i];

            float maxMeshDot = -1;
            for (int j = 0; j < verticiesWorld.Length; j++) {
                float d = Vector4.Dot(verticiesWorld[j], dir);
                maxMeshDot = Mathf.Max(d,maxMeshDot);
            }

            float pointDot = Vector4.Dot(point, dir);

            float dotSpace = pointDot - maxMeshDot;

            if (maxMeshDot > 0.05f) continue;

            if (dotSpace > maxDotSpace) {
                maxDotSpace = dotSpace;
                maxSpaceMesh = directionMeshPoints[i];
            }
        }

        return maxSpaceMesh;
    }

    public void LineClose(Vector4 v1, Vector4 v2, ref Vector4 outLine, ref Vector4 outMesh)
    {
        List<Vector4> checkDirections = new List<Vector4>();
        List<Vector4> directionMeshPoints = new List<Vector4>();
        List<Vector4> directionLinePoints = new List<Vector4>();
        //List<Vector4> linePoints = new List<Vector4>();

        for (int i = 0; i < triangles.Length; i++)
        {
            Vector4 norm = normals[i];
            checkDirections.Add(norm);

            float dot1 = Vector4.Dot(v1,norm);
            float dot2 = Vector4.Dot(v2,norm);

            if (dot1 < dot2) {
                directionMeshPoints.Add(UFunc.ProjectToVectorNormal(v1,norm).normalized);
                directionLinePoints.Add(v1);
            } else {
                directionMeshPoints.Add(UFunc.ProjectToVectorNormal(v2,norm).normalized);
                directionLinePoints.Add(v2);
            }
        }

        for (int i = 0; i < edges.Length; i++)
        {
            Vector4 e1 = verticiesWorld[edges[i].x];
            Vector4 e2 = verticiesWorld[edges[i].y];

            Vector4 edgeC = new Vector4();
            Vector4 lineC = new Vector4();
            UFunc.DoubleArcClose(e1,e2,v1,v2, ref edgeC, ref lineC);
            Vector4 edgeDir = UFunc.DirectionFromTo(edgeC,lineC);

            checkDirections.Add(edgeDir);
            checkDirections.Add(-edgeDir);

            directionMeshPoints.Add(edgeC);
            directionMeshPoints.Add(edgeC);
            directionLinePoints.Add(lineC);
            directionLinePoints.Add(lineC);
        }

        for (int i = 0; i < verticiesWorld.Length; i++)
        {
            Vector4 vert = verticiesWorld[i];

            Vector4 close = UFunc.SlerpPointClose(v1,v2,vert);

            Vector4 dir = UFunc.DirectionFromTo(vert,close);

            checkDirections.Add(dir);
            //checkDirections.Add(-dir);

            directionMeshPoints.Add(vert);
            //directionMeshPoints.Add(vert);
            directionLinePoints.Add(close);
            //directionLinePoints.Add(close);
        }

        float maxDotSpace = -1;
        Vector4 maxSpaceMesh = new Vector4();
        Vector4 maxSpaceLine = new Vector4();

        for (int i = 0; i < checkDirections.Count; i++)
        {
            Vector4 dir = checkDirections[i];

            float maxMeshDot = -1;
            for (int j = 0; j < verticiesWorld.Length; j++) {
                float d = Vector4.Dot(verticiesWorld[j], dir);
                maxMeshDot = Mathf.Max(d,maxMeshDot);
            }

            float minLineDot = Mathf.Min(Vector4.Dot(v1, dir),Vector4.Dot(v2, dir));

            float dotSpace = minLineDot - maxMeshDot;

            int edgeNum = i - triangles.Length;
            int vertNum = edgeNum - edges.Length*2;
            print(edgeNum+" "+vertNum+" "+maxMeshDot+" "+minLineDot+" "+dotSpace+" "+dir+" "+directionMeshPoints[i]+" "+directionLinePoints[i]);

            if (maxMeshDot > 0.05f) continue;

            if (dotSpace > maxDotSpace) {
                maxDotSpace = dotSpace;
                maxSpaceMesh = directionMeshPoints[i];
                maxSpaceLine = directionLinePoints[i];
            }
        }

        print(maxDotSpace+" "+maxSpaceMesh+" "+maxSpaceLine+" "+UFunc.DistanceS(maxSpaceMesh,maxSpaceLine));

        outLine = maxSpaceLine;
        outMesh = maxSpaceMesh;
    }

    /*
    public void LineClose(Vector4 v1, Vector4 v2, ref Vector4 outLine, ref Vector4 outMesh)
    {
        //Vector4 projPoint1 = v1;
        //Vector4 projPoint2 = v2;

        Vector4 closeMesh = new Vector4();
        Vector4 closeLine = new Vector4();

        float maxFaceDot = -1;

        for (int i = 0; i < triangles.Length; i++)
        {
            Vector4 norm = normals[i];
            
            float d1 = Vector4.Dot(v1,norm);
            float d2 = Vector4.Dot(v2,norm);

            if (Mathf.Min(d1,d2) > maxFaceDot)
            {
                if (d1 < d2)
                {
                    closeMesh = (v1 - d1*norm).normalized;
                    closeLine = v1;
                    maxFaceDot = d1;
                }
                else
                {
                    closeMesh = (v2 - d2*norm).normalized;
                    closeLine = v2;
                    maxFaceDot = d2;
                }
            }
        }

        Vector4 edgeCloseMesh = new Vector4();
        Vector4 edgeCloseLine = new Vector4();
        float closeEdgeDot = -1;

        for (int i = 0; i < edges.Length; i++)
        {
            Vector4 e1 = verticiesWorld[edges[i].x];
            Vector4 e2 = verticiesWorld[edges[i].y];

            Vector4 edgePoint = new Vector4();
            Vector4 linePoint = new Vector4();

            UFunc.DoubleArcClose(e1,e2,v1,v2, ref edgePoint, ref linePoint);
            Vector4 edgeDir = UFunc.DirectionFromTo(edgePoint, linePoint);

            //float edgeCloseDot = Vector4.Dot(edgePoint,linePoint);

            float minDot = 1;
            float maxDot = -1;
            int minDotIndex = 0;
            int maxDotIndex = 0;

            for (int j = 0; j < verticiesWorld.Length; j++)
            {
                if (j == edges[i].x || j == edges[i].y) continue;

                Vector4 vert = verticiesWorld[j];

                float dot = Vector4.Dot(edgeDir,vert);


                if (dot < minDot) 
                {
                    minDot = dot;
                    minDotIndex = j;
                }
                if (dot > maxDot) 
                {
                    maxDot = dot;
                    maxDotIndex = j;
                }
            }

            float v1Dot = Vector4.Dot(v1, edgeDir);
            float v2Dot = Vector4.Dot(v2, edgeDir);
            float disDot = Mathf.Min(v1Dot,v2Dot);

            float maxDif = maxDot-disDot;
            float minDif = minDot-disDot;

            Vector4 targetPoint = new Vector4();

            if (Mathf.Abs(minDif) < Mathf.Abs(maxDif))
            {
                edgeDir *= -1;

                targetPoint = UFunc.Slerp4Angle(edgePoint,linePoint,minDot);
            }
            else
            {
                targetPoint = UFunc.Slerp4Angle(edgePoint,linePoint,maxDot);
            }

            edgeDir = new Rotor(edgePoint,targetPoint) * edgeDir;
            float slerpDot = Vector4.Dot(edgeDir,targetPoint);

            print(i+" "+edges[i]+" "+targetPoint+" "+linePoint+" "+edgeDir+" "+slerpDot);

            if (slerpDot < closeEdgeDot)
            {
                closeEdgeDot = slerpDot;
                edgeCloseMesh = targetPoint;
                edgeCloseLine = linePoint;
            }
        }

        if (maxFaceDot > closeEdgeDot)
        {
            outLine = closeLine;
            outMesh = closeMesh;
        }
        else
        {
            outLine = edgeCloseLine;
            outMesh = edgeCloseMesh;
        }
    }
    */

    public void MeshClose(MeshColliderS meshCol, ref Vector4 outThis, ref Vector4 outMesh)
    {
        List<float> pointDots = new List<float>();
        List<Vector4> thisPoints = new List<Vector4>();
        List<Vector4> meshPoints = new List<Vector4>();

        for (int i = 0; i < normals.Length; i++)
        {
            Vector4 norm = normals[i];

            float minDot = -1;
            int minIndex = 0;
            for (int j = 0; j < meshCol.verticiesWorld.Length; j++)
            {
                Vector4 vert = meshCol.verticiesWorld[j];
                float dot = Vector4.Dot(vert,norm);
                if (dot < minDot) {
                    minDot = dot;
                    minIndex = j;
                }
            }

            Vector4 meshPoint = meshCol.verticiesWorld[minIndex];

            pointDots.Add(minDot);
            thisPoints.Add((meshPoint - minDot*norm).normalized);
            meshPoints.Add(meshPoint);
        }

        for (int i = 0; i < meshCol.normals.Length; i++)
        {
            Vector4 norm = meshCol.normals[i];

            float minDot = -1;
            int minIndex = 0;
            for (int j = 0; j < verticiesWorld.Length; j++)
            {
                Vector4 vert = verticiesWorld[j];
                float dot = Vector4.Dot(vert,norm);
                if (dot < minDot) {
                    minDot = dot;
                    minIndex = j;
                }
            }

            Vector4 meshPoint = verticiesWorld[minIndex];

            pointDots.Add(minDot);
            thisPoints.Add(meshPoint);
            meshPoints.Add((meshPoint - minDot*norm).normalized);
        }
    }
}
 