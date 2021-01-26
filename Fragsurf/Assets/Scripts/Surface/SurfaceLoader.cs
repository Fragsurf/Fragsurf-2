using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UIForia;
using UIForia.Elements;
using UnityEngine;

namespace Bhop
{
    public class SurfaceLoader : MonoBehaviour
    {

        [SerializeField]
        private Camera _camera;
        private UIForia.Application _application;
        private string _appName = "Bhop";

        private void Start()
        {
            _application = GameApplication.CreateFromRuntimeTemplates(GetTemplateSettings(typeof(TestTemplate)), _camera, DoDependencyInjection);
            _application.SetScreenSize(Screen.width, Screen.height);
        }

        private void Update()
        {
            _application?.Update();
            _application?.GetView(0).SetSize((int)_application.Width, (int)_application.Height);
        }

        private void OnDestroy()
        {
            _application?.Destroy();
        }

        private void DoDependencyInjection(UIElement element)
        {
            // DiContainer.Inject(element);
        }

        private TemplateSettings GetTemplateSettings(Type type)
        {
            var result = new TemplateSettings();
            result.rootType = type;
            result.applicationName = _appName;
            result.assemblyName = "Assembly-CSharp";
            result.outputPath = Path.Combine(UnityEngine.Application.dataPath, "UIForiaGenerated2");
            result.codeFileExtension = "generated.cs";
            result.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath, "User Interface");
            result.styleBasePath = string.Empty;

            return result;
        }

    }
}

