﻿using System.Collections.Generic;
 using NUnit.Framework;
 using UIForia.Exceptions;
 using UIForia.Parsing.Expressions.Tokenizer;
 using UIForia.Util;

 [TestFixture]
    public class TokenizingTests {

        [Test]
        public void TokenizeBasicString() {
            string input = "item.thing";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual("item", tokens[0].value);
            Assert.AreEqual(".", tokens[1].value);
            Assert.AreEqual("thing", tokens[2].value);
        }

        [Test]
        public void Tokenize_Boolean() {
            string input = "true";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("true", tokens[0].value);

            input = "false";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("false", tokens[0].value);
        }

        [Test]
        public void Tokenize_Number() {
            string input = "6264.1";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("6264.1", tokens[0].value);
            Assert.AreEqual(ExpressionTokenType.Number, tokens[0].expressionTokenType);

            input = "-6264.1";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual("-", tokens[0].value);
            Assert.AreEqual("6264.1", tokens[1].value);
            Assert.AreEqual(ExpressionTokenType.Minus, tokens[0].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Number, tokens[1].expressionTokenType);
            
            input = "-6264";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual("-", tokens[0].value);
            Assert.AreEqual("6264", tokens[1].value);
            Assert.AreEqual(ExpressionTokenType.Minus, tokens[0].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Number, tokens[1].expressionTokenType);
            
            input = "-6264f";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual("-", tokens[0].value);
            Assert.AreEqual("6264f", tokens[1].value);
            Assert.AreEqual(ExpressionTokenType.Minus, tokens[0].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Number, tokens[1].expressionTokenType);
            
            input = "-6264.414f ";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual("-", tokens[0].value);
            Assert.AreEqual("6264.414f", tokens[1].value);
            Assert.AreEqual(ExpressionTokenType.Minus, tokens[0].expressionTokenType);
            
            Assert.AreEqual(ExpressionTokenType.Number, tokens[1].expressionTokenType);
            input = "6264";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("6264", tokens[0].value);
            Assert.AreEqual(ExpressionTokenType.Number, tokens[0].expressionTokenType);
        }

        [Test]
        public void Tokenize_String() {
            string input = "'some string'";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("some string", tokens[0].value);
        }

        [Test]
        public void Tokenize_Operators() {
            string input = "+";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.Plus, tokens[0].expressionTokenType);
            
            input = "-";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.Minus, tokens[0].expressionTokenType);
            
            input = "*";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.Times, tokens[0].expressionTokenType);
            
            input = "/";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.Divide, tokens[0].expressionTokenType);
            
            input = "%";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.Mod, tokens[0].expressionTokenType);
        }

        [Test]
        public void Tokenize_Conditionals() {
            string input = "&&";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.AndAlso, tokens[0].expressionTokenType);
            
            input = "||";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.OrElse, tokens[0].expressionTokenType);
            
            input = "==";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.Equals, tokens[0].expressionTokenType);
            
            input = "!=";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.NotEquals, tokens[0].expressionTokenType);
            
            input = ">";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.GreaterThan, tokens[0].expressionTokenType);
            
            input = "<";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.LessThan, tokens[0].expressionTokenType);
            
            input = ">=";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.GreaterThanEqualTo, tokens[0].expressionTokenType);
            
            input = "<=";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.LessThanEqualTo, tokens[0].expressionTokenType);
            
            input = "!";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.Not, tokens[0].expressionTokenType);
        }

        [Test]
        public void Tokenize_ArrayAccess() {
            string input = "[";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.ArrayAccessOpen, tokens[0].expressionTokenType);
            
            input = "]";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.ArrayAccessClose, tokens[0].expressionTokenType);
        }

        [Test]
        public void Tokenize_ExpressionStatement() {
            string input = "{";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.ExpressionOpen, tokens[0].expressionTokenType);
            
            input = "}";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.ExpressionClose, tokens[0].expressionTokenType);
        }

       
        [Test]
        public void Tokenize_CompoundOperatorExpression() {
            string input = "52 + 2.4";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.Number, tokens[0].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Plus, tokens[1].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Number, tokens[2].expressionTokenType);
            
            input = "-52 * 714";
            tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(4, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.Minus, tokens[0].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Number, tokens[1].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Times, tokens[2].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Number, tokens[3].expressionTokenType);
        }

        [Test]
        public void Tokenize_CompoundPropertyAccess() {
            string input = "366 + something.first.second.third";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(9, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.Number, tokens[0].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Plus, tokens[1].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Identifier, tokens[2].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Dot, tokens[3].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Identifier, tokens[4].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Dot, tokens[5].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Identifier, tokens[6].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Dot, tokens[7].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Identifier, tokens[8].expressionTokenType);
        }

        [Test]
        public void Tokenize_CompoundArrayAccess() {
            string input = "366 + something[first]second.third";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            Assert.AreEqual(9, tokens.Count);
            Assert.AreEqual(ExpressionTokenType.Number, tokens[0].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Plus, tokens[1].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Identifier, tokens[2].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.ArrayAccessOpen, tokens[3].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Identifier, tokens[4].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.ArrayAccessClose, tokens[5].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Identifier, tokens[6].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Dot, tokens[7].expressionTokenType);
            Assert.AreEqual(ExpressionTokenType.Identifier, tokens[8].expressionTokenType);
        }

       
        
        [Test]
        public void Tokenize_ComplexUnary() {
            string input = "item != 55 && !someCondition || -(11 * 4)";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            List<ExpressionTokenType> types = new List<ExpressionTokenType>();
            
            types.Add(ExpressionTokenType.Identifier);
            types.Add(ExpressionTokenType.NotEquals);
            types.Add(ExpressionTokenType.Number);
            types.Add(ExpressionTokenType.AndAlso);
            types.Add(ExpressionTokenType.Not);
            types.Add(ExpressionTokenType.Identifier);
            types.Add(ExpressionTokenType.OrElse);
            types.Add(ExpressionTokenType.Minus);
            types.Add(ExpressionTokenType.ParenOpen);
            types.Add(ExpressionTokenType.Number);
            types.Add(ExpressionTokenType.Times);
            types.Add(ExpressionTokenType.Number);
            types.Add(ExpressionTokenType.ParenClose);
            
            AssertTokenTypes(types, tokens);

        }

        [Test]
        public void Tokenize_SpecialIdentifier() {
            string input = "1 + $ident";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            List<ExpressionTokenType> types = new List<ExpressionTokenType>();
            types.Add(ExpressionTokenType.Number);
            types.Add(ExpressionTokenType.Plus);
            types.Add(ExpressionTokenType.Identifier);
            
            AssertTokenTypes(types, tokens);
        }

        [Test]
        public void Tokenize_Comma() {
            string input = "method(1, 2, 3)";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            List<ExpressionTokenType> types = new List<ExpressionTokenType>();
            types.Add(ExpressionTokenType.Identifier);
            types.Add(ExpressionTokenType.ParenOpen);
            types.Add(ExpressionTokenType.Number);
            types.Add(ExpressionTokenType.Comma);
            types.Add(ExpressionTokenType.Number);
            types.Add(ExpressionTokenType.Comma);
            types.Add(ExpressionTokenType.Number);
            types.Add(ExpressionTokenType.ParenClose);
            
            AssertTokenTypes(types, tokens);
        }

        [Test]
        public void Tokenize_Ternary() {
            string input = "value ? 1 : 2";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            List<ExpressionTokenType> types = new List<ExpressionTokenType>();
            types.Add(ExpressionTokenType.Identifier);
            types.Add(ExpressionTokenType.QuestionMark);
            types.Add(ExpressionTokenType.Number);
            types.Add(ExpressionTokenType.Colon);
            types.Add(ExpressionTokenType.Number);
            
            AssertTokenTypes(types, tokens);
        }
        
        [Test]
        public void FailsToTokenizeUnterminatedString() {
            Assert.Throws<ParseException>(() => {
                ExpressionTokenizer.Tokenize("'havelstring");
            });
        }

        [Test]
        public void AllowKeyWordAsIdentifierPartInExpression() {
            string input = "isThing ? 1 : 2";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            List<ExpressionTokenType> types = new List<ExpressionTokenType>();
            types.Add(ExpressionTokenType.Identifier);
            types.Add(ExpressionTokenType.QuestionMark);
            types.Add(ExpressionTokenType.Number);
            types.Add(ExpressionTokenType.Colon);
            types.Add(ExpressionTokenType.Number);
            AssertTokenTypes(types, tokens);
        }
        
        [Test]
        public void AllowKeyWordAsIdentifierPart() {
            string input = "isThing";
            StructList<ExpressionToken> tokens = ExpressionTokenizer.Tokenize(input);
            List<ExpressionTokenType> types = new List<ExpressionTokenType>();
            types.Add(ExpressionTokenType.Identifier);
            AssertTokenTypes(types, tokens);
        }
        
        private static void AssertTypesAndValues(StructList<ExpressionToken> expectedTokens, StructList<ExpressionToken> actualTokens) {
            Assert.AreEqual(expectedTokens.Count, actualTokens.Count);
            for (int i = 0; i < actualTokens.Count; i++) {
                Assert.AreEqual(expectedTokens[i].expressionTokenType, actualTokens[i].expressionTokenType);
                Assert.AreEqual(expectedTokens[i].value, actualTokens[i].value);
            } 
        }
        
        private static void AssertTokenTypes(List<ExpressionTokenType> types, StructList<ExpressionToken> tokens) {
            Assert.AreEqual(types.Count, tokens.Count);
            for (int i = 0; i < types.Count; i++) {
                Assert.AreEqual(tokens[i].expressionTokenType, types[i]);
            }
        }
    }

