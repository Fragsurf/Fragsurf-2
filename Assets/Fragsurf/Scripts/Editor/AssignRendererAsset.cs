using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad] 
public class AssignRendererAsset
{

    static AssignRendererAsset()
    {
        EditorApplication.update -= CheckUrpAsset;
        EditorApplication.update += CheckUrpAsset;
    }

    static void CheckUrpAsset()
    {
        if(!GraphicsSettings.renderPipelineAsset)
        {
            GraphicsSettings.renderPipelineAsset = Resources.Load<RenderPipelineAsset>("UniversalRenderPipelineAsset");
            EditorApplication.update -= CheckUrpAsset;
        }
    }

}
