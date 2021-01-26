using System.Collections.Generic;
using UIForia.Exceptions;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Parsing.Expressions.Tokenizer;
using UIForia.Util;

namespace UIForia.Parsing.Expressions {

    public class TypeBodyParser {

        private TokenStream tokenStream;
        private Stack<ASTNode> expressionStack;
        private Stack<OperatorNode> operatorStack;

        private static readonly LambdaArgument[] s_EmptySignature = { };

        public TypeBodyNode Parse(string input, string fileName, int lineStart) {
            tokenStream = new TokenStream(ExpressionTokenizer.Tokenize(input, StructList<ExpressionToken>.Get()));

            if (!tokenStream.HasMoreTokens) {
                throw new ParseException("Failed trying to parse empty expression");
            }

            TypeBodyNode retn = new TypeBodyNode();

            int cnt = 0;
            while (tokenStream.HasMoreTokens && cnt < 10000) {
                cnt++;
                ExpressionToken current = tokenStream.Current;

                ASTNode node = null;

                if (ParseDeclaration(ref node)) {
                    retn.nodes.Add(node);
                    continue;
                }

                if (current == tokenStream.Current) {
                    throw new ParseException($"Failed to parse {fileName}. Got stuck on {current.value}");
                }
            }

            return retn;
        }

        private bool ParseAttribute(ref AttributeNode node) {
            if (tokenStream.Current != ExpressionTokenType.ArrayAccessOpen) {
                return false;
            }

            tokenStream.Save();
            tokenStream.Advance();
            TypeLookup typeLookup = default;
            ExpressionParser parser = new ExpressionParser(tokenStream);

            if (!parser.ParseTypePath(ref typeLookup)) {
                goto fail;
            }

            tokenStream.Set(parser.GetTokenPosition());
            parser.Release(false);

            if (tokenStream.Current == ExpressionTokenType.ArrayAccessClose) {
                tokenStream.Advance();
                node = new AttributeNode() {
                    typeLookup = typeLookup
                };
                return true;
            }

            fail:
            {
                typeLookup.Release();
                parser.Release(false);
                return false;
            }
        }

        private bool ParseStatement(ref ASTNode node) {
            if (ParseReturnStatement(ref node)) return true;
            if (ParseIfStatement(ref node)) return true;
            // if (ParseSwitchStatement(ref node)) return true;
            // if (ParseForLoop(ref node)) return true;
            // if (ParseWhileLoop(ref node)) return true;
            if (ParseLocalVariableDeclaration(ref node)) return true;
            if (ParseTerminatedExpression(ref node)) return true;

            return false;
        }

        private bool ParseParenExpression(ref ASTNode node) {
            tokenStream.Save();

            TokenStream subStream = tokenStream.AdvanceAndGetSubStreamBetween(ExpressionTokenType.ParenOpen, ExpressionTokenType.ParenClose);

            if (!subStream.HasMoreTokens) {
                tokenStream.Restore();
                return false;
            }

            ASTNode expression = ExpressionParser.Parse(subStream);

            if (expression == null) {
                tokenStream.Restore();
                return false;
            }

            node = expression;
            return true;
        }

        private bool ParseTerminatedExpression(ref ASTNode node) {
            int endIndex = tokenStream.FindNextIndex(ExpressionTokenType.SemiColon);

            if (endIndex == -1) {
                return false;
            }

            tokenStream.Save();

            TokenStream subStream = tokenStream.AdvanceAndReturnSubStream(endIndex);

            tokenStream.Advance();

            ASTNode expression = ExpressionParser.Parse(subStream);

            if (expression == null) {
                tokenStream.Restore();
                return false;
            }

            node = expression;

            return true;
        }

        private bool ParseReturnStatement(ref ASTNode node) {
            if (tokenStream.Current != ExpressionTokenType.Return) {
                return false;
            }

            if (tokenStream.NextTokenIs(ExpressionTokenType.SemiColon)) {
                tokenStream.Advance(2);
                node = new ReturnStatementNode();
                return true;
            }

            tokenStream.Save();
            tokenStream.Advance();

            ASTNode expression = null;
            if (!ParseTerminatedExpression(ref expression)) {
                throw new ParseException("Failed to parse return statement");
            }

            node = new ReturnStatementNode() {
                expression = expression
            };

            return true;
        }

