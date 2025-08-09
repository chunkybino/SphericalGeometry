#ifndef SHADERFUNC_HLSL
#define SHADERFUNC_HLSL

float4 SteroProject(float4 pos4, float4 radius)
{
    float Step = step(1, -pos4.w);

    pos4.w += Step;

    pos4 = pos4 / (1 + pos4.w);
    pos4 *= radius;
    pos4.w = 1;

    pos4.x = lerp(pos4.x, 999999, Step);
    pos4.y = lerp(pos4.y, 0, Step);
    pos4.z = lerp(pos4.z, 0, Step);

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

float4 SlerpHalf(float4 a, float4 b)
{
    return normalize((a+b)/2);
}

#endif