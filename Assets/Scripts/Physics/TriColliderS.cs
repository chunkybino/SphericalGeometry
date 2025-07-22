using UnityEngine;

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
        Vector4 projPoint = (point - planeCenter*UFunc.Dot(point, planeCenter)).normalized;

        print(projPoint);  
        //return projPoint;

        Vector4 p1 = worldVertex1;
        Vector4 p2 = worldVertex2;
        Vector4 p3 = worldVertex3;

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

        //if all dots negative, the point is inside the triangle
        if (edgeDot1 < 0 && edgeDot2 < 0 && edgeDot3 < 0) {
            return projPoint;
        }

        //if 2 are positive, choose the corner between those edges
        if (edgeDot3 > 0 && edgeDot1 > 0) {
            return p1;
        }
        if (edgeDot1 > 0 && edgeDot2 > 0) {
            return p2;
        }
        if (edgeDot2 > 0 && edgeDot3 > 0) {
            return p3;
        }

        if (edgeDot1 > 0) {
            return UFunc.SlerpPointClose(p1,p2,point);
        }
        if (edgeDot2 > 0) {
            return UFunc.SlerpPointClose(p2,p3,point);
        }
        if (edgeDot3 > 0) {
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
