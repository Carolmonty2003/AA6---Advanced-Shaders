using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SnowRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material stencilWriterMaterial;
        public Material snowFullscreenMaterial;
        public LayerMask snowExcludedLayer;
    }

    public Settings settings = new Settings();
    private StencilWriterPass _stencilPass;
    private SnowFullscreenPass _snowPass;

    public override void Create()
    {
        _stencilPass = new StencilWriterPass(
            settings.stencilWriterMaterial,
            settings.snowExcludedLayer
        );
        _snowPass = new SnowFullscreenPass(settings.snowFullscreenMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.stencilWriterMaterial != null)
            renderer.EnqueuePass(_stencilPass);
        if (settings.snowFullscreenMaterial != null)
            renderer.EnqueuePass(_snowPass);
    }
}

public class StencilWriterPass : ScriptableRenderPass
{
    private readonly Material _mat;
    private FilteringSettings _filter;

    private static readonly ShaderTagId[] _shaderTags =
    {
        new ShaderTagId("UniversalForward"),
        new ShaderTagId("UniversalForwardOnly"),
        new ShaderTagId("LightweightForward"),
        new ShaderTagId("SRPDefaultUnlit")
    };

    public StencilWriterPass(Material mat, LayerMask layer)
    {
        _mat = mat;
        _filter = new FilteringSettings(RenderQueueRange.all, layer);
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing - 1;
    }

    [System.Obsolete]
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_mat == null) return;

        foreach (var tag in _shaderTags)
        {
            var drawSettings = CreateDrawingSettings(
                tag, ref renderingData, SortingCriteria.CommonOpaque);

            drawSettings.overrideMaterial = _mat;
            drawSettings.overrideMaterialPassIndex = 0;

            context.DrawRenderers(
                renderingData.cullResults, ref drawSettings, ref _filter);
        }
    }
}

public class SnowFullscreenPass : ScriptableRenderPass
{
    private readonly Material _mat;
    private static readonly int TempRTId = Shader.PropertyToID("_SnowTempRT");

    public SnowFullscreenPass(Material mat)
    {
        _mat = mat;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

    }

    [System.Obsolete]
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_mat == null) return;
        if (renderingData.cameraData.cameraType == CameraType.Preview) return;
        if (renderingData.cameraData.cameraType == CameraType.SceneView) return;

        float snowAmount = _mat.GetFloat("_SnowAmount");
        if (snowAmount <= 0.1f) return;

        var cmd = CommandBufferPool.Get("SnowFullscreen");

        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1;

        cmd.GetTemporaryRT(TempRTId, desc, FilterMode.Point);

        var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
        var depth = renderingData.cameraData.renderer.cameraDepthTargetHandle;

        cmd.Blit(source, TempRTId);
        cmd.SetGlobalTexture(Shader.PropertyToID("_CameraColorTexture"), TempRTId);

        cmd.SetRenderTarget(source, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                            depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);

        cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _mat, 0, 0);

        cmd.ReleaseTemporaryRT(TempRTId);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}