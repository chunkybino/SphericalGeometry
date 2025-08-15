Shader "Mine/Boring"
{
    Properties
    {
        _MainTexture("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)

        _Scale("Scale", Vector) = (1,1,1,0)

        _MatC0("MatrixC0", Vector) = (1,0,0,0)
        _MatC1("MatrixC1", Vector) = (0,1,0,0)
        _MatC2("MatrixC2", Vector) = (0,0,1,0)
        _MatC3("MatrixC3", Vector) = (0,0,0,1)

        _Radius("Radius", Float) = 1
        _DoV4("DoVertex4", Float) = 0
        _Subdivisions("Subdivisions", Float) = 1

        _Lit("Lit", Float) = 1
        _DoubleSideLit("DoubelSideLit", Float) = 0

        _DoShadow("DoShadow", Float) = 1
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vertexFunc
            #pragma fragment fragmentFunc
            #pragma geometry geometryFunc

            #include "UnityCG.cginc"
            #include "ShaderFunc.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float4 position4 : POSITION;
                float2 uv : TEXCOORD0; 
                float4 positionWorld : TANGENT;
            };

            struct g2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;  
                float4 normal : NORMAL;
                float4 positionWorld : TANGENT;
            };


            fixed4 _Color;
            sampler2D _MainTexture;
            fixed4 _Scale;

            fixed4 _MatC0;
            fixed4 _MatC1;
            fixed4 _MatC2;
            fixed4 _MatC3;

            float _Radius;
            float _DoV4;
            float _Subdivisions;

            float _Lit;
            float _DoubleSideLit;

            struct LightData
            {
                float4 position;
                float3 color;
                float intensity;

                float doFalloff;
                float falloffStart;
                float falloffRange;
                float ambience;

                float4 direction;
                float rangeAngle;
                float rangeFalloffAngleMult;
            };

            StructuredBuffer<int> _LightCount;
            StructuredBuffer<LightData> _LightData;

            struct ShadowData
            {
                float4 center;
                float4 norm1;
                float4 norm2;
                float4 norm3;
            };

            StructuredBuffer<int> _ShadowCount;
            StructuredBuffer<ShadowData> _ShadowData;

            v2g vertexFunc(appdata IN)
            {
                v2g OUT;

                float4x4 model = float4x4(_MatC0, _MatC1, _MatC2, _MatC3);

                //convert model coords to sphere coords, sterographic inverse
                float4 pos4 = IN.vertex;

                if (_DoV4 != 1) //skip this step if we already have out vertex in spherical coords
                {
                    //convert model coords to sphere coords, using sterographic inverse
                    pos4 *= _Scale / _Radius;
                    pos4 = (2 / ((pos4.x*pos4.x) + (pos4.y*pos4.y) + (pos4.z*pos4.z) + 1)) * float4(pos4.x,pos4.y,pos4.z,1);
                    pos4.w -= 1;
                }

                //apply model transformation to sphere coords
                pos4 = mul(model,pos4);
                OUT.positionWorld = pos4;

                pos4 = mul(UNITY_MATRIX_V, pos4);

                OUT.position4 = pos4;

                OUT.uv = IN.uv;

                return OUT;
            }


            [maxvertexcount(24)]
            void geometryFunc(triangle v2g IN[3], inout TriangleStream<g2f> OUT)
            {
                g2f v1;
                g2f v2;
                g2f v3;

                float4 norm = HyperCross(IN[0].positionWorld, IN[1].positionWorld, IN[2].positionWorld);

                v1.normal = norm;
                v2.normal = norm;
                v3.normal = norm;

                v1.positionWorld = IN[0].positionWorld;
                v2.positionWorld = IN[1].positionWorld;
                v3.positionWorld = IN[2].positionWorld;

                v1.uv = IN[0].uv;
                v2.uv = IN[1].uv;
                v3.uv = IN[2].uv;

                v1.position = mul(UNITY_MATRIX_P, SteroProject(IN[0].position4, _Radius));
                v2.position = mul(UNITY_MATRIX_P, SteroProject(IN[1].position4, _Radius));
                v3.position = mul(UNITY_MATRIX_P, SteroProject(IN[2].position4, _Radius));

                if (_Subdivisions == 0)
                {
                    OUT.Append(v1);
                    OUT.Append(v2);
                    OUT.Append(v3);
                }
                else
                {
                    float4 pos3[3] = {IN[0].position4,IN[1].position4,IN[2].position4};
                    float4 world3[3] = {IN[0].positionWorld,IN[1].positionWorld,IN[2].positionWorld};
                    float2 uv3[3] = {IN[0].uv,IN[1].uv,IN[2].uv};

                    float4 pos6[6];
                    float4 world6[6];
                    float2 uv6[6];

                    SubdivTri(pos3, world3, uv3, pos6, world6, uv6);

                    g2f v4;
                    g2f v5;
                    g2f v6;

                    v4.position = mul(UNITY_MATRIX_P, SteroProject(pos6[3], _Radius));
                    v5.position = mul(UNITY_MATRIX_P, SteroProject(pos6[4], _Radius));
                    v6.position = mul(UNITY_MATRIX_P, SteroProject(pos6[5], _Radius));

                    v4.positionWorld = world6[3];
                    v5.positionWorld = world6[4];
                    v6.positionWorld = world6[5];

                    v4.uv = uv6[3];
                    v5.uv = uv6[4];
                    v6.uv = uv6[5];

                    v4.normal = norm;
                    v5.normal = norm;
                    v6.normal = norm;

                    if (_Subdivisions == 1)
                    {
                        OUT.Append(v1);
                        OUT.Append(v4);
                        OUT.Append(v6);
                        OUT.Append(v5);
                        OUT.Append(v3);

                        OUT.RestartStrip();

                        OUT.Append(v4);
                        OUT.Append(v2);
                        OUT.Append(v5);
                    }
                    else
                    {
                        float4 pos15[15];
                        float4 world15[15];
                        float2 uv15[15];

                        SubdivTri2(pos6, world6, uv6, pos15, world15, uv15);

                        g2f v7;
                        g2f v8;
                        g2f v9;
                        g2f v10;
                        g2f v11;
                        g2f v12;
                        g2f v13;
                        g2f v14;
                        g2f v15;

                        v7.position = mul(UNITY_MATRIX_P, SteroProject(pos15[6], _Radius));
                        v8.position = mul(UNITY_MATRIX_P, SteroProject(pos15[7], _Radius));
                        v9.position = mul(UNITY_MATRIX_P, SteroProject(pos15[8], _Radius));
                        v10.position = mul(UNITY_MATRIX_P, SteroProject(pos15[9], _Radius));
                        v11.position = mul(UNITY_MATRIX_P, SteroProject(pos15[10], _Radius));
                        v12.position = mul(UNITY_MATRIX_P, SteroProject(pos15[11], _Radius));
                        v13.position = mul(UNITY_MATRIX_P, SteroProject(pos15[12], _Radius));
                        v14.position = mul(UNITY_MATRIX_P, SteroProject(pos15[13], _Radius));
                        v15.position = mul(UNITY_MATRIX_P, SteroProject(pos15[14], _Radius));

                        v7.positionWorld = world15[6];
                        v8.positionWorld = world15[7];
                        v9.positionWorld = world15[8];
                        v10.positionWorld = world15[9];
                        v11.positionWorld = world15[10];
                        v12.positionWorld = world15[11];
                        v13.positionWorld = world15[12];
                        v14.positionWorld = world15[13];
                        v15.positionWorld = world15[14];

                        v7.uv = uv15[6];
                        v8.uv = uv15[7];
                        v9.uv = uv15[8];
                        v10.uv = uv15[9];
                        v11.uv = uv15[10];
                        v12.uv = uv15[11];
                        v13.uv = uv15[12];
                        v14.uv = uv15[13];
                        v15.uv = uv15[14];

                        v7.normal = norm;
                        v8.normal = norm;
                        v9.normal = norm;
                        v10.normal = norm;
                        v11.normal = norm;
                        v12.normal = norm;
                        v13.normal = norm;
                        v14.normal = norm;
                        v15.normal = norm;
          
                        OUT.Append(v1);
                        OUT.Append(v7);
                        OUT.Append(v9);
                        OUT.Append(v8);
                        OUT.Append(v6);
                        OUT.Append(v13);
                        OUT.Append(v15);
                        OUT.Append(v14);
                        OUT.Append(v3);
                        
                        OUT.RestartStrip();

                        OUT.Append(v7);
                        OUT.Append(v4);
                        OUT.Append(v8);
                        OUT.Append(v12);
                        OUT.Append(v13);
                        OUT.Append(v5);
                        OUT.Append(v14);

                        OUT.RestartStrip();

                        OUT.Append(v4);
                        OUT.Append(v10);
                        OUT.Append(v12);
                        OUT.Append(v11);
                        OUT.Append(v5);

                        OUT.RestartStrip();

                        OUT.Append(v10);
                        OUT.Append(v2);
                        OUT.Append(v11);
                    }
                }
            }

            fixed4 fragmentFunc(g2f IN) : SV_Target
            {
                fixed4 pixelColor = float4(1,1,1,1);
                float4 normal = normalize(IN.normal);

                pixelColor = tex2D(_MainTexture, IN.uv);
                pixelColor *= _Color;

                if (_Lit == 0)
                {
                    return pixelColor;
                }

                float3 totalLight = float3(0,0,0);

                for (int i = 0; i < _LightCount[0]; i++)
                {
                    LightData light = _LightData[i];

                    float4 lightDis = light.position - IN.positionWorld;
                    float4 dirToLight = lightDis - IN.positionWorld*dot(lightDis,IN.positionWorld);
                    dirToLight = normalize(dirToLight);

                    float normalLightDot = dot(dirToLight, normal);
                    normalLightDot = lerp(normalLightDot,abs(normalLightDot),_DoubleSideLit); //should we illiminate from the back side?
                    normalLightDot = max(lerp(normalLightDot,1,light.ambience),0);

                    //fall off
                    float distance = acos(clamp(dot(light.position,IN.positionWorld), -1,1));//1.57*(1-disDot);
                    precise float falloffIntesity = 1;

                    if (light.doFalloff)
                    {
                        //falloffIntesity = 1 - saturate((distance-light.falloffStart)*light.falloffRange);
                        falloffIntesity = 1/(_Radius*(1.57)*sin(distance/_Radius));
                    }

                    float4 lightToPos = lightDis - light.position*dot(lightDis,light.position);
                    lightToPos = -normalize(lightToPos);
                    float lightRangeAngle = acos(dot(lightToPos, light.direction));

                    float coneIntensity = 1 - saturate((lightRangeAngle-light.rangeAngle)*light.rangeFalloffAngleMult);

                    float intensity = light.intensity*falloffIntesity*coneIntensity;

                    //shadowtime
                    for (int j = 0; j < _ShadowCount[0]; j++)
                    {
                        ShadowData shadow = _ShadowData[j];

                        precise float lightPlaneAngle = 1.57 - acos(dot(shadow.center, light.position));
                        precise float posPlaneAngle = 1.57 - acos(dot(shadow.center, IN.positionWorld));

                        if (sign(lightPlaneAngle) == sign(posPlaneAngle) || abs(posPlaneAngle) < 0.005) 
                        {
                            continue;
                        }

                        precise float4 planePos = Slerp4(IN.positionWorld,light.position,abs(posPlaneAngle),abs(lightPlaneAngle-posPlaneAngle));

                        float dot1 = dot(planePos,shadow.norm1);
                        float dot2 = dot(planePos,shadow.norm2);
                        float dot3 = dot(planePos,shadow.norm3);

                        //intensity = 0;

                        if (dot1 < 0 && dot2 < 0 && dot3 < 0)
                        {
                            intensity = 0;
                            break;
                        }
                    }

                    totalLight += normalLightDot*intensity*light.color;
                }

                pixelColor.rgb *= totalLight; 

                return pixelColor;
            }

            ENDCG
        }
    }
}