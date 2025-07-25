using UnityEngine;

public class SphereColliderS : ColliderS
{
    public float radius = 1;
    public override float m_radius {get{return radius;}}

    public override float boundingRadius {get{return radius;}}

    public Vector4 center {get{
        return transform4.positionNorm;
    }}

    public override int colliderType {get{return 0;}}
    public override SphereColliderS sphere {get{return this;}}

    public override Vector4 PointClose(Vector4 point) {
        return center;
    }
}
