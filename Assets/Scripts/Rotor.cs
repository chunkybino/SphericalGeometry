using UnityEngine;

[System.Serializable]
public struct Rotor
{
    public Vector4 factor1;
    public Vector4 factor2;

    public Bivector bivectorNormal;

    float sin;
    float cos;
    public float angle;
    
    public Rotor(Vector4 v1, Vector4 v2)
    {
        factor1 = v1.normalized;
        factor2 = Vector4.Lerp(v1,v2,0.5f).normalized;

        cos = Vector4.Dot(v1,v2);
        bivectorNormal = new Bivector(v1,v2);

        sin = bivectorNormal.magnitude;
        angle = Mathf.Atan2(sin,cos);

        if (angle != 0) bivectorNormal.SetOrtho();
    }

    public Rotor(Vector4 v1, Vector4 v2, float theta)
    {
        bool zero1 = v1 == Vector4.zero;
        bool zero2 = v2 == Vector4.zero;
        if (zero1 && zero2)
        {
            v1 = new Vector4(0,0,0,1);
            v2 = new Vector4(0,0,0,1);
            theta = 0;
        }
        else if (zero1)
        {
            v2 = v1;
            theta = 0;
        }
        else if (zero2)
        {
            v1 = v2;
            theta = 0;
        }

        theta = UFunc.RepeatRange(theta,-Mathf.PI,Mathf.PI);

        factor1 = v1.normalized;
        factor2 = UFunc.Slerp4Angle(v1,v2,theta/2).normalized;

        //Debug.Log(theta+"_"+v1+""+v2+"_"+factor1+""+factor2);

        cos = Mathf.Cos(theta);
        sin = Mathf.Sin(theta);
        angle = theta;

        if (theta != 0) {
            bivectorNormal = new Bivector(v1,v2);
        } else {
            bivectorNormal = new Bivector(v1,new Vector4(1,2,3,4));
            //Debug.Log("Heeeere "+bivectorNormal.factor1+" "+bivectorNormal.factor2);
        }
        bivectorNormal.SetOrtho();

        if (theta == 0) {
            //Debug.Log("Heeeere2222222222222 "+bivectorNormal.factor1+" "+bivectorNormal.factor2);
        }
    }
    public Rotor(Bivector bi, float theta)
    {
        if (bi.magnitude == 0)
        {
            bi = new Bivector(new Vector4(0,0,0,1), new Vector4(1,0,0,0));
            theta = 0;
        }

        theta = UFunc.RepeatRange(theta,-Mathf.PI,Mathf.PI);

        angle = theta;

        bivectorNormal = bi;
        bivectorNormal.SetOrtho();

        factor1 = bivectorNormal.factor1;
        factor2 = bivectorNormal.FactorSlerp(theta/2);

        cos = Mathf.Cos(angle);
        sin = Mathf.Sin(angle);

        //Debug.Log(theta);
    }

    /*
    public void SetAngle(float t)
    {
        t = UFunc.RepeatRange(t,-Mathf.PI,Mathf.PI);

        factor2 = UFunc.Slerp4Angle(factor1,factor2,t/2);
        angle = t;
        cos = Mathf.Cos(angle);
        sin = Mathf.Sin(angle);
    }
    */
    public void MultAngle(float t)
    {
        factor2 = UFunc.Slerp4(factor1,factor2,t);
        angle *= t;
        cos = Mathf.Cos(angle);
        sin = Mathf.Sin(angle);
    }

    public void TranslateRotor(Rotor r)
    {
        factor1 = r * factor1;
        factor2 = r * factor2;
        bivectorNormal.factor1 = r * bivectorNormal.factor1;
        bivectorNormal.factor2 = r * bivectorNormal.factor2;
    }

    public Vector4 RotateFull(Vector4 v) //rotates by pi/2 in this rotors plane
    {
        return UFunc.BiReflectVector(v, bivectorNormal.factor1, bivectorNormal.halfVector);
    }

    public static Rotor operator *(Rotor left, Rotor right)
    {
        Bivector bi = (left.angle*left.bivectorNormal) + (right.angle*right.bivectorNormal);
        float theta = bi.magnitude;

        return new Rotor(bi, theta);
    }

    public static Rotor operator *(float mult, Rotor r)
    {
        r.MultAngle(mult);
        return r;
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
