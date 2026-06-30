Shader "Mechanomics/PrismOutline"
{
    // Hover highlight: light up the building's hard EDGES/corners, view-independent
    // (front and back), by comparing the flat face normal (NORMAL) with the smooth
    // averaged normal (UV1). They match across a flat face (dark) and diverge at the
    // edges where faces meet (bright). Drawn over the building's own geometry as a 2nd
    // additive material, so it never bleeds into neighbours and ZTest occludes correctly.
    Properties
    {
        _OutlineColor ("Edge Color", Color) = (1.0, 0.92, 0.45, 1)
        _EdgeHardness ("Edge Hardness", Range(0.25, 6)) = 1.5
        _EdgeStrength ("Edge Strength", Range(0, 12)) = 7
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha One     // additive glow
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float3 smoothN : TEXCOORD1; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float3 flatN : TEXCOORD0; float3 smoothN : TEXCOORD1; };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _EdgeHardness;
                float _EdgeStrength;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.flatN = IN.normalOS;
                o.smoothN = dot(IN.smoothN, IN.smoothN) > 1e-6 ? IN.smoothN : IN.normalOS;
                return o;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 f = normalize(IN.flatN);
                float3 s = normalize(IN.smoothN);
                float edge = pow(1.0 - saturate(dot(f, s)), _EdgeHardness) * _EdgeStrength;  // 0 on faces, >0 at edges
                return half4(_OutlineColor.rgb * edge, saturate(edge));
            }
            ENDHLSL
        }
    }
    Fallback Off
}
