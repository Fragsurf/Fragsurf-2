using System.IO;
using NUnit.Framework;
using Tests.Mocks;
using UIForia;
using UIForia.Compilers.Style;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Util;
using Application = UnityEngine.Application;

[TestFixture]
public class StyleSheetConstantImporterTests {

    [Test]
    public void CreateContextWithMultipleConstants() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();
        nodes.Add(StyleASTNodeFactory.ExportNode(StyleASTNodeFactory.ConstNode("col0", StyleASTNodeFactory.ColorNode("red"))));
        nodes.Add(StyleASTNodeFactory.ExportNode(StyleASTNodeFactory.ConstNode("thing0", StyleASTNodeFactory.StringLiteralNode("someVal"))));
        nodes.Add(StyleASTNodeFactory.ExportNode(StyleASTNodeFactory.ConstNode("number", StyleASTNodeFactory.NumericLiteralNode("1"))));

        var context = new StyleSheetConstantImporter(new StyleSheetImporter(null, null)).CreateContext(nodes, default);

        Assert.AreEqual(3, context.constants.Count);
        Assert.AreEqual("col0", context.constants[0].name);
        Assert.True(context.constants[0].exported);
        
        Assert.AreEqual("thing0", context.constants[1].name);
        Assert.True(context.constants[1].exported);
        
        Assert.AreEqual("number", context.constants[2].name);
        Assert.True(context.constants[2].exported);
    }

    [Test]
    public void CreateContextWithReferences() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();
        nodes.Add(StyleASTNodeFactory.ExportNode(StyleASTNodeFactory.ConstNode("x", StyleASTNodeFactory.ConstReferenceNode("y"))));
        nodes.Add(StyleASTNodeFactory.ExportNode(StyleASTNodeFactory.ConstNode("y", StyleASTNodeFactory.ConstReferenceNode("z"))));
        var stringValue = StyleASTNodeFactory.StringLiteralNode("you win!");
        nodes.Add(StyleASTNodeFactory.ExportNode(StyleASTNodeFactory.ConstNode("z", stringValue)));

        var context = new StyleSheetConstantImporter(new StyleSheetImporter(null, null)).CreateContext(nodes, default);

        Assert.AreEqual(3, context.constants.Count);
        
        Assert.AreEqual("x", context.constants[2].name);
        Assert.AreEqual(stringValue, context.constants[2].value);
        Assert.True(context.constants[2].exported);
        
        Assert.AreEqual("y", context.constants[1].name);
        Assert.AreEqual(stringValue, context.constants[1].value);
        Assert.True(context.constants[1].exported);
        
        Assert.AreEqual("z", context.constants[0].name);
        Assert.AreEqual(stringValue, context.constants[0].value);
        Assert.True(context.constants[0].exported);
        
        Assert.AreEqual(0, context.constantsWithReferences.Count, "There should be no unresolved const left.");
    }

    [Test]
    public void ImportAndUseConsts() {
        LightList<StyleASTNode> nodes = new LightList<StyleASTNode>();
        nodes.Add(StyleASTNodeFactory.ImportNode("importedThing", "Data/Styles/ImportFromMe.style"));
        TemplateSettings templateSettings = new TemplateSettings {
            templateResolutionBasePath = Path.Combine(Application.dataPath, "..", "Packages", "UIForia", "Tests")
        };
        StyleCompileContext context = new StyleSheetConstantImporter(new StyleSheetImporter(templateSettings, null)).CreateContext(nodes, default);
        Assert.AreEqual(1, context.importedStyleConstants.Count);
        Assert.AreEqual(1, context.importedStyleConstants["importedThing"].Count);
        Assert.AreEqual("colorRed", context.importedStyleConstants["importedThing"][0].name);
    }
}
