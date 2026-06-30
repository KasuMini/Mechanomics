Shader "Mechanomics/CityPrism"
{
    // Translucent prism with cheap depth cues: fake directional face shading so
    // top and sides differ, plus a fresnel rim that outlines the silhouette and
    // firms up the edges (rim also pushes alpha so edges read crisp).
    Properties
    {
        _BaseColor   ("Base Color", Color) = (0.55, 0.70, 0.95, 0.42)
        _RimColor    ("Rim Color", Color)  = (0.85, 0.95, 1.0, 1.0)
        _RimPower    ("Rim Power", Range(0.5, 8)) = 3.0
        _RimStrength ("Rim Strength", Range(0, 3)) = 1.2
        _RimAlpha    ("Rim Alpha Boost", Range(0, 1)) = 0.6
        _ShadeMin    ("Shade Min", Range(0, 1)) = 0.5
        _TopBoost    ("Top Boost", Range(0, 1)) = 0.22
        _LightDir    ("Light Dir", Vector) = (0.35, 0.45, -0.55, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 positionWS : TEXCOORD1; };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RimColor;
                float _RimPower;
                float _RimStrength;
                float _RimAlpha;
                float _ShadeMin;
                float _TopBoost;
                float4 _LightDir;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(IN.positionOS.xyz);
                o.positionHCS = p.positionCS;
                o.positionWS  = p.positionWS;
                o.normalWS    = TransformObjectToWorldNormal(IN.normalOS);
                return o;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float3 L = normalize(_LightDir.xyz);

                float ndl   = saturate(dot(N, L));
                float shade = lerp(_ShadeMin, 1.0, ndl);
                float top   = saturate(dot(N, normalize(float3(0.0, 0.4, -1.0)))); // faces toward viewer-up
                float3 base = _BaseColor.rgb * shade + _TopBoost * top;

                float rim   = pow(1.0 - saturate(dot(N, V)), _RimPower) * _RimStrength;
                float3 col  = base + _RimColor.rgb * rim;
                return half4(col, 1.0);   // opaque: depth-tested 3D, correct draw order
            }
            ENDHLSL
        }
    }
    Fallback Off
}
