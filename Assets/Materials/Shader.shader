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

        _WorldLight("World Light Source", Vector) = (0,1,0,0)
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

            float4 _WorldLight;

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
                //OUT.position = mul(UNITY_MATRIX_P, SteroProject(pos4, _Radius));

                OUT.uv = IN.uv;

                return OUT;
            }


            [maxvertexcount(12)]
            void geometryFunc(triangle v2g IN[3], inout TriangleStream<g2f> OUT)
            {
                g2f v1;
                g2f v2;
                g2f v3;

                g2f v4;
                g2f v5;
                g2f v6;

                v1.positionWorld = IN[0].positionWorld;
                v2.positionWorld = IN[1].positionWorld;
                v3.positionWorld = IN[2].positionWorld;

                v4.positionWorld = SlerpHalf(IN[0].positionWorld,IN[1].positionWorld);
                v5.positionWorld = SlerpHalf(IN[1].positionWorld,IN[2].positionWorld);
                v6.positionWorld = SlerpHalf(IN[2].positionWorld,IN[0].positionWorld);

                v1.uv = IN[0].uv;
                v2.uv = IN[1].uv;
                v3.uv = IN[2].uv;

                v4.uv = lerp(IN[0].uv,IN[1].uv,0.5f);
                v5.uv = lerp(IN[1].uv,IN[2].uv,0.5f);
                v6.uv = lerp(IN[2].uv,IN[0].uv,0.5f);

                v1.position = mul(UNITY_MATRIX_P, SteroProject(IN[0].position4, _Radius));
                v2.position = mul(UNITY_MATRIX_P, SteroProject(IN[1].position4, _Radius));
                v3.position = mul(UNITY_MATRIX_P, SteroProject(IN[2].position4, _Radius));
                v4.position = mul(UNITY_MATRIX_P, SteroProject(SlerpHalf(IN[0].position4,IN[1].position4), _Radius));
                v5.position = mul(UNITY_MATRIX_P, SteroProject(SlerpHalf(IN[1].position4,IN[2].position4), _Radius));
                v6.position = mul(UNITY_MATRIX_P, SteroProject(SlerpHalf(IN[2].position4,IN[0].position4), _Radius));

                float4 norm = HyperCross(IN[0].positionWorld, IN[1].positionWorld, IN[2].positionWorld);

                v1.normal = norm;
                v2.normal = norm;
                v3.normal = norm;
                v4.normal = norm;
                v5.normal = norm;
                v6.normal = norm;

                if (_Subdivisions == 1)
                {
                    OUT.Append(v1);
                    OUT.Append(v4);
                    OUT.Append(v6);

                    OUT.Append(v5);

                    OUT.RestartStrip();

                    OUT.Append(v4);
                    OUT.Append(v2);
                    OUT.Append(v5);

                    OUT.RestartStrip();

                    OUT.Append(v6);
                    OUT.Append(v5);
                    OUT.Append(v3);
                }
                else
                {
                    OUT.Append(v1);
                    OUT.Append(v2);
                    OUT.Append(v3);
                }
            }

            fixed4 fragmentFunc(g2f IN) : SV_Target
            {
                fixed4 pixelColor = float4(1,1,1,1);
                float4 normal = normalize(IN.normal);

                pixelColor = tex2D(_MainTexture, IN.uv);
                pixelColor *= _Color;

                //float4 colorNorm = abs(float4(normal.x,normal.w,normal.z,normal.y));
                //pixelColor *= normalize(colorNorm)+float4(1,1,1,1)/2;

                float4 lightDir = _WorldLight - IN.positionWorld;
                lightDir -= normal*dot(lightDir,normal);
                lightDir = normalize(lightDir);

                float lightDot = dot(_WorldLight, normal);
                lightDot = (lightDot+1)/2;

                pixelColor *= lightDot; 

                return pixelColor;
            }

            ENDCG
        }
    }
}