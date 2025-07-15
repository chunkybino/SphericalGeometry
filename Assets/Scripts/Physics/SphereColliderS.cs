using UnityEngine;

public class SphereColliderS : ColliderS
{
    public float radius = 1;

    public Vector4 center {get{
        return transform4.positionNorm;
    }}

    public override int colliderType {get{return 0;}}
    public override SphereColliderS sphere {get{return this;}}
}
