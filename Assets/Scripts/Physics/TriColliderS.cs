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

    [SerializeField] Vector4 planeCenter;

    /*
    Vector4 localVertex1 {get{
        return new Vector4(vertex1.x,vertex1.y,vertex1.z,1).normalized;
    }}
    Vector4 localVertex2 {get{
        return new Vector4(vertex2.x,vertex2.y,vertex2.z,1).normalized;
    }}
    Vector4 localVertex3 {get{
        return new Vector4(vertex3.x,vertex3.y,vertex3.z,1).normalized;
    }}
    */

    void Update()
    {
        localVertex1 = new Vector4(vertex1.x,vertex1.y,vertex1.z,1).normalized;
        localVertex2 = new Vector4(vertex2.x,vertex2.y,vertex2.z,1).normalized;
        localVertex3 = new Vector4(vertex3.x,vertex3.y,vertex3.z,1).normalized;

        worldVertex1 = transform4.matrix * localVertex1;
        worldVertex2 = transform4.matrix * localVertex2;
        worldVertex3 = transform4.matrix * localVertex3;

        planeCenter = UFunc.HyperCross(worldVertex1,worldVertex2,worldVertex3).normalized;
    }

    /*
    public Vector4 worldVertex1 {get{
        return transform4.matrix * localVertex1;
    }}
    public Vector4 worldVertex2 {get{
        return transform4.matrix * localVertex2;
    }}
    public Vector4 worldVertex3 {get{
        return transform4.matrix * localVertex3;
    }}
    */

    /*
    public Vector4 planeCenter {get{
        Vector4 cross = UFunc.HyperCross(localVertex1,localVertex2,localVertex3);
        return transform4.matrix * cross.normalized;
    }}
    */

    public override int colliderType {get{return 2;}}
    public override TriColliderS triangle {get{return this;}}

    public Vector4 PointClose(Vector4 point)
    {
        //center of sphere created by the plane of our tri
        Vector4 pCenter = planeCenter;

        //point projected onto the plane of our tri
        //Vector4 projPoint = (point - planeCenter*UFunc.Dot(point, planeCenter)).normalized;
        Vector4 projPoint = UFunc.Slerp4Angle(point,planeCenter, UFunc.DistanceS(point,planeCenter) - Mathf.PI/2);

        //print(projPoint);  
        //print(projPoint2);  
        //print(UFunc.DistanceS(point,projPoint));  

        Vector4 p1 = worldVertex1;
        Vector4 p2 = worldVertex2;
        Vector4 p3 = worldVertex3;

        //UFunc.PrintList(UFunc.DistanceS(point,p1), UFunc.DistanceS(point,p2), UFunc.DistanceS(point,p3));  

        //center of the ring created by extending one of the tris edges
        Vector4 edgeNormal1 = UFunc.HyperCross(p1,p2,pCenter);
        Vector4 edgeNormal2 = UFunc.HyperCross(p2,p3,pCenter);
        Vector4 edgeNormal3 = UFunc.HyperCross(p3,p1,pCenter);

        //if this dot product is positive, that means that all the normals are facing inwards instead of outwards, we need to flip
        if (UFunc.Dot(edgeNormal1, p3) > 0) {
            edgeNormal1 *= -1;
            edgeNormal2 *= -1;
            edgeNormal3 *= -1;
        }

        float edgeDot1 = UFunc.Dot(edgeNormal1, projPoint);
        float edgeDot2 = UFunc.Dot(edgeNormal2, projPoint);
        float edgeDot3 = UFunc.Dot(edgeNormal3, projPoint);

        float edgeDis1 = Mathf.Acos(edgeDot1) - Mathf.PI/2;
        float edgeDis2 = Mathf.Acos(edgeDot2) - Mathf.PI/2;
        float edgeDis3 = Mathf.Acos(edgeDot3) - Mathf.PI/2;

        UFunc.PrintList(edgeDis1,edgeDis2,edgeDis3);
        //print("1-2 " + UFunc.SlerpPointCloseFactor(p1,p2,point,true,true) + UFunc.SlerpPointClose(p1,p2,point));

        List<Vector4> validPoint = new List<Vector4>();

        //if all dots negative, the point is inside the triangle
        if (edgeDot1 < 0 && edgeDot2 < 0 && edgeDot3 < 0) {
            //print("real");
            validPoint.Add(projPoint);
            //return projPoint;
        }



        
        Vector4[] edgePoints = new Vector4[] {
            UFunc.SlerpPointClose(p1,p2,point),
            UFunc.SlerpPointClose(p2,p3,point),
            UFunc.SlerpPointClose(p3,p1,point)
        };
        /*
        Vector4[] edgePoints = new Vector4[] {
            UFunc.Slerp4Angle(projPoint,edgeNormal1,edgeDis1),
            UFunc.Slerp4Angle(projPoint,edgeNormal2,edgeDis2),
            UFunc.Slerp4Angle(projPoint,edgeNormal3,edgeDis3)
        };
        */

        if (UFunc.BetweenS(p1,p2, edgePoints[0])) validPoint.Add(edgePoints[0]);
        if (UFunc.BetweenS(p2,p3, edgePoints[1])) validPoint.Add(edgePoints[1]);
        if (UFunc.BetweenS(p3,p1, edgePoints[2])) validPoint.Add(edgePoints[2]);

        //print(edgePoints[0].ToString()+edgePoints[1].ToString()+edgePoints[2].ToString());
        print(edgePoints[0]);
        print(UFunc.BetweenS(p1,p2, edgePoints[0]));

        validPoint.Add(p1);
        validPoint.Add(p2);
        validPoint.Add(p3);

        //print(UFunc.DistanceS(point,projPoint));  
        //UFunc.PrintList(UFunc.DistanceS(point,edgePoints[0]), UFunc.DistanceS(point,edgePoints[1]), UFunc.DistanceS(point,edgePoints[2]));

        if (validPoint.Count > 0) {
            Vector4 close = validPoint[0];
            foreach (Vector4 v in validPoint) {
                if (UFunc.Dot(point,v) > UFunc.Dot(point,close)) {
                    close = v;
                }
            }

            print("lame"+close);
            return close;
        }

        //return closest;

        //print(p2-projPoint);

        //if 2 are positive, choose the corner between those edges
        if (edgeDot3 > 0 && edgeDot1 > 0) {
            print("p1");
            return p1;
        }
        if (edgeDot1 > 0 && edgeDot2 > 0) {
            print("p2");
            return p2;
        }
        if (edgeDot2 > 0 && edgeDot3 > 0) {
            print("p3");
            return p3;
        }

        //return projPoint;

        if (edgeDot1 > 0) {
            //print("1-2 " + UFunc.SlerpPointCloseFactor(p1,p2,point,true,true) + UFunc.SlerpPointClose(p1,p2,point));
            print("1-2");
            return UFunc.SlerpPointClose(p1,p2,point);
        }
        if (edgeDot2 > 0) {
            //print("2-3 " + UFunc.SlerpPointCloseFactor(p2,p3,point,true,true) + UFunc.SlerpPointClose(p2,p3,point));
            print("2-3");
            return UFunc.SlerpPointClose(p2,p3,point);
        }
        if (edgeDot3 > 0) {
            //print("3-1 " + UFunc.SlerpPointCloseFactor(p3,p1,point,true,true) + UFunc.SlerpPointClose(p3,p1,point));
            print("3-1");
            return UFunc.SlerpPointClose(p3,p1,point);
        }

        //if somehow all dot products are positive, you done messed up
        Debug.LogWarning("ya messed up, all dot products are positive for this point close " + this.gameObject.ToString());
        return -projPoint;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLineStrip(new Vector3[] {
            UFunc.SterographicProjection(worldVertex1,1),
            UFunc.SterographicProjection(worldVertex2,1),
            UFunc.SterographicProjection(worldVertex3,1)}, 
            true);
    }
}
