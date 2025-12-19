Shader "Custom/PotionRedShadowOnly"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Sprite Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (1,0,0,0.5)
        _ShadowHeight ("Shadow Height", Range(0.05,0.5)) = 0.2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Cull Off
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
            fixed4 _ShadowColor;
            float _ShadowHeight;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Warna sprite asli
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                // Bayangan merah di bagian bawah sprite
                float shadowFactor = saturate((_ShadowHeight - i.uv.y) / _ShadowHeight);
                fixed4 shadow = _ShadowColor * shadowFactor;

                // Gabungkan sprite + shadow
                return col + shadow;
            }
            ENDCG
        }
    }
}

