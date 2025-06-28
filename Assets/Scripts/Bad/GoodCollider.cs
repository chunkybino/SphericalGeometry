using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[ExecuteInEditMode]
public class GoodCollider : MonoBehaviour
{
    public Vector3 colliderVectorX = Vector3.right;
    public Vector3 colliderVectorY = Vector3.up;
    public Vector3 colliderVectorZ = Vector3.forward;

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
        Vector3 moveVec = CheckVertexOnPlane(col);
        if (moveVec == Vector3.zero) moveVec = -col.CheckVertexOnPlane(this);

        return moveVec;
    }

    public Vector3 CheckVertexOnPlane(GoodCollider col) //vertices of input collider, faces of this collider
    {
        Vector3 posDifference = col.transform.position - transform.position;
        Vector3[] verticies = new Vector3[8];

        for (int i = 0; i < verticies.Length; i++) {
            verticies[i] = col.GetVertex(i) - posDifference;
            print(verticies[i]);
        }

        /*List<Vector3> insideVertex = new List<Vector3>();

        for (int i = 0; i < verticies.Length; i++)
        {
            Vector3 vertex = verticies[i];

            if (GetIsInside(vertex)) insideVertex.Add(vertex);
        }

        bool GetIsInside(Vector3 v) {
            return Mathf.Abs(v.x) <= 1 && Mathf.Abs(v.y) <= 1 && Mathf.Abs(v.z) <= 1;
        }*/

        Vector2 hiLoX = new Vector2(verticies[0].x,verticies[0].x);
        Vector2 hiLoY = new Vector2(verticies[0].y,verticies[0].y);
        Vector2 hiLoZ = new Vector2(verticies[0].z,verticies[0].z);
        foreach (Vector3 v in verticies)
        {
            bool inX = Mathf.Abs(v.x) <= 1;
            bool inY = Mathf.Abs(v.y) <= 1;
            bool inZ = Mathf.Abs(v.z) <= 1;
            
            if (inY & inZ) {
                hiLoX.x = Mathf.Max(hiLoX.x, v.x);
                hiLoX.y = Mathf.Min(hiLoX.y, v.x);
            }
            if (inX & inZ) {
                hiLoY.x = Mathf.Max(hiLoY.x, v.y);
                hiLoY.y = Mathf.Min(hiLoY.y, v.y);
            }
            if (inX & inY) {
                hiLoZ.x = Mathf.Max(hiLoZ.x, v.z);
                hiLoZ.y = Mathf.Min(hiLoZ.y, v.z);
            }
        }

        print(hiLoX);
        print(hiLoY);
        print(hiLoZ);

        int closestSideIndex = 0;
        float closestSideDistance = 999;

        for (int i = 0; i < 6; i++) {
            CheckSideDistance(i);
        }

        void CheckSideDistance(int index)
        {
            Vector3 hiLo = hiLoX;
            if (index == 1) {
                hiLo = hiLoY;
            } else if (index == 2) {
                hiLo = hiLoZ;
            }

            if (index % 2 == 0)
            {
                if (-hiLo.y+1 < closestSideDistance) { //distance from positive side
                    closestSideIndex = index;
                    closestSideDistance = -hiLo.y+1;
                }
            }
            else
            {
                if (hiLo.x+1 < closestSideDistance) { //distance from negative side
                    closestSideIndex = index;
                    closestSideDistance = hiLo.x+1;
                }
            }
        }

        if (closestSideDistance < 0) closestSideDistance = 0;
        if (closestSideIndex % 2 == 1) closestSideDistance *= -1; //if we mvoe to a negative side

        Vector3 force = closestSideDistance * colliderVectorX;
        if (Mathf.Abs(closestSideIndex) == 1) {
            force = closestSideDistance * colliderVectorY;
        } else if (Mathf.Abs(closestSideIndex) == 2) {
            force = closestSideDistance * colliderVectorZ;
        }

        force *= -1;

        print(force);

        return force;
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