using UnityEngine;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class CustomRenderPipeline : RenderPipeline
{
    CullingResults cull;
    Material errorMaterial;
    CommandBuffer cameraBuffer = new CommandBuffer() {
        name = "Render Camera"
    };

    CameraRenderer renderer = new CameraRenderer();

    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        foreach(var camera in cameras) {
            renderer.Render(context, camera);
        }
    }
}
