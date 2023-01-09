using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer {

    private partial void DrawGizmos();

    private partial void DrawUnsupportedShaders();

    private partial void PrepareForSceneWindow();

#if UNITY_EDITOR

    private static ShaderTagId[] legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

    private static Material errorMaterial;

    private partial void DrawGizmos() {
        if (Handles.ShouldRenderGizmos()) {
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
    }

    private partial void DrawUnsupportedShaders() {
        if (errorMaterial == null) {
            errorMaterial =
                new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        var drawningSettings = new DrawingSettings(
            legacyShaderTagIds[0],new SortingSettings(_camera)){
            overrideMaterial = errorMaterial
        };
        for (int i = 1; i < legacyShaderTagIds.Length; i++) {
            drawningSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;
        _context.DrawRenderers(
            _cullingResult, ref drawningSettings, ref filteringSettings);
    }

    private partial void PrepareForSceneWindow() {
        if (_camera.cameraType == CameraType.SceneView) {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }

#endif
}