using System;
using System.Collections.Generic;
using NUnit.Framework;
using UIForia;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Parsing.Expressions.AstNodes;
using UnityEngine;
using static Tests.TestUtils;

[TestFixture]
public class ExpressionParserTests {

    [Test]
    public void ParseLiteralValueRoot() {
        ASTNode root = ExpressionParser.Parse("5");
        Assert.IsInstanceOf<LiteralNode>(root);
        LiteralNode n = (LiteralNode) root;
        Assert.AreEqual("5", n.rawValue);

        root = ExpressionParser.Parse("-5.6");
        Assert.IsInstanceOf<LiteralNode>(root);
        LiteralNode doubleNode0 = (LiteralNode) root;
        Assert.AreEqual("-5.6", doubleNode0.rawValue);

        root = ExpressionParser.Parse("'hello'");
        Assert.IsInstanceOf<LiteralNode>(root);
        LiteralNode n2 = (LiteralNode) root;
        Assert.AreEqual("hello", n2.rawValue);

        root = ExpressionParser.Parse("'hello ' ");
        Assert.IsInstanceOf<LiteralNode>(root);
        n2 = (LiteralNode) root;
        Assert.AreEqual("hello ", n2.rawValue);

        root = ExpressionParser.Parse("true");
        Assert.IsInstanceOf<LiteralNode>(root);
        LiteralNode n3 = (LiteralNode) root;
        Assert.AreEqual("true", n3.rawValue);

        root = ExpressionParser.Parse(" false");
        Assert.IsInstanceOf<LiteralNode>(root);
        n3 = (LiteralNode) root;
        Assert.AreEqual("false", n3.rawValue);

        root = ExpressionParser.Parse(" default");
        Assert.IsInstanceOf<LiteralNode>(root);
        n3 = (LiteralNode) root;
        Assert.AreEqual("default", n3.rawValue);

        root = ExpressionParser.Parse(" null");
        Assert.IsInstanceOf<LiteralNode>(root);
        n3 = (LiteralNode) root;
        Assert.AreEqual("null", n3.rawValue);
    }

    [Test]
    public void Parse_OperatorExpression_Literals() {
        ASTNode root = ExpressionParser.Parse("5 + 6");
        Assert.IsInstanceOf<OperatorNode>(root);
        OperatorNode opEx = (OperatorNode) root;
        Assert.AreEqual(OperatorType.Plus, opEx.operatorType);
        Assert.IsInstanceOf<LiteralNode>(opEx.left);
        Assert.IsInstanceOf<LiteralNode>(opEx.right);
        LiteralNode left = (LiteralNode) opEx.left;
        LiteralNode right = (LiteralNode) opEx.right;
        Assert.AreEqual("5", left.rawValue);
        Assert.AreEqual("6", right.rawValue);
    }

    [Test]
    public void Parse_OperatorExpression_NegatedLiterals() {
        ASTNode root = ExpressionParser.Parse("5 + -6");
        Assert.IsInstanceOf<OperatorNode>(root);
        OperatorNode opEx = (OperatorNode) root;
        Assert.AreEqual(OperatorType.Plus, opEx.operatorType);
        Assert.IsInstanceOf<LiteralNode>(opEx.left);
        Assert.IsInstanceOf<LiteralNode>(opEx.right);
        LiteralNode left = (LiteralNode) opEx.left;
        LiteralNode right = (LiteralNode) opEx.right;
        Assert.AreEqual("5", left.rawValue);
        Assert.AreEqual("-6", right.rawValue);
    }

    [Test]
    public void Parse_NegatedLiterals() {
        ASTNode root = ExpressionParser.Parse("-6");
        Assert.IsInstanceOf<LiteralNode>(root);
        LiteralNode node = (LiteralNode) root;
        Assert.AreEqual("-6", node.rawValue);
    }

    [Test]
    public void Parse_PositiveLiterals() {
        ASTNode root = ExpressionParser.Parse("+6");
        Assert.IsInstanceOf<LiteralNode>(root);
        LiteralNode node = (LiteralNode) root;
        Assert.AreEqual("6", node.rawValue);
    }

    [Test]
    public void Parse_OperatorExpression_MultipleLiterals() {
        ASTNode root = ExpressionParser.Parse("5 + 6 * 7");
        Assert.IsInstanceOf<OperatorNode>(root);
        OperatorNode opEx = (OperatorNode) root;
        Assert.AreEqual(OperatorType.Plus, opEx.operatorType);
        LiteralNode v0 = (LiteralNode) opEx.left;
        Assert.IsInstanceOf<OperatorNode>(opEx.right);
        opEx = (OperatorNode) opEx.right;
        Assert.AreEqual(OperatorType.Times, opEx.operatorType);
        LiteralNode v1 = (LiteralNode) opEx.left;
        LiteralNode v2 = (LiteralNode) opEx.right;
        Assert.AreEqual("5", v0.rawValue);
        Assert.AreEqual("6", v1.rawValue);
        Assert.AreEqual("7", v2.rawValue);
    }

