
using System;
using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;

using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Substance.EditorHelper
{
    public class Scripter
    {
        public static void Hello()
        {
            Debug.Log("Scripter's Hello");

#if UNITY_2019_3_OR_NEWER
            Debug.Log("UNITY_2019_3_OR_NEWER");
#endif
        }

        public static class UnityPipeline
        {
            // The active project context is in the 'Edit->Project Settings->Graphics->Scriptable Render Pipeline Settings' field.
            public static bool IsHDRP()
            {
#if UNITY_2019_3_OR_NEWER
            bool bActive = false;

            UnityEngine.Rendering.RenderPipelineAsset asset;
            asset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;

            if ((asset != null) &&
                (asset.GetType().ToString().EndsWith(".HDRenderPipelineAsset")))
            {
                    bActive = true;
            }

            return bActive;
#else
                return false;
#endif
            }

            public static bool IsURP()
            {
#if UNITY_2019_3_OR_NEWER
            bool bActive = false;

            UnityEngine.Rendering.RenderPipelineAsset asset;
            asset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;

            if ((asset != null) &&
                (asset.GetType().ToString().EndsWith(".UniversalRenderPipelineAsset")))
            {
                bActive = true;
            }

            return bActive;
#else
                return false;
#endif
            }
        }
    }
}