        private bool ParseIfStatement(ref ASTNode node) {
            if (tokenStream.Current != ExpressionTokenType.If) {
                return false;
            }

            LightList<ElseIfNode> statements = LightList<ElseIfNode>.Get();

            tokenStream.Advance();

            ASTNode condition = null;
            if (!ParseParenExpression(ref condition)) {
                throw new ParseException("Expected a condition statement wrapped in parentheses but failed.");
            }

            BlockNode thenBlock = ParseBlock();
            if (thenBlock == null) {
                throw new ParseException("Expected a block statement following an if statement but failed to parse the block");
            }

            if (tokenStream.Current != ExpressionTokenType.ElseIf && tokenStream.Current != ExpressionTokenType.Else) {
                node = new IfStatementNode() {
                    // elseIfStatements = statements.ToArray(),
                    condition = condition,
                    thenBlock = thenBlock
                };
                return true;
            }

            while (tokenStream.Current == ExpressionTokenType.ElseIf) {
                tokenStream.Advance();

                ASTNode elseIfCondition = null;

                if (!ParseParenExpression(ref elseIfCondition)) {
                    throw new ParseException("Expected a condition statement wrapped in parentheses but failed.");
                }

                BlockNode block = ParseBlock();

                if (block == null) {
                    throw new ParseException("Expected a block statement following an if statement but failed to parse the block");
                }

                statements.Add(new ElseIfNode() {
                    condition = elseIfCondition,
                    thenBlock = block
                });
            }

            BlockNode elseBlock = null;
            if (tokenStream.Current == ExpressionTokenType.Else) {
                tokenStream.Advance();
                elseBlock = ParseBlock();

                if (elseBlock == null) {
                    throw new ParseException("Expected a block statement following an else statement but failed to parse the block");
                }
            }

            node = new IfStatementNode() {
                condition = condition,
                thenBlock = thenBlock,
                elseIfStatements = statements.size == 0 ? null : statements.ToArray(),
                elseBlock = elseBlock
            };

            statements.Release();

            return true;
        }

        private BlockNode ParseBlock() {
            BlockNode retn = new BlockNode();

            int expressionMatch = tokenStream.FindMatchingIndexNoAdvance(ExpressionTokenType.ExpressionOpen, ExpressionTokenType.ExpressionClose);

            if (expressionMatch == -1) {
                retn.Release();
                return null;
            }

            tokenStream.Save();

            tokenStream.Advance();

            while (tokenStream.CurrentIndex != expressionMatch) {
                ASTNode statement = null;

                int current = tokenStream.CurrentIndex;

                if (!ParseStatement(ref statement)) {
                    retn.Release();
                    tokenStream.Restore();
                    return null;
                }

                if (current == tokenStream.CurrentIndex) {
                    throw new ParseException("fail recurse");
                }

                retn.statements.Add(statement);
            }

            tokenStream.Advance();

            return retn;
        }

        private bool ParseLocalVariableDeclaration(ref ASTNode node) {
            ExpressionParser parser = default;
            TypeLookup typeLookup = default;

            if (tokenStream.Current == ExpressionTokenType.Var) {
                if (!tokenStream.NextTokenIs(ExpressionTokenType.Identifier)) {
                    return false;
                }

                tokenStream.Advance();
            }
            else if (tokenStream.Current == ExpressionTokenType.Identifier) {
                tokenStream.Save();
                parser = new ExpressionParser(tokenStream);
                if (!parser.ParseTypePath(ref typeLookup)) {
                    goto fail;
                }

                tokenStream.Set(parser.GetTokenPosition());
                parser.Release(false);
            }
            else {
                goto fail;
            }

            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                goto fail;
            }

            string name = tokenStream.Current.value;

            tokenStream.Advance();

            if (tokenStream.Current == ExpressionTokenType.SemiColon) {
                tokenStream.Advance();
                node = new LocalVariableNode() {
                    name = name,
                    typeLookup = typeLookup // will be default for var
                };
                return true;
            }

            if (tokenStream.Current == ExpressionTokenType.Assign) {
                // todo -- would fail on this: var x = new F(() => { return x; });

                tokenStream.Advance();

                ASTNode expression = null;
                if (!ParseTerminatedExpression(ref expression)) {
                    goto fail;
                }

                node = new LocalVariableNode() {
                    name = name,
                    value = expression,
                    typeLookup = typeLookup // will be default for var
                };

                return true;
            }

            fail:
            {
                parser.Release(false);
                typeLookup.Release();
                tokenStream.Restore();
                return false;
            }
        }

        private bool ParseDeclaration(ref ASTNode node) {
            AttributeNode attrNode = null;
            LightList<AttributeNode> attributes = LightList<AttributeNode>.Get();
            while (ParseAttribute(ref attrNode)) {
                attributes.Add(attrNode);
                if (tokenStream.Current != ExpressionTokenType.ArrayAccessOpen) {
                    break;
                }
            }

            if (attributes.size == 0) {
                LightList<AttributeNode>.Release(ref attributes);
            }

            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                return false;
            }

            // modifiers? -> returnType -> name -> signature -> openBrace * closeBrace

            tokenStream.Save();

            bool isStatic = false;

            if (tokenStream.Current == "static") {
                isStatic = true;
                tokenStream.Advance();
            }

            ExpressionParser parser = new ExpressionParser(tokenStream);
            StructList<LambdaArgument> signature = null;
            TypeLookup typeLookup = default;

            if (!parser.ParseTypePath(ref typeLookup)) {
                goto fail;
            }

            tokenStream.Set(parser.GetTokenPosition());
            parser.Release(false);

            if (tokenStream.Current != ExpressionTokenType.Identifier) {
                goto fail;
            }

            string name = tokenStream.Current.value;

            tokenStream.Advance();

