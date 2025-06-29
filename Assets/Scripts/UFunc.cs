using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public static class UFunc
{
    public static float SinDeg(float t) {
        return Mathf.Rad2Deg * Mathf.Sin(t * Mathf.Deg2Rad);
    }
    public static float CosDeg(float t) {
        return Mathf.Rad2Deg * Mathf.Cos(t * Mathf.Deg2Rad);
    }
    public static float Atan2Deg(float y, float x) {
        return Mathf.Rad2Deg * Mathf.Atan2(y,x);
    }

    public static float Magnitude(float x, float y, float z = 0, float w = 0) {
        return Mathf.Sqrt(x*x + y*y + z*z + w*w);
    }

    public static float Dot(Vector2 v1, Vector2 v2)
    {
        return v1.x*v2.x + v1.y*v2.y;
    }
    public static float Dot(Vector3 v1, Vector3 v2)
    {
        return v1.x*v2.x + v1.y*v2.y + v1.z*v2.z;
    }
    public static float Dot(Vector4 v1, Vector4 v2)
    {
        return v1.x*v2.x + v1.y*v2.y + v1.z*v2.z + v1.w*v2.w;
    }

    public static Vector3 LerpVec3(Vector3 v1, Vector3 v2, float t)
    {
        return new Vector3(Mathf.Lerp(v1.x,v2.x,t), Mathf.Lerp(v1.y,v2.y,t), Mathf.Lerp(v1.z,v2.z,t));
    }

    public static Vector3 TriNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        return Vector3.Cross(v2-v1, v3-v1).normalized;
    }

    public static Vector4 ProjectToVectorNormal(Vector4 v1, Vector4 normal)
    {
        return v1 - normal*UFunc.Dot(normal.normalized, v1);
    }

    public static float AngleBetweenVectors(Vector4 v1, Vector4 v2)
    {
        return Mathf.Acos(Mathf.Clamp(Dot(v1.normalized, v2.normalized), -1, 1));
    }

    public static Vector3 SterographicProjection(Vector4 pos, float radius)
    {
        if (pos.w == -radius) return new Vector3(9999, 0, 0);
        return radius * new Vector3(pos.x, pos.y, pos.z) / (radius + pos.w);
    }
    public static Vector4 SterographicInverse(Vector3 pos, float radius)
    {
        Vector4 newPos = (radius * 2 / ((pos.x*pos.x) + (pos.y*pos.y) + (pos.z*pos.z) + 1)) * new Vector4(pos.x,pos.y,pos.z,1);
        newPos.w -= radius;
        return newPos;
    }

    public static Matrix4x4 MatXYRot(float angle) {
        return MatPlaneRot(angle, 0,1);
    }
    public static Matrix4x4 MatYXRot(float angle) {
        return MatPlaneRot(angle, 1,0);
    }
    public static Matrix4x4 MatXZRot(float angle) {
        return MatPlaneRot(angle, 0,2);
    }
    public static Matrix4x4 MatZXRot(float angle) {
        return MatPlaneRot(angle, 0,2);
    }
    public static Matrix4x4 MatXWRot(float angle) {
        return MatPlaneRot(angle, 0,3);
    }
    public static Matrix4x4 MatYZRot(float angle) {
        return MatPlaneRot(angle, 1,2);
    }
    public static Matrix4x4 MatZYRot(float angle) {
        return MatPlaneRot(angle, 2,1);
    }
    public static Matrix4x4 MatYWRot(float angle) {
        return MatPlaneRot(angle, 1,3);
    }
    public static Matrix4x4 MatZWRot(float angle) {
        return MatPlaneRot(angle, 2,3);
    }

    public static Matrix4x4 MatPlaneRot(float angle, int axis1, int axis2)
    {
        float sin = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        Matrix4x4 mat = Matrix4x4.identity;
        mat[axis1,axis1] = cos;
        mat[axis2,axis1] = -sin;
        mat[axis1,axis2] = sin;
        mat[axis2,axis2] = cos;

        return mat;
    }

    public static Matrix4x4 MatXYZWRot(float angle1, float angle2) {
        float sin1 = Mathf.Sin(angle1);
        float cos1 = Mathf.Cos(angle1);
        float sin2 = Mathf.Sin(angle2);
        float cos2 = Mathf.Cos(angle2);

        Matrix4x4 mat = Matrix4x4.identity;
        mat.SetRow(0, new Vector4(cos1,-sin1, 0,  0));
        mat.SetRow(1, new Vector4(sin1, cos1, 0,  0));
        mat.SetRow(2, new Vector4(0,    0,   cos2,-sin2));
        mat.SetRow(3, new Vector4(0,    0,   sin2,cos2));

        return mat;
    }

    //return matrix that transforms a point to the X axis
    public static Matrix4x4 PosToX(Vector4 pos)
    {
        Vector3 angles = -PosToAngles(pos);

        float sinX = Mathf.Sin(angles.y);
        float cosX = Mathf.Cos(angles.y);
        float sinY = Mathf.Sin(angles.z);
        float cosY = Mathf.Cos(angles.z);
        float sinZ = Mathf.Sin(angles.x);
        float cosZ = Mathf.Cos(angles.x);

        Matrix4x4 mat = Matrix4x4.identity;
        mat.SetRow(0, new Vector4(cosX*cosZ, -sinX*cosZ, -cosY*sinZ, sinY*sinZ));
        mat.SetRow(1, new Vector4(sinX,      cosX,       0,          0));
        mat.SetRow(2, new Vector4(cosX*sinZ, -sinX*sinZ, cosY*cosZ,  -sinY*cosZ));
        mat.SetRow(3, new Vector4(0,         0,          sinY,       cosY));

        return mat;
    }

    //returns euler angles to point, XZ XY ZW 
    public static Vector3 PosToAngles(Vector4 pos)
    {
        return new Vector3(
            Mathf.Atan2(Magnitude(pos.z,pos.w),Magnitude(pos.x,pos.y)),
            Mathf.Atan2(pos.y,pos.x),
            Mathf.Atan2(pos.w,pos.z)
        );
    }

    //returns matrix that rotates point to XY plane without affecting its X value
    public static Matrix4x4 PosToXY(Vector4 pos)
    {
        Vector2 angles = new Vector2(-Mathf.Atan2(pos.w, pos.z), -Mathf.Atan2(Magnitude(pos.z,pos.w), pos.y));

        float sinX = Mathf.Sin(angles.x);
        float cosX = Mathf.Cos(angles.x);
        float sinY = Mathf.Sin(angles.y);
        float cosY = Mathf.Cos(angles.y);

        Matrix4x4 mat = Matrix4x4.identity;
        mat.SetRow(1, new Vector4(0, cosY, -cosX*sinY, sinX*sinY));
        mat.SetRow(2, new Vector4(0, sinY, cosX*cosY,  -sinX*cosY));
        mat.SetRow(3, new Vector4(0, 0,    sinX,       cosX));

        return mat;
    }

    //returs matrix that roates 2 points to both be on XY plane with p1 on x axis
    public static Matrix4x4 PlaneToXY(Vector4 p1, Vector4 p2)
    {
        Matrix4x4 posToX = PosToX(p1); //brings pos to X axis
        p2 = posToX * p2;
        Matrix4x4 targetToXY = PosToXY(p2); //brings the transformed target point to the xy plane while keeping pos on the X axis

        return targetToXY * posToX;
    }

    public static Vector4 RotateTowards(Vector4 pos, Vector4 target, float angle)
    {
        Matrix4x4 planeToXY = PlaneToXY(pos, target);
        Vector4 newPos = new Vector4(Mathf.Cos(angle), -Mathf.Sin(angle), 0, 0);

        return planeToXY.transpose * newPos;
    }

    public static Matrix4x4 RotateTowardsMatrix(Vector4 pos, Vector4 target)
    {
        return RotateTowardsMatrix(pos, target, -AngleBetweenVectors(pos, target));
    }
    public static Matrix4x4 RotateTowardsMatrix(Vector4 pos, Vector4 target, float angle)
    {
        Matrix4x4 planeToXY = PlaneToXY(pos, target);

        return planeToXY.transpose * MatXYRot(angle) * planeToXY;
    }

    public static Vector4 LineXIntersect(Vector4 v1, Vector4 v2, float intersectVal)
    {
        Vector4 d = v2-v1;

        float theFactor = (intersectVal-v1.x)/d.x;

        return new Vector4(intersectVal, d.y*theFactor + v1.y, d.z*theFactor + v1.z, d.w*theFactor + v1.w);
    }
    public static Vector4 LineYIntersect(Vector4 v1, Vector4 v2, float intersectVal)
    {
        Vector4 d = v2-v1;

        float theFactor = (intersectVal-v1.y)/d.y;

        return new Vector4(d.x*theFactor + v1.x, intersectVal, d.z*theFactor + v1.z, d.w*theFactor + v1.w);
    }
    public static Vector4 LineZIntersect(Vector4 v1, Vector4 v2, float intersectVal)
    {
        Vector4 d = v2-v1;

        float theFactor = (intersectVal-v1.z)/d.z;

        return new Vector4(d.x*theFactor + v1.x, d.y*theFactor + v1.y, intersectVal, d.w*theFactor + v1.w);
    }

    public static float RoundDigit(float num, int digit)
    {
        float degree = Mathf.Pow(10, digit);
        return Mathf.Round(num*degree)/degree;
    }

    public static bool SameSign(float n1, float n2)
    {
        return Mathf.Sign(n1) == Mathf.Sign(n2);
    }

    public static bool LessThanAll(float n, params float[] list)
    {
        for (int i = 0; i < list.Length; i++) {
            if (n > list[i]) return false;
        }
        return true;
    }
    public static bool GreaterThanAll(float n, params float[] list)
    {
        for (int i = 0; i < list.Length; i++) {
            if (n < list[i]) return false;
        }
        return true;
    }
    public static bool FurtherThanAll(float n, params float[] list) //further from zero
    {
        n = Mathf.Abs(n);
        for (int i = 0; i < list.Length; i++) {
            if (n < Mathf.Abs(list[i])) return false;
        }
        return true;
    }

    public static float FurthestOfList(params float[] list) //returns the element thats furtherst from zero
    {
        float furthest = list[0];
        for (int i = 1; i < list.Length; i++) {
            if (Mathf.Abs(list[i]) > Mathf.Abs(furthest)) furthest = list[i];
        }
        return furthest;
    }
    public static int FurthestOfListIndex(params float[] list) //returns the element thats furtherst from zero
    {
        float furthest = Mathf.Abs(list[0]);
        int index = 0;
        for (int i = 1; i < list.Length; i++) {
            if (Mathf.Abs(list[i]) > Mathf.Abs(furthest)) {
                furthest = Mathf.Abs(list[i]);
                index = i;
            }
        }
        return index;
    }
    public static int ClosestOfListIndex(params float[] list) //returns the element thats closest to zero
    {
        float furthest = Mathf.Abs(list[0]);
        int index = 0;
        for (int i = 1; i < list.Length; i++) {
            if (Mathf.Abs(list[i]) < Mathf.Abs(furthest)) {
                furthest = Mathf.Abs(list[i]);
                index = i;
            }
        }
        return index;
    }

    public static int MinIndex(params float[] list)
    {
        int index = 0;
        for (int i = 1; i < list.Length; i++) {
            if (list[i] < list[index]) {
                index = i;
            }
        }
        return index;
    }
    public static int MaxIndex(params float[] list)
    {
        int index = 0;
        for (int i = 1; i < list.Length; i++) {
            if (list[i] > list[index]) {
                index = i;
            }
        }
        return index;
    }

    public static Vector3 VectorBasisShift(Vector3 vec, Vector3 xBase, Vector3 yBase, Vector3 zBase)
    {
        return new Vector3(
            vec.x*xBase.x + vec.y*yBase.x + vec.z*zBase.x,
            vec.x*xBase.y + vec.y*yBase.y + vec.z*zBase.y,
            vec.x*xBase.z + vec.y*yBase.z + vec.z*zBase.z
        );
    }
}
