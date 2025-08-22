#ifndef SHADERFUNC_HLSL
#define SHADERFUNC_HLSL

float4 SteroProject(float4 pos4)
{
    if (pos4.w != -1)
    {
        pos4 = pos4 / (1 + pos4.w);
        pos4.w = 1;
    }
    else
    {
        pos4 = float4(99999,0,0,1);
    }

    return pos4;
}

float4 GnomonicProject(float4 pos4)
{
    if (pos4.w != 0)
    {
        pos4 = pos4 / pos4.w;
    }
    else
    {
        pos4 = pos4 * 99999;
    }
    pos4.w = 1;

    return pos4;
}

float4 HyperCross(float4 a, float4 b, float4 c)
{
    float xy = a.x*b.y - a.y*b.x;
    float xz = a.x*b.z - a.z*b.x;
    float xw = a.x*b.w - a.w*b.x;
    float yz = a.y*b.z - a.z*b.y;
    float yw = a.y*b.w - a.w*b.y;
    float zw = a.z*b.w - a.w*b.z;

    return float4(
        -c.w*yz + c.z*yw - c.y*zw, //zyw
        c.w*xz - c.z*xw + c.x*zw, //xzw
        -c.w*xy - c.x*yw + c.y*xw, //xwy
        c.z*xy + c.x*yz - c.y*xz //xyz
    );
}

float4 Slerp4(float4 a, float4 b, float4 angle, float arc)
{
    if (arc == 0) return a;

    float4 outV;

    for (int i = 0; i < 4; i++)
    {
        outV[i] = (a[i]*sin(arc-angle) + b[i]*sin(angle)) / sin(arc);
    }

    return outV;
}

float4 SlerpHalf(float4 a, float4 b)
{
    return normalize((a+b)/2);
}

inline uint Subdivide2_Index1(uint inIndex)
{
    switch (inIndex)
    {
        case 6:
            return 0;
        case 7:
            return 3;
        case 8:
            return 5;
        case 9:
            return 3;
        case 10:
            return 1;
        case 11:
            return 4;
        case 12:
            return 5;
        case 13:
            return 4;
        case 14:
            return 2;
    }
    return 0;
}
inline uint Subdivide2_Index2(uint inIndex)
{
    switch (inIndex)
    {
        case 6:
            return 3;
        case 7:
            return 5;
        case 8:
            return 0;
        case 9:
            return 1;
        case 10:
            return 4;
        case 11:
            return 3;
        case 12:
            return 4;
        case 13:
            return 2;
        case 14:
            return 5;
    }
    return 0;
}

void SubdivTri(float4 inPos[3], float4 inWorld[3], float2 inUV[3], inout float4 outPos[6], inout float4 outWorld[6], inout float2 outUV[6])
{
    for (int i = 0; i < 3; i++) {
        outPos[i] = inPos[i];
        outWorld[i] = inWorld[i];
        outUV[i] = inUV[i];
    }

    outWorld[3] = SlerpHalf(inWorld[0],inWorld[1]);
    outWorld[4] = SlerpHalf(inWorld[1],inWorld[2]);
    outWorld[5] = SlerpHalf(inWorld[2],inWorld[0]);

    outUV[3] = lerp(inUV[0],inUV[1],0.5f);
    outUV[4] = lerp(inUV[1],inUV[2],0.5f);
    outUV[5] = lerp(inUV[2],inUV[0],0.5f);

    outPos[3] = SlerpHalf(inPos[0],inPos[1]);
    outPos[4] = SlerpHalf(inPos[1],inPos[2]);
    outPos[5] = SlerpHalf(inPos[2],inPos[0]);
}
void SubdivTri2(float4 inPos[6], float4 inWorld[6], float2 inUV[6], inout float4 outPos[15], inout float4 outWorld[15], inout float2 outUV[15])
{
    for (uint i = 0; i < 6; i++) {
        outPos[i] = inPos[i];
        outWorld[i] = inWorld[i];
        outUV[i] = inUV[i];
    }

    for (uint j = 6; j < 15; j++)
    {
        uint int_1 = Subdivide2_Index1(j);
        uint int_2 = Subdivide2_Index2(j);
        outWorld[j] = SlerpHalf(inWorld[int_1],inWorld[int_2]);
        outUV[j] = lerp(inUV[int_1],inUV[int_2],0.5f);
        outPos[j] = SlerpHalf(inPos[int_1],inPos[int_2]);
    }
}

#endif