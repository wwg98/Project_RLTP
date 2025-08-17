Shader "MatthewGuz/Transparent_2_Built in"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1) 
        _MainTex ("Texture", 2D) = "white" {} 
        _Alpha ("Alpha", Range(0,1)) = 0.5 
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" } 
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha 
            ZWrite Off 
            Cull Off 

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
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
            float4 _Color;
            float _Alpha;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.a *= _Alpha; 
                return col;
            }
            ENDCG
        }
    }
}