            // if semi colon then we have a field!
            if (tokenStream.Current == ExpressionTokenType.SemiColon) {
                tokenStream.Advance();
                node = new FieldNode() {
                    name = name,
                    isStatic = isStatic,
                    attributes = attributes,
                    typeLookup = typeLookup
                };
                return true;
            }

            if (tokenStream.Current != ExpressionTokenType.ParenOpen) {
                goto fail;
            }

            signature = StructList<LambdaArgument>.Get();

            if (tokenStream.NextTokenIs(ExpressionTokenType.ParenClose)) {
                tokenStream.Advance(2);
            }
            else {
                int matchingIndex = tokenStream.FindMatchingIndex(ExpressionTokenType.ParenOpen, ExpressionTokenType.ParenClose);

                if (matchingIndex == -1) {
                    goto fail;
                }

                TokenStream subStream = tokenStream.AdvanceAndReturnSubStream(matchingIndex);
                subStream.Advance();
                tokenStream.Advance();
                if (!ExpressionParser.ParseSignature(subStream, signature)) {
                    goto fail;
                }

                for (int i = 0; i < signature.size; i++) {
                    if (signature.array[i].type == null) {
                        throw new ParseException($"When defining a method you must specify a type for all arguments. Found identifier {signature.array[i].identifier} but no type was given.");
                    }
                }
            }

            if (tokenStream.Current != ExpressionTokenType.ExpressionOpen) {
                goto fail;
            }

            BlockNode block = ParseBlock();

            node = new MethodNode() {
                body = block,
                returnTypeLookup = typeLookup,
                attributes = attributes,
                name = name,
                isStatic = isStatic,
                signatureList = signature != null ? signature.ToArray() : s_EmptySignature
            };

            StructList<LambdaArgument>.Release(ref signature);
            parser.Release(false);

            return true;

            fail:
            {
                tokenStream.Restore();
                parser.Release(false);
                typeLookup.Release();
                signature?.Release();
                return false;
            }
        }

    }

    public class BlockNode : ASTNode {

        public LightList<ASTNode> statements = LightList<ASTNode>.Get();

        public BlockNode() {
            type = ASTNodeType.Block;
        }

        public override void Release() {
            for (int i = 0; i < statements.size; i++) {
                statements.array[i].Release();
            }

            LightList<ASTNode>.Release(ref statements);
        }

    }

    public class TypeBodyNode : ASTNode {

        public LightList<ASTNode> nodes = new LightList<ASTNode>();

        public override void Release() {
            for (int i = 0; i < nodes.size; i++) {
                nodes[i].Release();
            }
        }

    }

    public class AttributeNode : ASTNode {

        public TypeLookup typeLookup;

        public override void Release() {
            typeLookup.Release();
        }

    }

    public abstract class DeclarationNode : ASTNode {

        public string name;
        public bool isStatic;
        public LightList<AttributeNode> attributes;

        public override void Release() {
            if (attributes == null) return;
            for (int i = 0; i < attributes.size; i++) {
                attributes[i].Release();
            }
        }

    }

    public class FieldNode : DeclarationNode {

        public TypeLookup typeLookup;

        public FieldNode() {
            type = ASTNodeType.Field;
        }

        public override void Release() {
            base.Release();
            typeLookup.Release();
        }

    }

    public class MethodNode : DeclarationNode {

        public TypeLookup returnTypeLookup;
        public LambdaArgument[] signatureList;
        public BlockNode body;

        public MethodNode() {
            type = ASTNodeType.Method;
        }
        // todo -- modifier list

        public override void Release() {
            base.Release();
            returnTypeLookup.Release();

            body?.Release();
            if (signatureList == null) return;

            for (int i = 0; i < signatureList.Length; i++) {
                signatureList[i].type?.Release();
            }
        }

    }

    public class LocalVariableNode : ASTNode {

        public string name;
        public TypeLookup typeLookup;
        public ASTNode value;

        public LocalVariableNode() {
            type = ASTNodeType.VariableDeclaration;
        }

        public override void Release() {
            typeLookup.Release();
            value?.Release();
        }

    }

    public class IfStatementNode : ASTNode {

        public ASTNode condition;
        public BlockNode elseBlock;
        public BlockNode thenBlock;
        public ElseIfNode[] elseIfStatements;

        public IfStatementNode() {
            type = ASTNodeType.IfStatement;
        }

        public override void Release() {
            thenBlock?.Release();
            condition?.Release();
            elseBlock?.Release();
            if (elseIfStatements != null) {
                for (int i = 0; i < elseIfStatements.Length; i++) {
                    elseIfStatements[i].Release();
                }
            }
        }

    }

    public class ElseIfNode : ASTNode {

        public ASTNode condition;
        public BlockNode thenBlock;

        public ElseIfNode() {
            type = ASTNodeType.ElseIf;
        }

        public override void Release() {
            condition?.Release();
            thenBlock?.Release();
        }

    }

    public class ReturnStatementNode : ASTNode {

        public ASTNode expression;

        public ReturnStatementNode() {
            type = ASTNodeType.Return;
        }

        public override void Release() {
            expression?.Release();
        }

    }

}