Shader "MatthewGuz/Transparent_Built in"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01
        _Transparency ("Transparency", Range(0, 1)) = 0.5
        _AngleThreshold ("Angle Threshold", Range(0, 1)) = 0.3 // Controls visibility of internal parts
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite Off

        Pass
        {
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _Transparency;
            float _AngleThreshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate the angle between the view direction and the normal
                float visibility = dot(i.worldNormal, i.viewDir);

                // Discard fragments that are not sufficiently visible
                if (visibility > -_AngleThreshold && visibility < _AngleThreshold)
                {
                    discard;
                }

                // Base texture color
                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color;

                // Outline effect based on the camera angle
                float outlineFactor = smoothstep(1.0 - _OutlineWidth, 1.0, abs(visibility));

                // Final color with transparency
                fixed4 finalColor = lerp(_OutlineColor, texColor, outlineFactor);
                finalColor.a *= _Transparency;

                return finalColor;
            }
            ENDCG
        }
    }

    FallBack "Transparent/Cutout/VertexLit"
}
