Shader "Custom/PinkOutlineSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _FillColor ("Fill Color", Color) = (1,0.8,0.85,1) // 淡いピンク
        _Thickness ("Outline Thickness", Range(0,5)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off Lighting Off ZWrite Off Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // 自動的にセットされる
            fixed4 _OutlineColor;
            fixed4 _FillColor;
            float _Thickness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 元画像のアルファ
                fixed4 c = tex2D(_MainTex, i.uv);
                float alpha = c.a;

                if (alpha < 0.1)
                    discard;

                // 周囲ピクセルのアルファをチェック
                float2 offset = _MainTex_TexelSize.xy * _Thickness;
                float a1 = tex2D(_MainTex, i.uv + float2(offset.x, 0)).a;
                float a2 = tex2D(_MainTex, i.uv + float2(-offset.x, 0)).a;
                float a3 = tex2D(_MainTex, i.uv + float2(0, offset.y)).a;
                float a4 = tex2D(_MainTex, i.uv + float2(0, -offset.y)).a;

                // 輪郭判定（外側が透明なら輪郭）
                if ((a1 < 0.1) || (a2 < 0.1) || (a3 < 0.1) || (a4 < 0.1))
                {
                    return _OutlineColor;
                }

                // 内部は淡いピンクで塗りつぶし
                return _FillColor;
            }
            ENDCG
        }
    }
}
