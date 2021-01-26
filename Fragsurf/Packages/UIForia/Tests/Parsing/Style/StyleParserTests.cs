using System;
using System.Reflection;
using NUnit.Framework;
using UIForia;
using UIForia.Exceptions;
using UIForia.Parsing.Style;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Parsing.Style.Tokenizer;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

[TestFixture]
public class StyleParserTests {

    [Test]
    public void ParseMaterialStyle() {
        var nodes = StyleParser.Parse(@"
            style simple {
                Material = ""noise"" { 
                    shake = 4;
                };
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        StyleRootNode rootNode = ((StyleRootNode) nodes[0]);
        Assert.AreEqual("simple", rootNode.identifier);
        Assert.AreEqual(null, rootNode.tagName);
        Assert.AreEqual(1, rootNode.children.Count);
        
        var propertyNode = rootNode.children[0];
        Assert.AreEqual(StyleASTNodeType.Property, propertyNode.type);
        
        PropertyNode typedPropertyNode = (((PropertyNode) propertyNode));
        Assert.AreEqual("Material", typedPropertyNode.identifier);
        Assert.AreEqual(StyleASTNodeType.StringLiteral, typedPropertyNode.children[0].type);
    }
    
    [Test]
    public void ParseSimpleStyle() {
        var nodes = StyleParser.Parse(@"
            style simple {
                MarginTop = 10px;
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        var rootNode = ((StyleRootNode) nodes[0]);
        Assert.AreEqual("simple", rootNode.identifier);
        Assert.AreEqual(null, rootNode.tagName);
        Assert.AreEqual(1, rootNode.children.Count);

        var propertyNode = rootNode.children[0];
        Assert.AreEqual(StyleASTNodeType.Property, propertyNode.type);

        var typedPropertyNode = (((PropertyNode) propertyNode));
        Assert.AreEqual("MarginTop", typedPropertyNode.identifier);
        Assert.AreEqual(StyleASTNodeType.Measurement, typedPropertyNode.children[0].type);
    }

    [Test]
    public void ParseColorProperty() {
        var nodes = StyleParser.Parse(@"
            style withBg {
                BackgroundColor = rgba(10, 20, 30, 40);
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        var rootNode = ((StyleRootNode) nodes[0]);
        Assert.AreEqual("withBg", rootNode.identifier);
        Assert.AreEqual(null, rootNode.tagName);
        Assert.AreEqual(1, rootNode.children.Count);

        var property = (((PropertyNode) rootNode.children[0]));
        Assert.AreEqual("BackgroundColor", property.identifier);
        Assert.AreEqual(StyleASTNodeType.Rgba, property.children[0].type);

        var rgbaNode = (RgbaNode) property.children[0];
        Assert.AreEqual(StyleASTNodeType.Rgba, rgbaNode.type);
        Assert.AreEqual(StyleASTNodeFactory.NumericLiteralNode("10"), rgbaNode.red);
        Assert.AreEqual(StyleASTNodeFactory.NumericLiteralNode("20"), rgbaNode.green);
        Assert.AreEqual(StyleASTNodeFactory.NumericLiteralNode("30"), rgbaNode.blue);
        Assert.AreEqual(StyleASTNodeFactory.NumericLiteralNode("40"), rgbaNode.alpha);
    }

    [Test]
    public void ParseRgbColorProperty() {
        var nodes = StyleParser.Parse(@"
            style withBg {
                BackgroundColor = rgb(10, 20, 30);
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        var rootNode = ((StyleRootNode) nodes[0]);
        Assert.AreEqual("withBg", rootNode.identifier);
        Assert.AreEqual(null, rootNode.tagName);
        Assert.AreEqual(1, rootNode.children.Count);

        var property = (((PropertyNode) rootNode.children[0]));
        Assert.AreEqual("BackgroundColor", property.identifier);
        Assert.AreEqual(StyleASTNodeType.Rgb, property.children[0].type);

        var rgbNode = (RgbNode) property.children[0];
        Assert.AreEqual(StyleASTNodeType.Rgb, rgbNode.type);
        Assert.AreEqual(StyleASTNodeFactory.NumericLiteralNode("10"), rgbNode.red);
        Assert.AreEqual(StyleASTNodeFactory.NumericLiteralNode("20"), rgbNode.green);
        Assert.AreEqual(StyleASTNodeFactory.NumericLiteralNode("30"), rgbNode.blue);
    }

    [Test]
    public void ParseUrl() {
        var nodes = StyleParser.Parse(@"
            style withBg {
                Background = url(path/to/image);
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        var rootNode = ((StyleRootNode) nodes[0]);
        Assert.AreEqual("withBg", rootNode.identifier);
        Assert.AreEqual(null, rootNode.tagName);
        Assert.AreEqual(1, rootNode.children.Count);

        var property = (((PropertyNode) rootNode.children[0]));
        Assert.AreEqual("Background", property.identifier);
        Assert.AreEqual(StyleASTNodeType.Url, property.children[0].type);

        var urlNode = (UrlNode) property.children[0];
        Assert.AreEqual(StyleASTNodeType.Url, urlNode.type);
        Assert.AreEqual(StyleASTNodeFactory.IdentifierNode("path/to/image"), urlNode.url);
    }

    [Test]
    public void ParsePropertyWithReference() {
        var nodes = StyleParser.Parse(@"
            style hasReferenceToBackgroundImagePath {
                Background = url(@pathRef);
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        var property = (((PropertyNode) ((StyleRootNode) nodes[0]).children[0]));
        Assert.AreEqual("Background", property.identifier);
        Assert.AreEqual(StyleASTNodeType.Url, property.children[0].type);

        var urlNode = (UrlNode) property.children[0];
        Assert.AreEqual(StyleASTNodeType.Url, urlNode.type);
        Assert.AreEqual(StyleASTNodeFactory.ConstReferenceNode("pathRef"), urlNode.url);
    }

    [Test]
    public void ParseStyleState() {
        var nodes = StyleParser.Parse(@"
            style hasBackgroundOnHover {
                [hover] { Background = url(@pathRef.member); }
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        var stateGroupContainer = (((StyleStateContainer) ((StyleRootNode) nodes[0]).children[0]));
        Assert.AreEqual("hover", stateGroupContainer.identifier);

        var property = (PropertyNode) stateGroupContainer.children[0];
        Assert.AreEqual("Background", property.identifier);

        var urlNode = (UrlNode) property.children[0];
        Assert.AreEqual(StyleASTNodeType.Url, urlNode.type);
        Assert.AreEqual(StyleASTNodeType.Reference, urlNode.url.type);
        var refNode = (ConstReferenceNode) urlNode.url;
        Assert.AreEqual("pathRef", refNode.identifier);
        Assert.AreEqual(1, refNode.children.Count);
        Assert.AreEqual(StyleASTNodeType.DotAccess, refNode.children[0].type);
        var dotAccess = (DotAccessNode) refNode.children[0];
        Assert.AreEqual("member", dotAccess.propertyName);
    }

    [Test]
    public void ParseAttributeGroup() {
        var nodes = StyleParser.Parse(@"
            style hasBackgroundOnHover {
                [attr:attrName] { Background = url(@pathRef); }
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        var attributeGroupContainer = (((AttributeNodeContainer) ((StyleRootNode) nodes[0]).children[0]));
        Assert.AreEqual("attrName", attributeGroupContainer.identifier);

        var property = (PropertyNode) attributeGroupContainer.children[0];
        Assert.AreEqual("Background", property.identifier);
        Assert.AreEqual(StyleASTNodeType.Url, property.children[0].type);

        var urlNode = (UrlNode) property.children[0];
        Assert.AreEqual(StyleASTNodeFactory.ConstReferenceNode("pathRef"), urlNode.url);
    }

    [Test]
    public void ParseEmptyGroups() {
        var nodes = StyleParser.Parse(@"
            style hasBackgroundOnHover {
                [attr:attrName] { }
                [hover] {}
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual(2, ((StyleRootNode) nodes[0]).children.Count);

        var attributeGroupContainer = (((AttributeNodeContainer) ((StyleRootNode) nodes[0]).children[0]));
        var stateGroupContainer = (((StyleStateContainer) ((StyleRootNode) nodes[0]).children[1]));
        Assert.AreEqual("attrName", attributeGroupContainer.identifier);
        Assert.AreEqual(0, attributeGroupContainer.children.Count);

        Assert.AreEqual("hover", stateGroupContainer.identifier);
        Assert.AreEqual(0, stateGroupContainer.children.Count);
    }

    [Test]
    public void ParseAttributeGroupWithStateGroup() {
        var nodes = StyleParser.Parse(@"
            style mixingItAllUp {
                TextColor = green;
                [attr:attrName] { 
                    Background = url(@pathRef); 
                    [hover] {
                        TextColor = red;
                        TextColor = yellow;
                    }
                    TextColor = blue;
                }
                MarginTop = 10px;
            }
            style mixingItAllUp2 {
                TextColor = green;
                [attr:attrName] { 
                    Background = url(@pathRef); 
                    [hover] {
                        TextColor = red;
                    }
                    TextColor = blue;
                }
                MarginTop = 10px;
            }
        ");

        // there should be two style nodes
        Assert.AreEqual(2, nodes.Count);

        // ...3 nodes in a style
        var styleChildren = ((StyleRootNode) nodes[0]).children;
        Assert.AreEqual(3, styleChildren.Count);


        // first node is the property color = green
        var property1 = (PropertyNode) styleChildren[0];
        Assert.AreEqual("TextColor", property1.identifier);
        Assert.AreEqual(StyleASTNodeType.Identifier, property1.children[0].type);

        // next the attribute group that in turn has 3 children
        var attributeGroupContainer = (((AttributeNodeContainer) styleChildren[1]));
        Assert.AreEqual("attrName", attributeGroupContainer.identifier);

        // and the trailing margin property is the third of the style's properties 
        var property2 = (PropertyNode) styleChildren[2];
        Assert.AreEqual("MarginTop", property2.identifier);
        Assert.AreEqual(1, property2.children.Count);
        Assert.AreEqual(StyleASTNodeType.Measurement, property2.children[0].type);
        Assert.AreEqual("px", ((MeasurementNode) property2.children[0]).unit.value);
        Assert.AreEqual("10", ((StyleLiteralNode) ((MeasurementNode) property2.children[0]).value).rawValue);

        // now assert the existence of the three attribute group children
        Assert.AreEqual(3, attributeGroupContainer.children.Count);
        var attrProperty1 = (PropertyNode) attributeGroupContainer.children[0];
        var stateGroup = (StyleStateContainer) attributeGroupContainer.children[1];
        var attrProperty2 = (PropertyNode) attributeGroupContainer.children[2];

        // assert values for attr property 1
        Assert.AreEqual("Background", attrProperty1.identifier);

        var urlNode = (UrlNode) attrProperty1.children[0];
        Assert.AreEqual(StyleASTNodeType.Url, urlNode.type);
        Assert.AreEqual(StyleASTNodeFactory.ConstReferenceNode("pathRef"), urlNode.url);

        // assert values for attr property 2
        Assert.AreEqual("TextColor", attrProperty2.identifier);
        Assert.AreEqual(StyleASTNodeType.Identifier, attrProperty2.children[0].type);

        // assert the state group
        Assert.AreEqual("hover", stateGroup.identifier);
        // just asserting that multiple properties in a state group can be a thing
        Assert.AreEqual(2, stateGroup.children.Count);
        var stateGroupChild = (PropertyNode) stateGroup.children[0];
        Assert.AreEqual("TextColor", stateGroupChild.identifier);
        Assert.AreEqual(StyleASTNodeType.Identifier, stateGroupChild.children[0].type);
    }


    [Test]
    public void ParseExportKeyword() {
        var nodes = StyleParser.Parse(@"
            export const color0 = rgba(1, 0, 0, 1);
        ");

        // there should be two style nodes
        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual(StyleASTNodeType.Export, nodes[0].type);

        ExportNode exportNode = (ExportNode) nodes[0];
        Assert.AreEqual("color0", exportNode.constNode.constName);
        Assert.AreEqual(StyleASTNodeType.Rgba, exportNode.constNode.value.type);
    }

    [Test]
    public void ParseMultipleAttributes() {
        var nodes = StyleParser.Parse(@"
            style hasBackgroundOnHover {
                not [attr:attrName1] and not [attr:attrName2] and [attr:attrName3]{ }
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        StyleRootNode styleRootNode = (StyleRootNode) nodes[0];
        Assert.AreEqual(1, styleRootNode.children.Count);

        var attributeGroupContainer3 = (((AttributeNodeContainer) styleRootNode.children[0]));
        Assert.AreEqual("attrName3", attributeGroupContainer3.identifier);
        Assert.AreEqual(false, attributeGroupContainer3.invert);

        var attributeGroupContainer2 = attributeGroupContainer3.next;
        Assert.AreEqual("attrName2", attributeGroupContainer2.identifier);
        Assert.AreEqual(true, attributeGroupContainer2.invert);

        var attributeGroupContainer1 = attributeGroupContainer2.next;
        Assert.AreEqual("attrName1", attributeGroupContainer1.identifier);
        Assert.AreEqual(true, attributeGroupContainer1.invert);
        Assert.IsNull(attributeGroupContainer1.next);
    }

    [Test]
    public void ImportFromFile() {
        var nodes = StyleParser.Parse(@"
            import ""mypath/to/myfile"" as myconsts;
        ");

        Assert.AreEqual(1, nodes.Count);
        ImportNode importNode = (ImportNode) nodes[0];
        Assert.AreEqual("myconsts", importNode.alias);
        Assert.AreEqual("mypath/to/myfile", importNode.source);
    }

    [Test]
    public void ParseBrokenStyle() {
        try {
            StyleParser.Parse(@"
style s { BrokenUrl = url() }
            ".Trim());
            Assert.Fail("This should not have parsed!");
        }
        catch (ParseException e) {
            Assert.AreEqual(1, e.token.line);
            Assert.AreEqual(27, e.token.column);
        }
    }

    [Test]
    public void ParseEmptyAnimationVariableHeader() {
        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
            animation anim1 {
                [variables] {}
            }
        ");
        Assert.AreEqual(1, nodes.Count);
        Assert.IsInstanceOf<AnimationRootNode>(nodes[0]);
    }

    [Test]
    public void ParseAnimationVariableHeader() {
        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
            animation anim1 {
                [variables] {
                    float val = 127;
                }
            }
        ");
        Assert.AreEqual(1, nodes.Count);
        Assert.IsInstanceOf<AnimationRootNode>(nodes[0]);
        AnimationRootNode rootNode = nodes[0] as AnimationRootNode;
        Assert.AreEqual("anim1", rootNode.animName);
        Assert.AreEqual(1, rootNode.variableNodes.Count);
        VariableDefinitionNode varNode = rootNode.variableNodes[0];
        Assert.AreEqual("val", varNode.name);
        Assert.AreEqual(typeof(float), varNode.variableType);
        Assert.AreEqual(StyleASTNodeType.NumericLiteral, varNode.value.type);
    }

    [Test]
    public void ParseAnimationVariableHeaderMultipleValues() {
        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
            animation anim1 {
                [variables] {
                    float val = 127;
                    UIMeasurement measure = 0.4pca;
                    Measurement measure2 = 0.5pca;
                    UIFixedLength fm1 = 40px;
                    FixedLength fm2 = 50px;
                    TransformOffset t1 = 70px;
                    Offset t2 = 80px;
                    int i = 10;
                    Color c = #11223344;
                    int ref = @intref;
                    
                }
            }
        ");
        Assert.AreEqual(1, nodes.Count);
        Assert.IsInstanceOf<AnimationRootNode>(nodes[0]);
        AnimationRootNode rootNode = nodes[0] as AnimationRootNode;
        Assert.AreEqual("anim1", rootNode.animName);
        Assert.AreEqual(10, rootNode.variableNodes.Count);
        VariableDefinitionNode varNode0 = rootNode.variableNodes[0];
        Assert.AreEqual("val", varNode0.name);
        Assert.AreEqual(typeof(float), varNode0.variableType);
        Assert.AreEqual(StyleASTNodeType.NumericLiteral, varNode0.value.type);

        VariableDefinitionNode varNode1 = rootNode.variableNodes[1];
        Assert.AreEqual("measure", varNode1.name);
        Assert.AreEqual(typeof(UIMeasurement), varNode1.variableType);
        Assert.AreEqual(StyleASTNodeType.Measurement, varNode1.value.type);

        VariableDefinitionNode varNode2 = rootNode.variableNodes[2];
        Assert.AreEqual("measure2", varNode2.name);
        Assert.AreEqual(typeof(UIMeasurement), varNode2.variableType);
        Assert.AreEqual(StyleASTNodeType.Measurement, varNode2.value.type);

        VariableDefinitionNode varNode3 = rootNode.variableNodes[3];
        Assert.AreEqual("fm1", varNode3.name);
        Assert.AreEqual(typeof(UIFixedLength), varNode3.variableType);
        Assert.AreEqual(StyleASTNodeType.Measurement, varNode3.value.type);

        VariableDefinitionNode varNode4 = rootNode.variableNodes[4];
        Assert.AreEqual("fm2", varNode4.name);
        Assert.AreEqual(typeof(UIFixedLength), varNode4.variableType);
        Assert.AreEqual(StyleASTNodeType.Measurement, varNode4.value.type);

        VariableDefinitionNode varNode5 = rootNode.variableNodes[5];
        Assert.AreEqual("t1", varNode5.name);
        Assert.AreEqual(typeof(OffsetMeasurement), varNode5.variableType);
        Assert.AreEqual(StyleASTNodeType.Measurement, varNode5.value.type);

        VariableDefinitionNode varNode6 = rootNode.variableNodes[6];
        Assert.AreEqual("t2", varNode6.name);
        Assert.AreEqual(typeof(OffsetMeasurement), varNode6.variableType);
        Assert.AreEqual(StyleASTNodeType.Measurement, varNode6.value.type);

        VariableDefinitionNode varNode7 = rootNode.variableNodes[7];
        Assert.AreEqual("i", varNode7.name);
        Assert.AreEqual(typeof(int), varNode7.variableType);
        Assert.AreEqual(StyleASTNodeType.NumericLiteral, varNode7.value.type);

        VariableDefinitionNode varNode8 = rootNode.variableNodes[8];
        Assert.AreEqual("c", varNode8.name);
        Assert.AreEqual(typeof(Color), varNode8.variableType);
        Assert.AreEqual(StyleASTNodeType.Color, varNode8.value.type);

        VariableDefinitionNode varNode9 = rootNode.variableNodes[9];
        Assert.AreEqual("ref", varNode9.name);
        Assert.AreEqual(typeof(int), varNode9.variableType);
        Assert.AreEqual(StyleASTNodeType.Reference, varNode9.value.type);
    }

    [Test]
    public void ParseAnimationOptionHeaderEmpty() {
        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
            animation anim1 {
                [options] {}
            }
        ");
        Assert.AreEqual(1, nodes.Count);
        Assert.IsInstanceOf<AnimationRootNode>(nodes[0]);
    }

    [Test]
    public void ParseAnimationOptionsHeader() {
        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
            animation anim1 {
                [options] {
                    delay = 127;
                    loopType = constant;
                    loopTime = 100;
                    iterations = 99;
                    duration = 34;
                    forwardStartDelay = 1;
                    reverseStartDelay = 1;
                    direction = forward;
                    timingFunction = linear;
                }
            }
        ");
        Assert.AreEqual(1, nodes.Count);
        AnimationRootNode rootNode = nodes[0] as AnimationRootNode;

        AnimationOptionNode opt0 = rootNode.optionNodes[0];
        Assert.AreEqual("delay", opt0.optionName);
        Assert.AreEqual(StyleASTNodeType.NumericLiteral, opt0.value.type);

        AnimationOptionNode opt1 = rootNode.optionNodes[1];
        Assert.AreEqual("loopType", opt1.optionName);
        Assert.AreEqual(StyleASTNodeType.Identifier, opt1.value.type);

        AnimationOptionNode opt2 = rootNode.optionNodes[2];
        Assert.AreEqual("loopTime", opt2.optionName);
        Assert.AreEqual(StyleASTNodeType.NumericLiteral, opt2.value.type);

        AnimationOptionNode opt3 = rootNode.optionNodes[3];
        Assert.AreEqual("iterations", opt3.optionName);
        Assert.AreEqual(StyleASTNodeType.NumericLiteral, opt3.value.type);

        AnimationOptionNode opt4 = rootNode.optionNodes[4];
        Assert.AreEqual("duration", opt4.optionName);
        Assert.AreEqual(StyleASTNodeType.NumericLiteral, opt4.value.type);

        AnimationOptionNode opt5 = rootNode.optionNodes[5];
        Assert.AreEqual("forwardStartDelay", opt5.optionName);
        Assert.AreEqual(StyleASTNodeType.NumericLiteral, opt5.value.type);

        AnimationOptionNode opt6 = rootNode.optionNodes[6];
        Assert.AreEqual("reverseStartDelay", opt6.optionName);
        Assert.AreEqual(StyleASTNodeType.NumericLiteral, opt6.value.type);

        AnimationOptionNode opt7 = rootNode.optionNodes[7];
        Assert.AreEqual("direction", opt7.optionName);
        Assert.AreEqual(StyleASTNodeType.Identifier, opt7.value.type);

        AnimationOptionNode opt8 = rootNode.optionNodes[8];
        Assert.AreEqual("timingFunction", opt8.optionName);
        Assert.AreEqual(StyleASTNodeType.Identifier, opt8.value.type);
        
    }

    [Test]
    public void ParseKeyFrames() {
        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
            animation anim1 {
                [keyframes] {
                    0% { BackgroundColor = red; }
                    100% { BackgroundColor = green; }
                }
            }
        ");
        Assert.AreEqual(1, nodes.Count);
        AnimationRootNode rootNode = nodes[0] as AnimationRootNode;

        KeyFrameNode keyFrameNode0 = rootNode.keyframeNodes[0];
        Assert.AreEqual(0, keyFrameNode0.keyframes[0]);
        Assert.AreEqual(1, keyFrameNode0.children.Count);
        Assert.AreEqual(StyleASTNodeType.Property, keyFrameNode0.children[0].type);

        KeyFrameNode keyFrameNode1 = rootNode.keyframeNodes[1];
        Assert.AreEqual(100, keyFrameNode1.keyframes[0]);
        Assert.AreEqual(1, keyFrameNode1.children.Count);
        Assert.AreEqual(StyleASTNodeType.Property, keyFrameNode1.children[0].type);
    }

    [Test]
    public void ParseKeyFramesMultipleProperties() {
        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
            animation anim1 {
                [keyframes] {
                    0% { 
                        BackgroundColor = red; 
                        BackgroundColor = red; 
                    }
                    50% {
                        TextFontSize = 11;
                    }
                    100% {
                         BackgroundColor = green; 
                         BackgroundColor = green; 
                    }
                }
            }
        ");
        Assert.AreEqual(1, nodes.Count);
        AnimationRootNode rootNode = nodes[0] as AnimationRootNode;

        KeyFrameNode keyFrameNode0 = rootNode.keyframeNodes[0];
        Assert.AreEqual(0, keyFrameNode0.keyframes[0]);
        Assert.AreEqual(2, keyFrameNode0.children.Count);
        Assert.AreEqual(StyleASTNodeType.Property, keyFrameNode0.children[0].type);
        Assert.AreEqual(StyleASTNodeType.Property, keyFrameNode0.children[1].type);

        KeyFrameNode keyFrameNode1 = rootNode.keyframeNodes[1];
        Assert.AreEqual(50, keyFrameNode1.keyframes[0]);
        Assert.AreEqual(1, keyFrameNode1.children.Count);
        Assert.AreEqual(StyleASTNodeType.Property, keyFrameNode1.children[0].type);

        KeyFrameNode keyFrameNode2 = rootNode.keyframeNodes[2];
        Assert.AreEqual(100, keyFrameNode2.keyframes[0]);
        Assert.AreEqual(2, keyFrameNode2.children.Count);
        Assert.AreEqual(StyleASTNodeType.Property, keyFrameNode2.children[0].type);
        Assert.AreEqual(StyleASTNodeType.Property, keyFrameNode2.children[1].type);
    }

    private class StyleNodeTestDef {

        public Type type;
        public string identifier;
        public StyleNodeTestDef[] children;
        public StyleASTNodeType nodeType;
        public string rawValue;
        public StyleNodeTestDef value;
        public StyleNodeTestDef unit;

        public static StyleNodeTestDef CreateMeasurementNode(string value, string unit) {
            return new StyleNodeTestDef() {
                type = typeof(MeasurementNode),
                value = new StyleNodeTestDef() {
                    type = typeof(StyleLiteralNode),
                    nodeType = StyleASTNodeType.NumericLiteral,
                    rawValue = value
                },
                unit = new StyleNodeTestDef() {
                    type = typeof(UnitNode),
                    nodeType = StyleASTNodeType.Unit,
                    rawValue = unit
                }
            };
        }

    }

    private static void AssertStyleNodesEqual(StyleNodeTestDef expected, StyleASTNode actual) {
        Assert.AreEqual(expected.type, actual.GetType());
        if (expected.identifier != null) {
            if (actual is StyleNodeContainer n) {
                Assert.AreEqual(expected.identifier, n.identifier);
            }
            else if (actual is StyleIdentifierNode id) {
                Assert.AreEqual(expected.identifier, id.name);
            }
            else {
                Assert.IsTrue(false, $"Expected node to have an identifier {expected.identifier} but {actual} did not");
            }
        }

        if (expected.nodeType != 0) {
            Assert.AreEqual(expected.nodeType, actual.type);
        }

        if (expected.type == typeof(MeasurementNode)) {
            AssertMeasurementNode(expected, actual as MeasurementNode);
        }


        if (expected.rawValue != null) {
            FieldInfo fieldInfo = null;
            if (ReflectionUtil.IsField(actual.GetType(), "rawValue", out fieldInfo) || ReflectionUtil.IsField(actual.GetType(), "value", out fieldInfo)) {
                Assert.AreEqual(expected.rawValue, fieldInfo.GetValue(actual));
            }
            else {
                Assert.IsTrue(false, $"Expected {actual} to have a value or rawValue field, it did not");
            }
        }

        if (expected.children != null) {
            if (actual is StyleNodeContainer c) {
                Assert.AreEqual(expected.children.Length, c.children.Count);

                for (int i = 0; i < expected.children.Length; i++) {
                    AssertStyleNodesEqual(expected.children[i], c.children[i]);
                }
            }
            else {
                Assert.IsTrue(false, $"Expected node to have children but {actual} is not a StyleContainer");
            }
        }
    }

    private static void AssertMeasurementNode(StyleNodeTestDef expected, MeasurementNode actual) {
        AssertStyleNodesEqual(expected.value, actual.value);
        AssertStyleNodesEqual(expected.unit, actual.unit);
    }

    [Test]
    public void ParseGridTemplateComplex() {
        LightList<StyleASTNode> nodes = StyleParser.Parse(@"
            style grid-thing {
                GridLayoutRowTemplate = repeat(4, 1mx) repeat(auto-fill, grow(1cnt, 300px) 300px) repeat(2, shrink(200px, 1fr));
            }
        ");

        Assert.AreEqual(1, nodes.Count);
        StyleRootNode rootNode = nodes[0] as StyleRootNode;
        Assert.AreEqual(1, rootNode.children.Count);
        PropertyNode propertyNode0 = (PropertyNode) rootNode.children[0];

        Assert.AreEqual(3, propertyNode0.children.Count);

        Assert.IsInstanceOf<StyleFunctionNode>(propertyNode0.children[0]);
        StyleFunctionNode repeat0 = propertyNode0.children[0] as StyleFunctionNode;
        StyleFunctionNode repeat1 = propertyNode0.children[1] as StyleFunctionNode;
        StyleFunctionNode repeat2 = propertyNode0.children[2] as StyleFunctionNode;

        AssertStyleNodesEqual(new StyleNodeTestDef() {
            type = typeof(StyleFunctionNode),
            identifier = "repeat",
            children = new[] {
                new StyleNodeTestDef() {
                    type = typeof(StyleLiteralNode),
                    nodeType = StyleASTNodeType.NumericLiteral,
                    rawValue = "4"
                },
                StyleNodeTestDef.CreateMeasurementNode("1", "mx")
            }
        }, repeat0);

        AssertStyleNodesEqual(new StyleNodeTestDef() {
            type = typeof(StyleFunctionNode),
            identifier = "repeat",
            children = new[] {
                new StyleNodeTestDef() {
                    type = typeof(StyleIdentifierNode),
                    nodeType = StyleASTNodeType.Identifier,
                    identifier = "auto-fill"
                },
                new StyleNodeTestDef() {
                    type = typeof(StyleFunctionNode),
                    identifier = "grow",
                    children = new[] {
                        StyleNodeTestDef.CreateMeasurementNode("1", "cnt"),
                        StyleNodeTestDef.CreateMeasurementNode("300", "px")
                    }
                },
                StyleNodeTestDef.CreateMeasurementNode("300", "px")
            }
        }, repeat1);

        AssertStyleNodesEqual(new StyleNodeTestDef() {
            type = typeof(StyleFunctionNode),
            identifier = "repeat",
            children = new[] {
                new StyleNodeTestDef() {
                    type = typeof(StyleLiteralNode),
                    nodeType = StyleASTNodeType.NumericLiteral,
                    rawValue = "2"
                },
                new StyleNodeTestDef() {
                    type = typeof(StyleFunctionNode),
                    identifier = "shrink",
                    children = new[] {
                        StyleNodeTestDef.CreateMeasurementNode("200", "px"),
                        StyleNodeTestDef.CreateMeasurementNode("1", "fr")
                    }
                }
            }
        }, repeat2);
        
    }

}