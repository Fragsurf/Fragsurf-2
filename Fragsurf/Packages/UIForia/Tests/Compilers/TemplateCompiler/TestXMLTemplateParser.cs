using NUnit.Framework;
using UIForia.Compilers;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Util;
using Assert = UnityEngine.Assertions.Assert;

[TestFixture]
public class TestXMLTemplateParser {

    [Test]
    public void ProcessText_Constant() {
        const string input = "this is a normal piece of text";
        StructList<TextExpression> output = new StructList<TextExpression>();
        TextTemplateProcessor.ProcessTextExpressions(input, output);
        Assert.AreEqual(1, output.size);
        Assert.AreEqual(input, output[0].text);
        Assert.AreEqual(false, output[0].isExpression);
    }
    
    [Test]
    public void ProcessText_ConstantWithWhiteSpace() {
        const string input = "\n\n\tthis is a normal piece of text";
        StructList<TextExpression> output = new StructList<TextExpression>();
        TextTemplateProcessor.ProcessTextExpressions(input, output);
        Assert.AreEqual(input, output[0].text);
        Assert.AreEqual(false, output[0].isExpression);
    }
    
    [Test]
    public void ProcessText_Expression() {
        const string input = "{some expression}";
        StructList<TextExpression> output = new StructList<TextExpression>();
        TextTemplateProcessor.ProcessTextExpressions(input, output);
        Assert.AreEqual(1, output.size);
        Assert.AreEqual("some expression", output[0].text);
        Assert.IsTrue(output[0].isExpression);
    }
    
    [Test]
    public void ProcessText_ExpressionWithWhitespace() {
        const string input = "\n\n\n\n{some expression}";
        StructList<TextExpression> output = new StructList<TextExpression>();
        TextTemplateProcessor.ProcessTextExpressions(input, output);
        Assert.AreEqual(2, output.size);
        Assert.AreEqual("\n\n\n\n", output[0].text);
        Assert.AreEqual(false, output[0].isExpression);
        Assert.AreEqual("some expression", output[1].text);
        Assert.AreEqual(true, output[1].isExpression);
    }
    
    [Test]
    public void ProcessText_ExpressionWithNestedBraces() {
        const string input = "\n\n\n\n{some expression{}}";
        StructList<TextExpression> output = new StructList<TextExpression>();
        TextTemplateProcessor.ProcessTextExpressions(input, output);
        Assert.AreEqual(2, output.size);
        Assert.AreEqual("\n\n\n\n", output[0].text);
        Assert.IsFalse(output[0].isExpression);
        Assert.AreEqual("some expression{}", output[1].text);
        Assert.IsTrue(output[1].isExpression);
    }
    
    [Test]
    public void ProcessText_EscapedBrace() {
        const string input = "&obrc;";
        StructList<TextExpression> output = new StructList<TextExpression>();
        TextTemplateProcessor.ProcessTextExpressions(input, output);
        Assert.AreEqual(1, output.size);
        Assert.AreEqual("{", output[0].text);
        Assert.IsFalse(output[0].isExpression);
    }
    
    [Test]
    public void ProcessText_EscapedBracePair() {
        const string input = "&obrc;stuff&cbrc;";
        StructList<TextExpression> output = new StructList<TextExpression>();
        TextTemplateProcessor.ProcessTextExpressions(input, output);
        Assert.AreEqual(1, output.size);
        Assert.AreEqual("{stuff}", output[0].text);
        Assert.IsFalse(output[0].isExpression);
    }
    
    [Test]
    public void ProcessText_MultipleExpressions() {
        const string input = "{hello}{there}";
        StructList<TextExpression> output = new StructList<TextExpression>();
        TextTemplateProcessor.ProcessTextExpressions(input, output);
        Assert.AreEqual(2, output.size);
        Assert.AreEqual("hello", output[0].text);
        Assert.AreEqual("there", output[1].text);
        Assert.IsTrue(output[0].isExpression);
        Assert.IsTrue(output[1].isExpression);
    }
    
    [Test]
    public void ProcessText_MultipleExpressionsWithConstants() {
        const string input = "{hello}{'there'}";
        StructList<TextExpression> output = new StructList<TextExpression>();
        TextTemplateProcessor.ProcessTextExpressions(input, output);
        Assert.AreEqual(2, output.size);
        Assert.AreEqual("hello", output[0].text);
        Assert.AreEqual("'there'", output[1].text);
        Assert.IsTrue(output[0].isExpression);
        Assert.IsTrue(output[1].isExpression);
    }
    
    [Test]
    public void ProcessText_MultipleExpressionsMixed() {
        const string input = "{hello}hi there{'there'}";
        StructList<TextExpression> output = new StructList<TextExpression>();
        TextTemplateProcessor.ProcessTextExpressions(input, output);
        Assert.AreEqual(3, output.size);
        Assert.AreEqual("hello", output[0].text);
        Assert.AreEqual("hi there", output[1].text);
        Assert.AreEqual("'there'", output[2].text);
        Assert.IsTrue(output[0].isExpression);
        Assert.IsFalse(output[1].isExpression);
        Assert.IsTrue(output[2].isExpression);
    }
    
}