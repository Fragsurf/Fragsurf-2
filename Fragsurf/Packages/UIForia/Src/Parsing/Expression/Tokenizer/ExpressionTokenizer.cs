using System;
using UIForia.Exceptions;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Parsing.Expressions.Tokenizer {

    public static class ExpressionTokenizer {

        private static readonly char stringCharacter = '\'';

        private static void TryReadCharacterSequence(TokenizerContext context, string match1, string match2, ExpressionTokenType expressionTokenType, StructList<ExpressionToken> output) {
            
            if (context.ptr + match1.Length > context.input.Length) return;

            int ptr = context.ptr;
            
            for (int i = 0; i < match1.Length; i++) {
                if (context.input[ptr++] != match1[i]) {
                    return;
                }
            }

            while (ptr < context.input.Length) {
                if (char.IsWhiteSpace(context.input[ptr])) {
                    ptr++;
                }
                else {
                    break;
                }
            }
            
            for (int i = 0; i < match2.Length; i++) {
                if (context.input[ptr++] != match2[i]) {
                    return;
                }
            }
            
            output.Add(new ExpressionToken(expressionTokenType, match1 + " " + match2, context.line, context.column));
            TryConsumeWhiteSpace(context.Advance(ptr - context.ptr));
        }
              
        private static void TryReadCharacters(TokenizerContext context, string match, ExpressionTokenType expressionTokenType, StructList<ExpressionToken> output) {
            if (context.ptr + match.Length > context.input.Length) return;
            for (int i = 0; i < match.Length; i++) {
                if (context.input[context.ptr + i] != match[i]) {
                    return;
                }
            }

            output.Add(new ExpressionToken(expressionTokenType, match, context.line, context.column));
            TryConsumeWhiteSpace(context.Advance(match.Length));
        }

        private static void TryConsumeWhiteSpace(TokenizerContext context) {
            if (context.IsConsumed()) {
                return;
            }

            while (context.HasMore() && char.IsWhiteSpace(context.input[context.ptr])) {
                context.Advance();
            }
        }

        private static void TryConsumeComment(TokenizerContext context) {
            if (context.ptr + 1 >= context.input.Length) {
                return;
            }

            if (context.input[context.ptr] == '/' && context.input[context.ptr + 1] == '/') {
                while (context.HasMore()) {
                    char current = context.input[context.ptr];
                    if (current == '\n') {
                        TryConsumeWhiteSpace(context);
                        TryConsumeComment(context);
                        return;
                    }

                    context.Advance();
                }

                return;
            }

            if (context.input[context.ptr] != '/' || context.input[context.ptr + 1] != '*') {
                return;
            }

            context.Advance(2);
            while (context.HasMore()) {
                if (context.input[context.ptr] == '*' && context.input[context.ptr + 1] == '/') {
                    context.Advance(2);
                    TryConsumeWhiteSpace(context);
                    TryConsumeComment(context);
                    return;
                }

                context.Advance();
            }
        }

        private static void TryReadDigit(TokenizerContext context, StructList<ExpressionToken> output) {
            if (context.IsConsumed()) return;
            bool foundDot = false;
            int startIndex = context.ptr;

            context.Save();
            if (context.input[context.ptr] == '-') {
                context.Advance();
            }

            if (!char.IsDigit(context.input[context.ptr])) {
                context.Restore();
                return;
            }

            while (context.HasMore() && (char.IsDigit(context.input[context.ptr]) || (!foundDot && context.input[context.ptr] == '.'))) {
                if (context.input[context.ptr] == '.') {
                    foundDot = true;
                }

                context.Advance();
            }


            if (context.HasMore()) {
                char next = context.input[context.ptr];
                // todo -- enable the below to making parsing numbers better in the compiler (since we already know what type to try to parse it as)
                //ExpressionTokenType type = ExpressionTokenType.Number;
//                if (next == 'f') {
//                    type = ExpressionTokenType.Number_Float;
//                }
//
//                if (next == 'd') {
//                    type = ExpressionTokenType.Number_Double;
//                }
//
//                if (next == 'l') {
//                    type = ExpressionTokenType.Number_Long;
//                }
//
//                if (next == 'u') {
//                    // todo -- check for ul here
//                    type = ExpressionTokenType.Number_UInt;
//                }
//
//                if (next == 'm') {
//                    type = ExpressionTokenType.Number_Decimal;
//                }

                if (next == 'f' || next == 'd' || next == 'l' || next == 'u' || next == 'm') {
                    if (next != '.') {
                        context.Advance();
                    }
                }
            }

//            if (context.HasMore() 
//                && context.input[context.ptr] == 'f'
//                && context.input[context.ptr - 1] != '.') {
//                context.Advance();
//            }

            int length = context.ptr - startIndex;
            string digit = context.input.Substring(startIndex, length);

            context.Restore();
            output.Add(new ExpressionToken(ExpressionTokenType.Number, digit, context.line, context.column));
            context.Advance(length);

            TryConsumeWhiteSpace(context);
        }

        private static void TryReadIdentifier(TokenizerContext context, StructList<ExpressionToken> output) {
            if (context.IsConsumed()) return;
            int start = context.ptr;
            char first = context.input[context.ptr];
            
            if (!char.IsLetter(first) && first != '_' && first != '$') return;

            context.Save();
            
            while (context.HasMore()) {
                char character = context.input[context.ptr];
                
                if (!(char.IsLetterOrDigit(character) || character == '_' || character == '$')) {
                    break;
                }

                context.Advance();
            }

            int length = context.ptr - start;
            string identifier = context.input.Substring(start, length);
            context.Restore();
            output.Add(TransformIdentifierToTokenType(context, identifier));
            context.Advance(length);
            TryConsumeWhiteSpace(context);
        }

        private static ExpressionToken TransformIdentifierToTokenType(TokenizerContext context, string identifier) {
            switch (identifier) {
                case "var": return new ExpressionToken(ExpressionTokenType.Var, identifier, context.line, context.column);
                case "if": return new ExpressionToken(ExpressionTokenType.If, identifier, context.line, context.column);
                case "else": return new ExpressionToken(ExpressionTokenType.Else, identifier, context.line, context.column);
                case "for": return new ExpressionToken(ExpressionTokenType.For, identifier, context.line, context.column);
                case "while": return new ExpressionToken(ExpressionTokenType.While, identifier, context.line, context.column);
                case "return": return new ExpressionToken(ExpressionTokenType.Return, identifier, context.line, context.column);
                case "null": return new ExpressionToken(ExpressionTokenType.Null, identifier, context.line, context.column);
                case "true": return new ExpressionToken(ExpressionTokenType.Boolean, identifier, context.line, context.column);
                case "false": return new ExpressionToken(ExpressionTokenType.Boolean, identifier, context.line, context.column);
                case "as": return new ExpressionToken(ExpressionTokenType.As, identifier, context.line, context.column);
                case "is": return new ExpressionToken(ExpressionTokenType.Is, identifier, context.line, context.column);
                case "new": return new ExpressionToken(ExpressionTokenType.New, identifier, context.line, context.column);
                case "typeof": return new ExpressionToken(ExpressionTokenType.TypeOf, identifier, context.line, context.column);
                case "default": return new ExpressionToken(ExpressionTokenType.Default, identifier, context.line, context.column);

                default: {
                    return new ExpressionToken(ExpressionTokenType.Identifier, identifier, context.line, context.column);
                }
            }
        }

        // todo handle {} inside of strings
        // read until end or unescaped {
        // if unescaped { found, find matching index
        // add token for string 0 to index({)
        // add + token
        // run parse loop on contents of {}

        private static void TryReadString(TokenizerContext context, StructList<ExpressionToken> output) {
            if (context.IsConsumed()) return;
            if (context.input[context.ptr] != stringCharacter) return;
            int start = context.ptr;

            context.Save();
            context.Advance();

            while (context.HasMore() && context.input[context.ptr] != stringCharacter) {
                context.Advance();
            }

            if (context.IsConsumed()) {
                context.Restore();
                return;
            }

            if (context.input[context.ptr] != stringCharacter) {
                context.Restore();
                return;
            }

            context.Advance();

            // strip the quotes
            // "abc" 
            // 01234
            int length = context.ptr - start;
            string substring = context.input.Substring(start + 1, length - 2);
            context.Restore();
            output.Add(new ExpressionToken(ExpressionTokenType.String, substring, context.line, context.column));
            context.Advance(length);

            TryConsumeWhiteSpace(context);
        }

        public enum ClassBodyTokenType {

            Identifier,
            Statement,
            Block,
            BlockBody,
            Attribute,
            SemiColon
        
        }

        public struct ClassBodyToken {

            public ClassBodyTokenType type;
            public int charStart;
            public int charEnd;

        }

       
        // todo take optional file / line number for error message
        public static StructList<ExpressionToken> Tokenize(string input, StructList<ExpressionToken> retn = null) {
            StructList<ExpressionToken> output = retn ?? new StructList<ExpressionToken>();
            TokenizerContext context = new TokenizerContext(input);
            TryConsumeWhiteSpace(context);

            TryConsumeWhiteSpace(context);
            while (context.ptr < input.Length) {
                int start = context.ptr;

                TryConsumeComment(context);

                TryReadCharacters(context, "@", ExpressionTokenType.At, output);
                TryReadCharacters(context, "&&", ExpressionTokenType.AndAlso, output);
                TryReadCharacters(context, "??", ExpressionTokenType.Coalesce, output);
                TryReadCharacters(context, "?.", ExpressionTokenType.Elvis, output);
                TryReadCharacters(context, "||", ExpressionTokenType.OrElse, output);
                TryReadCharacters(context, "=>", ExpressionTokenType.LambdaArrow, output);
                TryReadCharacters(context, "==", ExpressionTokenType.Equals, output);
                TryReadCharacters(context, "!=", ExpressionTokenType.NotEquals, output);
                
                TryReadCharacters(context, "++", ExpressionTokenType.Increment, output);
                TryReadCharacters(context, "--", ExpressionTokenType.Decrement, output);
                
                TryReadCharacters(context, "=", ExpressionTokenType.Assign, output);
                TryReadCharacters(context, "+=", ExpressionTokenType.AddAssign, output);
                TryReadCharacters(context, "-=", ExpressionTokenType.SubtractAssign, output);
                TryReadCharacters(context, "*=", ExpressionTokenType.MultiplyAssign, output);
                TryReadCharacters(context, "/=", ExpressionTokenType.DivideAssign, output);
                TryReadCharacters(context, "%=", ExpressionTokenType.ModAssign, output);
                TryReadCharacters(context, "&=", ExpressionTokenType.AndAssign, output);
                TryReadCharacters(context, "|=", ExpressionTokenType.OrAssign, output);
                TryReadCharacters(context, "^=", ExpressionTokenType.XorAssign, output);
                TryReadCharacters(context, "<<=", ExpressionTokenType.LeftShiftAssign, output);
                TryReadCharacters(context, ">>=", ExpressionTokenType.RightShiftAssign, output);
                
                TryReadCharacters(context, ">=", ExpressionTokenType.GreaterThanEqualTo, output);
                TryReadCharacters(context, "<=", ExpressionTokenType.LessThanEqualTo, output);
                TryReadCharacters(context, ">", ExpressionTokenType.GreaterThan, output);
                TryReadCharacters(context, "<", ExpressionTokenType.LessThan, output);
                TryReadCharacters(context, "<", ExpressionTokenType.LessThan, output);

                TryReadCharacters(context, "!", ExpressionTokenType.Not, output);
                TryReadCharacters(context, "+", ExpressionTokenType.Plus, output);
                TryReadCharacters(context, "-", ExpressionTokenType.Minus, output);
                TryReadCharacters(context, "/", ExpressionTokenType.Divide, output);
                TryReadCharacters(context, "*", ExpressionTokenType.Times, output);
                TryReadCharacters(context, "%", ExpressionTokenType.Mod, output);
                TryReadCharacters(context, "~", ExpressionTokenType.BinaryNot, output);
                TryReadCharacters(context, "|", ExpressionTokenType.BinaryOr, output);
                TryReadCharacters(context, "&", ExpressionTokenType.BinaryAnd, output);
                TryReadCharacters(context, "^", ExpressionTokenType.BinaryXor, output);
                TryReadCharacters(context, "?", ExpressionTokenType.QuestionMark, output);
                TryReadCharacters(context, ":", ExpressionTokenType.Colon, output);
                TryReadCharacters(context, ";", ExpressionTokenType.SemiColon, output);

                TryReadCharacters(context, ".", ExpressionTokenType.Dot, output);
                TryReadCharacters(context, ",", ExpressionTokenType.Comma, output);
                TryReadCharacters(context, "(", ExpressionTokenType.ParenOpen, output);
                TryReadCharacters(context, ")", ExpressionTokenType.ParenClose, output);
                TryReadCharacters(context, "[", ExpressionTokenType.ArrayAccessOpen, output);
                TryReadCharacters(context, "]", ExpressionTokenType.ArrayAccessClose, output);
                TryReadCharacters(context, "{", ExpressionTokenType.ExpressionOpen, output);
                TryReadCharacters(context, "}", ExpressionTokenType.ExpressionClose, output);
                
                TryReadCharacters(context, "if", ExpressionTokenType.If, output);
                TryReadCharacterSequence(context, "else", "if", ExpressionTokenType.ElseIf, output);
                TryReadCharacters(context, "else", ExpressionTokenType.Else, output);

                TryReadDigit(context, output);
                TryReadString(context, output);
                TryReadIdentifier(context, output);
                TryConsumeWhiteSpace(context);

                if (context.ptr == start && context.ptr < input.Length) {
                    int nextNewLine = input.IndexOf("\n", context.ptr + 1, input.Length - context.ptr - 1, StringComparison.Ordinal);
                    nextNewLine = Mathf.Clamp(nextNewLine, context.ptr + 1, input.Length - 1);
                    string errorLine = input.Substring(context.ptr, nextNewLine - context.ptr);
                    throw new ParseException($"Tokenizer failed at line {context.line}, column {context.column}.\n" +
                                             $" Processed {input.Substring(0, context.ptr)}\n" +
                                             $" ...but then got stuck on {errorLine}.\n");
                }
            }

            return output;
        }

    }

}