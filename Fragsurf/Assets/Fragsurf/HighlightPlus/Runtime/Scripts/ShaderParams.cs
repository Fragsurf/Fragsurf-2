using UnityEngine;

namespace HighlightPlus {

    public static class ShaderParams {

        // general uniforms
        public static int StereoRendering = Shader.PropertyToID("_StereoRendering");
        public static int Cull = Shader.PropertyToID("_Cull");
        public static int BlurScale = Shader.PropertyToID("_BlurScale");
        public static int Speed = Shader.PropertyToID("_Speed");
        public static int ConstantWidth = Shader.PropertyToID("_ConstantWidth");
        public static int CutOff = Shader.PropertyToID("_CutOff");
        public static int ZTest = Shader.PropertyToID("_ZTest");
        public static int Flip = Shader.PropertyToID("_Flip");
        public static int Debug = Shader.PropertyToID("_Debug");
        public static int Color = Shader.PropertyToID("_Color");
        public static int MainTex = Shader.PropertyToID("_MainTex");
        public static int BaseMap = Shader.PropertyToID("_BaseMap");

        // outline uniforms
        public static int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
        public static int OutlineZTest = Shader.PropertyToID("_OutlineZTest");
        public static int OutlineDirection = Shader.PropertyToID("_OutlineDirection");
        public static int OutlineColor = Shader.PropertyToID("_OutlineColor");

        // glow uniforms
        public static int GlowZTest = Shader.PropertyToID("_GlowZTest");
        public static int GlowStencilOp = Shader.PropertyToID("_GlowStencilOp");
        public static int GlowDirection = Shader.PropertyToID("_GlowDirection");
        public static int Glow = Shader.PropertyToID("_Glow");
        public static int GlowColor = Shader.PropertyToID("_GlowColor");
        public static int Glow2 = Shader.PropertyToID("_Glow2");

        // see-through uniforms
        public static int SeeThrough = Shader.PropertyToID("_SeeThrough");
        public static int SeeThroughNoise = Shader.PropertyToID("_SeeThroughNoise");
        public static int SeeThroughBorderWidth = Shader.PropertyToID("_SeeThroughBorderWidth");
        public static int SeeThroughBorderConstantWidth = Shader.PropertyToID("_SeeThroughBorderConstantWidth");
        public static int SeeThroughTintColor = Shader.PropertyToID("_SeeThroughTintColor");
        public static int SeeThroughBorderColor = Shader.PropertyToID("_SeeThroughBorderColor");

        // inner glow uniforms
        public static int InnerGlowWidth = Shader.PropertyToID("_InnerGlowWidth");
        public static int InnerGlowZTest = Shader.PropertyToID("_InnerGlowZTest");

        // overlay uniforms
        public static int OverlayData = Shader.PropertyToID("_OverlayData");
        public static int OverlayBackColor = Shader.PropertyToID("_OverlayBackColor");

        // keywords
        public const string SKW_ALPHACLIP = "HP_ALPHACLIP";
        public const string SKW_DEPTHCLIP = "HP_DEPTHCLIP";

    }
}

