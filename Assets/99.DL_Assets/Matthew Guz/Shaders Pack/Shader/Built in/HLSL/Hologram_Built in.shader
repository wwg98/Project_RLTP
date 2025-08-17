Shader "MatthewGuz/Hologram Shader Built in"
{
    Properties
    {
        _Color("Color", Color) = (0, 1, 1, 1)
        _MainTex("Base (RGB)", 2D) = "white" {}
        _AlphaTexture ("Alpha Mask (R)", 2D) = "white" {}

        _Scale ("Alpha Tiling", Float) = 3
        _ScrollSpeedV("Alpha scroll Speed", Range(0, 5.0)) = 1.0

        _GlowIntensity ("Glow Intensity", Range(0.01, 1.0)) = 0.5

        _GlitchSpeed ("Glitch Speed", Range(0, 50)) = 50.0
        _GlitchIntensity ("Glitch Intensity", Range(0.0, 0.1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Lighting Off
            ZWrite Off
            Blend SrcAlpha One
            Cull Back

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : NORMAL;
            };

            fixed4 _Color;
            sampler2D _MainTex, _AlphaTexture;
            half _Scale, _ScrollSpeedV, _GlowIntensity, _GlitchSpeed, _GlitchIntensity;

            v2f vert(appdata_t v)
            {
                v2f o;

                // Glitch Effect
                v.vertex.z += sin(_Time.g * _GlitchSpeed * 5 * v.vertex.y) * _GlitchIntensity;

                // Convert position to clip space
                o.pos = UnityObjectToClipPos(v.vertex);

                // UV transformation
                o.uv = v.uv;

                // World Position & Normal
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Alpha texture scrolling
                fixed4 alphaColor = tex2D(_AlphaTexture, i.worldPos.xy * _Scale + _Time.g * _ScrollSpeedV);

                // Main texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Apply alpha mask
                col.a = alphaColor.r;

                // Rim effect based on world normals
                half rim = 1.0 - saturate(dot(normalize(_WorldSpaceCameraPos - i.worldPos), i.worldNormal));

                // Final color with glow
                return col * _Color * (rim + _GlowIntensity);
            }
            ENDCG
        }
    }
}
