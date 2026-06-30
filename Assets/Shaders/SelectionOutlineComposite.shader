Shader "Hidden/City/SelectionOutlineComposite"
{
    // Fullscreen edge-detect over the silhouette mask (bound as _BlitTexture). Draws a
    // uniform-width ring just OUTSIDE the silhouette and leaves the interior untouched,
    // alpha-blended over the camera colour. Width is constant in pixels (screen-space).
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1, 0.9, 0.2, 1)
        _Thickness("Thickness (px)", Float) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off
        Cull Off
        Pass
        {
            Name "Composite"
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            SAMPLER(sampler_BlitTexture);
            float4 _OutlineColor;
            float  _Thickness;

            half4 frag(Varyings i) : SV_Target
            {
                // Inside the silhouette: draw nothing so the building stays visible.
                float center = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.texcoord).r;
                if (center > 0.5)
                    return half4(0, 0, 0, 0);

                // Outside: light up if any neighbour within the radius is inside the
                // silhouette -> a clean ring of width _Thickness around the object.
                int T = (int)max(1.0, _Thickness);
                float inside = 0.0;
                [loop] for (int x = -T; x <= T; x++)
                {
                    [loop] for (int y = -T; y <= T; y++)
                    {
                        if (x * x + y * y > T * T) continue;   // round corners
                        float2 uv = i.texcoord + float2(_BlitTexture_TexelSize.x * x, _BlitTexture_TexelSize.y * y);
                        inside = max(inside, SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv).r);
                    }
                }
                float ring = step(0.5, inside);
                return half4(_OutlineColor.rgb, _OutlineColor.a * ring);
            }
            ENDHLSL
        }
    }
}
