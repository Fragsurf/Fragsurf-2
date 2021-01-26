//using System.IO;
//using NUnit.Framework;
//using Tests;
//using Tests.Mocks;
//using UIForia;
//using UIForia.Compilers;
//using UIForia.Elements;
//using UIForia.Rendering;
//using UIForia.Test.TestData;
//using Application = UnityEngine.Application;
//
//[TestFixture]
//public class TestTemplateLoading {
//
//    public TemplateSettings Setup(string appName) {
//        TemplateSettings settings = new TemplateSettings();
//        settings.applicationName = appName;
//        settings.assemblyName = GetType().Assembly.GetName().Name;
//        settings.outputPath = Path.Combine(Application.dataPath, "..", "Packages", "UIForia", "Tests", "UIForiaGenerated");
//        settings.codeFileExtension = "cs";
//        settings.preCompiledTemplatePath = "Assets/UIForia_Generated/" + appName;
//        settings.templateResolutionBasePath = Path.Combine(Application.dataPath, "..", "Packages", "UIForia", "Tests");
//        return settings;
//    }
//
//    public CompiledTemplateData GetTemplateData<T>(string appName) {
//        TemplateSettings settings = Setup(appName);
//
//        TemplateCompiler compiler = new TemplateCompiler(settings);
//
//        // maybe this should also know the root type for an application
////        CompiledTemplateData compiledOutput = new RuntimeTemplateData(settings);
////        CompiledTemplateData compiledOutput = new PreCompiledTemplateData(settings);
//
////        compiler.CompileTemplates(typeof(T), compiledOutput);
//
////        if (compiledOutput is PreCompiledTemplateData preCompiledTemplateData) {
////            preCompiledTemplateData.GenerateCode();
////        }
//
//        compiledOutput.LoadTemplates();
//
//        return compiledOutput;
//    }
//
//    [Test]
//    public void LoadTemplateFromFile() {
//        CompiledTemplateData templates = GetTemplateData<LoadTemplate0>(nameof(LoadTemplateFromFile));
//
//        MockApplication app = new MockApplication(templates, null);
//
//        LoadTemplate0 root = ElementTestUtil.AssertElementType<LoadTemplate0>(app.GetView(0).RootElement);
//
//        Assert.AreEqual(2, root.ChildCount);
//        Assert.IsInstanceOf<UITextElement>(root.GetChild(0));
//
//        LoadTemplateHydrate hydrate = ElementTestUtil.AssertElementType<LoadTemplateHydrate>(root.GetChild(1));
//
//        ElementTestUtil.AssertHasAttribute(hydrate, "nonconst");
//        ElementTestUtil.AssertHasAttribute(hydrate, "some-attr", "this-is-attr");
//        ElementTestUtil.AssertHasAttribute(hydrate, "first", "true");
//
//        Assert.AreEqual(42f, hydrate.floatVal);
//
//        app.Update();
//
//        Assert.AreEqual(14244 + 144, hydrate.intVal);
//        Assert.AreEqual(hydrate.style.PreferredWidth, new UIMeasurement(300));
//    }
//
//}