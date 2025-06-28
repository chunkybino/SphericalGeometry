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
    Vector3Int[] edgeAdjacents = new Vector3Int[] {
        new Vector3Int(1,2,4),
        new Vector3Int(0,3,5),
        new Vector3Int(0,3,6),
        new Vector3Int(1,2,7),
        new Vector3Int(0,5,6),
        new Vector3Int(1,4,7),
        new Vector3Int(2,4,7),
        new Vector3Int(3,5,6)
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
        Vector3 signV = new Vector3();

        signV.x = (i % 2 > 0) ? 1 : -1;
        signV.y = (i % 4 > 1) ? 1 : -1;
        signV.z = (i > 3) ? 1 : -1;

        return colliderVectorX*signV.x + colliderVectorY*signV.y + colliderVectorZ*signV.z;
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
        List<Vector3> validMoveVecs = new List<Vector3>();

        Vector3 checkMoveVec = new Vector3();

        if (CheckVertexOnPlane(col, ref checkMoveVec)) {
            validMoveVecs.Add(checkMoveVec);
        }
        if (col.CheckVertexOnPlane(this, ref checkMoveVec)) {
            validMoveVecs.Add(-checkMoveVec);
        }

        if (validMoveVecs.Count == 0) return Vector3.zero;

        Vector3 minVec = validMoveVecs[0];
        for (int i = 1; i < validMoveVecs.Count; i++) {
            print(validMoveVecs[i]);
            if (validMoveVecs[i].magnitude < minVec.magnitude) minVec = validMoveVecs[i];
        }

        return minVec;
    }

    public bool CheckVertexOnPlane(GoodCollider col, ref Vector3 outV) //vertices of input collider, faces of this collider
    {
        Vector3 posDifference = col.transform.position - transform.position;
        Vector3[] verticies = new Vector3[8];

        for (int i = 0; i < verticies.Length; i++) {
            Vector3 vertex = col.GetVertex(i) + posDifference;
            verticies[i] = new Vector3(
                UFunc.Dot(vertex,colliderVectorX) / Mathf.Pow(transform.localScale.x,2), 
                UFunc.Dot(vertex,colliderVectorY) / Mathf.Pow(transform.localScale.y,2), 
                UFunc.Dot(vertex,colliderVectorZ) / Mathf.Pow(transform.localScale.z,2)
            );
        }

        Vector2 hiLoX = new Vector2(verticies[0].x,verticies[0].x);
        Vector2 hiLoY = new Vector2(verticies[0].y,verticies[0].y);
        Vector2 hiLoZ = new Vector2(verticies[0].z,verticies[0].z);

        Vector2Int hiLoXIndex = new Vector2Int();
        Vector2Int hiLoYIndex = new Vector2Int();
        Vector2Int hiLoZIndex = new Vector2Int();

        for (int i = 0; i < verticies.Length; i++)
        {
            Vector3 v = verticies[i];

            bool inX = Mathf.Abs(v.x) <= 1;
            bool inY = Mathf.Abs(v.y) <= 1;
            bool inZ = Mathf.Abs(v.z) <= 1;
            
            if (inY & inZ) {
                if (v.x < hiLoX.x) {
                    hiLoX.x = v.x;
                    hiLoXIndex.x = i;
                }
                if (v.x > hiLoX.y) {
                    hiLoX.y = v.x;
                    hiLoXIndex.y = i;
                }
            }
            if (inX & inZ) {
                if (v.y < hiLoY.x) {
                    hiLoY.x = v.y;
                    hiLoYIndex.x = i;
                }
                if (v.y > hiLoY.y) {
                    hiLoY.y = v.y;
                    hiLoYIndex.y = i;
                }
            }
            if (inX & inY) {
                if (v.z < hiLoZ.x) {
                    hiLoZ.x = v.z;
                    hiLoZIndex.x = i;
                }
                if (v.z > hiLoZ.y) {
                    hiLoZ.y = v.z;
                    hiLoZIndex.y = i;
                }
            }
        }

        List<Vector3> moveVectors = new List<Vector3>();

        for (int i = 0; i < 6; i++) {
            CheckDirection(i);
        }

        //funtion tiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiimeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee
        void CheckDirection(int index)
        {
            int vertexIndex = 0;
            float vetexSidePos = 0; //vertex posiiton around dir axis
            Vector3 dirVec = new Vector3();

            switch (index) {
                default:
                    vertexIndex = hiLoXIndex.y;
                    vetexSidePos = hiLoX.y;
                    dirVec = Vector3.left;
                    break;
                case 1:
                    vertexIndex = hiLoXIndex.x;
                    vetexSidePos = hiLoX.x;
                    dirVec = Vector3.right;
                    break;
                case 2:
                    vertexIndex = hiLoYIndex.y;
                    vetexSidePos = hiLoY.y;
                    dirVec = Vector3.down;
                    break;
                case 3:
                    vertexIndex = hiLoYIndex.x;
                    vetexSidePos = hiLoY.x;
                    dirVec = Vector3.up;
                    break;
                case 4:
                    vertexIndex = hiLoZIndex.y;
                    vetexSidePos = hiLoZ.y;
                    dirVec = Vector3.back;
                    break;
                case 5:
                    vertexIndex = hiLoZIndex.x;
                    vetexSidePos = hiLoZ.x;
                    dirVec = Vector3.forward;
                    break;
            }

            //the vectors that moves furthest against the dir vec
            Vector3 furthestOpposeEdge = verticies[vertexIndex];
            float furthestOpposeDistance =  UFunc.Dot(dirVec, furthestOpposeEdge);
            bool opposeVertexFound = false;

            Vector3Int adjacentEdges = edgeAdjacents[vertexIndex];

            for (int i = 0; i < 3; i++) {
                float dot = UFunc.Dot(dirVec, verticies[adjacentEdges[i]]);
                if (dot < furthestOpposeDistance) {
                    furthestOpposeEdge = verticies[adjacentEdges[i]];
                    furthestOpposeDistance = dot;
                    opposeVertexFound = true;
                }
            }

            if (!opposeVertexFound) //normal point on face collision, get the distance from face and add it to list
            {
                float distance = (index % 2 == 0) ? 
                    vetexSidePos+1 : //distance from negative side
                    -vetexSidePos+1; //distance from positive side

                if (distance > 0) moveVectors.Add(dirVec * distance);
            }
        }
        //fucniton eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeendddddddddddddddddddddddddddddddddddddddd

        if (moveVectors.Count == 0) return false;

        Vector3 closestMoveVector = moveVectors[0];
        float closestMag = GetWarpMagnitude(closestMoveVector);
        
        for (int i = 0; i < moveVectors.Count; i++) 
        {
            float mag = GetWarpMagnitude(moveVectors[i]);
            if (mag < closestMag) {
                closestMoveVector = moveVectors[i];// * mag;
                closestMag = mag;
            }
        }

        float GetWarpMagnitude(Vector3 v) //get magnitude accounting for the scale of the collider, and how that warps the different axis lengths
        {
            return Mathf.Sqrt(
                Mathf.Pow(v.x * transform.localScale.x, 2) +
                Mathf.Pow(v.y * transform.localScale.y, 2) +
                Mathf.Pow(v.z * transform.localScale.z, 2)
            );
        }

        print(closestMoveVector);

        closestMoveVector = 
            closestMoveVector.x * colliderVectorX +
            closestMoveVector.y * colliderVectorY +
            closestMoveVector.z * colliderVectorZ;

        //closestMoveVector *= -1;

        print(closestMoveVector);

        outV = closestMoveVector;
        return true;
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