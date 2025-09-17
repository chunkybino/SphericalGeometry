using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TriColliderS : ColliderS
{
    //verticies local to transform in linear space
    public Vector3 vertex1;
    public Vector3 vertex2;
    public Vector3 vertex3;

    [SerializeField] bool gizmo;

    [SerializeField] Vector4 localVertex1;
    [SerializeField] Vector4 localVertex2;
    [SerializeField] Vector4 localVertex3;

    [SerializeField] Vector4 worldVertex1;
    [SerializeField] Vector4 worldVertex2;
    [SerializeField] Vector4 worldVertex3;

    [SerializeField] Vector4 edgeNormal1;
    [SerializeField] Vector4 edgeNormal2;
    [SerializeField] Vector4 edgeNormal3;

    [SerializeField] Vector4 planeCenter;

    public override float boundingRadius {get{return furthestVertexDistance;}}
    [SerializeField] float furthestVertexDistance;

    void OnValidate()
    {
        UpdateVertex();
        UpdateWorldVertex();
    }

    void UpdateVertex()
    {
        localVertex1 = UFunc.ProjectLocal3QuickS(vertex1);
        localVertex2 = UFunc.ProjectLocal3QuickS(vertex2);
        localVertex3 = UFunc.ProjectLocal3QuickS(vertex3);

        furthestVertexDistance = Mathf.Max(
            Mathf.Acos(localVertex1.w),
            Mathf.Acos(localVertex2.w),
            Mathf.Acos(localVertex3.w)
        );
    }
    void UpdateWorldVertex()
    {
        worldVertex1 = transform4.matrix * localVertex1;
        worldVertex2 = transform4.matrix * localVertex2;
        worldVertex3 = transform4.matrix * localVertex3;

        planeCenter = UFunc.HyperCross(worldVertex1,worldVertex2,worldVertex3).normalized;

        edgeNormal1 = UFunc.HyperCross(worldVertex1,worldVertex2,planeCenter).normalized;
        edgeNormal2 = UFunc.HyperCross(worldVertex2,worldVertex3,planeCenter).normalized;
        edgeNormal3 = UFunc.HyperCross(worldVertex3,worldVertex1,planeCenter).normalized;

        if (UFunc.Dot(edgeNormal1, worldVertex3) > 0) {
            edgeNormal1 *= -1;
            edgeNormal2 *= -1;
            edgeNormal3 *= -1;
        }
    }

    public override int colliderType {get{return 2;}}
    public override TriColliderS triangle {get{return this;}}

    public override Vector4 PointClose(Vector4 point)
    {
        return PointCloseTri(point, worldVertex1, worldVertex2, worldVertex3, planeCenter, edgeNormal1, edgeNormal2, edgeNormal3);
    }

    public override void LineClose(Vector4 v1, Vector4 v2, ref Vector4 outLine, ref Vector4 outThis)
    {
        Vector4[] points = new Vector4[4];

        FindMin(0, ref points[0], ref points[1]);
        FindMin(1, ref points[2], ref points[3]);

        //take the closest
        if (UFunc.Dot(points[0],points[1]) > UFunc.Dot(points[2],points[3])) {
            outLine = points[0];
            outThis = points[1];
        } else {
            outLine = points[2];
            outThis = points[3];
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

    //static funcition, where we define the triagnle too
    public static Vector4 PointCloseTri(Vector4 point, Vector4 p1, Vector4 p2, Vector4 p3)
    {
        Vector4 pCenter = UFunc.HyperCross(p1,p2,p3).normalized;
        Vector4 edgeNorm1 = UFunc.HyperCross(p1,p2,pCenter).normalized;
        Vector4 edgeNorm2 = UFunc.HyperCross(p2,p3,pCenter).normalized;
        Vector4 edgeNorm3 = UFunc.HyperCross(p3,p1,pCenter).normalized;
        return PointCloseTri(point,p1,p2,p3,pCenter,edgeNorm1,edgeNorm2,edgeNorm3);
    }
    public static Vector4 PointCloseTri(Vector4 point, Vector4 p1, Vector4 p2, Vector4 p3, Vector4 pCenter, Vector4 edgeNorm1, Vector4 edgeNorm2, Vector4 edgeNorm3)
    {
        //point projected onto the plane of our tri
        Vector4 projPoint = (point - pCenter*UFunc.Dot(point, pCenter)).normalized;

        float edgeDot1 = UFunc.Dot(edgeNorm1, projPoint);
        float edgeDot2 = UFunc.Dot(edgeNorm2, projPoint);
        float edgeDot3 = UFunc.Dot(edgeNorm3, projPoint);

        //if all dots negative, the point is inside the triangle
        if (edgeDot1 < 0 && edgeDot2 < 0 && edgeDot3 < 0) {
            return projPoint;
        }

        //check edges

        Vector4[] edgePoints = new Vector4[] {
            (projPoint - edgeNorm1*edgeDot1).normalized,
            (projPoint - edgeNorm2*edgeDot2).normalized,
            (projPoint - edgeNorm3*edgeDot3).normalized
        };

        List<Vector4> validEdge = new List<Vector4>();

        if (edgeDot1 > 0)
        {
            Vector4 edgePoint = (projPoint - edgeNorm1*edgeDot1).normalized;
            if (UFunc.BetweenS(p1,p2, edgePoint)) validEdge.Add(edgePoint);
        }
        if (edgeDot2 > 0)
        {
            Vector4 edgePoint = (projPoint - edgeNorm2*edgeDot2).normalized;
            if (UFunc.BetweenS(p2,p3, edgePoint)) validEdge.Add(edgePoint);
        }
        if (edgeDot3 > 0)
        {
            Vector4 edgePoint = (projPoint - edgeNorm3*edgeDot3).normalized;
            if (UFunc.BetweenS(p3,p1, edgePoint)) validEdge.Add(edgePoint);
        }

        if (edgeDot2 < 0) validEdge.Add(p1);
        if (edgeDot3 < 0) validEdge.Add(p2);
        if (edgeDot1 < 0) validEdge.Add(p3);

        Vector4 close = new Vector4();
        if (validEdge.Count > 0) {
            close = validEdge[0];
            foreach (Vector4 v in validEdge) {
                if (UFunc.Dot(point,v) > UFunc.Dot(point,close)) {
                    close = v;
                }
            }
            return close;
        }

        return projPoint; //ya dont messed up your math if it gets here
    }
}
