﻿using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer {
    public const string BUFFER_NAME = "Render Camera";

    private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    private CommandBuffer _buffer = new CommandBuffer { name = BUFFER_NAME };
    private ScriptableRenderContext _context;
    private Camera _camera;
    private CullingResults _cullingResult;

    public void Render(ScriptableRenderContext context, Camera camera) {
        _context = context;
        _camera = camera;
        PrepareForSceneWindow();
        if (!Cull()) {
            return;
        }
        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }

    private void DrawVisibleGeometry() {
        var sortingSettings = new SortingSettings(_camera){
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(
            unlitShaderTagId,sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        _context.DrawRenderers(
            _cullingResult,
            ref drawingSettings,
            ref filteringSettings);
        _context.DrawSkybox(_camera);
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        _context.DrawRenderers(
            _cullingResult, ref drawingSettings, ref filteringSettings
        );
    }

    private void Setup() {
        _context.SetupCameraProperties(_camera);
        _buffer.ClearRenderTarget(true, true, Color.clear);
        _buffer.BeginSample(BUFFER_NAME);
        ExecuteBuffer();
    }

    private void Submit() {
        _buffer.EndSample(BUFFER_NAME);
        _context.Submit();
    }

    private void ExecuteBuffer() {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    private bool Cull() {
        if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p)) {
            _cullingResult = _context.Cull(ref p);
            return true;
        }
        return false;
    }
}