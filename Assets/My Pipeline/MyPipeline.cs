using UnityEngine;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class MyPipeline : RenderPipeline
{
    CullingResults cull;
    Material errorMaterial;
    CommandBuffer cameraBuffer = new CommandBuffer() {
        name = "Render Camera"
    };

    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        foreach(var camera in cameras) {
            Render(context, camera);
        }
    }

    void Render(ScriptableRenderContext context, Camera camera) {
        ScriptableCullingParameters cullingParameters;
        if (!camera.TryGetCullingParameters(out cullingParameters)) {
            return;
        }

        cull = context.Cull(ref cullingParameters);

        context.SetupCameraProperties(camera);

        CameraClearFlags clearFlags = camera.clearFlags;
        cameraBuffer.ClearRenderTarget((clearFlags & CameraClearFlags.Depth) != 0, (clearFlags & CameraClearFlags.Color) != 0, camera.backgroundColor);

        cameraBuffer.BeginSample("Render Camera");
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();

        var drawingSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), new SortingSettings() {
              criteria = SortingCriteria.CommonOpaque
        });
        var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
        context.DrawRenderers(cull, ref drawingSettings, ref filterSettings);

        context.DrawSkybox(camera);

        drawingSettings.sortingSettings = new SortingSettings() {
            criteria = SortingCriteria.CommonTransparent
        };
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        //filterSettings = new FilteringSettings(RenderQueueRange.transparent);
        context.DrawRenderers(cull, ref drawingSettings, ref filterSettings);

        DrawDefaultPipeline(context, camera);

        cameraBuffer.EndSample("Render Camera");
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();
        
        context.Submit();
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera) {
        if (errorMaterial == null) {
            Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
            errorMaterial = new Material(errorShader) {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        var drawSettings = new DrawingSettings(new ShaderTagId("ForwardBase"), new SortingSettings());
        drawSettings.SetShaderPassName(1, new ShaderTagId("PrepassBase"));
        drawSettings.SetShaderPassName(2, new ShaderTagId("Always"));
        drawSettings.SetShaderPassName(3, new ShaderTagId("Vertex"));
        drawSettings.SetShaderPassName(4, new ShaderTagId("VertexLMRGBM"));
        drawSettings.SetShaderPassName(5, new ShaderTagId("VertexLM"));
        drawSettings.overrideMaterialPassIndex = 0;
        drawSettings.overrideMaterial = errorMaterial;

        var filterSettings = new FilteringSettings(RenderQueueRange.all);

        context.DrawRenderers(cull, ref drawSettings, ref filterSettings);
    }
}
