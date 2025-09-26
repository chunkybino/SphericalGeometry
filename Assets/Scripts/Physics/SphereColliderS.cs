using UnityEngine;

public class SphereColliderS : ColliderS
{
    public float radius = 1;
    public override float m_radius {get{return radius*transform4.scale/Transform4D.radius;}}

    public override float boundingRadius {get{return m_radius;}}

    public Vector4 center {get{
        return transform4.positionNorm;
    }}

    public override int colliderType {get{return 0;}}
    public override SphereColliderS sphere {get{return this;}}

    public override Vector4 PointClose(Vector4 point) {
        return center;
    }

    public override void LineClose(Vector4 v1, Vector4 v2, ref Vector4 outLine, ref Vector4 outThis)
    {
        Vector4 s = UFunc.SlerpPointClose(v1,v2,transform4.positionNorm);
        outLine = UFunc.SlerpPointClose(v1,v2,s);
        outThis = transform4.positionNorm;
    }
}
