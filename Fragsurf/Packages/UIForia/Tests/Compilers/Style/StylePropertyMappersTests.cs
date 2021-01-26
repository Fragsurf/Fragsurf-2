using System;
using NUnit.Framework;
using UIForia;
using UIForia.Compilers.Style;
using UIForia.Exceptions;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Rendering;

[TestFixture]
public class StylePropertyMappersTests {

    [Test]
    public void AssertAllStylePropertiesAreMapped() {
        foreach (var propId in Enum.GetValues(typeof(StylePropertyId))) {
            
            if (propId.ToString().StartsWith("__")) continue;
            
            var propertyNode = StyleASTNodeFactory.PropertyNode(propId.ToString());
            // this node should fail in a compile exception if this property is mapped.
            propertyNode.children.Add(new StyleRootNode());
            UIStyle target = new UIStyle();

            try {
                StylePropertyMappers.MapProperty(target, propertyNode, new StyleCompileContext(default));
                Assert.Fail($"Property {propId} is probably not mapped. Have a look!");
            }
            catch (CompileException) {
                // expected that
            }
        }
    }
}
