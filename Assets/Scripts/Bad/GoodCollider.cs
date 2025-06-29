using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[ExecuteInEditMode]
public class GoodCollider : MonoBehaviour
{
    public Vector3 colliderVectorX = Vector3.right;
    public Vector3 colliderVectorY = Vector3.up;
    public Vector3 colliderVectorZ = Vector3.forward;//

    int[] edges = new int[] {
        0,1,
        2,3,
        4,5,
        6,7,

        0,2,
        1,3,
        4,6,
        5,7,

        0,4,
        1,5,
        2,6,
        3,7
    };

    //the indexes of the adjacent verticies of input index
    static Vector3Int[] edgeAdjacents = new Vector3Int[] {
        new Vector3Int(1,2,4),
        new Vector3Int(0,3,5),
        new Vector3Int(3,0,6),
        new Vector3Int(2,1,7),
        new Vector3Int(5,6,0),
        new Vector3Int(4,7,1),
        new Vector3Int(7,4,2),
        new Vector3Int(6,5,3)
    };

    [SerializeField] bool doCheck;
    [SerializeField] GoodCollider doCheckTarget;

    void OnValidate()
    {
        if (doCheck) {
            doCheck = false;
            Vector3 move = doCheckTarget.Check(this);
            transform.position += move;
        }

        if (transform.hasChanged) UpdateVectors();
    }

    void UpdateVectors()
    {
        colliderVectorX = transform.rotation * Vector3.right * transform.localScale.x;
        colliderVectorY = transform.rotation * Vector3.up * transform.localScale.y;
        colliderVectorZ = transform.rotation * Vector3.forward * transform.localScale.z;

        transform.hasChanged = false;
    }

    void Update()
    {
        if (transform.hasChanged) UpdateVectors();
    }

    public Vector3 GetColliderVector(int i)
    {
        switch (i)
        {
            default:
                return colliderVectorX;
            case 1:
                return colliderVectorY;
            case 2:
                return colliderVectorZ;
        }
    }

    public Vector3 GetVertex(int i)
    {
        //all xyz start negative, then altrnate to positive 
        Vector3 signV = GetVertexSign(i);

        return colliderVectorX*signV.x + colliderVectorY*signV.y + colliderVectorZ*signV.z;
    }
    public Vector3 GetVertexSign(int i)
    {
        return new Vector3(
            (i % 2 > 0) ? 1 : -1,
            (i % 4 > 1) ? 1 : -1,
            (i > 3) ? 1 : -1
        );
    }

    public Vector3 ColliderSpace(Vector3 inVec)
    {
        Vector3 v = new Vector3(UFunc.Dot(inVec, colliderVectorX.normalized) / colliderVectorX.magnitude, 
            UFunc.Dot(inVec, colliderVectorY.normalized) / colliderVectorY.magnitude, 
            UFunc.Dot(inVec, colliderVectorZ.normalized) / colliderVectorZ.magnitude
        );
        return v;
    }

    public Vector3 Check(GoodCollider col)
    {   
        Vector3 checkMoveVec = new Vector3();

        if (CheckVertexOnPlane(col, ref checkMoveVec)) {
            
        }

        return checkMoveVec;
    }

    public bool CheckVertexOnPlane(GoodCollider col, ref Vector3 outV) //vertices of input collider against faces of this collider
    {
        Vector3 posDifference = col.transform.position - transform.position;
        Vector3 colCenter = new Vector3(
            UFunc.Dot(posDifference,colliderVectorX) / Mathf.Pow(transform.localScale.x,2), 
            UFunc.Dot(posDifference,colliderVectorY) / Mathf.Pow(transform.localScale.y,2), 
            UFunc.Dot(posDifference,colliderVectorZ) / Mathf.Pow(transform.localScale.z,2)
        );

        Vector3[] verticies = new Vector3[8];

        for (int i = 0; i < verticies.Length; i++) {
            Vector3 vertex = col.GetVertex(i) + posDifference;
            verticies[i] = new Vector3(
                UFunc.Dot(vertex,colliderVectorX) / Mathf.Pow(transform.localScale.x,2), 
                UFunc.Dot(vertex,colliderVectorY) / Mathf.Pow(transform.localScale.y,2), 
                UFunc.Dot(vertex,colliderVectorZ) / Mathf.Pow(transform.localScale.z,2)
            );
        }

        List<Vector3> moveVectors = new List<Vector3>();

        DoVertex();
        DoEdge();
        DoFace();

        if (moveVectors.Count == 0) return false;

        Vector3 minVector = moveVectors[0];
        foreach (Vector3 v in moveVectors)
        {
            if (v.magnitude < minVector.magnitude) minVector = v;
        }

        outV = minVector;
        return true;

        void DoVertex()
        {
            //indexes of points furthest from griven face (vertexLo is distance from -x, -y, and -z plane respectivly)
            Vector3Int vertexLo = new Vector3Int();
            Vector3Int vertexHi = new Vector3Int();

            for (int i = 0; i < 8; i++)
            {
                if (verticies[i].x < verticies[vertexLo.x].x) vertexLo.x = i;
                if (verticies[i].y < verticies[vertexLo.y].y) vertexLo.y = i;
                if (verticies[i].z < verticies[vertexLo.z].z) vertexLo.z = i;
            }

            vertexHi.x = 7-vertexLo.x; // the Hi is always the vertex on the opposite side from the Lo index
            vertexHi.y = 7-vertexLo.y;
            vertexHi.z = 7-vertexLo.z;

            float[] distances = new float[] {1-verticies[vertexLo.x].x, 1+verticies[vertexHi.x].x, 1-verticies[vertexLo.y].y, 1+verticies[vertexHi.y].y, 1-verticies[vertexLo.z].z, 1+verticies[vertexHi.z].z};
            int furthestIndex = UFunc.ClosestOfListIndex(distances);

            if (distances[furthestIndex] <= 0) return;

            Vector3 addVector = new Vector3();
            addVector[furthestIndex/2] = distances[furthestIndex];
            if (furthestIndex % 2 == 1) addVector *= -1; 
            moveVectors.Add(addVector);
        }

        void DoEdge()
        {

        }

        void DoFace()
        {
            Vector3[] faceNormals = new Vector3[] {
                col.colliderVectorX.normalized, -col.colliderVectorX.normalized,
                col.colliderVectorY.normalized, -col.colliderVectorY.normalized,
                col.colliderVectorZ.normalized, -col.colliderVectorZ.normalized
            };

            //int[] faceFurthestVertex = new int[6];
            float[] distances = new float[6];

            for (int i = 0; i < 6; i++)
            {
                Vector3 norm = faceNormals[i];
                int vertexIndex = 0;
                if (norm.x > 0) vertexIndex += 1;
                if (norm.y > 0) vertexIndex += 2;
                if (norm.z > 0) vertexIndex += 4;

                distances[i] = UFunc.Dot(GetVertexSign(vertexIndex)-colCenter, norm) + transform.localScale[i/3];
            }

            int minIndex = UFunc.MinIndex(distances);

            if (distances[minIndex] <= 0) return;

            moveVectors.Add(faceNormals[minIndex] * distances[minIndex]);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3[] linePoints = new Vector3[edges.Length];

        for (int i = 0; i < edges.Length; i++) {
            linePoints[i] = GetVertex(edges[i]) + transform.position;
        }

        Gizmos.DrawLineList(linePoints);
    }
}