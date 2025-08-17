Shader "MatthewGuz/Vanish_Burn_Built in"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Base Map", 2D) = "white" {}
        _NoiseTex ("Noise Map", 2D) = "white" {}
        _BrightnessTex ("Brightness Map", 2D) = "white" {}

        _Cutoff ("Dissolve Amount", Range(0, 1)) = 0.25
        _EdgeWidth ("Edge Width", Range(0, 1)) = 0.05
        [HDR] _EdgeColor ("Edge Color", Color) = (1,1,1,1)
        _BurnColor ("Burnt Color", Color) = (0.1,0.05,0.05,1)
        _BurnIntensity ("Burn Intensity", Range(0, 5)) = 1.0

        _OscillationEnabled ("Enable Oscillation", Range(0, 1)) = 1.0
        _OscillationAmplitude ("Oscillation Amplitude", Range(0, 5)) = 1.0
        _OscillationSpeed ("Oscillation Speed", Range(0, 5)) = 1.0

        _Speed ("Brightness Speed", Range(0, 5)) = 1.0
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
            sampler2D _BrightnessTex;

            fixed4 _BaseColor;
            fixed4 _EdgeColor;
            fixed4 _BurnColor;
            float _Cutoff;
            float _EdgeWidth;
            float _BurnIntensity;

            float _OscillationEnabled;
            float _OscillationAmplitude;
            float _OscillationSpeed;
            float _Speed;

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

                // Offset UV coordinates for animated brightness effect
                float2 movingUV = i.uv + float2(_Time.y * _Speed, 0);
                float brightnessValue = tex2D(_BrightnessTex, movingUV).r;

                // Edge effect calculation
                float edgeThreshold = _Cutoff * (1.0 + _EdgeWidth);

                // Burn effect logic
                fixed4 finalColor;
                if (noiseValue < _Cutoff)
                {
                    // Oscillation effect if enabled
                    float oscillation = _OscillationEnabled > 0.5 ? 
                                        _OscillationAmplitude * sin(_Time.y * _OscillationSpeed) : 0.0;
                    finalColor = _BurnColor * (_BurnIntensity + oscillation) * brightnessValue;
                }
                else
                {
                    finalColor = baseTex;
                }

                // Apply edge effect
                fixed4 edgeColor = (noiseValue < edgeThreshold && noiseValue >= _Cutoff) ? _EdgeColor : fixed4(0,0,0,0);

                return finalColor + edgeColor;
            }
            ENDCG
        }
    }
}
