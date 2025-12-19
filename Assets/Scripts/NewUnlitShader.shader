Shader "Custom/GlowOutline2D"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,1,0,1)
        _GlowSize ("Glow Size", Range(0.0, 0.5)) = 0.05
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Cull Off
        Lighting Off
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _GlowColor;
            float _GlowSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // If transparent pixel, check neighbors for glow
                if (col.a < 0.1)
                {
                    float glow = 0.0;
                    float2 offset = float2(_GlowSize, _GlowSize);

                    glow += tex2D(_MainTex, i.uv + offset).a;
                    glow += tex2D(_MainTex, i.uv - offset).a;
                    glow += tex2D(_MainTex, i.uv + offset.xy).a;
                    glow += tex2D(_MainTex, i.uv - offset.xy).a;

                    if (glow > 0.0)
                        return _GlowColor;
                }

                return col * _Color;
            }
            ENDCG
        }
    }
}
