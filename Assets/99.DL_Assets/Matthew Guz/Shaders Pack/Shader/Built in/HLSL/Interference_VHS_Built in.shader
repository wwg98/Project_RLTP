Shader "MatthewGuz/Interference_VHS_Built in"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.1
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.2
        _Color ("Color Tint", Color) = (1, 1, 1, 1)
        _TimeMultiplier ("Time Multiplier", Range(0.1, 10)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Lighting Off
            ZWrite Off
            Blend SrcAlpha One
            Cull Off

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _DistortionStrength;
            float _NoiseStrength;
            float _TimeMultiplier;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float rand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // Distortion effect using a sine wave
                uv.x += sin(uv.y * 500 + _Time.y * _TimeMultiplier) * _DistortionStrength;
                
                // Random noise effect
                uv += (rand(uv + _Time.xy * _TimeMultiplier) - 0.5) * _NoiseStrength;

                // Sample the texture
                fixed4 col = tex2D(_MainTex, uv);

                // Apply the color tint
                return col * _Color;
            }
            ENDCG
        }
    }
}
