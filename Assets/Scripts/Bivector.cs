using UnityEngine;

[System.Serializable]
public struct Bivector
{
    public Vector4 factor1;
    public Vector4 factor2;

    float m_xy;
    float m_xz;
    float m_xw;
    float m_yz;
    float m_yw;
    float m_zw;

    public float xy {get{return m_xy;}}
    public float xz {get{return m_xz;}}
    public float xw {get{return m_xw;}}
    public float yz {get{return m_yz;}}
    public float yw {get{return m_yw;}}
    public float zw {get{return m_zw;}}

    public float this[int index]
    {
        get{
            switch (index)
            {
                default:
                    return m_xy;
                case 1:
                    return m_xz;
                case 2:
                    return m_xw;
                case 3:
                    return m_yz;
                case 4:
                    return m_yw;
                case 5:
                    return m_zw;
            }
        }
        set{
            switch (index)
            {
                default:
                    m_xy = value;
                    break;
                case 1:
                    m_xz = value;
                    break;
                case 2:
                    m_xw = value;
                    break;
                case 3:
                    m_yz = value;
                    break;
                case 4:
                    m_yw = value;
                    break;
                case 5:
                    m_zw = value;
                    break;
                    
            }
        }
    }

    public float sqrMagnitude;
    public float magnitude;

    public Bivector normalized {get{
        return this / magnitude;
    }}

    public Bivector(Vector4 v1, Vector4 v2)
    {
        factor1 = v1;
        factor2 = v2;

        m_xy = v1.x*v2.y - v1.y*v2.x;
        m_xz = v1.x*v2.z - v1.z*v2.x;
        m_xw = v1.x*v2.w - v1.w*v2.x;
        m_yz = v1.y*v2.z - v1.z*v2.y;
        m_yw = v1.y*v2.w - v1.w*v2.y;
        m_zw = v1.z*v2.w - v1.w*v2.z;

        sqrMagnitude = UFunc.SqrSum(m_xy,m_xz,m_xw,m_yz,m_yw,m_zw);
        magnitude = Mathf.Sqrt(sqrMagnitude);
    }

    void CalcMag()
    {
        sqrMagnitude = UFunc.SqrSum(m_xy,m_xz,m_xw,m_yz,m_yw,m_zw);
        magnitude = Mathf.Sqrt(sqrMagnitude);
    }
    public void Normalize()
    {
        sqrMagnitude = UFunc.SqrSum(m_xy,m_xz,m_xw,m_yz,m_yw,m_zw);
        magnitude = Mathf.Sqrt(sqrMagnitude);

        for (int i = 0; i < 6; i++) {
            this[i] = this[i] / magnitude;
        }
        magnitude = 1;
        sqrMagnitude = 1;
    }
    public void SetOrtho()
    {
        Normalize();

        //start with a random vector, and project it onto bivector
        factor1 = Dot(new Vector4(1,2,3,4), this).normalized;
        factor2 = Dot(factor1, this);
    }

    public Vector4 FactorSlerp(float angle)
    {
        return UFunc.Slerp4Angle(factor1,factor2,angle);
    }

    public static Bivector operator *(float operand, Bivector bi)
    {
        Bivector bi2 = new Bivector();
        for (int i = 0; i < 6; i++) {
            bi2[i] = bi[i] * operand;
        }
        bi2.magnitude = bi.magnitude * operand;
        bi2.sqrMagnitude = bi.magnitude * (operand*operand);
        return bi2;
    }
    public static Bivector operator *(Bivector bi, float operand)
    {
        Bivector bi2 = new Bivector();
        for (int i = 0; i < 6; i++) {
            bi2[i] = bi[i] * operand;
        }
        bi2.magnitude = bi.magnitude * operand;
        bi2.sqrMagnitude = bi.magnitude * (operand*operand);
        return bi2;
    }
    public static Bivector operator /(Bivector bi, float operand)
    {
        Bivector bi2 = new Bivector();
        for (int i = 0; i < 6; i++) {
            bi2[i] = bi[i] / operand;
        }
        bi2.magnitude = bi.magnitude / operand;
        bi2.sqrMagnitude = bi.magnitude / (operand*operand);
        return bi2;
    }

    public static Bivector operator +(Bivector left, Bivector right)
    {
        Bivector bi = new Bivector();
        for (int i = 0; i < 6; i++) {
            bi[i] = left[i] + right[i];
        }
        bi.CalcMag();
        return bi;
    }

    public static Vector4 Dot(Vector4 v, Bivector b)
    {
        return new Vector4(
            -v.y*b.xy - v.z*b.xz - v.w*b.xw,
            v.x*b.xy - v.z*b.yz - v.w*b.yw,
            v.x*b.xz + v.y*b.yz - v.w*b.zw,
            v.x*b.xw + v.y*b.yw + v.z*b.zw
        );
    }
}
