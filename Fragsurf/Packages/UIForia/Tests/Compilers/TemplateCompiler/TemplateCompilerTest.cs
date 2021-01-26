using System.IO;
using System.Linq.Expressions;
using Mono.Linq.Expressions;
using NUnit.Framework;
using UIForia;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Elements;

namespace Tests {

    [Template("Data/TemplateCompiler/TemplateCompilerTest_DotAccess.xml")]
    public class TestDotAccess : UIElement {
        public class InnerAccess {
            public string value;
        }

        public InnerAccess left = new InnerAccess() { value = "left value"};
        public InnerAccess right = null;
    }

    public class TemplateCompilerTest {

        [Test]
        public void CompileNullableDotAccess() {
            var compiledTemplateData = TemplateLoader.LoadRuntimeTemplates(typeof(TestDotAccess), new TemplateSettings() {
                assemblyName = "UIForia.Test",
                rootType = typeof(TestDotAccess),
                templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath, "../Packages/UIForia/Tests")
            });
            Assert.NotNull(compiledTemplateData);
            Expression bindingFn = compiledTemplateData.compiledTemplates[0].bindings[0].bindingFn;
            TestUtils.AssertStringsEqual(@"
                (UIForia.Elements.UIElement __root, UIForia.Elements.UIElement __element) =>
                {
                    UIForia.Elements.UITextElement __castElement;
                    Tests.TestDotAccess __castRoot;
                    Tests.TestDotAccess.InnerAccess nullCheck;
                    Tests.TestDotAccess.InnerAccess nullCheck_0;

                    __castElement = ((UIForia.Elements.UITextElement)__element);
                    UIForia.Util.StringUtil.s_CharStringBuilder.Clear();
                    __castRoot = ((Tests.TestDotAccess)__root);
                    nullCheck = __castRoot.left;
                    if (nullCheck == null)
                    {
                        goto section_0_0;
                    }
                    UIForia.Util.StringUtil.s_CharStringBuilder.Append(nullCheck.value);
                section_0_0:
                    UIForia.Util.StringUtil.s_CharStringBuilder.Append(@"" vs "");
                    nullCheck_0 = __castRoot.right;
                    if (nullCheck_0 == null)
                    {
                        goto section_0_1;
                    }
                    UIForia.Util.StringUtil.s_CharStringBuilder.Append(nullCheck_0.value);
                section_0_1:
                    if (UIForia.Util.StringUtil.s_CharStringBuilder.EqualsString(__castElement.text) == false)
                    {
                        __castElement.SetText(UIForia.Util.StringUtil.s_CharStringBuilder.ToString());
                    }
                    UIForia.Util.StringUtil.s_CharStringBuilder.Clear();
                retn:
                    return;
                }
            ", bindingFn.ToCSharpCode());
        }
    }
}