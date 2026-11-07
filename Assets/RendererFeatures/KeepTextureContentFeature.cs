using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class KeepTextureContentFeature : ScriptableRendererFeature {
  private class ImportTexturePass : ScriptableRenderPass {
    private RTHandle m_persistentTexture;

    public void SetTexture(RenderTexture targetRT) {
      if (m_persistentTexture == null || m_persistentTexture.rt != targetRT) {
        m_persistentTexture?.Release();
        m_persistentTexture = RTHandles.Alloc(targetRT);
      }
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
      var cameraData = frameData.Get<UniversalCameraData>();
      // Camera must render to an RT
      if (cameraData.camera.targetTexture == null) {
        return;
      }

      var texture = renderGraph.ImportTexture(m_persistentTexture, new ImportResourceParams {
        clearOnFirstUse = false,
        discardOnLastUse = false
      });
      
      // Set clear texture
      var resourceData = frameData.Get<UniversalResourceData>();
      resourceData.cameraColor = texture;
    }

    public void Dispose() {
      m_persistentTexture?.Release();
    }
  }

  private ImportTexturePass m_pass;

  public override void Create() {
    m_pass = new();
    m_pass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
  }

  public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
    var targetRT = renderingData.cameraData.targetTexture;
    // Camera must render to an RT & clear flag should be Nothing 
    if (targetRT != null && renderingData.cameraData.camera.clearFlags == CameraClearFlags.Nothing) {
      m_pass.SetTexture(targetRT);
      renderer.EnqueuePass(m_pass);
    }
  }

  protected override void Dispose(bool disposing) {
    m_pass?.Dispose();
  }
}