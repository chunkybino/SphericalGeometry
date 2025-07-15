using UnityEngine;

public class CapsuleColliderS : ColliderS
{
    public float radius = 1;
    public float length = 1;

    public Axis axis;
    public enum Axis {X,Y,Z};

    public int capsuleAxis {get{
        if (axis == Axis.X) return 0;
        if (axis == Axis.Y) return 1;
        return 2;
    }}

    public override int colliderType {get{return 1;}}
    public override CapsuleColliderS capsule {get{return this;}}
}
