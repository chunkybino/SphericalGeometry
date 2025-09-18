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
    public Vector4[] verticiesWorld = new Vector4[0];
    [SerializeField] Vector4[] normals = new Vector4[0];

    public Vector2Int[] edges = new Vector2Int[0];
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

    [SerializeField] bool drawGizmoVertex;

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

    public override void LineClose(Vector4 v1, Vector4 v2, ref Vector4 outLine, ref Vector4 outMesh)
    {
        float maxAngleSpace = -2;
        Vector4 maxSpaceMesh = new Vector4();
        Vector4 maxSpaceLine = new Vector4();
        
        
        for (int i = 0; i < triangles.Length; i++)
        {
            Vector4 norm = normals[i];

            float dot1 = Vector4.Dot(v1,norm);
            float dot2 = Vector4.Dot(v2,norm);

            Vector4 meshP = new Vector4();
            Vector4 lineP = new Vector4();
            float minD = 0;

            if (dot1 < dot2) {
                meshP = UFunc.ProjectToVectorNormal(v1,norm).normalized;
                lineP = v1;
                minD = dot1;
            } else {
                meshP = UFunc.ProjectToVectorNormal(v2,norm).normalized;
                lineP = v2;
                minD = dot2;
            }

            float ang = Mathf.PI/2 - Mathf.Acos(minD); 

            if (ang > maxAngleSpace)
            {
                maxAngleSpace = ang;
                maxSpaceLine = lineP;
                maxSpaceMesh = meshP;
            }
        }

        List<Vector4> checkDirections = new List<Vector4>();
        List<Vector4> directionMeshPoints = new List<Vector4>();
        List<Vector4> directionLinePoints = new List<Vector4>();

        
        for (int i = 0; i < edges.Length; i++)
        {
            Vector4 e1 = verticiesWorld[edges[i].x];
            Vector4 e2 = verticiesWorld[edges[i].y];

            Vector4 edgeC = new Vector4();
            Vector4 lineC = new Vector4();
            UFunc.DoubleArcClose(e1,e2,v1,v2, ref edgeC, ref lineC);

            Vector4 edgeDir = UFunc.DirectionFromTo(edgeC,lineC);

            Vector4 triNorm1 = normals[edgeTriangles[i].x];
            Vector4 triNorm2 = normals[edgeTriangles[i].y];
 
            Vector4 triNormProjDir = UFunc.ProjectVectorToPlane(triNorm1,triNorm2,edgeDir);
            bool between = UFunc.BetweenS(triNorm1,triNorm2, triNormProjDir);
            bool betweenReverse = UFunc.BetweenS(triNorm1,triNorm2, -triNormProjDir);

            if (between || betweenReverse)
            {
                if (betweenReverse) edgeDir *= -1;

                float maxMeshDot = -1;
                for (int j = 0; j < verticiesWorld.Length; j++) {
                    Vector4 projV = UFunc.ProjectVectorToPlane(edgeC,edgeDir, verticiesWorld[j]);
                    float d = Vector4.Dot(projV, edgeDir);
                    maxMeshDot = Mathf.Max(d,maxMeshDot);
                }

                float meshAng = Mathf.PI/2 - Mathf.Acos(maxMeshDot); 
                float lineAng = Mathf.PI/2 - Mathf.Acos(Vector4.Dot(edgeDir,lineC)); 
                float angleSpace = lineAng - meshAng;

                if (angleSpace > maxAngleSpace)
                {
                    maxAngleSpace = angleSpace;
                    maxSpaceLine = lineC;
                    maxSpaceMesh = edgeC;
                }
            }
        }

        outLine = maxSpaceLine;
        outMesh = maxSpaceMesh;
        return;
    }


    public void MeshClose(MeshColliderS meshCol, ref Vector4 outThis, ref Vector4 outMesh)
    {
        List<Vector4> checkDirections = new List<Vector4>();

        for (int i = 0; i < normals.Length; i++)
        {
            checkDirections.Add(normals[i]);
        }
        for (int i = 0; i < meshCol.normals.Length; i++)
        {
            checkDirections.Add(meshCol.normals[i]);
        }

        for (int i = 0; i < edges.Length; i++)
        {
            Vector4 e1_1 = verticiesWorld[edges[i].x];
            Vector4 e2_1 = verticiesWorld[edges[i].y];

            for (int j = 0; j < meshCol.edges.Length; j++)
            {
                Vector4 e1_2 = meshCol.verticiesWorld[meshCol.edges[i].x];
                Vector4 e2_2 = meshCol.verticiesWorld[meshCol.edges[i].y];


                Vector4 close1 = new Vector4();
                Vector4 close2 = new Vector4();
                UFunc.DoubleArcClose(e1_1,e2_1,e1_2,e2_2, ref close1, ref close2);
                Vector4 edgeDir = UFunc.DirectionFromTo(close1,close2);

                checkDirections.Add(edgeDir);
                checkDirections.Add(-edgeDir);
            }
        }

        float maxDotSpace = -1;
        Vector4 maxSpaceDir = new Vector4();

        for (int i = 0; i < checkDirections.Count; i++)
        {
            Vector4 dir = checkDirections[i];

            float maxMeshDot1 = -1;
            float minMeshDot1 = 1;
            for (int j = 0; j < verticiesWorld.Length; j++) {
                float d = Vector4.Dot(verticiesWorld[j], dir);
                maxMeshDot1 = Mathf.Max(d,maxMeshDot1);
                minMeshDot1 = Mathf.Min(d,minMeshDot1);
            }

            float maxMeshDot2 = -1;
            float minMeshDot2 = 1;
            for (int j = 0; j < meshCol.verticiesWorld.Length; j++) {
                float d = Vector4.Dot(meshCol.verticiesWorld[j], dir);
                maxMeshDot2 = Mathf.Max(d,maxMeshDot2);
                minMeshDot2 = Mathf.Min(d,minMeshDot2);
            }

            float space1 = minMeshDot2 - maxMeshDot1;
            float space2 = minMeshDot1 - maxMeshDot2;

            if (space1 > space2)
            {
                if (space1 > maxDotSpace) {
                    maxDotSpace = space1;
                    maxSpaceDir = dir;
                }
            }
            else
            {
                if (space2 > maxDotSpace) {
                    maxDotSpace = space2;
                    maxSpaceDir = -dir;
                }
            }
        }

        float maxThisDot = -1;
        float minMeshDot = 1;

        for (int i = 0; i < verticiesWorld.Length; i++)
        {
            float d = Vector4.Dot(verticiesWorld[i], maxSpaceDir);
            if (d > maxThisDot)
            {
                maxThisDot = d;
                outThis = verticiesWorld[i];
            }
        }
        for (int i = 0; i < meshCol.verticiesWorld.Length; i++)
        {
            float d = Vector4.Dot(meshCol.verticiesWorld[i], maxSpaceDir);
            if (d < minMeshDot)
            {
                minMeshDot = d;
                outThis = meshCol.verticiesWorld[i];
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmoVertex) return;

        Vector4[] transformVertex = new Vector4[verticiesWorld.Length];
        Vector3[] projVertex = new Vector3[verticiesWorld.Length];
        Vector3[] projVertex2 = new Vector3[verticiesWorld.Length];
        for (int i = 0; i < verticiesWorld.Length; i++) {
            transformVertex[i] = Camera.main.worldToCameraMatrix * verticiesWorld[i];
            projVertex[i] = UFunc.SterographicProjection(transformVertex[i]);
            projVertex2[i] = Camera.main.worldToCameraMatrix.inverse * projVertex[i];
        }

        Vector3[] edgeDraw = new Vector3[edges.Length*2] ;

        for (int i = 0; i < edges.Length; i++)
        {
            edgeDraw[2*i + 0] = projVertex2[edges[i].x];
            edgeDraw[2*i + 1] = projVertex2[edges[i].y];
        }

        /*
        for (int i = 0; i < verticiesWorld.Length; i++) {
            print(i+" "+projVertex[i]);
            print(i+" "+projVertex2[i]);
        }
        */

        Gizmos.color = Color.green;
        Gizmos.DrawLineList(edgeDraw);
    }
}
 