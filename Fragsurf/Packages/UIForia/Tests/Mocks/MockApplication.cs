using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Src.Systems;
using UIForia;
using UIForia.Animation;
using UIForia.Elements;
using UIForia.Routing;
using UIForia.Systems;
using UnityEngine;
using Application = UIForia.Application;

namespace Tests.Mocks {

    public class MockApplication : Application {

        public static bool s_GenerateCode;
        public static bool s_UsePreCompiledTemplates;

        protected MockApplication(bool isPreCompiled, TemplateSettings templateData, ResourceManager resourceManager, Action<UIElement> onRegister) : base(isPreCompiled, templateData, resourceManager, onRegister) { }

        protected override void CreateSystems() {
            styleSystem = new StyleSystem();
            layoutSystem = new MockLayoutSystem(this);
            inputSystem = new MockInputSystem(layoutSystem);
            renderSystem = new MockRenderSystem(Camera, this);
            routingSystem = new RoutingSystem();
            animationSystem = new AnimationSystem();
            linqBindingSystem = new LinqBindingSystem();
        }

        public static void Generate(bool shouldGenerate = true) {
            s_GenerateCode = shouldGenerate;
            s_UsePreCompiledTemplates = shouldGenerate;
        }
        
        public static TemplateSettings GetDefaultSettings<T>(string appName) {
            TemplateSettings settings = new TemplateSettings();
            settings.applicationName = appName;
            settings.assemblyName = typeof(MockApplication).Assembly.GetName().Name;
            settings.outputPath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests", "UIForiaGenerated");
            settings.codeFileExtension = "generated.xml.cs";
            settings.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath, "..", "Packages", "UIForia", "Tests");
            settings.rootType = typeof(T);
            return settings;
        }

        public static MockApplication Setup<T>(TemplateSettings settings = null) where T : UIElement {
            string appName = settings?.applicationName;
            if (appName == null) {
                StackTrace stackTrace = new StackTrace();
                appName = stackTrace.GetFrame(1).GetMethod().Name;
            }

            if (settings == null) {
                settings = GetDefaultSettings<T>(appName);
            }

            if (s_GenerateCode) {
                TemplateCodeGenerator.Generate(typeof(T), settings);
            }

            MockApplication app = new MockApplication(s_UsePreCompiledTemplates, settings, null, null);
            app.Initialize();
            return app;
        }

         public static MockApplication Setup(TemplateSettings settings,  bool usePreCompiledTemplates = false) {
            MockApplication app = new MockApplication(usePreCompiledTemplates, settings, null, null);
            app.Initialize();
            return app;
        }
         
        public new MockInputSystem InputSystem => (MockInputSystem) inputSystem;
        public UIElement RootElement => views[0].RootElement;

        public void SetViewportRect(Rect rect) {
            views[0].Viewport = rect;
        }

    }

    public class MockRenderSystem : VertigoRenderSystem {

        public override void OnUpdate() {
            // do nothing
        }

        public MockRenderSystem(Camera camera, Application application) : base(camera, application) { }

    }

}