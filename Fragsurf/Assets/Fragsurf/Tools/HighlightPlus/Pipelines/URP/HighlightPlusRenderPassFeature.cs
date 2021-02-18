using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HighlightPlus {

    public class HighlightPlusRenderPassFeature : ScriptableRendererFeature {
        class HighlightPass : ScriptableRenderPass {

            ScriptableRenderer renderer;
            RenderTextureDescriptor cameraTextureDescriptor;

            public bool usesCameraOverlay = false;

            public void Setup(RenderPassEvent renderPassEvent, ScriptableRenderer renderer) {
                this.renderPassEvent = renderPassEvent;
                this.renderer = renderer;
            }
            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in an performance manner.
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
                this.cameraTextureDescriptor = cameraTextureDescriptor;
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {

                Camera cam = renderingData.cameraData.camera;

                RenderTargetIdentifier cameraColorTarget = renderer.cameraColorTarget;
                RenderTargetIdentifier cameraDepthTarget = renderer.cameraDepthTarget;
                if (!usesCameraOverlay && (cameraTextureDescriptor.msaaSamples > 1 || cam.cameraType == CameraType.SceneView)) {
                    cameraDepthTarget = cameraColorTarget;
                }
                int count = HighlightEffect.instances.Count;
                for (int k = 0; k < count; k++) {
                    HighlightEffect effect = HighlightEffect.instances[k];
                    if (effect == null) continue;
                    if (effect.isActiveAndEnabled) {
                        CommandBuffer cb = effect.GetCommandBuffer(cam, cameraColorTarget, cameraDepthTarget);
                        if (cb != null) {
                            context.ExecuteCommandBuffer(cb);
                        }
                    }
                }
            }

            /// Cleanup any allocated resources that were created during the execution of this render pass.
            public override void FrameCleanup(CommandBuffer cmd) {
            }
        }

        HighlightPass renderPass;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public static bool installed;


        void OnDisable() {
            installed = false;
        }

        public override void Create() {
            renderPass = new HighlightPass();
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
#if UNITY_2019_4_OR_NEWER
            if (renderingData.cameraData.renderType == CameraRenderType.Base) {
                Camera cam = renderingData.cameraData.camera;
                renderPass.usesCameraOverlay = cam.GetUniversalAdditionalCameraData().cameraStack.Count > 0;
            } // else return; // do not render highlight effects on overlay cameras
#endif
            renderPass.Setup(renderPassEvent, renderer);
            renderer.EnqueuePass(renderPass);
            installed = true;
        }
    }

}