    [Test]
    public void Parse_OperatorExpression_Equals() {
        ASTNode root = ExpressionParser.Parse("12 == 3");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.Equals, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_NotEquals() {
        ASTNode root = ExpressionParser.Parse("12 != 3");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.NotEquals, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_GreaterThan() {
        ASTNode root = ExpressionParser.Parse("12 > 3");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.GreaterThan, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_GreaterThanEqualTo() {
        ASTNode root = ExpressionParser.Parse("12 >= 3");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.GreaterThanEqualTo, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_LessThan() {
        ASTNode root = ExpressionParser.Parse("12 < 3");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.LessThan, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_LessThanEqualTo() {
        ASTNode root = ExpressionParser.Parse("12 <= 3");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.LessThanEqualTo, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_And() {
        ASTNode root = ExpressionParser.Parse("true && false");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.And, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_OperatorExpression_Or() {
        ASTNode root = ExpressionParser.Parse("true || false || true");
        Assert.IsInstanceOf<OperatorNode>(root);
        Assert.AreEqual(OperatorType.Or, ((OperatorNode) root).operatorType);
    }

    [Test]
    public void Parse_IdentifierValue() {
        ASTNode root = ExpressionParser.Parse("someIdentifier");
        Assert.IsInstanceOf<IdentifierNode>(root);
        root = ExpressionParser.Parse("{ someIdentifier }");
        Assert.IsInstanceOf<IdentifierNode>(root);
    }

    [Test]
    public void Parse_AliasValue() {
        ASTNode root = ExpressionParser.Parse("{$someIdentifier}");
        Assert.IsInstanceOf<IdentifierNode>(root);
        root = ExpressionParser.Parse("{ $someIdentifier }");
        Assert.IsInstanceOf<IdentifierNode>(root);
    }

    [Test]
    public void Parse_IdentifierWithOperator() {
        ASTNode v0 = ExpressionParser.Parse("{someIdentifier + someIdentifier2}");
        ASTNode v1 = ExpressionParser.Parse("{someIdentifier - someIdentifier2}");
        ASTNode v2 = ExpressionParser.Parse("{someIdentifier * someIdentifier2}");
        ASTNode v3 = ExpressionParser.Parse("{someIdentifier / someIdentifier2}");
        ASTNode v4 = ExpressionParser.Parse("{someIdentifier % someIdentifier2}");
        ASTNode v5 = ExpressionParser.Parse("{someIdentifier >> someIdentifier2}");
        ASTNode v6 = ExpressionParser.Parse("{someIdentifier << someIdentifier2}");
        ASTNode v7 = ExpressionParser.Parse("{someIdentifier & someIdentifier2}");
        ASTNode v8 = ExpressionParser.Parse("{someIdentifier | someIdentifier2}");
        ASTNode v9 = ExpressionParser.Parse("{someIdentifier ^ someIdentifier2}");

        void Verify(ASTNode node, OperatorType opType) {
            OperatorNode opEx = (OperatorNode) node;
            Assert.AreEqual(opType, ((OperatorNode) node).operatorType);
            Assert.IsInstanceOf<IdentifierNode>(opEx.left);
            Assert.IsInstanceOf<IdentifierNode>(opEx.right);
        }

        Verify(v0, OperatorType.Plus);
        Verify(v1, OperatorType.Minus);
        Verify(v2, OperatorType.Times);
        Verify(v3, OperatorType.Divide);
        Verify(v4, OperatorType.Mod);
        Verify(v5, OperatorType.ShiftRight);
        Verify(v6, OperatorType.ShiftLeft);
        Verify(v7, OperatorType.BinaryAnd);
        Verify(v8, OperatorType.BinaryOr);
        Verify(v9, OperatorType.BinaryXor);
    }

    [Test]
    public void Parse_ExpressionWithOuterParens_NotStripped() {
        ASTNode root = ExpressionParser.Parse("{ (someIdentifier + 735.2) }");
        Assert.IsInstanceOf<ParenNode>(root);
        ParenNode parenNode = (ParenNode) root;
        Assert.IsInstanceOf<OperatorNode>(parenNode.expression);
        OperatorNode opNode = (OperatorNode) parenNode.expression;
        Assert.IsInstanceOf<IdentifierNode>(opNode.left);
        Assert.IsInstanceOf<LiteralNode>(opNode.right);
    }

    [Test]
    public void Parse_ExpressionWithOuterParens_Stripped() {
        ASTNode root = ExpressionParser.Parse("{ (someIdentifier) }");
        Assert.IsInstanceOf<IdentifierNode>(root);
    }

    [Test]
    public void Parse_ExpressionWithInnerParens_Stripped() {
        ASTNode root = ExpressionParser.Parse("{someIdentifier + (735.2)}");
        Assert.IsInstanceOf<OperatorNode>(root);
        OperatorNode opEx = (OperatorNode) root;
        Assert.IsInstanceOf<IdentifierNode>(opEx.left);
        Assert.IsInstanceOf<LiteralNode>(opEx.right);
    }

    [Test]
    public void Parse_ExpressionWithInnerParens_NotStripped() {
        ASTNode root = ExpressionParser.Parse("{someIdentifier + (5 + 735.2)}");
        Assert.IsInstanceOf<OperatorNode>(root);
        OperatorNode opEx = (OperatorNode) root;
        Assert.IsInstanceOf<IdentifierNode>(opEx.left);
        Assert.IsInstanceOf<ParenNode>(opEx.right);
    }

    [Test]
    public void Parse_NestedParenAddition() {
        ASTNode root = ExpressionParser.Parse("(12 + (4 * 3)) * 2");
        Assert.IsInstanceOf<OperatorNode>(root);
        OperatorNode opEx = (OperatorNode) root;
        Assert.IsInstanceOf<ParenNode>(opEx.left);
        Assert.IsInstanceOf<LiteralNode>(opEx.right);
        ParenNode parenNode = (ParenNode) opEx.left;
        Assert.IsInstanceOf<OperatorNode>(parenNode.expression);
        opEx = (OperatorNode) parenNode.expression;
        Assert.IsInstanceOf<LiteralNode>(opEx.left);
        Assert.IsInstanceOf<ParenNode>(opEx.right);
    }

    [Test]
    public void Parse_DotAccessExpression() {
        ASTNode root = ExpressionParser.Parse("someProperty.someValue");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(1, node.parts.Count);
        DotAccessNode dotNode = node.parts[0] as DotAccessNode;
        Assert.AreEqual("someValue", dotNode.propertyName);
        Assert.AreEqual("someProperty", node.identifier);
    }

    [Test]
    public void Parse_DotAccessExpression_Multiple() {
        ASTNode root = ExpressionParser.Parse("someProperty.someValue.someOtherValue.evenMore");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(3, node.parts.Count);
        Assert.AreEqual("someProperty", node.identifier);
        Assert.AreEqual("someValue", ((DotAccessNode) node.parts[0]).propertyName);
        Assert.AreEqual("someOtherValue", ((DotAccessNode) node.parts[1]).propertyName);
        Assert.AreEqual("evenMore", ((DotAccessNode) node.parts[2]).propertyName);
    }

    [Test]
    public void Parse_IndexExpression() {
        ASTNode root = ExpressionParser.Parse("someProperty[4]");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<IndexNode>(node.parts[0]);
    }

    [Test]
    public void Parse_InvokeAccessExpression_NoArgs() {
        ASTNode root = ExpressionParser.Parse("someProperty()");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<InvokeNode>(node.parts[0]);
    }

    [Test]
    public void Parse_InvokeAccessExpression_1Arg() {
        ASTNode root = ExpressionParser.Parse("someProperty(1)");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<InvokeNode>(node.parts[0]);
        InvokeNode invokeNode = (InvokeNode) node.parts[0];
        Assert.AreEqual(1, invokeNode.parameters.Count);
        Assert.IsInstanceOf<LiteralNode>(invokeNode.parameters[0]);
    }

    [Test]
    public void Parse_InvokeAccessExpression_2Arg() {
        ASTNode root = ExpressionParser.Parse("someProperty(1, something)");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<InvokeNode>(node.parts[0]);
        InvokeNode invokeNode = (InvokeNode) node.parts[0];
        Assert.AreEqual(2, invokeNode.parameters.Count);
        Assert.IsInstanceOf<LiteralNode>(invokeNode.parameters[0]);
        Assert.IsInstanceOf<IdentifierNode>(invokeNode.parameters[1]);
    }

    [Test]
    public void Parse_InvokeAccessExpression_2Arg_Chained() {
        ASTNode root = ExpressionParser.Parse("someProperty(1, something)()");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual(2, node.parts.Count);
        Assert.IsInstanceOf<InvokeNode>(node.parts[0]);
        Assert.IsInstanceOf<InvokeNode>(node.parts[1]);
        InvokeNode invokeNode = (InvokeNode) node.parts[0];
        Assert.AreEqual(2, invokeNode.parameters.Count);
        Assert.IsInstanceOf<LiteralNode>(invokeNode.parameters[0]);
        Assert.IsInstanceOf<IdentifierNode>(invokeNode.parameters[1]);
    }

    [Test]
    public void Parse_Ternary() {
        ASTNode root = ExpressionParser.Parse("{x ? y : 1}");
        OperatorNode ternary = AssertInstanceOfAndReturn<OperatorNode>(root);
        Assert.AreEqual(OperatorType.TernaryCondition, ternary.operatorType);
        OperatorNode selection = AssertInstanceOfAndReturn<OperatorNode>(ternary.right);
        Assert.IsInstanceOf<IdentifierNode>(ternary.left);
        Assert.AreEqual(OperatorType.TernarySelection, selection.operatorType);
        Assert.IsInstanceOf<IdentifierNode>(selection.left);
        Assert.IsInstanceOf<LiteralNode>(selection.right);
    }

    [Test]
    public void Parse_Ternary_With_New() {
        ASTNode root = ExpressionParser.Parse("{x ? new OffsetMeasurement((float) 1 / (float) 2, OffsetMeasurementUnit.Percent)}");
        OperatorNode ternary = AssertInstanceOfAndReturn<OperatorNode>(root);
        Assert.AreEqual(OperatorType.TernaryCondition, ternary.operatorType);
        OperatorNode selection = AssertInstanceOfAndReturn<OperatorNode>(ternary.right);
        Assert.IsInstanceOf<IdentifierNode>(ternary.left);
        Assert.AreEqual(OperatorType.TernarySelection, selection.operatorType);
        Assert.IsInstanceOf<NewExpressionNode>(selection.left);
        Assert.AreEqual("default", AssertInstanceOfAndReturn<LiteralNode>(selection.right).rawValue);
    }

    [Test]
    public void Parse_Ternary_NestedExpression() {
        ASTNode root = ExpressionParser.Parse("{x ? y + z : 1}");
        OperatorNode ternary = AssertInstanceOfAndReturn<OperatorNode>(root);
        Assert.AreEqual(OperatorType.TernaryCondition, ternary.operatorType);
        OperatorNode selection = AssertInstanceOfAndReturn<OperatorNode>(ternary.right);
        Assert.IsInstanceOf<IdentifierNode>(ternary.left);
        Assert.AreEqual("x", ((IdentifierNode) ternary.left).name);

        Assert.AreEqual(OperatorType.TernarySelection, selection.operatorType);

        OperatorNode nestedOp = AssertInstanceOfAndReturn<OperatorNode>(selection.left);
        Assert.AreEqual(OperatorType.Plus, nestedOp.operatorType);

        Assert.AreEqual("y", ((IdentifierNode) nestedOp.left).name);
        Assert.AreEqual("z", ((IdentifierNode) nestedOp.right).name);

        Assert.AreEqual(OperatorType.Plus, nestedOp.operatorType);

        Assert.IsInstanceOf<LiteralNode>(selection.right);
    }

    [Test]
    public void Parse_ArrayAccess_RootContext_LiteralExpression() {
        ASTNode root = ExpressionParser.Parse("{rootContext[ 1 + 1 ]}");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual("rootContext", node.identifier);
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<IndexNode>(node.parts[0]);
        Assert.IsInstanceOf<OperatorNode>(((IndexNode) node.parts[0]).arguments[0]);
        OperatorNode opNode = (OperatorNode) ((IndexNode) node.parts[0]).arguments[0];
        Assert.AreEqual(OperatorType.Plus, opNode.operatorType);
        Assert.IsInstanceOf<LiteralNode>(opNode.left);
        Assert.IsInstanceOf<LiteralNode>(opNode.right);
    }

    [Test]
    public void Parse_ArrayAccess_ParenExpression() {
        ASTNode root = ExpressionParser.Parse("{rootContext[ (1 + 1) ]}");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual("rootContext", node.identifier);
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<IndexNode>(node.parts[0]);
        Assert.IsInstanceOf<ParenNode>(((IndexNode) node.parts[0]).arguments[0]);

        Assert.IsInstanceOf<ParenNode>(((IndexNode) node.parts[0]).arguments[0]);
        ParenNode parenNodeOld = (ParenNode) ((IndexNode) node.parts[0]).arguments[0];
        OperatorNode opNode = (OperatorNode) parenNodeOld.expression;
        Assert.AreEqual(OperatorType.Plus, opNode.operatorType);
        Assert.IsInstanceOf<LiteralNode>(opNode.left);
        Assert.IsInstanceOf<LiteralNode>(opNode.right);
    }

    [Test]
    public void Parse_ArrayAccess_NestedArrayAccess() {
        ASTNode root = ExpressionParser.Parse("{rootContext[ rootContext[3] ]}");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual("rootContext", node.identifier);
        Assert.AreEqual(1, node.parts.Count);
        Assert.IsInstanceOf<IndexNode>(node.parts[0]);
        Assert.IsInstanceOf<MemberAccessExpressionNode>(((IndexNode) node.parts[0]).arguments[0]);
    }

    [Test]
    public void Parse_PropertyAccessMixedArrayAccess() {
        ASTNode root = ExpressionParser.Parse("{rootContext.list[1].property1.property2}");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual("rootContext", node.identifier);
        Assert.AreEqual(4, node.parts.Count);
        Assert.IsInstanceOf<DotAccessNode>(node.parts[0]);
        Assert.IsInstanceOf<IndexNode>(node.parts[1]);
        Assert.IsInstanceOf<DotAccessNode>(node.parts[2]);
        Assert.IsInstanceOf<DotAccessNode>(node.parts[3]);
    }

    [Test]
    public void Parse_PropertyAccessMixedArrayAccessWithInvoke() {
        ASTNode root = ExpressionParser.Parse("{rootContext()[1].property1.property2}");
        Assert.IsInstanceOf<MemberAccessExpressionNode>(root);
        MemberAccessExpressionNode node = (MemberAccessExpressionNode) root;
        Assert.AreEqual("rootContext", node.identifier);
        Assert.AreEqual(4, node.parts.Count);
        Assert.IsInstanceOf<InvokeNode>(node.parts[0]);
        Assert.IsInstanceOf<IndexNode>(node.parts[1]);
        Assert.IsInstanceOf<DotAccessNode>(node.parts[2]);
        Assert.IsInstanceOf<DotAccessNode>(node.parts[3]);
    }

    [Test]
    public void Parse_UnaryExpression_Not() {
        ASTNode root = ExpressionParser.Parse("!someValue");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.UnaryNot, node.type);
        Assert.IsInstanceOf<IdentifierNode>(node.expression);
    }

    [Test]
    public void Parse_UnaryExpression_Not_Complex() {
        ASTNode root = ExpressionParser.Parse("!(someValue > 4)");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.UnaryNot, node.type);
        Assert.IsInstanceOf<ParenNode>(node.expression);
    }

    [Test]
    public void Parse_UnaryExpression_BitwiseNot() {
        ASTNode root = ExpressionParser.Parse("~someValue");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.UnaryBitwiseNot, node.type);
        Assert.IsInstanceOf<IdentifierNode>(node.expression);
    }

    [Test]
    public void Parse_UnaryExpression_BitwiseNot_Complex() {
        ASTNode root = ExpressionParser.Parse("~(someValue > 4)");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.UnaryBitwiseNot, node.type);
        Assert.IsInstanceOf<ParenNode>(node.expression);
    }

    [Test]
    public void Parse_UnaryExpression_Minus() {
        ASTNode root = ExpressionParser.Parse("-someValue");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.UnaryMinus, node.type);
        Assert.IsInstanceOf<IdentifierNode>(node.expression);
    }

    [Test]
    public void Parse_UnaryExpression_Minus_Complex() {
        ASTNode root = ExpressionParser.Parse("-(someValue + 4)");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.UnaryMinus, node.type);
        Assert.IsInstanceOf<ParenNode>(node.expression);
    }

    [Test]
    public void Parse_UnaryExpression_DirectCast() {
        ASTNode root = ExpressionParser.Parse("(int)5.6f");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.DirectCast, node.type);
        Assert.AreEqual("int", node.typeLookup.typeName);

        root = ExpressionParser.Parse("(UnityEngine.Vector3)5.6f");
        node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.DirectCast, node.type);
        Assert.AreEqual("UnityEngine", node.typeLookup.namespaceName);
        Assert.AreEqual("Vector3", node.typeLookup.typeName);
    }

    [Test]
    public void Parse_UnaryExpression_DirectCastGeneric() {
        ASTNode root = ExpressionParser.Parse("(List<float, int>)someIdentifier");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual(ASTNodeType.DirectCast, node.type);
        Assert.AreEqual("List", node.typeLookup.typeName);
        Assert.AreEqual("float", node.typeLookup.generics[0].typeName);
        Assert.AreEqual("int", node.typeLookup.generics[1].typeName);
    }

    [Test]
    public void Parse_UnaryExpression_DirectCastGenericList() {
        ASTNode root = ExpressionParser.Parse("(List<Something.Tuple<int, ValueTuple>>)expr");
        UnaryExpressionNode node = AssertInstanceOfAndReturn<UnaryExpressionNode>(root);
        Assert.AreEqual("List", node.typeLookup.typeName);
        TypeLookup outerTuple = node.typeLookup.generics[0];
        TypeLookup outerGen0 = outerTuple.generics[0];
        TypeLookup outerGen1 = outerTuple.generics[1];

        Assert.AreEqual("Something", outerTuple.namespaceName);
        Assert.AreEqual("Tuple", outerTuple.typeName);
        Assert.AreEqual("int", outerGen0.typeName);
        Assert.AreEqual("ValueTuple", outerGen1.typeName);
    }

    [Test]
    public void Parse_TypePathExpression_WithGenerics() {
        ASTNode root = ExpressionParser.Parse("System.Collections.Generic.List<System.Tuple<int, System.Collections.Generic.List<string>>>.Value");
        MemberAccessExpressionNode node = AssertInstanceOfAndReturn<MemberAccessExpressionNode>(root);
        var t = typeof(System.Collections.Generic.List<System.Tuple<int, System.Collections.Generic.List<string>>>);
        Assert.AreEqual("System", node.identifier);
        Assert.AreEqual("Collections", AssertInstanceOfAndReturn<DotAccessNode>(node.parts[0]).propertyName);
        Assert.AreEqual("Generic", AssertInstanceOfAndReturn<DotAccessNode>(node.parts[1]).propertyName);
        Assert.AreEqual("List", AssertInstanceOfAndReturn<DotAccessNode>(node.parts[2]).propertyName);
        GenericTypePathNode typeNode = AssertInstanceOfAndReturn<GenericTypePathNode>(node.parts[3]);
        Assert.AreEqual(1, typeNode.genericPath.generics.Count);
        Assert.AreEqual(typeof(Tuple<int, System.Collections.Generic.List<string>>), TypeProcessor.ResolveType(typeNode.genericPath.generics[0]));
    }

    [Test]
    public void Parse_TypeOfExpression() {
        ASTNode root = ExpressionParser.Parse("typeof(UnityEngine.Vector3)");
        TypeNode node = AssertInstanceOfAndReturn<TypeNode>(root);
        Assert.AreEqual("UnityEngine", node.typeLookup.namespaceName);
        Assert.AreEqual("Vector3", node.typeLookup.typeName);
    }

    [Test]
    public void Parse_NewExpression() {
        ASTNode root = ExpressionParser.Parse("new Vector3(1, 2, 3)");
        NewExpressionNode expressionNode = AssertInstanceOfAndReturn<NewExpressionNode>(root);
        Assert.AreEqual(typeof(Vector3), TypeProcessor.ResolveType(expressionNode.typeLookup, new string[] {"UnityEngine"}));
        Assert.AreEqual(3, expressionNode.parameters.Count);
        LiteralNode param0 = AssertInstanceOfAndReturn<LiteralNode>(expressionNode.parameters[0]);
        LiteralNode param1 = AssertInstanceOfAndReturn<LiteralNode>(expressionNode.parameters[1]);
        LiteralNode param2 = AssertInstanceOfAndReturn<LiteralNode>(expressionNode.parameters[2]);
        Assert.AreEqual("1", param0.rawValue);
        Assert.AreEqual("2", param1.rawValue);
        Assert.AreEqual("3", param2.rawValue);
    }

    [Test]
    public void Parse_NewExpression_Generic() {
        ASTNode root = ExpressionParser.Parse("new Tuple<float, float, float>(1, 2, 3)");
        NewExpressionNode expressionNode = AssertInstanceOfAndReturn<NewExpressionNode>(root);
        Type type = TypeProcessor.ResolveType(expressionNode.typeLookup, new string[] {"System"});
        Assert.AreEqual(typeof(Tuple<float, float, float>), type);
        Assert.AreEqual(3, expressionNode.parameters.Count);
        LiteralNode param0 = AssertInstanceOfAndReturn<LiteralNode>(expressionNode.parameters[0]);
        LiteralNode param1 = AssertInstanceOfAndReturn<LiteralNode>(expressionNode.parameters[1]);
        LiteralNode param2 = AssertInstanceOfAndReturn<LiteralNode>(expressionNode.parameters[2]);
        Assert.AreEqual("1", param0.rawValue);
        Assert.AreEqual("2", param1.rawValue);
        Assert.AreEqual("3", param2.rawValue);
    }

    [Test]
    public void Parse_NewExpression_NamespaceWithGeneric() {
        ASTNode root = ExpressionParser.Parse("new System.Tuple<float, float, float>(1, 2, 3)");
        NewExpressionNode expressionNode = AssertInstanceOfAndReturn<NewExpressionNode>(root);
        Assert.AreEqual(typeof(Tuple<float, float, float>), TypeProcessor.ResolveType(expressionNode.typeLookup));
        Assert.AreEqual(3, expressionNode.parameters.Count);
        LiteralNode param0 = AssertInstanceOfAndReturn<LiteralNode>(expressionNode.parameters[0]);
        LiteralNode param1 = AssertInstanceOfAndReturn<LiteralNode>(expressionNode.parameters[1]);
        LiteralNode param2 = AssertInstanceOfAndReturn<LiteralNode>(expressionNode.parameters[2]);
        Assert.AreEqual("1", param0.rawValue);
        Assert.AreEqual("2", param1.rawValue);
        Assert.AreEqual("3", param2.rawValue);
    }

    [Test]
    public void Parse_NewExpressionNoArguments() {
        ASTNode root = ExpressionParser.Parse("new Vector3()");
        NewExpressionNode expressionNode = AssertInstanceOfAndReturn<NewExpressionNode>(root);
        Assert.AreEqual(typeof(Vector3), TypeProcessor.ResolveType(expressionNode.typeLookup, new string[] {"UnityEngine"}));
        Assert.AreEqual(0, expressionNode.parameters.Count);
    }

    [Test]
    public void Parse_NewExpression_Nested() {
        ASTNode root = ExpressionParser.Parse("new Vector3(5, new Vector3(1, 2, 3), 2)");
        NewExpressionNode expressionNode = AssertInstanceOfAndReturn<NewExpressionNode>(root);
        Assert.AreEqual(typeof(Vector3), TypeProcessor.ResolveType(expressionNode.typeLookup, new string[] {"UnityEngine"}));
        Assert.AreEqual(3, expressionNode.parameters.Count);

        LiteralNode p0 = AssertInstanceOfAndReturn<LiteralNode>(expressionNode.parameters[0]);
        LiteralNode p1 = AssertInstanceOfAndReturn<LiteralNode>(expressionNode.parameters[2]);

        NewExpressionNode nestedNew = AssertInstanceOfAndReturn<NewExpressionNode>(expressionNode.parameters[1]);

        Assert.AreEqual(3, nestedNew.parameters.Count);
        LiteralNode param0 = AssertInstanceOfAndReturn<LiteralNode>(nestedNew.parameters[0]);
        LiteralNode param1 = AssertInstanceOfAndReturn<LiteralNode>(nestedNew.parameters[1]);
        LiteralNode param2 = AssertInstanceOfAndReturn<LiteralNode>(nestedNew.parameters[2]);
        Assert.AreEqual("5", p0.rawValue);
        Assert.AreEqual("2", p1.rawValue);
        Assert.AreEqual("1", param0.rawValue);
        Assert.AreEqual("2", param1.rawValue);
        Assert.AreEqual("3", param2.rawValue);
    }

    [Test]
    public void Parse_ArrayLiteralExpression() {
        ASTNode root = ExpressionParser.Parse("[1, 3, 4]");
        Assert.IsInstanceOf<ListInitializerNode>(root);
        ListInitializerNode node = (ListInitializerNode) root;
        Assert.AreEqual(3, node.list.Count);
        Assert.IsInstanceOf<LiteralNode>(node.list[0]);
        Assert.IsInstanceOf<LiteralNode>(node.list[1]);
        Assert.IsInstanceOf<LiteralNode>(node.list[2]);
    }

    [Test]
    public void Parse_IsOperator() {
        ASTNode root = ExpressionParser.Parse("something is Vector3");
        OperatorNode node = AssertInstanceOfAndReturn<OperatorNode>(root);
        Assert.AreEqual(OperatorType.Is, node.operatorType);
        Assert.IsInstanceOf<IdentifierNode>(node.left);
        Assert.IsInstanceOf<TypeNode>(node.right);
    }

    [Test]
    public void Parse_AsOperator() {
        ASTNode root = ExpressionParser.Parse("something as Vector3");
        OperatorNode node = AssertInstanceOfAndReturn<OperatorNode>(root);
        Assert.AreEqual(OperatorType.As, node.operatorType);
        Assert.IsInstanceOf<IdentifierNode>(node.left);
        Assert.IsInstanceOf<TypeNode>(node.right);
    }

    [Test]
    public void Parse_AliasAsMethod() {
        ASTNode root = ExpressionParser.Parse("$something(4 + 1)");
        var node = AssertInstanceOfAndReturn<MemberAccessExpressionNode>(root);
        Assert.IsInstanceOf<InvokeNode>(node.parts[0]);
    }

    [Test]
    public void Parse_AliasInTernary() {
        ASTNode root = ExpressionParser.Parse("$item.isInDirectControl ? '1' : '2'");
        OperatorNode node = AssertInstanceOfAndReturn<OperatorNode>(root);
        Assert.AreEqual(OperatorType.TernaryCondition, node.operatorType);
        MemberAccessExpressionNode left = AssertInstanceOfAndReturn<MemberAccessExpressionNode>(node.left);
    }

    [Test]
    public void Parse_Comments() {
        ASTNode root = ExpressionParser.Parse("$item /* yes, I really need to put comments in expressions in templates!! */");
        IdentifierNode node = AssertInstanceOfAndReturn<IdentifierNode>(root);
        Assert.AreEqual("$item", node.name);
    }

    [Test]
    public void Parse_LambdaNoParameters() {
        ASTNode root = ExpressionParser.Parse("() => value");
        LambdaExpressionNode node = AssertInstanceOfAndReturn<LambdaExpressionNode>(root);
        Assert.AreEqual(0, node.signature.Count);
    }

    [Test]
    public void Parse_LambdaNoParameters_NoBody() {
        Assert.Throws<ParseException>(() => { ExpressionParser.Parse("() => "); });
    }

    [Test]
    public void Parse_LambdaTypedParameter_1() {
        ASTNode root = ExpressionParser.Parse("(string value) => value");
        LambdaExpressionNode node = AssertInstanceOfAndReturn<LambdaExpressionNode>(root);
        Assert.AreEqual(1, node.signature.Count);
        Assert.AreEqual("string", node.signature[0].type.Value.typeName);
        Assert.AreEqual("value", node.signature[0].identifier);
    }

    [Test]
    public void Parse_LambdaTypedParameter_2() {
        ASTNode root = ExpressionParser.Parse("(string value, System.Collections.Generic.List<int> listValue) => value");
        LambdaExpressionNode node = AssertInstanceOfAndReturn<LambdaExpressionNode>(root);
        Assert.AreEqual(2, node.signature.Count);
        Assert.AreEqual("string", node.signature[0].type.Value.typeName);
        Assert.AreEqual("value", node.signature[0].identifier);
        Assert.AreEqual(typeof(List<int>), TypeProcessor.ResolveType(node.signature[1].type.Value));
        Assert.AreEqual("listValue", node.signature[1].identifier);
    }

    [Test]
    public void Parse_LambdaUnTypedParameters_1() {
        ASTNode root = ExpressionParser.Parse("(value) => value");
        LambdaExpressionNode node = AssertInstanceOfAndReturn<LambdaExpressionNode>(root);
        Assert.AreEqual(1, node.signature.Count);
        Assert.AreEqual(null, node.signature[0].type);
        Assert.AreEqual("value", node.signature[0].identifier);
    }

    [Test]
    public void Parse_LambdaUnTypedParameters_2() {
        ASTNode root = ExpressionParser.Parse("(v0, v1) => value");
        LambdaExpressionNode node = AssertInstanceOfAndReturn<LambdaExpressionNode>(root);
        Assert.AreEqual(2, node.signature.Count);
        Assert.AreEqual(null, node.signature[0].type);
        Assert.AreEqual("v0", node.signature[0].identifier);
        Assert.AreEqual(null, node.signature[1].type);
        Assert.AreEqual("v1", node.signature[1].identifier);
    }

    [Test]
    public void Parse_LambdaWithBody() {
        ASTNode root = ExpressionParser.Parse("(value) => 1 + 1");
        LambdaExpressionNode node = AssertInstanceOfAndReturn<LambdaExpressionNode>(root);
        Assert.AreEqual(1, node.signature.Count);
        Assert.AreEqual(null, node.signature[0].type);
        Assert.AreEqual("value", node.signature[0].identifier);
        OperatorNode operatorNode = AssertInstanceOfAndReturn<OperatorNode>(node.body);
        Assert.AreEqual(OperatorType.Plus, operatorNode.operatorType);
        Assert.IsInstanceOf<LiteralNode>(operatorNode.left);
        Assert.IsInstanceOf<LiteralNode>(operatorNode.right);
    }

    [Test]
    public void Parse_CoalesceOperator() {
        ASTNode root = ExpressionParser.Parse("value ?? otherValue");
        OperatorNode node = AssertInstanceOfAndReturn<OperatorNode>(root);
        Assert.AreEqual(OperatorType.Coalesce, node.operatorType);
        Assert.IsInstanceOf<IdentifierNode>(node.left);
        Assert.IsInstanceOf<IdentifierNode>(node.right);
    }

    [Test]
    public void Parse_SafeAccessor_Dot() {
        ASTNode root = ExpressionParser.Parse("instance?.property");
        MemberAccessExpressionNode node = AssertInstanceOfAndReturn<MemberAccessExpressionNode>(root);
        Assert.AreEqual(1, node.parts.Count);
        Assert.AreEqual("instance", node.identifier);
        Assert.IsInstanceOf<DotAccessNode>(node.parts[0]);
        Assert.IsTrue(((DotAccessNode) node.parts[0]).isNullableAccess);
        Assert.AreEqual("property", ((DotAccessNode) node.parts[0]).propertyName);
    }

    [Test]
    public void Parse_SafeAccessor_Invoke() {
        ASTNode root = ExpressionParser.Parse("instance?.ToString()");
        MemberAccessExpressionNode node = AssertInstanceOfAndReturn<MemberAccessExpressionNode>(root);
        Assert.AreEqual(2, node.parts.Count);
        Assert.AreEqual("instance", node.identifier);
        Assert.IsInstanceOf<DotAccessNode>(node.parts[0]);
        Assert.IsTrue(((DotAccessNode) node.parts[0]).isNullableAccess);
        Assert.AreEqual("ToString", ((DotAccessNode) node.parts[0]).propertyName);
        Assert.IsInstanceOf<InvokeNode>(node.parts[1]);
    }

    [Test]
    public void Parse_SafeAccessor_Index() {
        ASTNode root = ExpressionParser.Parse("instance?[1]");
        MemberAccessExpressionNode node = AssertInstanceOfAndReturn<MemberAccessExpressionNode>(root);
        Assert.AreEqual(1, node.parts.Count);
        Assert.AreEqual("instance", node.identifier);
        Assert.IsInstanceOf<IndexNode>(node.parts[0]);
        IndexNode indexNode = (IndexNode) node.parts[0];
        Assert.IsTrue(indexNode.isNullableAccess);
        Assert.AreEqual(1, indexNode.arguments.Count);
        Assert.IsInstanceOf<LiteralNode>(indexNode.arguments[0]);
    }

    [Test]
    public void Parse_Ternary_Basic() {
        ASTNode root = ExpressionParser.Parse(("1 > 2 ? true : false"));
        OperatorNode outerTernary = AssertInstanceOfAndReturn<OperatorNode>(root);
        Assert.AreEqual(OperatorType.TernaryCondition, outerTernary.operatorType);
        OperatorNode greaterThan = AssertInstanceOfAndReturn<OperatorNode>(outerTernary.left);
        Assert.AreEqual(OperatorType.GreaterThan, greaterThan.operatorType);
        Assert.IsInstanceOf<LiteralNode>(greaterThan.left);
        Assert.IsInstanceOf<LiteralNode>(greaterThan.right);

        OperatorNode select = AssertInstanceOfAndReturn<OperatorNode>(outerTernary.right);
        Assert.AreEqual(OperatorType.TernarySelection, select.operatorType);
        Assert.AreEqual("true", AssertInstanceOfAndReturn<LiteralNode>(select.left).rawValue);
        Assert.AreEqual("false", AssertInstanceOfAndReturn<LiteralNode>(select.right).rawValue);
    }

    [Test]
    public void Parse_Ternary_Nested() {
        ASTNode root = ExpressionParser.Parse(("thing != null ? thing.floatValue > 5 ? 1 : default : default"));

        OperatorNode outerTernary = AssertOpNode(root, OperatorType.TernaryCondition);

        OperatorNode outerCondition = AssertOpNode(outerTernary.left, OperatorType.NotEquals);
        OperatorNode outerSelection = AssertOpNode(outerTernary.right, OperatorType.TernarySelection);

        Assert.AreEqual("thing", AssertInstanceOfAndReturn<IdentifierNode>(outerCondition.left).name);
        Assert.AreEqual("null", AssertInstanceOfAndReturn<LiteralNode>(outerCondition.right).rawValue);

        OperatorNode innerTernary = AssertOpNode(outerSelection.left, OperatorType.TernaryCondition);

        OperatorNode innerCondition = AssertInstanceOfAndReturn<OperatorNode>(innerTernary.left);
        Assert.IsInstanceOf<MemberAccessExpressionNode>(innerCondition.left);
        Assert.AreEqual("5", AssertInstanceOfAndReturn<LiteralNode>(innerCondition.right).rawValue);

        OperatorNode innerSelection = AssertOpNode(innerTernary.right, OperatorType.TernarySelection);
        Assert.AreEqual("1", AssertInstanceOfAndReturn<LiteralNode>(innerSelection.left).rawValue);
        Assert.AreEqual("default", AssertInstanceOfAndReturn<LiteralNode>(innerSelection.right).rawValue);

        Assert.AreEqual("default", AssertInstanceOfAndReturn<LiteralNode>(outerSelection.right).rawValue);
    }

    [Test]
    public void Parse_Ternary_Partial() {
        ASTNode root = ExpressionParser.Parse(("1 > 2 ? 5"));
        OperatorNode condition = AssertOpNode(root, OperatorType.TernaryCondition);

        OperatorNode gtNode = AssertOpNode(condition.left, OperatorType.GreaterThan);
        OperatorNode selection = AssertOpNode(condition.right, OperatorType.TernarySelection);

        Assert.AreEqual("5", AssertInstanceOfAndReturn<LiteralNode>(selection.left).rawValue);
        Assert.AreEqual("default", AssertInstanceOfAndReturn<LiteralNode>(selection.right).rawValue);
    }

    private OperatorNode AssertOpNode(ASTNode node, OperatorType operatorType) {
        OperatorNode retn = AssertInstanceOfAndReturn<OperatorNode>(node);
        Assert.AreEqual(operatorType, retn.operatorType);
        return retn;
    }

    [Test]
    public void Parse_Ternary_Partial_Nested() {
        ASTNode root = ExpressionParser.Parse(("thing != null ? thing.floatValue > 5 ? 1"));

        OperatorNode outerTernary = AssertOpNode(root, OperatorType.TernaryCondition);

        OperatorNode outerCondition = AssertOpNode(outerTernary.left, OperatorType.NotEquals);
        OperatorNode outerSelection = AssertOpNode(outerTernary.right, OperatorType.TernarySelection);

        Assert.AreEqual("thing", AssertInstanceOfAndReturn<IdentifierNode>(outerCondition.left).name);
        Assert.AreEqual("null", AssertInstanceOfAndReturn<LiteralNode>(outerCondition.right).rawValue);

        OperatorNode innerTernary = AssertOpNode(outerSelection.left, OperatorType.TernaryCondition);

        OperatorNode innerCondition = AssertInstanceOfAndReturn<OperatorNode>(innerTernary.left);
        Assert.IsInstanceOf<MemberAccessExpressionNode>(innerCondition.left);
        Assert.AreEqual("5", AssertInstanceOfAndReturn<LiteralNode>(innerCondition.right).rawValue);

        OperatorNode innerSelection = AssertOpNode(innerTernary.right, OperatorType.TernarySelection);
        Assert.AreEqual("1", AssertInstanceOfAndReturn<LiteralNode>(innerSelection.left).rawValue);
        Assert.AreEqual("default", AssertInstanceOfAndReturn<LiteralNode>(innerSelection.right).rawValue);

        Assert.AreEqual("default", AssertInstanceOfAndReturn<LiteralNode>(outerSelection.right).rawValue);
    }

    [Test]
    public void Parse_Ternary_Partial_Nested_WithFull() {
        ASTNode root = ExpressionParser.Parse(("thing != null ? thing.floatValue > 5 ? 1 : 12"));

        OperatorNode outerTernary = AssertOpNode(root, OperatorType.TernaryCondition);

        OperatorNode outerCondition = AssertOpNode(outerTernary.left, OperatorType.NotEquals);
        OperatorNode outerSelection = AssertOpNode(outerTernary.right, OperatorType.TernarySelection);

        Assert.AreEqual("thing", AssertInstanceOfAndReturn<IdentifierNode>(outerCondition.left).name);
        Assert.AreEqual("null", AssertInstanceOfAndReturn<LiteralNode>(outerCondition.right).rawValue);

        OperatorNode innerTernary = AssertOpNode(outerSelection.left, OperatorType.TernaryCondition);

        OperatorNode innerCondition = AssertInstanceOfAndReturn<OperatorNode>(innerTernary.left);
        Assert.IsInstanceOf<MemberAccessExpressionNode>(innerCondition.left);
        Assert.AreEqual("5", AssertInstanceOfAndReturn<LiteralNode>(innerCondition.right).rawValue);

        OperatorNode innerSelection = AssertOpNode(innerTernary.right, OperatorType.TernarySelection);
        Assert.AreEqual("1", AssertInstanceOfAndReturn<LiteralNode>(innerSelection.left).rawValue);
        Assert.AreEqual("12", AssertInstanceOfAndReturn<LiteralNode>(innerSelection.right).rawValue);

        Assert.AreEqual("default", AssertInstanceOfAndReturn<LiteralNode>(outerSelection.right).rawValue);
    }

    [Test]
    public void Parse_Ternary_Partial_Nested_WithFullParens() {
        ASTNode root = ExpressionParser.Parse(("thing != null ? (thing.floatValue > 5 ? 1) : 12"));

        OperatorNode outerTernary = AssertOpNode(root, OperatorType.TernaryCondition);

        OperatorNode outerCondition = AssertOpNode(outerTernary.left, OperatorType.NotEquals);
        OperatorNode outerSelection = AssertOpNode(outerTernary.right, OperatorType.TernarySelection);

        Assert.AreEqual("thing", AssertInstanceOfAndReturn<IdentifierNode>(outerCondition.left).name);
        Assert.AreEqual("null", AssertInstanceOfAndReturn<LiteralNode>(outerCondition.right).rawValue);

        ParenNode parenNode = AssertInstanceOfAndReturn<ParenNode>(outerSelection.left);
        OperatorNode innerTernary = AssertOpNode(parenNode.expression, OperatorType.TernaryCondition);

        OperatorNode innerCondition = AssertInstanceOfAndReturn<OperatorNode>(innerTernary.left);
        Assert.IsInstanceOf<MemberAccessExpressionNode>(innerCondition.left);
        Assert.AreEqual("5", AssertInstanceOfAndReturn<LiteralNode>(innerCondition.right).rawValue);

        OperatorNode innerSelection = AssertOpNode(innerTernary.right, OperatorType.TernarySelection);
        Assert.AreEqual("1", AssertInstanceOfAndReturn<LiteralNode>(innerSelection.left).rawValue);
        Assert.AreEqual("default", AssertInstanceOfAndReturn<LiteralNode>(innerSelection.right).rawValue);

        Assert.AreEqual("12", AssertInstanceOfAndReturn<LiteralNode>(outerSelection.right).rawValue);
    }

    [Test]
    public void Parse_Ternary_WithSafeAccess() {
        ASTNode root = ExpressionParser.Parse(("thing != null ? thing?.floatValue > 5 ? 1"));

        OperatorNode outerTernary = AssertOpNode(root, OperatorType.TernaryCondition);

        OperatorNode outerCondition = AssertOpNode(outerTernary.left, OperatorType.NotEquals);
        OperatorNode outerSelection = AssertOpNode(outerTernary.right, OperatorType.TernarySelection);

        Assert.AreEqual("thing", AssertInstanceOfAndReturn<IdentifierNode>(outerCondition.left).name);
        Assert.AreEqual("null", AssertInstanceOfAndReturn<LiteralNode>(outerCondition.right).rawValue);

        OperatorNode innerTernary = AssertOpNode(outerSelection.left, OperatorType.TernaryCondition);

        OperatorNode innerCondition = AssertOpNode(innerTernary.left, OperatorType.GreaterThan);
        Assert.IsInstanceOf<MemberAccessExpressionNode>(innerCondition.left);
        Assert.AreEqual("5", AssertInstanceOfAndReturn<LiteralNode>(innerCondition.right).rawValue);

        OperatorNode innerSelection = AssertOpNode(innerTernary.right, OperatorType.TernarySelection);
        Assert.AreEqual("1", AssertInstanceOfAndReturn<LiteralNode>(innerSelection.left).rawValue);
        Assert.AreEqual("default", AssertInstanceOfAndReturn<LiteralNode>(innerSelection.right).rawValue);

        Assert.AreEqual("default", AssertInstanceOfAndReturn<LiteralNode>(outerSelection.right).rawValue);
    }

    [Test]
    public void Parse_Ternary_WithSafeAccess_Coalesce() {
        ASTNode root = ExpressionParser.Parse(("thing != null ? thing?.floatValue ?? 10 > 5 ? 1"));

        OperatorNode outerTernary = AssertOpNode(root, OperatorType.TernaryCondition);

        OperatorNode outerCondition = AssertOpNode(outerTernary.left, OperatorType.NotEquals);
        OperatorNode outerSelection = AssertOpNode(outerTernary.right, OperatorType.TernarySelection);

        Assert.AreEqual("thing", AssertInstanceOfAndReturn<IdentifierNode>(outerCondition.left).name);
        Assert.AreEqual("null", AssertInstanceOfAndReturn<LiteralNode>(outerCondition.right).rawValue);

        OperatorNode innerTernary = AssertOpNode(outerSelection.left, OperatorType.TernaryCondition);

        OperatorNode innerCondition = AssertInstanceOfAndReturn<OperatorNode>(innerTernary.left);
        OperatorNode coalesce = AssertOpNode(innerCondition.left, OperatorType.Coalesce);

        Assert.IsInstanceOf<MemberAccessExpressionNode>(coalesce.left);
        Assert.IsInstanceOf<LiteralNode>(coalesce.right);

        Assert.AreEqual("5", AssertInstanceOfAndReturn<LiteralNode>(innerCondition.right).rawValue);

        OperatorNode innerSelection = AssertOpNode(innerTernary.right, OperatorType.TernarySelection);
        Assert.AreEqual("1", AssertInstanceOfAndReturn<LiteralNode>(innerSelection.left).rawValue);
        Assert.AreEqual("default", AssertInstanceOfAndReturn<LiteralNode>(innerSelection.right).rawValue);

        Assert.AreEqual("default", AssertInstanceOfAndReturn<LiteralNode>(outerSelection.right).rawValue);
    }

    [Test]
    public void Parse_Ternary_WithParenExpression() {
        ASTNode root = ExpressionParser.Parse("thing != null ? data.ToString() : 'Anything'}");
        OperatorNode outerTernary = AssertOpNode(root, OperatorType.TernaryCondition);

        OperatorNode outerCondition = AssertOpNode(outerTernary.left, OperatorType.NotEquals);
        OperatorNode outerSelection = AssertOpNode(outerTernary.right, OperatorType.TernarySelection);

        Assert.AreEqual("thing", AssertInstanceOfAndReturn<IdentifierNode>(outerCondition.left).name);
        Assert.AreEqual("null", AssertInstanceOfAndReturn<LiteralNode>(outerCondition.right).rawValue);

        MemberAccessExpressionNode access = AssertInstanceOfAndReturn<MemberAccessExpressionNode>(outerSelection.left);
        LiteralNode anything = AssertInstanceOfAndReturn<LiteralNode>(outerSelection.right);
        Assert.AreEqual(anything.rawValue, "Anything");

    }

    [Test]
    public void Parse_Assignment() {
        ASTNode root = ExpressionParser.Parse("thing.x = 5");
    }

    [Test]
    public void Parse_PlusEqual() {
        ASTNode root = ExpressionParser.Parse("thing.x += 5");
    }

    [Test]
    public void Parse_MethodChainExpression() {
        ASTNode root = ExpressionParser.Parse(("Method()().Property"));
    }

    [Test]
    public void Parse_ParenExpression_DotAccess() {
        ASTNode root = ExpressionParser.Parse(("(5 + 8).ToString()"));
    }

}