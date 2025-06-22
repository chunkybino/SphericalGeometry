using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class TopologyMaker : MonoBehaviour
{
    public Vector4[] inVertex;

    public Vector4[] outVertex;
    public Vector2[] outUV;
    public Vector3Int[] outTri;

    [SerializeField] bool makeCircleStrip;
    [SerializeField] bool makeSphere;

    [SerializeField] int divisions = 16;
    [SerializeField] float thickAngle = Mathf.PI/16;

    [SerializeField] int axis1 = 0;
    [SerializeField] int axis2 = 1;
    [SerializeField] int axisThick = 3;

    void Update()
    {
        if (makeCircleStrip)
        {
            makeCircleStrip = false;
            CircleStrip();
        }
        if (makeSphere)
        {
            makeSphere = false;
            SphereTime();
        }
    }

    void CircleStrip()
    {
        Vector4[] circlePoints = new Vector4[divisions * 3];
        Vector3Int[] circleTri = new Vector3Int[divisions * 8];
        Vector2[] circleUV = new Vector2[circlePoints.Length];

        Vector4 thickPos = GetCircleThickAxis() *Mathf.Sin(thickAngle);
        float thickMult = Mathf.Cos(thickAngle);

        for (int i = 0; i < divisions; i++)
        {
            float angle = i * 2*Mathf.PI / divisions;
            Vector4 circlePos = GetCircleAxis(angle);
            circlePoints[3*i + 0] = circlePos;
            circlePoints[3*i + 1] = thickMult*circlePos + thickPos;
            circlePoints[3*i + 2] = thickMult*circlePos - thickPos;

            circleUV[3*i + 0] = GetUV(0);
            circleUV[3*i + 1] = GetUV(1);
            circleUV[3*i + 2] = GetUV(2);

            Vector2 GetUV(int localIndex)
            {
                Vector2 outV = new Vector2();

                switch (localIndex)
                {
                    case 0:
                        outV.y = 0.5f;
                        break;
                    case 1:
                        outV.y = 1;
                        break;
                    case 2:
                        outV.y = 0;
                        break;
                }

                if (i % 2 == 1) 
                {
                    outV.x = 0.5f;
                    outV.y *= -1;
                }

                return outV;
            }

            /*
                0,1,n1
                0,n1,n0
                2,0,n0
                2,n0,n2
            */

            circleTri[8*i + 0] = GetVIndex(0,1,4);
            circleTri[8*i + 1] = GetVIndex(0,4,3);
            circleTri[8*i + 2] = GetVIndex(2,0,3);
            circleTri[8*i + 3] = GetVIndex(2,3,5);

            //flip it around
            circleTri[8*i + 4] = GetVIndex(4,1,0);
            circleTri[8*i + 5] = GetVIndex(3,4,0);
            circleTri[8*i + 6] = GetVIndex(3,0,2);
            circleTri[8*i + 7] = GetVIndex(5,3,2);

            Vector3Int GetVIndex(int x, int y, int z)
            {
                return new Vector3Int(GetIndex(x),GetIndex(y),GetIndex(z));
            }
            int GetIndex(int localIndex) {
                int local = localIndex % 3;
                int offset = localIndex / 3;
                int anchor = i+offset;
                if (i+offset >= divisions) anchor -= divisions;
                return 3*anchor + local;
            }
        }

        outVertex = circlePoints;
        outUV = circleUV;
        outTri = circleTri;
    }

    void SphereTime()
    {
        int sideDivisions = (divisions-2)/2;

        float[] sideDivisonSin = new float[sideDivisions];
        float[] sideDivisonCos = new float[sideDivisions];
        float anglePerDivision = Mathf.PI / (sideDivisions+1);

        for (int i = 0; i < sideDivisions; i++)
        {
            float angle = (i+1)*anglePerDivision - Mathf.PI/2;
            sideDivisonSin[i] = Mathf.Sin(angle);
            sideDivisonCos[i] = Mathf.Cos(angle);
        }

        Vector4[] circlePoints = new Vector4[divisions * sideDivisions + 2];
        Vector3Int[] circleTri = new Vector3Int[2*(divisions * 2*(sideDivisions-1) + 4*divisions)];
        Vector2[] circleUV = new Vector2[circlePoints.Length];

        Vector4 thickAxis = GetCircleThickAxis();
        float thickMult = Mathf.Cos(thickAngle);

        for (int i = 0; i < divisions; i++)
        {
            float angle = i * 2*Mathf.PI / divisions;
            Vector4 circlePos = GetCircleAxis(angle);

            for (int j = 0; j < sideDivisions; j++)
            {
                circlePoints[sideDivisions*i + j] = sideDivisonCos[j]*circlePos + sideDivisonSin[j]*thickAxis;
                circleUV[sideDivisions*i + j] = GetUV(j);

                if (j < sideDivisions-1)
                {
                    int index = (sideDivisions+1)*2*i + 2*j;
                    circleTri[index] = new Vector3Int(GetIndex(j),GetIndex(j+1),GetIndexNext(j+1));
                    circleTri[index+1] = new Vector3Int(GetIndex(j),GetIndexNext(j+1),GetIndexNext(j));
                }
            }

            circleTri[(sideDivisions+1)*2*i + 2*(sideDivisions-1)] = new Vector3Int(circlePoints.Length-2, GetIndex(0), GetIndexNext(0));
            circleTri[(sideDivisions+1)*2*i + 2*(sideDivisions-1) + 1] = new Vector3Int(circlePoints.Length-1, GetIndex(sideDivisions-1), GetIndexNext(sideDivisions-1));

            Vector2 GetUV(int localIndex)
            {
                Vector2 outV = new Vector2();

                outV.y = 0.5f*(localIndex%3);

                if (i % 2 == 1) 
                {
                    outV.x = 0.5f;
                    outV.y *= -1;
                }

                return outV;
            }

            int GetIndex(int local)
            {
                return sideDivisions*i + local;
            }
            int GetIndexNext(int local) //get tri index for the next "i" iteration
            {
                int anchor = i+1;
                if (anchor >= divisions) anchor -= divisions;

                return sideDivisions*anchor + local;
            }
        }

        circlePoints[^2] = -thickAxis;
        circlePoints[^1] = thickAxis;

        circleUV[^2] = new Vector2(0.5f,0.5f);
        circleUV[^1] = new Vector2(0.5f,0.5f);

        //doubel tri
        for (int i = 0; i < circleTri.Length/2; i++) {
            circleTri[i+(circleTri.Length/2)] = new Vector3Int(circleTri[i].z,circleTri[i].y,circleTri[i].x);
        }

        outVertex = circlePoints;
        outUV = circleUV;
        outTri = circleTri;
    }

    Vector4 GetCircleAxis(float angle)
    {
        Vector4 v = new Vector4();
        v[axis1] = Mathf.Cos(angle);
        v[axis2] = Mathf.Sin(angle);
        return v;
    }
    Vector4 GetCircleThickAxis()
    {
        Vector4 v = new Vector4();
        v[axisThick] = 1;
        return v;
    }
}
