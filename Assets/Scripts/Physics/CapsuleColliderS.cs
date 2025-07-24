using UnityEngine;

public class CapsuleColliderS : ColliderS
{
    public float radius = 1;
    public float length = 1;

    public override float m_radius {get{return radius;}}

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
        Vector4 v = new Vector4(0,0,0,Mathf.Cos(length/Transform4D.radius));
        v[capsuleAxis] = Mathf.Sin(length/Transform4D.radius);
        return v;
    }}
    public Vector4 point2local {get{
        Vector4 v = new Vector4(0,0,0,Mathf.Cos(length/Transform4D.radius));
        v[capsuleAxis] = -Mathf.Sin(length/Transform4D.radius);
        return v;
    }}

    public override int colliderType {get{return 1;}}
    public override CapsuleColliderS capsule {get{return this;}}

    public override Vector4 PointClose(Vector4 point)
    {
        return UFunc.SlerpPointClose(point1,point2,point);
    }
}
