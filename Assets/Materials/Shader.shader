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
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vertexFunc
            #pragma fragment fragmentFunc

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;  
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

            v2f vertexFunc(appdata IN)
            {
                v2f OUT;

                float4x4 model = float4x4(_MatC0, _MatC1, _MatC2, _MatC3);

                //convert model coords to sphere coords, sterographic inverse
                float4 pos4 = IN.vertex;

                if (_DoV4 != 1) //skip this step if we already have out vertex in spherical coords
                {
                    //convert model coords to sphere coords, using sterographic inverse
                    pos4 *= _Scale;
                    pos4 = (2 / ((pos4.x*pos4.x) + (pos4.y*pos4.y) + (pos4.z*pos4.z) + 1)) * float4(pos4.x,pos4.y,pos4.z,1);
                    pos4.w -= 1;
                }

                //apply model transformation to sphere coords
                pos4 = mul(model,pos4);

                pos4 = mul(UNITY_MATRIX_V, pos4);

                //sterographic projection back into 3d
                if (pos4.w == -1) {
                    pos4 = float4(999999,0,0,1);
                } else {
                    pos4 = pos4 / (1 + pos4.w);
                    pos4 *= _Radius;
                    pos4.w = 1;
                }

                OUT.position = mul(UNITY_MATRIX_P, pos4);

                OUT.uv = IN.uv;

                return OUT;
            }

            fixed4 fragmentFunc(v2f IN) : SV_Target
            {
                fixed4 pixelColor = tex2D(_MainTexture, IN.uv);
                pixelColor *= _Color;

                return pixelColor;
            }

            ENDCG
        }
    }
}