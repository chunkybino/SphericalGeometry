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

    public override float boundingRadius { get { return furthestVertexDistance; } }
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

        planeCenter = UFunc.HyperCross(worldVertex1, worldVertex2, worldVertex3).normalized;

        edgeNormal1 = UFunc.HyperCross(worldVertex1, worldVertex2, planeCenter).normalized;
        edgeNormal2 = UFunc.HyperCross(worldVertex2, worldVertex3, planeCenter).normalized;
        edgeNormal3 = UFunc.HyperCross(worldVertex3, worldVertex1, planeCenter).normalized;

        if (UFunc.Dot(edgeNormal1, worldVertex3) > 0)
        {
            edgeNormal1 *= -1;
            edgeNormal2 *= -1;
            edgeNormal3 *= -1;
        }
    }

    public override int colliderType { get { return 2; } }
    public override TriColliderS triangle { get { return this; } }

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
        if (UFunc.Dot(points[0], points[1]) > UFunc.Dot(points[2], points[3]))
        {
            outLine = points[0];
            outThis = points[1];
        }
        else
        {
            outLine = points[2];
            outThis = points[3];
        }

        void FindMin(float startT, ref Vector4 point1, ref Vector4 point2)
        {
            int iterations = 6;

            point1 = UFunc.Slerp4(v1, v2, startT);
            Vector4 prev1 = point1;

            for (int i = 0; i < iterations; i++)
            {
                point2 = PointClose(point1);
                point1 = UFunc.SlerpPointClose(v1, v2, point2);
                if (prev1 == point1) return; //if we get back the same point, stop here
                prev1 = point1;
            }
        }
    }

    //static funcition, where we define the triagnle too
    public static Vector4 PointCloseTri(Vector4 point, Vector4 p1, Vector4 p2, Vector4 p3)
    {
        Vector4 pCenter = UFunc.HyperCross(p1, p2, p3).normalized;
        Vector4 edgeNorm1 = UFunc.HyperCross(p1, p2, pCenter).normalized;
        Vector4 edgeNorm2 = UFunc.HyperCross(p2, p3, pCenter).normalized;
        Vector4 edgeNorm3 = UFunc.HyperCross(p3, p1, pCenter).normalized;
        return PointCloseTri(point, p1, p2, p3, pCenter, edgeNorm1, edgeNorm2, edgeNorm3);
    }
    public static Vector4 PointCloseTri(Vector4 point, Vector4 p1, Vector4 p2, Vector4 p3, Vector4 pCenter, Vector4 edgeNorm1, Vector4 edgeNorm2, Vector4 edgeNorm3)
    {
        Vector4 projCenter = UFunc.ProjectToVectorNormal(point, pCenter).normalized;

        float edgeDot = Vector4.Dot(projCenter, edgeNorm1);
        if (edgeDot > 0)
        {
            projCenter = UFunc.ProjectToVectorNormal(projCenter, edgeNorm1).normalized;
        }

        edgeDot = Vector4.Dot(projCenter, edgeNorm2);
        if (edgeDot > 0)
        {
            projCenter = UFunc.ProjectToVectorNormal(projCenter, edgeNorm2).normalized;
        }

        edgeDot = Vector4.Dot(projCenter, edgeNorm3);
        if (edgeDot > 0)
        {
            projCenter = UFunc.ProjectToVectorNormal(projCenter, edgeNorm3).normalized;
        }

        return projCenter;
    }

    public static float LineCloseTri(Vector4 line1, Vector4 line2, Vector4 tri1, Vector4 tri2, Vector4 tri3, ref Vector4 outLine, ref Vector4 outTri)
    {
        Vector4 triNorm = UFunc.HyperCross(tri1, tri2, tri3).normalized;
        Vector4 sideNorm1 = UFunc.HyperCross(tri1, tri2, triNorm).normalized;
        Vector4 sideNorm2 = UFunc.HyperCross(tri2, tri3, triNorm).normalized;
        Vector4 sideNorm3 = UFunc.HyperCross(tri3, tri1, triNorm).normalized;

        float closeDot = -1;

        float lineDot1 = Vector4.Dot(line1, triNorm);
        float lineDot2 = Vector4.Dot(line2, triNorm);

        if (Mathf.Abs(lineDot1) < Mathf.Abs(lineDot2))
        {
            closeDot = Mathf.Abs(lineDot1);
            outLine = line1;
            outTri = UFunc.ProjectToVectorNormal(line1, triNorm).normalized;
        }
        else
        {
            closeDot = Mathf.Abs(lineDot2);
            outLine = line2;
            outTri = UFunc.ProjectToVectorNormal(line2, triNorm).normalized;
        }

        if (Mathf.Sign(lineDot1) != Mathf.Sign(lineDot2))
        {
            closeDot *= -1;
        }

        Vector4 v1 = tri1; 
        Vector4 v2 = tri2; 
        Vector4 v3 = tri3;

        for (int i = 0; i < 3; i++)
        {
            Vector4 closeLine = new Vector4();
            Vector4 closeEdge = new Vector4();
            UFunc.DoubleArcClose(line1, line2, v1, v2, ref closeLine, ref closeEdge);

            Vector4 dir = UFunc.ProjectToVectorNormal(closeEdge, closeLine).normalized;
            float dot = Vector4.Dot(dir, closeEdge);

            float triDot3 = Vector4.Dot(dir, v3);

            if (triDot3 > 0) // if the third point is in this direction, flip the outward direction chief
            {
                dot *= -1;
            }

            if (dot > closeDot)
            {
                closeDot = dot;
                outLine = closeLine;
                outTri = closeEdge;
            }

            (v1, v2) = (v2, v1);
            (v1, v3) = (v3, v1);
        }

        return closeDot;
    }
}
