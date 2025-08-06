using UnityEngine;

[System.Serializable]
public struct Bivector
{
    public Vector4 vector1;
    public Vector4 vector2;

    Vector4 vectorNormal1;
    Vector4 vectorNormal2;

    Vector4 vectorHalf;

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

    public Bivector(Vector4 v1, Vector4 v2)
    {
        vector1 = v1;
        vectorNormal1 = v1.normalized;
        vector2 = v2;
        vectorNormal2 = v2.normalized;

        vectorHalf = Vector4.Lerp(v1,v2,0.5f).normalized;

        m_xy = v1.x*v2.y - v1.y*v2.x;
        m_xz = v1.x*v2.z - v1.z*v2.x;
        m_xw = v1.x*v2.w - v1.w*v2.x;
        m_yz = v1.y*v2.z - v1.z*v2.y;
        m_yw = v1.y*v2.w - v1.w*v2.y;
        m_zw = v1.z*v2.w - v1.w*v2.z;
    }

    public Vector4 RotorVector(Vector4 v)
    {
        v = UFunc.ReflectVector(v, vectorNormal1);
        v = UFunc.ReflectVector(v, vectorHalf);
        return v;
    }

    public Vector4 RotateVectorAngle(Vector4 v, float angle)
    {
        v = UFunc.ReflectVector(v, vectorNormal1);
        v = UFunc.ReflectVector(v, UFunc.Slerp4Angle(vectorNormal1,vectorNormal2,angle/2));
        return v;
    }
}
