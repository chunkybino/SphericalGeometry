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
        List<Vector3> validMoveVecs = new List<Vector3>();

        Vector3 checkMoveVec = new Vector3();

        if (CheckVertexOnPlane(col, ref checkMoveVec, true)) {
            validMoveVecs.Add(checkMoveVec);
        }
        if (col.CheckVertexOnPlane(this, ref checkMoveVec, false)) {
            validMoveVecs.Add(-checkMoveVec);
        }

        if (validMoveVecs.Count == 0) return Vector3.zero;

        Vector3 minVec = validMoveVecs[0];
        print(validMoveVecs[0]);
        for (int i = 1; i < validMoveVecs.Count; i++) {
            print(validMoveVecs[i]);
            if (validMoveVecs[i].magnitude < minVec.magnitude) minVec = validMoveVecs[i];
        }

        return minVec;
    }

    public bool CheckVertexOnPlane(GoodCollider col, ref Vector3 outV, bool tryEdges = true) //vertices of input collider against faces of this collider
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

            print("vertex " + vertexIndex.ToString());

            Vector3 vertex = verticies[vertexIndex];

            //the vectors that moves furthest against the dir vec
            Vector3 furthestOpposeEdge = vertex;
            float furthestOpposeDistance = UFunc.Dot(dirVec, vertex);
            //print(furthestOpposeDistance);
            //int opposeVertexCount = 0;
            List<int> opposeVertexEdgeIndex = new List<int>();

            Vector3Int adjacentEdges = edgeAdjacents[vertexIndex];

            for (int i = 0; i < 3; i++) {
                float dot = UFunc.Dot(dirVec, verticies[adjacentEdges[i]]);
                if (dot < furthestOpposeDistance) {
                    furthestOpposeEdge = verticies[adjacentEdges[i]];
                    furthestOpposeDistance = dot;
                    opposeVertexEdgeIndex.Add(i);
                }
                //print(dot);
            }

            print(opposeVertexEdgeIndex.Count);

            if (opposeVertexEdgeIndex.Count == 0) //normal point on face collision, get the distance from face and add it to list
            {
                float distance = (index % 2 == 0) ? 
                    vetexSidePos+1 : //distance from negative side
                    -vetexSidePos+1; //distance from positive side

                if (distance > 0) moveVectors.Add(dirVec * distance);
            }
            else if (opposeVertexEdgeIndex.Count == 1 && tryEdges && false) //edge-edge collision time
            {
                Vector3 vertexCenterDelta = colCenter-vertex; //distance from vertex to center of its collider
                int cornerVertexIndex = 0;

                //find whaty quadrants the delta sits in to get the corner of the edges we'll be checking
                if (vertexCenterDelta.x > 0) cornerVertexIndex += 1;
                if (vertexCenterDelta.y > 0) cornerVertexIndex += 2;
                if (vertexCenterDelta.z > 0) cornerVertexIndex += 4;

                Vector3 otherAdjacentVertex1 = verticies[adjacentEdges[1]]; //the adjacent vertex that is not furthestOpposeEdge
                Vector3 otherAdjacentVertex2 = verticies[adjacentEdges[2]];
                if (opposeVertexEdgeIndex[0] == 1) {
                    otherAdjacentVertex1 = verticies[adjacentEdges[0]];
                    otherAdjacentVertex2 = verticies[adjacentEdges[2]];
                } else if (opposeVertexEdgeIndex[0] == 2) {
                    otherAdjacentVertex1 = verticies[adjacentEdges[0]];
                    otherAdjacentVertex2 = verticies[adjacentEdges[1]];
                }

                Vector3 minMove = new Vector3();

                //if the other adjacent verticies are on opposite sides of our vertex along a certian axis, we can intersect the edge along that axis
                List<int> edgeCheckIndex = new List<int>();

                Vector3 cornerSignV = GetVertexSign(cornerVertexIndex);

                bool xValid = UFunc.SameSign(otherAdjacentVertex1.x-vertex.x, cornerSignV.x) && UFunc.SameSign(otherAdjacentVertex2.x-vertex.x, cornerSignV.x);
                bool yValid = UFunc.SameSign(otherAdjacentVertex1.y-vertex.y, cornerSignV.y) && UFunc.SameSign(otherAdjacentVertex2.y-vertex.y, cornerSignV.y);
                bool zValid = UFunc.SameSign(otherAdjacentVertex1.z-vertex.z, cornerSignV.z) && UFunc.SameSign(otherAdjacentVertex2.z-vertex.z, cornerSignV.z);

                if (yValid && zValid) edgeCheckIndex.Add(0);
                if (xValid && zValid) edgeCheckIndex.Add(1);
                if (xValid && yValid) edgeCheckIndex.Add(2);

                for (int i = 0; i < edgeCheckIndex.Count; i++) //check the 3 edges of the corner
                {
                    print(cornerVertexIndex);
                    print(edgeCheckIndex[i]);
                    print(edgeAdjacents[cornerVertexIndex]);
                    print(edgeAdjacents[cornerVertexIndex][edgeCheckIndex[i]]);
                    print(furthestOpposeEdge-vertex);

                    Vector3 move = CheckEdgeOnEdge(
                        GetVertexSign(cornerVertexIndex),
                        GetVertexSign(edgeAdjacents[cornerVertexIndex][edgeCheckIndex[i]]), 
                        vertex, 
                        furthestOpposeEdge
                    );

                    print("move " +move.ToString());

                    if (i == 0 || move.magnitude < minMove.magnitude) minMove = -move;
                }

                print(minMove);

                moveVectors.Add(minMove);
            }
        }
        //fucniton eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeendddddddddddddddddddddddddddddddddddddddd

        if (moveVectors.Count == 0) return false;

        Vector3 closestMoveVector = moveVectors[0];
        float closestMag = GetWarpMagnitude(closestMoveVector);
        
        print("the collection");
        for (int i = 0; i < moveVectors.Count; i++) 
        {
            print(moveVectors[i]);
            float mag = GetWarpMagnitude(moveVectors[i]);
            print(mag);
            if (mag < closestMag) {
                closestMoveVector = moveVectors[i];// * mag;
                closestMag = mag;
            }
        }

        print(closestMoveVector);

        float GetWarpMagnitude(Vector3 v) //get magnitude accounting for the scale of the collider, and how that warps the different axis lengths
        {
            return Mathf.Sqrt(
                Mathf.Pow(v.x * transform.localScale.x, 2) +
                Mathf.Pow(v.y * transform.localScale.y, 2) +
                Mathf.Pow(v.z * transform.localScale.z, 2)
            );
        }

        closestMoveVector = 
            closestMoveVector.x * colliderVectorX +
            closestMoveVector.y * colliderVectorY +
            closestMoveVector.z * colliderVectorZ;

        //closestMoveVector *= -1;

        print(closestMoveVector);

        outV = closestMoveVector;
        return true;
    }

    Vector3 CheckEdgeOnEdge(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        print(v1.ToString()+" "+v2.ToString());
        Vector3 d1 = v2-v1;
        Vector3 d2 = v4-v3;

        Vector3 norm = Vector3.Cross(d1,d2).normalized;

        print(norm);
        float distance = UFunc.Dot(norm, v3-v1);
        print(distance);

        return norm * distance;
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