using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ColliderBad : MonoBehaviour
{
    public Vector3 vector1;
    public Vector3 vector2;

    public Vector3 sideNorm1;
    public Vector3 sideNorm2;

    public float sideLength1; //perpendicual distance from edge to center
    public float sideLength2;

    public Vector3 normal;

    int[] edges = new int[] {
        0,1,
        1,2,
        2,3,
        3,0
    };
    Vector3 GetEdgeNormal(int i)
    {
        switch (i)
        {
            default:
                return sideNorm1;
            case 1:
                return -sideNorm2;
            case 2:
                return -sideNorm1;
            case 3:
                return sideNorm2;
        }
    }

    public Vector3[] verticies = new Vector3[4];
    Vector3[] verticiesWorld = new Vector3[4];

    [SerializeField] bool initialize;

    [SerializeField] bool testCollision;
    [SerializeField] ColliderBad testCollisionObject;
    [SerializeField] Vector3 testCollisionVelocity;

    public Vector3 this[int i]
    {
        get {
            return verticies[i];
        }
        set {
            verticies[i] = value;
        }
    }

    void OnValidate()
    {
        if (initialize) {
            initialize = false;
            Initialize();
        }

        if (testCollision) {
            testCollision = false;
            transform.position += testCollisionVelocity + testCollisionObject.TryCollisionVel(this, testCollisionVelocity);

            UpdateWorldVerticies();
        }
    }

    void Initialize()
    {
        normal = Vector3.Cross(vector1,vector2).normalized;

        sideNorm1 = Vector3.Cross(vector2, normal).normalized;
        sideNorm2 = Vector3.Cross(normal, vector1).normalized;

        sideLength1 = UFunc.Dot(sideNorm1, vector1);
        sideLength2 = UFunc.Dot(sideNorm2, vector2);

        verticies = new Vector3[] {
            vector1+vector2,
            vector1-vector2,
            -vector1-vector2,
            -vector1+vector2
        };

        UpdateWorldVerticies();
    }

    void UpdateWorldVerticies()
    {
        for (int i = 0; i < verticies.Length; i++) {verticiesWorld[i] = verticies[i] + transform.position;}
    }

    public Vector3 TryCollisionVel(ColliderBad col, Vector3 vel)
    {
        Vector3[] relativeVerticies = new Vector3[4];
        for (int i = 0; i < 4; i++) {relativeVerticies[i] = col.verticiesWorld[i] - transform.position;}

        List<Vector3> validVertex = new List<Vector3>();
        int centerVertexIndex = 0;
        List<int> validEdges = new List<int>();

        void TryAddValidVertex(int i)
        {
            if (!validVertex.Contains(relativeVerticies[i])) {
                validVertex.Add(relativeVerticies[i]);
            } else {
                centerVertexIndex = i;
            }
        }

        if (UFunc.Dot(vel, col.sideNorm1) > 0) {
            validVertex.Add(relativeVerticies[0]);
            validVertex.Add(relativeVerticies[1]);
            validEdges.Add(0);
            validEdges.Add(1);
        } else if (UFunc.Dot(vel, col.sideNorm1) < 0) {
            validVertex.Add(relativeVerticies[2]);
            validVertex.Add(relativeVerticies[3]);
            validEdges.Add(2);
            validEdges.Add(3);
        }

        if (UFunc.Dot(vel, col.sideNorm2) > 0) {
            TryAddValidVertex(3);
            TryAddValidVertex(0);
            validEdges.Add(3);
            validEdges.Add(0);
        } else if (UFunc.Dot(vel, col.sideNorm2) < 0) {
            TryAddValidVertex(1);
            TryAddValidVertex(2);
            validEdges.Add(1);
            validEdges.Add(2);
        }

        Vector3 GetDot(Vector3 v) {
            return new Vector3(UFunc.Dot(v, normal), UFunc.Dot(v, sideNorm1), UFunc.Dot(v, sideNorm2));
        }

        Vector3 velDot = GetDot(vel);

        Vector3[] normalDots = new Vector3[validVertex.Count];

        int smallestNormalIndex = 0; //smallest normal that point sin direciton of velocity
        bool allSameNormalSign = true;
        for (int i = 0; i < normalDots.Length; i++) 
        {
            normalDots[i] = GetDot(validVertex[i]);
            if (i > 0 && !UFunc.SameSign(normalDots[i].x,normalDots[i-1].x)) allSameNormalSign = false;

            if (!UFunc.SameSign(normalDots[i].x, velDot.x) &&
                Mathf.Abs(normalDots[i].x) < Mathf.Abs(normalDots[smallestNormalIndex].x))
            {
                smallestNormalIndex = i;
            }
        }

        Vector3 smallestNormalPoint = normalDots[smallestNormalIndex];

        if (allSameNormalSign)
        {
            //we dont move furth enough to intersect
            if (UFunc.SameSign(smallestNormalPoint.x, smallestNormalPoint.x + velDot.x)) return Vector3.zero;

            Vector2 planeIntersect = new Vector2(smallestNormalPoint.y, smallestNormalPoint.z) + new Vector2(velDot.y,velDot.z) * (smallestNormalPoint.x/velDot.x);

            if (Mathf.Abs(planeIntersect.x) < sideLength1 && Mathf.Abs(planeIntersect.y) < sideLength2)
            {
                //the point intersects the plane
                return normal * -(smallestNormalPoint.x + velDot.x);
            }
        }

        for (int i = 0; i < validEdges.Count/2; i++)
        {
            Vector3 v3 = relativeVerticies[validEdges[2*i]];
            Vector3 v4 = relativeVerticies[validEdges[2*i + 1]];
            Vector3 edgeNorm = col.GetEdgeNormal(validEdges[2*i]);
            print(edgeNorm);
            for (int j = 0; j < 4; j++)
            {
                print(edgeNorm+" "+GetEdgeNormal(j).ToString());
                if (UFunc.Dot(edgeNorm, GetEdgeNormal(j)) > 0) continue;
                print(new Vector2(i,j));

                Vector3 edgeVec = CheckEdgeOnEdge(verticies[edges[2*j]], verticies[edges[2*j+1]], v3, v4, GetEdgeNormal(j));
                print(edgeVec);
                if (edgeVec != Vector3.zero) return edgeVec;
            }
        }

        return Vector3.zero;
        
        Vector3 CheckEdgeOnEdge(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 edgeNormal) //v3 and v4 are moving edge
        {
            Vector3 delta1 = v2-v1;
            Vector3 delta2 = v4-v3;

            Vector3 norm = Vector3.Cross(delta1, delta2).normalized;

            print(v1.ToString()+" "+v2.ToString()+" "+v3.ToString()+" "+v4.ToString());
            print(delta1.ToString()+" "+delta2.ToString());

            print(edgeNormal.ToString() + " " + norm.ToString());
            print(UFunc.Dot(edgeNormal,norm));

            //if (UFunc.Dot(edgeNormal,norm) > 0) return Vector3.zero;

            Vector3 deltaPerp1 = Vector3.Cross(delta1, norm);
            Vector3 deltaPerp2 = Vector3.Cross(delta2, norm);

            Vector3 velDot = new Vector3(UFunc.Dot(norm, vel), UFunc.Dot(delta1.normalized, vel), UFunc.Dot(delta2.normalized, vel));

            float distance = UFunc.Dot(norm, v3-v1);

            print(distance);
            print(velDot);
            
            //we dont move furth enough to intersect
            if (UFunc.SameSign(distance, distance + velDot.x)) return Vector3.zero;

            Vector3 velMove = vel.normalized * velDot.x; //movement needed for the edges to touch

            print(UFunc.Dot(deltaPerp1, (v3+velMove) - v1).ToString()+" "+UFunc.Dot(deltaPerp1, (v4+velMove) - v1).ToString());
            print(UFunc.Dot(deltaPerp2, (v3+velMove) - v1).ToString()+" "+UFunc.Dot(deltaPerp2, (v3+velMove) - v2).ToString());

            //the edges dont interect in their bounds
            if (UFunc.SameSign(UFunc.Dot(deltaPerp1, (v3+velMove) - v1), UFunc.Dot(deltaPerp1, (v4+velMove) - v1))) return Vector3.zero;
            if (UFunc.SameSign(UFunc.Dot(deltaPerp2, (v3+velMove) - v1), UFunc.Dot(deltaPerp2, (v3+velMove) - v2))) return Vector3.zero;

            return norm * -(velDot.x + distance);
        }
    }

    void OnDrawGizmos()
    {
        UpdateWorldVerticies();

        Gizmos.color = Color.green;
        Gizmos.DrawLineStrip(verticiesWorld, true);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, vector1 + transform.position);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, vector2 + transform.position);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, normal + transform.position);
        Gizmos.DrawLine(transform.position, sideNorm1 + transform.position);
        Gizmos.DrawLine(transform.position, sideNorm2 + transform.position);
    }
}
