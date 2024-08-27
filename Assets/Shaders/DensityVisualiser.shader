Shader "Unlit/DensityVisualizer"
{
    Properties
    {
        _DensityTex ("Density Texture", 2D) = "white" {}
        _Color1 ("Color 1", Color) = (0.1, 0.2, 0.6, 1)
        _Color2 ("Color 2", Color) = (0.6, 0.1, 0.2, 1)
        _Color3 ("Color 3", Color) = (0.8, 0.8, 0.2, 1)
        _Color4 ("Color 4", Color) = (0.2, 0.8, 0.2, 1)
        _Color5 ("Color 5", Color) = (0.2, 0.8, 0.2, 1)
        _MaxMagnitude ("Maximum magnitude", float) = 50.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _DensityTex;
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float4 _Color4;
            float4 _Color5;
            float _MaxMagnitude;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Sample the density texture
                float densityValue = tex2D(_DensityTex, i.uv).r;

                densityValue = log(densityValue);
                densityValue /= _MaxMagnitude;

                // Map the density value to a color range
                float3 color;
                if (densityValue < 0.25)
                {
                    color = lerp(_Color1.rgb, _Color2.rgb, densityValue * 4);
                }
                else if (densityValue < 0.5)
                {
                    color = lerp(_Color2.rgb, _Color3.rgb, (densityValue - 0.25) * 4);
                }
                else if (densityValue < 0.75)
                {
                    color = lerp(_Color3.rgb, _Color4.rgb, (densityValue - 0.5) * 4);
                }
                else
                {
                    color = lerp(_Color4.rgb, _Color5.rgb, (densityValue - 0.75) * 4);
                }

                // Blend it with a slight transparency effect
                return float4(color, densityValue * 0.8 + 0.2); // Add some transparency to lighter densities
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}