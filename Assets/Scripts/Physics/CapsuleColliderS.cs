using UnityEngine;

public class CapsuleColliderS : ColliderS
{
    public float radius = 1;
    public float length = 1;

    public override float m_radius {get{return radius*transform4.scale;}}
    public float m_length {get{return length*transform4.scale;}}

    public override float boundingRadius {get{return m_radius + 2*m_length;}}

    public Axis axis;
    public enum Axis {X,Y,Z};

    public int capsuleAxis {get{
        if (axis == Axis.X) return 0;
        if (axis == Axis.Y) return 1;
        return 2;
    }}

    public Vector4 point1 {get{
        return transform4.matrix * point1local;
    }}
    public Vector4 point2 {get{
        return transform4.matrix * point2local;
    }}
    public Vector4 point1local {get{
        Vector4 v = new Vector4(0,0,0,Mathf.Cos(m_length/Transform4D.radius));
        v[capsuleAxis] = Mathf.Sin(m_length/Transform4D.radius);
        return v;
    }}
    public Vector4 point2local {get{
        Vector4 v = new Vector4(0,0,0,Mathf.Cos(m_length/Transform4D.radius));
        v[capsuleAxis] = -Mathf.Sin(m_length/Transform4D.radius);
        return v;
    }}

    public override int colliderType {get{return 1;}}
    public override CapsuleColliderS capsule {get{return this;}}

    public override Vector4 PointClose(Vector4 point)
    {
        Vector4 s = UFunc.SlerpPointClose(point1,point2,point);

        return UFunc.SlerpPointClose(point1,point2,point);
    }

    public override void LineClose(Vector4 v1, Vector4 v2, ref Vector4 outLine, ref Vector4 outThis)
    {
        Vector4 close1 = new Vector4();
        Vector4 close2 = new Vector4();

        UFunc.DoubleArcClose(point1,point2,v1,v2, ref close1, ref close2);

        /*
        Vector4 close1_2 = new Vector4();
        Vector4 close2_2 = new Vector4();
        UFunc.DoubleArcClose(v1,v2,point1,point2, ref close2_2, ref close1_2);

        if (Vector4.Dot(close1_2,close2_2) > Vector4.Dot(close1,close2))
        {
            close1 = close1_2;
            close2 = close2_2;
        }
        */

        outThis = close1;
        outLine = close2;
    }
}
