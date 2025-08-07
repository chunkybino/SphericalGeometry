using UnityEngine;

[System.Serializable]
public struct Rotor
{
    public Vector4 factor1;
    public Vector4 factor2;

    public Bivector bivectorNormal;

    float sin;
    float cos;// {get{return dot;}}
    public float angle;
    
    public Rotor(Vector4 v1, Vector4 v2)
    {
        factor1 = v1.normalized;
        factor2 = Vector4.Lerp(v1,v2,0.5f).normalized;

        cos = Vector4.Dot(v1,v2);
        //wedge = new Bivector(v1,v2);
        bivectorNormal = new Bivector(v1,v2);

        sin = bivectorNormal.magnitude;
        angle = Mathf.Atan2(sin,cos);

        bivectorNormal.SetOrtho();
    }

    public Rotor(Bivector bi, float theta)
    {
        angle = theta;
        bivectorNormal = bi;
        bivectorNormal.SetOrtho();

        factor1 = bivectorNormal.factor1;
        factor2 = bivectorNormal.FactorSlerp(theta);

        //wedge = bivectorNormal*angle;
        cos = Mathf.Cos(angle);
        sin = Mathf.Sin(angle);
    }

    public void SetAngle(float t)
    {
        factor2 = UFunc.Slerp4Angle(factor1,factor2,t);
        angle = t;
        cos = Mathf.Cos(angle);
        sin = Mathf.Sin(angle);
    }

    public static Rotor operator *(Rotor left, Rotor right)
    {
        Bivector bi = (left.angle*left.bivectorNormal) + (right.angle*right.bivectorNormal);
        float theta = bi.magnitude;

        return new Rotor(bi, theta);
    } 

    public static Vector4 operator *(Rotor r, Vector4 v)
    {
        return UFunc.BiReflectVector(v, r.factor1, r.factor2);
    }
    public static Matrix4x4 operator *(Rotor r, Matrix4x4 mat)
    {
        for (int i = 0; i < 4; i++) {
            mat.SetColumn(i, UFunc.BiReflectVector(mat.GetColumn(i), r.factor1, r.factor2)); 
        }
        return mat;
    }
}
