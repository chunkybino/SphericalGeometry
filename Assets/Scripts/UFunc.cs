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

    public static Vector4 HyperCross(Vector4 a, Vector4 b, Vector4 c)
    {
        float xy = BiProd(0,1);
        float xz = BiProd(0,2);
        float xw = BiProd(0,3);
        float yz = BiProd(1,2);
        float yw = BiProd(1,3);
        float zw = BiProd(2,3);

        float BiProd(int axis1, int axis2) {
            return a[axis1]*b[axis2]-a[axis2]*b[axis1];
        }

        return new Vector4(
            -c.w*yz + c.z*yw - c.y*zw, //zyw
            c.w*xz - c.z*xw + c.x*zw, //xzw
            -c.w*xy - c.x*yw + c.y*xw, //xwy
            c.z*xy + c.x*yz - c.y*xz //xyz
        );
    }

    //returns index of vector component with largest magnitude
    public static int VectorSignificant(Vector4 v)
    {
        int max = 0;
        if (Mathf.Abs(v[max]) < Mathf.Abs(v[1])) max = 1;
        if (Mathf.Abs(v[max]) < Mathf.Abs(v[2])) max = 2;
        if (Mathf.Abs(v[max]) < Mathf.Abs(v[3])) max = 3;
        return max;
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

    public static Vector4 DirectionFromTo(Vector4 from, Vector4 to)
    {
        return ProjectToVectorNormal(to-from,from).normalized;
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

    public static Matrix4x4 RotateTowardsMatrix(Vector4 pos, Vector4 target)
    {
        Vector4 reflectVector = ((pos+target)/2).normalized;

        Matrix4x4 mat = new Matrix4x4();
        mat.SetColumn(0, BiReflectVector(new Vector4(1,0,0,0), pos, reflectVector));
        mat.SetColumn(1, BiReflectVector(new Vector4(0,1,0,0), pos, reflectVector));
        mat.SetColumn(2, BiReflectVector(new Vector4(0,0,1,0), pos, reflectVector));
        mat.SetColumn(3, BiReflectVector(new Vector4(0,0,0,1), pos, reflectVector));

        return mat;
    }

    public static Matrix4x4 MatrixBiReflect(Vector4 v1, Vector4 v2)
    {
        v1 = v1.normalized;
        v2 = v2.normalized;

        Matrix4x4 mat = new Matrix4x4();
        mat.SetColumn(0, BiReflectVector(new Vector4(1,0,0,0), v1, v2));
        mat.SetColumn(1, BiReflectVector(new Vector4(0,1,0,0), v1, v2));
        mat.SetColumn(2, BiReflectVector(new Vector4(0,0,1,0), v1, v2));
        mat.SetColumn(3, BiReflectVector(new Vector4(0,0,0,1), v1, v2));

        return mat;
    }

    //public static Matrix4x4 RotateMatrixBivector(Matrix4x4)

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
    public static bool SameSign(Vector3 n1, Vector3 n2)
    {
        return SameSign(n1.x,n2.x) && SameSign(n1.y,n2.y) && SameSign(n1.z,n2.z);
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

    public static bool GreaterDirection(float n1, float n2, float dir) //is number greater in specified direction
    {
        if (dir >= 0) {
            return n1 > n2;
        } 
        return n1 < n2;
    }

    public static float Clamp01(float n) {
        return Mathf.Clamp(n,0,1);
    }
    public static float Clamp1(float n) {
        return Mathf.Clamp(n,-1,1);
    }

    public static bool SameQuadrant(Vector2 v1, Vector2 v2)
    {
        return SameSign(v1.x,v2.x) && SameSign(v1.y,v2.y);
    }

    public static Vector3 VectorBasisShift(Vector3 vec, Vector3 xBase, Vector3 yBase, Vector3 zBase)
    {
        return new Vector3(
            vec.x*xBase.x + vec.y*yBase.x + vec.z*zBase.x,
            vec.x*xBase.y + vec.y*yBase.y + vec.z*zBase.y,
            vec.x*xBase.z + vec.y*yBase.z + vec.z*zBase.z
        );
    }

    public static float VectorAngle(Vector4 v1, Vector4 v2)
    {
        return Mathf.Acos(Clamp1(Dot(v1.normalized,v2.normalized)));
    }

    public static float DistanceS(Vector4 v1, Vector4 v2)
    {
        return Mathf.Acos(Clamp1(Dot(v1,v2)));
    }

    public static bool BetweenS(Vector4 v1, Vector4 v2, Vector4 v3) //is v3 (along the line of v1-v2), between the 2 vectors in spherical space
    {
        float dot1 = Dot(v1,v2);
        float dot2 = Dot(v1,v3);
        float dot3 = Dot(v2,v3);

        return dot2 > dot1 && dot3 > dot1;
    }

    public static Vector4 ProjectLocal3QuickS(Vector3 v) //takes a 3d vector that represents some local position, and returns its projection onto 4d sphere from w=1
    {
        return new Vector4(v.x,v.y,v.z,1).normalized;
    }

    public static Vector4 Slerp4(Vector4 v1, Vector4 v2, float t)
    {
        float arc = Mathf.Acos(Clamp1(Dot(v1,v2)));

        if (arc == 0) return v1;

        Vector4 outV = new Vector4();

        for (int i = 0; i < 4; i++)
        {
            outV[i] = (v1[i]*Mathf.Sin(arc*(1-t)) + v2[i]*Mathf.Sin(arc*t)) / Mathf.Sin(arc);
        }

        return outV;
    }
    public static Vector4 Slerp4Angle(Vector4 v1, Vector4 v2, float angle) //slerp in direction by and angle, not a t val
    {
        float arc = Mathf.Acos(Clamp1(Dot(v1,v2)));

        if (arc == 0) return v1;

        Vector4 outV = new Vector4();

        for (int i = 0; i < 4; i++)
        {
            outV[i] = (v1[i]*Mathf.Sin(arc-angle) + v2[i]*Mathf.Sin(angle)) / Mathf.Sin(arc);
        }

        return outV;
    }

    //slerp between v1 and v2 till we find the point closest to the target point
    public static Vector4 SlerpPointClose(Vector4 v1, Vector4 v2, Vector4 target)
    {
        target = SlerpPointCloseUnclamped(v1,v2,target);
        return ClampBetweenVectors(target,v1,v2); 
    }
    public static Vector4 SlerpPointCloseUnclamped(Vector4 v1, Vector4 v2, Vector4 target)
    {
        return ProjectVectorToPlane(v1,v2,target);
    }
    public static Vector4 ClampBetweenVectors(Vector4 v, Vector4 clamp1, Vector4 clamp2)
    {
        float dotClamp = Vector4.Dot(clamp1,clamp2);

        float dot1 = Vector4.Dot(v,clamp1);
        float dot2 = Vector4.Dot(v,clamp2);

        if (dot1 > dotClamp && dot2 > dotClamp) return v;

        if (dot1 > dot2) {
            v = clamp1;
        }
        else
        {
            v = clamp2;
        }

        return v;
    }

    public static void DoubleArcClose(Vector4 v1, Vector4 v2, Vector4 u1, Vector4 u2, ref Vector4 close1, ref Vector4 close2)
    {
        Vector4 outV = new Vector4();
        Vector4 outU = new Vector4();

        UFunc.DoubleArcCloseUnclamped(v1,v2,u1,u2, ref outV, ref outU);

        close1 = outV;
        close2 = outU;

        Vector4 close2_set1 = UFunc.ClampBetweenVectors(close2, u1,u2);
        Vector4 close1_set1 = UFunc.SlerpPointClose(v1,v2,close2_set1);

        Vector4 close1_set2 = UFunc.ClampBetweenVectors(close1, v1,v2);
        Vector4 close2_set2 = UFunc.SlerpPointClose(u1,u2,close1_set2);

        if (Vector4.Dot(close1_set1,close2_set1) > Vector4.Dot(close1_set2,close2_set2))
        {
            close1 = close1_set1;
            close2 = close2_set1;
        }
        else
        {
            close1 = close1_set2;
            close2 = close2_set2;
        }
    }
    public static void DoubleArcCloseUnclamped(Vector4 v1, Vector4 v2, Vector4 u1, Vector4 u2, ref Vector4 close1, ref Vector4 close2)
    {
        //findes closest point on v arc to u arc (final point will be on v)

        Vector4 sphereNorm = HyperCross(v1,v2,u1).normalized;

        Vector4 u3 = (u2 - Vector4.Dot(u2,sphereNorm)*sphereNorm).normalized; //u2 projected onto sphere of v1,v2,u1

        Vector4 sphere1 = HyperCross(v1,v2,sphereNorm).normalized;
        Vector4 sphere2 = HyperCross(u1,u3,sphereNorm).normalized;

        close1 = HyperCross(sphere1,sphere2,sphereNorm).normalized;
        close2 = SlerpPointCloseUnclamped(u1,u2,close1);

        if (Vector4.Dot(close1,close2) < 0) {
            close2 *= -1;
        }
    }

    public static Vector4 ArcPlaneIntersect(Vector4 planeNorm, Vector4 arc1, Vector4 arc2)
    {
        Vector4 sphereNorm1 = HyperCross(planeNorm, arc1, arc2).normalized;
        Vector4 sphereNorm2 = HyperCross(sphereNorm1, arc1, arc2).normalized;

        return HyperCross(planeNorm,sphereNorm1,sphereNorm2);
    }

    public static Vector4 ProjectVectorToPlane(Vector4 plane1, Vector4 plane2, Vector4 v)
    {
        Vector4 hyperCross = HyperCross(plane1,plane2,v);
        Vector4 planeNorm = -HyperCross(plane1,plane2,hyperCross).normalized;

        return (v - Vector4.Dot(v,planeNorm)*planeNorm).normalized;
    }

    public static Vector4 SetVectorDirectionValue(Vector4 vec, Vector4 direction, float value)
    {
        if (direction == Vector4.zero) return vec;
        return vec - Vector4.Project(vec,direction) + value*direction;
    }

    public static Vector4 ReflectVector(Vector4 v, Vector4 reflect) //recflects the vector over the normal of th eother vector
    {
        if (reflect.sqrMagnitude == 0) return v;
        return v - 2*reflect*UFunc.Dot(reflect, v)/reflect.sqrMagnitude;
    }
    public static Vector4 BiReflectVector(Vector4 v, Vector4 reflect1, Vector4 reflect2)
    {
        return ReflectVector(ReflectVector(v, reflect1), reflect2);
    }

    public static float SqrSum(params float[] par)
    {
        float sum = 0;
        for (int i = 0; i < par.Length; i++)
        {
            sum += par[i]*par[i];
        }
        return sum;
    }

    public static float RepeatRange(float n, float lo, float hi)
    {
        if (hi < lo) {
            (lo,hi) = (hi,lo);
        }
        return Mathf.Repeat(n-lo,hi-lo)+lo;
    }

    public static bool CloseTo(float n, float target, float range)
    {
        return n <= target+range && n >= target-range;
    }

    public static void PrintList(params string[] par)
    {
        string s = "";
        for (int i = 0; i < par.Length; i++) {
            s = s + par[i] + " ";
        }
        Debug.Log(s);
    }
    public static void PrintList(params float[] par)
    {
        string s = "";
        for (int i = 0; i < par.Length; i++) {
            s = s + par[i].ToString() + " ";
        }
        Debug.Log(s);
    }

    public static T[] List2Array<T>(List<T> list)
    {
        T[] ar = new T[list.Count];
        for (int i = 0; i < ar.Length; i++) {
            ar[i] = list[i];
        }
        return ar;
    }

    public static List<string> ParseBySpace(string input)
    {
        return ParseByCharacter(input, " ");
    }
    public static List<string> ParseByComma(string input)
    {
        return ParseByCharacter(input, ",");
    }

    //keep quote - if true, any text between 2 quotation marks is considered one word
    public static List<string> ParseByCharacter(string input, string parseChar = " ", bool keepQuote = false)
    {
        List<string> outList = new List<string>();

        bool isSpace = true;
        bool quote = false;
        for (int i = 0; i < input.Length; i++)
        {
            string c = input[i].ToString();

            if (c == "\"" && keepQuote) {
                quote = !quote;
                continue;
            }

            if (quote) {
                if (isSpace) {
                    outList.Add(c);
                } else {
                    outList[^1] += c;
                }
                isSpace = false;
                continue;
            }

            if (c != parseChar)
            {
                if (isSpace) {
                    outList.Add(c);
                } else {
                    outList[^1] += c;
                }
            }

            isSpace = c == parseChar;
        }

        return outList;
    }
}
