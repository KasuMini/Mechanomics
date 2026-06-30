Shader "Hidden/City/SelectionOutlineMask"
{
    // Renders a selected object as a solid white silhouette into the outline mask.
    // ZTest Always + Cull Off so the full projected silhouette is captured even when
    // the building is partly occluded by another building (outline shows on top).
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Mask"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionHCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return o;
            }

            half4 frag() : SV_Target { return half4(1, 1, 1, 1); }
            ENDHLSL
        }
    }
}
