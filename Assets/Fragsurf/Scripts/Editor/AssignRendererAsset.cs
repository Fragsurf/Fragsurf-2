using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class AssignRendererAsset
{

    static AssignRendererAsset()
    {
        GraphicsSettings.renderPipelineAsset = Resources.Load<RenderPipelineAsset>("UniversalRenderPipelineAsset");
    }

}
