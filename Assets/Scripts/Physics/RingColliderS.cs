using UnityEngine;

public class RingColliderS : ColliderS
{
    public override float boundingRadius {get{return 20;}}

    public override int colliderType {get{return 4;}}
    public override RingColliderS ring {get{return this;}}

    public Axis faceAxis = Axis.Z;
    public Axis thickAxis = Axis.Y;

    public float thickness = 0.1f;

    public enum Axis {X,Y,Z};
    public int axis1 {get{
        if (faceAxis == Axis.X) return 0;
        if (faceAxis == Axis.Y) return 1;
        return 2;
    }}
    public int axis2 {get{
        if (thickAxis == Axis.X) return 0;
        if (thickAxis == Axis.Y) return 1;
        return 2;
    }}

    Vector4 faceDirection {get{return transform4.matrix.GetColumn(axis1);}}
    Vector4 thickDirection {get{return transform4.matrix.GetColumn(axis2);}}


    public override Vector4 PointClose(Vector4 point) 
    {
        point = UFunc.ProjectToVectorNormal(point,faceDirection).normalized;

        float dot = Vector4.Dot(point, thickDirection);
        float thickDot = Mathf.Sin(thickness);

        if (Mathf.Abs(dot) > thickDot) {
            point = UFunc.SetVectorDirectionValue(point, thickDirection, thickDot*Mathf.Sign(dot)).normalized;
        }

        return point;
    }

    public override void LineClose(Vector4 v1, Vector4 v2, ref Vector4 outLine, ref Vector4 outThis)
    {
        Vector4 p1 = PointClose(v1);
        Vector4 p2 = PointClose(v2);

        if (Vector4.Dot(p1,v1) > Vector4.Dot(p2,v2)) {
            outLine = v1;
            outThis = p1;
        } else {
            outLine = v2;
            outThis = p2;
        }
    }
}
