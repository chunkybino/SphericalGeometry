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

    [SerializeField] int divisions = 16;
    [SerializeField] float thickAngle = Mathf.PI/16;

    [SerializeField] int axisMode = 0;

    void Update()
    {
        if (makeCircleStrip)
        {
            makeCircleStrip = false;
            MakeSphere();
        }
    }

    void MakeSphere()
    {
        Vector4[] circlePoints = new Vector4[divisions * 3];
        Vector3Int[] circleTri = new Vector3Int[divisions * 8];
        Vector2[] circleUV = new Vector2[circlePoints.Length];

        Vector4 thickPos = GetCircleThickAxis();
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

    Vector4 GetCircleAxis(float angle)
    {
        switch (axisMode)
        {
            case 0:
                return new Vector4(Mathf.Cos(angle), Mathf.Sin(angle), 0, 0);
            case 1:
                return new Vector4(0, 0, Mathf.Cos(angle), Mathf.Sin(angle));
            case 2:
                return new Vector4(0, Mathf.Cos(angle), 0, Mathf.Sin(angle));
            case 3:
                return new Vector4(Mathf.Cos(angle), 0, Mathf.Sin(angle), 0);
            default:
                return new Vector4(Mathf.Cos(angle), Mathf.Sin(angle), 0, 0);
        }
    }
    Vector4 GetCircleThickAxis()
    {
        switch (axisMode)
        {
            case 0:
                return new Vector4(0, 0, Mathf.Sin(thickAngle), 0);
            case 1:
                return new Vector4(0, Mathf.Sin(thickAngle), 0, 0);
            case 2:
                return new Vector4(Mathf.Sin(thickAngle), 0, 0, 0);
            case 3:
                return new Vector4(0, Mathf.Sin(thickAngle), 0, 0);
            default:
                return new Vector4(0, 0, Mathf.Sin(thickAngle), 0);
        }
    }
}
