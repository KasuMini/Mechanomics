Shader "Mechanomics/MechPaletteSwap"
{
    // Source art uses 3 pure ramps (R / G / B). Each pixel's dominant channel picks a region,
    // its brightness samples that region's row in a 3-row palette LUT. Transparent stays transparent.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _PaletteTex ("Palette LUT (3 rows: R,G,B)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _MinValue ("Min Brightness", Range(0,1)) = 0.02
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
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

            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f     { float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            sampler2D _PaletteTex;
            fixed4 _Color;
            float _MinValue;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 src = tex2D(_MainTex, i.uv);
                fixed3 c = src.rgb;
                float v = max(c.r, max(c.g, c.b));
                if (v > _MinValue)
                {
                    // row centres for the 3-row LUT (R=bottom, G=mid, B=top)
                    float row; float shade;
                    if (c.r >= c.g && c.r >= c.b)       { row = 0.5 / 3.0; shade = c.r; }
                    else if (c.g >= c.b)                { row = 1.5 / 3.0; shade = c.g; }
                    else                                { row = 2.5 / 3.0; shade = c.b; }
                    src.rgb = tex2D(_PaletteTex, float2(shade, row)).rgb;
                }
                return src * i.color;
            }
            ENDCG
        }
    }
}
