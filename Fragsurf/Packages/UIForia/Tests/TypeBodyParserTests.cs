using System.Diagnostics;
using NUnit.Framework;
using UIForia.Parsing.Expressions;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Util;

namespace Tests {

    public class TypeBodyParserTests {

        [Test]
        public void ParseField() {
            TypeBodyParser parser = new TypeBodyParser();

            const string input = @"
                int v0;
                string v1;
                System.Collections.Generic.List<UnityEngine.Vector3> v2;
            ";

            TypeBodyNode astNode = parser.Parse(input, "", 0);
            Assert.AreEqual(3, astNode.nodes.size);
            Assert.AreEqual("v0", ((FieldNode) astNode.nodes[0]).name);
            Assert.AreEqual("v1", ((FieldNode) astNode.nodes[1]).name);
            Assert.AreEqual("v2", ((FieldNode) astNode.nodes[2]).name);
        }

        [Test]
        public void ParseFieldWithAttribute() {
            TypeBodyParser parser = new TypeBodyParser();

            const string input = @"
                [Required]
                System.Collections.Generic.List<UnityEngine.Vector3> v2;

                [Attr0]
                [Attr1]
                int value;
                string str;

            ";

            TypeBodyNode astNode = parser.Parse(input, "", 0);
            LightList<AttributeNode> attributes = ((FieldNode) astNode.nodes[0]).attributes;

            Assert.AreEqual(3, astNode.nodes.size);
            Assert.AreEqual(1, attributes.size);
            Assert.AreEqual("v2", ((FieldNode) astNode.nodes[0]).name);
            Assert.AreEqual("Required", attributes[0].typeLookup.typeName);
            attributes = ((FieldNode) astNode.nodes[1]).attributes;
            Assert.AreEqual(2, attributes.size);
            Assert.AreEqual("Attr0", attributes[0].typeLookup.typeName);
            Assert.AreEqual("Attr1", attributes[1].typeLookup.typeName);

            Assert.AreEqual("value", ((FieldNode) astNode.nodes[1]).name);
            Assert.AreEqual("str", ((FieldNode) astNode.nodes[2]).name);
        }

        [Test]
        public void ParseMethod_Args0() {
            TypeBodyParser parser = new TypeBodyParser();

            const string input = @"
                int GetValue() {
                    return someValue;
                }
            ";

            TypeBodyNode astNode = parser.Parse(input, "", 0);
            Assert.AreEqual(1, astNode.nodes.size);
            MethodNode methodNode = astNode.nodes[0] as MethodNode;
            Assert.AreEqual("GetValue", methodNode.name);
            Assert.AreEqual("int", methodNode.returnTypeLookup.typeName);
            Assert.AreEqual(0, methodNode.signatureList.Length);
        }

        [Test]
        public void ParseMethod_Args1() {
            TypeBodyParser parser = new TypeBodyParser();

            const string input = @"
                int GetValue(int someValue) {
                    return someValue;
                }
            ";

            TypeBodyNode astNode = parser.Parse(input, "", 0);
            Assert.AreEqual(1, astNode.nodes.size);
            MethodNode methodNode = astNode.nodes[0] as MethodNode;
            Assert.AreEqual("GetValue", methodNode.name);
            Assert.AreEqual("int", methodNode.returnTypeLookup.typeName);
            Assert.AreEqual(1, methodNode.signatureList.Length);
            Assert.AreEqual("int", methodNode.signatureList[0].type.Value.typeName);
            Assert.AreEqual("someValue", methodNode.signatureList[0].identifier);
        }

        [Test]
        public void ParseMethod_Args2() {
            TypeBodyParser parser = new TypeBodyParser();

            const string input = @"
                int GetValue(int someValue, System.Collections.Generic.Dictionary<int, UnityEngine.Vector3> dict) {
                    return someValue;
                }
            ";

            TypeBodyNode astNode = parser.Parse(input, "", 0);
            Assert.AreEqual(1, astNode.nodes.size);
            MethodNode methodNode = astNode.nodes[0] as MethodNode;
            Assert.AreEqual("GetValue", methodNode.name);
            Assert.AreEqual("int", methodNode.returnTypeLookup.typeName);
            Assert.AreEqual(2, methodNode.signatureList.Length);
            Assert.AreEqual("int", methodNode.signatureList[0].type.Value.typeName);
            Assert.AreEqual("someValue", methodNode.signatureList[0].identifier);
            Assert.AreEqual("Dictionary", methodNode.signatureList[1].type.Value.typeName);
            Assert.AreEqual("dict", methodNode.signatureList[1].identifier);
        }

