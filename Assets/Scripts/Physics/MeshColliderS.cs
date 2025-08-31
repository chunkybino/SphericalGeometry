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


        normals = new Vector4[triangles.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = UFunc.HyperCross(verticiesWorld[triangles[i].x],verticiesWorld[triangles[i].y],verticiesWorld[triangles[i].z]).normalized;
        }
    }

    public override Vector4 PointClose(Vector4 point)
    {
        if (normals.Length == 0) return point;

        float largestDot = -1;
        int largestDotIndex = 0;

        for (int i = 0; i < normals.Length; i++)
        {
            float dot = Vector4.Dot(normals[i], point);
            if (dot > largestDot) {
                largestDot = dot;
                largestDotIndex = i;
            }
        }

        Vector4 outV = (point - largestDot*normals[largestDotIndex]).normalized;

        float normalDistanceDot = Vector4.Dot(point,outV);

        for (int i = 0; i < verticiesWorld.Length; i++)
        {
            float dot = Vector4.Dot(verticiesWorld[i], point);

            if (dot > normalDistanceDot)
            {
                outV = verticiesWorld[i];
                normalDistanceDot = dot;
            }
        }

        for (int i = 0; i < edges.Length; i++)
        {
            Vector4 closePoint = UFunc.SlerpPointClose(verticiesWorld[edges[i].x],verticiesWorld[edges[i].y], point);

            float dot = Vector4.Dot(closePoint, point);

            if (dot > normalDistanceDot)
            {
                outV = verticiesWorld[i];
                normalDistanceDot = dot;
            }
        }

        return outV;
    }

    public void LineClose(Vector4 v1, Vector4 v2, ref Vector4 outLine, ref Vector4 outTri)
    {
        Vector4 projPoint1 = v1;
        Vector4 projPoint2 = v2;

        for (int i = 0; i < normals.Length; i++)
        {
            float d1 = Vector4.Dot(projPoint1,normals[i]);
            if (d1 > 0) {
                projPoint1 = (projPoint1 - d1*normals[i]).normalized;
            }

            float d2 = Vector4.Dot(projPoint2,normals[i]);
            if (d2 > 0) {
                projPoint2 = (projPoint2 - d2*normals[i]).normalized;
            }
        }

        float closeDistanceDot = Vector4.Dot(v1,projPoint1);
        Vector4 closeTri = projPoint1;
        Vector4 closeLine = v1;
        if (Vector4.Dot(v1,projPoint2) < closeDistanceDot)
        {
            closeDistanceDot = Vector4.Dot(v2,projPoint2);
            closeTri = projPoint2;
            closeLine = v2;
        }

        print(closeTri+" "+closeLine+" "+closeDistanceDot);

        //closeDistanceDot = -1;

        for (int i = 0; i < edges.Length; i++)
        {
            Vector4 close1 = new Vector4();
            Vector4 close2 = new Vector4();
            UFunc.DoubleArcClose(verticiesWorld[edges[i].x],verticiesWorld[edges[i].y], v1, v2, ref close1, ref close2);

            Vector4 norm = UFunc.DirectionFromTo(close1,close2);

            print("hi "+Vector4.Dot(norm,close2)+" "+Vector4.Dot(norm,v1)+" "+Vector4.Dot(norm,v2));
            //float closeDot = Vector4.Dot(norm,close2);

            bool invalidEdge = false; //check all the verticies, making sure they arnt further outward than our capsule, if they are, ignore this edge
            float vertexDir = 1;
            for (int j = 0; j < verticiesWorld.Length; j++)
            {
                float vertDot = Vector4.Dot(norm, verticiesWorld[j]);
                print(vertDot);

                if (j == 0) vertexDir = Mathf.Sign(vertDot);
                if (vertexDir == 1)
                {
                    if (vertDot < 0.01f) {
                        invalidEdge = true;
                        break;
                    }
                }
                else
                {
                    if (vertDot > 0.01f) {
                        invalidEdge = true;
                        break;
                    }
                }
            }

            print(norm+" "+invalidEdge+" "+i);
            print(verticiesWorld[edges[i].x]+" "+verticiesWorld[edges[i].y]);
            if (invalidEdge) continue;

            float thisDot = Vector4.Dot(close1,close2);
            //print(thisDot+" "+close1+" "+close2+" "+closeDistanceDot);
            if (thisDot > closeDistanceDot)
            {
                closeDistanceDot = thisDot;
                closeTri = close1;
                closeLine = close2;
            }
        }

        print(closeTri+" "+closeLine+" "+closeDistanceDot+" "+2);

        outLine = closeLine;
        outTri = closeTri;

        /*
        //faces
        float minFaceDot = 999;
        Vector4 minFaceNormal = new Vector4();

        for (int i = 0; i < normals.Length; i++)
        {
            float thisMinDot = Mathf.Min(Vector4.Dot(v1,normals[i]), Vector4.Dot(v2,normals[i]));

            if (thisMinDot < minFaceDot) {
                minFaceDot = thisMinDot;
                minFaceNormal = normals[i];
            }
        }

        Vector4 faceClose = new Vector4();
        Vector4 faceCloseLinePoint = new Vector4();

        if (Vector4.Dot(v1,minFaceNormal) <  Vector4.Dot(v2,minFaceNormal))
        {
            faceClose = (v1 - Vector4.Dot(v1,minFaceNormal)*minFaceNormal).normalized;
            faceCloseLinePoint = v1;
        }
        else
        {
            faceClose = (v2 - Vector4.Dot(v2,minFaceNormal)*minFaceNormal).normalized;
            faceCloseLinePoint = v2;
        }

        //edges
        float closeEdgeDot = -1;
        Vector4 closeEdgePoint = new Vector4();
        Vector4 closeEdgeLinePoint = new Vector4();
        for (int i = 0; i < edges.Length; i++)
        {
            Vector4 close1 = new Vector4();
            Vector4 close2 = new Vector4();
            UFunc.DoubleArcClose(verticiesWorld[edges[i].x],verticiesWorld[edges[i].y], v1, v2, ref close1, ref close2);

            float thisDot = Vector4.Dot(close1,close2);
            if (thisDot > closeEdgeDot)
            {
                closeEdgePoint = close1;
                closeEdgeLinePoint = close2;
            }
        }


        bool facePointInRange = true;
        for (int i = 0; i < normals.Length; i++)
        {
            float dot = Vector4.Dot(faceClose,normals[i]);
            if (dot > 0.001f) {
                facePointInRange = false;
                break;
            }
        }

        print(facePointInRange+" "+UFunc.DistanceS(faceClose, faceCloseLinePoint)+" "+UFunc.DistanceS(closeEdgePoint, closeEdgeLinePoint));
        print(closeEdgeLinePoint+" "+closeEdgePoint);

        if (facePointInRange)
        {
            float distanceFace = UFunc.DistanceS(faceClose, faceCloseLinePoint);
            float distanceEdge = UFunc.DistanceS(closeEdgePoint, closeEdgeLinePoint);

            if (distanceEdge < distanceFace)
            {
                outLine = closeEdgeLinePoint;
                outTri = closeEdgePoint;
            }
            else
            {
                outLine = faceCloseLinePoint;
                outTri = faceClose;
            }
        }
        else
        {
            outLine = closeEdgeLinePoint;
            outTri = closeEdgePoint;
        }
        */
    }
}
 