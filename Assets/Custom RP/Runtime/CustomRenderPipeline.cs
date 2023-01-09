using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline {
    private CameraRenderer _renderer = new CameraRenderer();

    protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        foreach (var camera in cameras) {
            _renderer.Render(context, camera);
        }
    }
}