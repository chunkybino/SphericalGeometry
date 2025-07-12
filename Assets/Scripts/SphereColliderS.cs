using UnityEngine;

public class SphereColliderS : ColliderS
{
    public float radius = 1;

    public override int colliderType {get{return 0;}}
    public override SphereColliderS sphere {get{return this;}}
}