        [Test]
        public void ParseMethodBody() {
            TypeBodyParser parser = new TypeBodyParser();

            const string input = @"
                int GetValue(int someValue, System.Collections.Generic.Dictionary<int, UnityEngine.Vector3> dict) {
                    var f;
                    int localVar = 4;
                    string str;
                    return someValue;
                }
            ";

            TypeBodyNode astNode = parser.Parse(input, "", 0);
            Assert.AreEqual(1, astNode.nodes.size);
            MethodNode methodNode = (MethodNode) astNode.nodes[0];

            Assert.AreEqual(4, methodNode.body.statements.size);
            Assert.IsInstanceOf<LocalVariableNode>(methodNode.body.statements[0]);
            Assert.IsInstanceOf<LocalVariableNode>(methodNode.body.statements[1]);
            Assert.IsInstanceOf<LocalVariableNode>(methodNode.body.statements[2]);
            Assert.IsInstanceOf<ReturnStatementNode>(methodNode.body.statements[3]);
            LocalVariableNode f = (LocalVariableNode) methodNode.body.statements[0];
            LocalVariableNode localVar = (LocalVariableNode) methodNode.body.statements[1];
            LocalVariableNode str = (LocalVariableNode) methodNode.body.statements[2];
            ReturnStatementNode retn = (ReturnStatementNode) methodNode.body.statements[3];

            Assert.AreEqual("f", f.name);
            Assert.AreEqual(default, f.typeLookup.typeName);

            Assert.AreEqual("localVar", localVar.name);
            Assert.AreEqual("int", localVar.typeLookup.typeName);
            Assert.IsInstanceOf<LiteralNode>(localVar.value);

            Assert.AreEqual("str", str.name);
            Assert.AreEqual("string", str.typeLookup.typeName);

            IdentifierNode retnVal = (IdentifierNode) retn.expression;
            Assert.AreEqual("someValue", retnVal.name);
        }

         [Test]
        public void ParseMethodBody_IfStatement() {
            TypeBodyParser parser = new TypeBodyParser();

            const string input = @"
                int GetValue(int someValue, System.Collections.Generic.Dictionary<int, UnityEngine.Vector3> dict) {

                    if(dict != null) {
                        return dict;
                    }

                    return someValue;
                }
            ";

            TypeBodyNode astNode = parser.Parse(input, "", 0);
            Assert.AreEqual(1, astNode.nodes.size);
            MethodNode methodNode = (MethodNode) astNode.nodes[0];

            Assert.AreEqual(2, methodNode.body.statements.size);
            
            Assert.IsInstanceOf<IfStatementNode>(methodNode.body.statements[0]);
            Assert.IsInstanceOf<ReturnStatementNode>(methodNode.body.statements[1]);

            IfStatementNode ifNode = (IfStatementNode)methodNode.body.statements[0];

            OperatorNode opNode = (OperatorNode)ifNode.condition;
            IdentifierNode left = (IdentifierNode) opNode.left;
            LiteralNode right = (LiteralNode) opNode.right;

            Assert.AreEqual("dict", left.name);
            Assert.AreEqual("null", right.rawValue);

        }
        
          [Test]
        public void ParseMethodBody_IfElseStatement() {
            TypeBodyParser parser = new TypeBodyParser();

            const string input = @"
                int GetValue(int someValue, System.Collections.Generic.Dictionary<int, UnityEngine.Vector3> dict) {

                    if(dict != null) {
                        return dict;
                    }
                    else {
                        dict = null;
                    }

                    return someValue;
                }
            ";

            TypeBodyNode astNode = parser.Parse(input, "", 0);
            Assert.AreEqual(1, astNode.nodes.size);
            MethodNode methodNode = (MethodNode) astNode.nodes[0];

            Assert.AreEqual(2, methodNode.body.statements.size);
            
            Assert.IsInstanceOf<IfStatementNode>(methodNode.body.statements[0]);
            Assert.IsInstanceOf<ReturnStatementNode>(methodNode.body.statements[1]);

            IfStatementNode ifNode = (IfStatementNode)methodNode.body.statements[0];

            OperatorNode opNode = (OperatorNode)ifNode.condition;
            IdentifierNode left = (IdentifierNode) opNode.left;
            LiteralNode right = (LiteralNode) opNode.right;

            Assert.AreEqual("dict", left.name);
            Assert.AreEqual("null", right.rawValue);

            BlockNode elseBlock = ifNode.elseBlock;
            Assert.AreEqual(1, elseBlock.statements.size);

        }

        
    }

}