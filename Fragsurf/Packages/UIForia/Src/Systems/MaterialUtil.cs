using Src.Systems;
using UnityEngine;

namespace UIForia.Rendering {

    internal static class MaterialUtil {

        private static readonly int s_SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int s_DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int s_BlendOp = Shader.PropertyToID("_BlendOp");
        private static readonly int s_ZWrite = Shader.PropertyToID("_ZWrite");
        private static readonly int s_ZTest = Shader.PropertyToID("_ZTest");

        public static void SetupState(Material material, in FixedRenderState renderState) {
            material.SetInt(s_SrcBlend, renderState.blendState.m_SourceColorBlendMode);
            material.SetInt(s_DstBlend, renderState.blendState.m_DestinationColorBlendMode);
            material.SetInt(s_BlendOp, renderState.blendState.m_ColorBlendOperation);
            material.SetInt(s_ZTest, (int) renderState.depthState.compareFunction);
            material.SetInt(s_ZWrite, renderState.depthState.writeEnabled ? 1 : 0);
        }

    }

}