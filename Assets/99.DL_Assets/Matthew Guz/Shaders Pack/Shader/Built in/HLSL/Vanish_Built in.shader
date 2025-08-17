Shader "MatthewGuz/Vanish_Built in"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Base Map", 2D) = "white" {}
        _NoiseTex ("Noise Map", 2D) = "white" {}

        _Cutoff ("Dissolve Amount", Range(0, 1)) = 0.25
        _EdgeWidth ("Edge Width", Range(0, 1)) = 0.05
        [HDR] _EdgeColor ("Edge Color", Color) = (1,1,1,1)
        
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Cull [_Cull]
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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

            sampler2D _MainTex;
            sampler2D _NoiseTex;

            fixed4 _BaseColor;
            fixed4 _EdgeColor;
            float _Cutoff;
            float _EdgeWidth;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 baseTex = tex2D(_MainTex, i.uv) * _BaseColor;
                float noiseValue = tex2D(_NoiseTex, i.uv).r;

                // Dissolve effect
                if (noiseValue < _Cutoff)
                    discard;

                // Edge effect
                float edgeThreshold = _Cutoff * (1.0 + _EdgeWidth);
                fixed4 edgeColor = (noiseValue < edgeThreshold) ? _EdgeColor : fixed4(0, 0, 0, 0);

                return baseTex + edgeColor;
            }
            ENDCG
        }
    }
}
