using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

// Screen-space per-object selection outline. Renders the currently-selected Renderers
// into a silhouette mask, then edge-detects a uniform-width ring around them over the
// camera colour. Selection is pushed in at runtime via SetRenderers (no layers, no
// materials on the targets). Render-graph native; works on the URP Forward renderer.
public class SelectionOutlineFeature : ScriptableRendererFeature
{
    // Located by callers (e.g. EventScheduler) at runtime to push selected renderers in.
    public static SelectionOutlineFeature Active { get; private set; }

    [SerializeField] RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingTransparents;
    [SerializeField] Material maskMaterial;        // Hidden/City/SelectionOutlineMask
    [SerializeField] Material compositeMaterial;   // Hidden/City/SelectionOutlineComposite
    [SerializeField] Color outlineColor = new Color(1f, 0.9f, 0.2f, 1f);
    [SerializeField, Range(1, 8)] int thickness = 3;

    static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    static readonly int ThicknessId = Shader.PropertyToID("_Thickness");

    Renderer[] targets;
    OutlinePass pass;

    // Set the objects to outline (empty/null clears it). Cheap to call every frame.
    public void SetRenderers(Renderer[] renderers) => targets = renderers;

    public override void Create()
    {
        name = "Selection Outline";
        pass = new OutlinePass(this);
        Active = this;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (maskMaterial == null || compositeMaterial == null) return;
        if (targets == null || targets.Length == 0) return;   // nothing selected -> no cost

        compositeMaterial.SetColor(OutlineColorId, outlineColor);
        compositeMaterial.SetFloat(ThicknessId, thickness);
        pass.renderPassEvent = renderEvent;
        renderer.EnqueuePass(pass);
    }

    class OutlinePass : ScriptableRenderPass
    {
        const string MaskTexName = "_SelectionOutlineMask";
        readonly SelectionOutlineFeature owner;

        public OutlinePass(SelectionOutlineFeature owner) { this.owner = owner; }

        class MaskData { public Renderer[] renderers; public Material material; }
        class CompositeData { public TextureHandle mask; public Material material; }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resources = frameData.Get<UniversalResourceData>();
            if (resources.isActiveTargetBackBuffer) return;   // avoid blitting the back buffer

            var cameraData = frameData.Get<UniversalCameraData>();
            var maskDesc = cameraData.cameraTargetDescriptor;
            maskDesc.colorFormat = RenderTextureFormat.R8;
            maskDesc.depthBufferBits = 0;
            maskDesc.msaaSamples = 1;

            TextureHandle mask = UniversalRenderer.CreateRenderGraphTexture(renderGraph, maskDesc, MaskTexName, clear: true);
            TextureHandle color = resources.activeColorTexture;
            if (!mask.IsValid() || !color.IsValid()) return;

            // 1) draw selected renderers as a solid silhouette into the mask
            using (var builder = renderGraph.AddRasterRenderPass<MaskData>("Selection Outline Mask", out var data))
            {
                data.renderers = owner.targets;
                data.material = owner.maskMaterial;
                builder.SetRenderAttachment(mask, 0);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((MaskData d, RasterGraphContext ctx) =>
                {
                    foreach (var r in d.renderers)
                    {
                        if (r == null) continue;
                        int count = r.sharedMaterials.Length;
                        for (int i = 0; i < count; i++)
                            ctx.cmd.DrawRenderer(r, d.material, i, 0);
                    }
                });
            }

            // 2) edge-detect the mask into a ring, blended over the camera colour
            using (var builder = renderGraph.AddRasterRenderPass<CompositeData>("Selection Outline Composite", out var data))
            {
                data.mask = mask;
                data.material = owner.compositeMaterial;
                builder.UseTexture(mask);
                builder.SetRenderAttachment(color, 0);
                builder.SetRenderFunc((CompositeData d, RasterGraphContext ctx) =>
                    Blitter.BlitTexture(ctx.cmd, d.mask, new Vector4(1f, 1f, 0f, 0f), d.material, 0));
            }
        }
    }
}
