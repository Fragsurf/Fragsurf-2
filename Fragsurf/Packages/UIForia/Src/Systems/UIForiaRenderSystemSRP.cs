using UIForia;
using UIForia.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Src.Systems {

    internal class UIForiaRenderPass : ScriptableRenderPass {

        private static readonly string _ProfilerTag = "UIForia Main Command Buffer";

        private RenderContext _renderContext;

        public UIForiaRenderPass(RenderContext renderContext) {
            renderPassEvent = RenderPassEvent.AfterRendering;
            _renderContext = renderContext;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get(_ProfilerTag);
            _renderContext.Render(renderingData.cameraData.camera, cmd);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

    }

    public class UIForiaRenderSystemSRP : ScriptableRendererFeature {

        public override void Create() { }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            foreach (Application application in Application.Applications) {
                if (application.Camera == renderingData.cameraData.camera) {
                    renderer.EnqueuePass(new UIForiaRenderPass(application.renderSystem.GetRenderContext()));
                }
            }
        }

    }

}