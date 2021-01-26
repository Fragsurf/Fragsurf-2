using System;
using System.Collections.Generic;
using UIForia.Exceptions;
using UnityEngine;

namespace UIForia.Parsing.Style.Tokenizer {

    internal static class StyleTokenizer {

        private static void TryReadCharacters(TokenizerContext context, string match, StyleTokenType styleTokenType, List<StyleToken> output) {
            if (context.ptr + match.Length > context.input.Length) return;
            for (int i = 0; i < match.Length; i++) {
                if (context.input[context.ptr + i] != match[i]) {
                    return;
                }
            }

            output.Add(new StyleToken(styleTokenType, match, context.line, context.column));
            TryConsumeWhiteSpace(context.Advance(match.Length));
        }

        private static void TryConsumeWhiteSpace(TokenizerContext context) {
            if (context.IsConsumed()) {
                return;
            }

            while (context.ptr < context.input.Length) {
                char c = context.input[context.ptr];
                if (!(c == ' ' || c >= '\t' && c <= '\r' || (c == ' ' || c == '\x0085'))) {
                    break;
                }

                context.Advance();
            }
        }

        private static void TryConsumeComment(TokenizerContext context) {
            if (context.ptr + 1 >= context.input.Length) {
                return;
            }

            if (!(context.input[context.ptr] == '/' && context.input[context.ptr + 1] == '/')) {
                return;
            }

            while (context.HasMore()) {
                char current = context.input[context.ptr];
                if (current == '\n') {
                    TryConsumeWhiteSpace(context);
                    TryConsumeComment(context);
                    return;
                }

                context.Advance();
            }
        }

        private static void TryReadHashColor(TokenizerContext context, List<StyleToken> output) {
            if (context.IsConsumed()) return;
            if (context.input[context.ptr] != '#') return;

            int start = context.ptr;
            while (context.HasMore() && context.input[context.ptr] != ';' && !char.IsWhiteSpace(context.input[context.ptr])) {
                context.Advance();
            }

            string colorHash = context.input.Substring(start, context.ptr - start);
            output.Add(new StyleToken(StyleTokenType.HashColor, colorHash, context.line, context.column));

            TryConsumeWhiteSpace(context);
        }

        private static void TryReadDigit(TokenizerContext context, List<StyleToken> output) {
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

            // 1
            // 1.4
            // 1.4f

            while (context.HasMore() && (char.IsDigit(context.input[context.ptr]) || (!foundDot && context.input[context.ptr] == '.'))) {
                if (context.input[context.ptr] == '.') {
                    foundDot = true;
                }

                context.Advance();
            }

            int length = context.ptr - startIndex;
            string digit = context.input.Substring(startIndex, length);

            context.Restore();
            output.Add(new StyleToken(StyleTokenType.Number, digit, context.line, context.column));
            context.Advance(length);

            // a trailing f should be considered part of a float, except the f is followed by more characters like in a unit '2fr'
            // we don't want it to appear in the value, though. If it's ever used to differentiate something we should add a 
            // more specific type like StyleTokenType.Float? Also what about 'm' and 'd' postfixes?
            if (context.HasMore() && context.input[context.ptr] == 'f' && context.HasMuchMore(1) && !char.IsLetter(context.input[context.ptr + 1])) {
                context.Advance();
            }

            TryConsumeWhiteSpace(context);
        }

        private static void TryReadIdentifier(TokenizerContext context, List<StyleToken> output) {
            if (context.IsConsumed()) return;
            int start = context.ptr;
            char first = context.input[context.ptr];
            if (!char.IsLetter(first) && first != '_' && first != '$') return;

            context.Save();

            while (context.HasMore()) {
                char character = context.input[context.ptr];

                if (!(char.IsLetterOrDigit(character) || character == '_' || character == '-' || character == '$')) {
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

        private static StyleToken TransformIdentifierToTokenType(TokenizerContext context, string identifier) {
            string identifierLowerCase = identifier.ToLower();
            switch (identifierLowerCase) {
                case "use": return new StyleToken(StyleTokenType.Use, identifierLowerCase, context.line, context.column);
                case "and": return new StyleToken(StyleTokenType.And, identifierLowerCase, context.line, context.column);
                case "not": return new StyleToken(StyleTokenType.Not, identifierLowerCase, context.line, context.column);
                case "style": return new StyleToken(StyleTokenType.Style, identifierLowerCase, context.line, context.column);
                case "animation": return new StyleToken(StyleTokenType.Animation, identifierLowerCase, context.line, context.column);
                case "spritesheet": return new StyleToken(StyleTokenType.SpriteSheet, identifierLowerCase, context.line, context.column);
                case "texture": return new StyleToken(StyleTokenType.Texture, identifierLowerCase, context.line, context.column);
                case "sound": return new StyleToken(StyleTokenType.Sound, identifierLowerCase, context.line, context.column);
                case "cursor": return new StyleToken(StyleTokenType.Cursor, identifierLowerCase, context.line, context.column);
                case "export": return new StyleToken(StyleTokenType.Export, identifierLowerCase, context.line, context.column);
                case "const": return new StyleToken(StyleTokenType.Const, identifierLowerCase, context.line, context.column);
                case "import": return new StyleToken(StyleTokenType.Import, identifierLowerCase, context.line, context.column);
                case "attr": return new StyleToken(StyleTokenType.AttributeSpecifier, identifierLowerCase, context.line, context.column);
                case "true": return new StyleToken(StyleTokenType.Boolean, identifierLowerCase, context.line, context.column);
                case "false": return new StyleToken(StyleTokenType.Boolean, identifierLowerCase, context.line, context.column);
                case "from": return new StyleToken(StyleTokenType.From, identifierLowerCase, context.line, context.column);
                case "as": return new StyleToken(StyleTokenType.As, identifierLowerCase, context.line, context.column);
                case "rgba": return new StyleToken(StyleTokenType.Rgba, identifierLowerCase, context.line, context.column);
                case "rgb": return new StyleToken(StyleTokenType.Rgb, identifierLowerCase, context.line, context.column);
                case "url": return new StyleToken(StyleTokenType.Url, identifierLowerCase, context.line, context.column);
                case "enterexit": return new StyleToken(StyleTokenType.EnterExit, identifierLowerCase, context.line, context.column);
                case "exit": return new StyleToken(StyleTokenType.Exit, identifierLowerCase, context.line, context.column);
                case "enter": return new StyleToken(StyleTokenType.Enter, identifierLowerCase, context.line, context.column);
                case "run": return new StyleToken(StyleTokenType.Run, identifierLowerCase, context.line, context.column);
                case "pause": return new StyleToken(StyleTokenType.Pause, identifierLowerCase, context.line, context.column);
                case "stop": return new StyleToken(StyleTokenType.Stop, identifierLowerCase, context.line, context.column);
                default: {
                    return new StyleToken(StyleTokenType.Identifier, identifier, context.line, context.column);
                }
            }
        }

        private static void TryReadString(TokenizerContext context, List<StyleToken> output) {
            if (context.IsConsumed()) return;
            if (context.input[context.ptr] != '"') return;
            int start = context.ptr;

            context.Save();
            context.Advance();

            while (context.HasMore() && context.input[context.ptr] != '"') {
                context.Advance();
            }

            if (context.IsConsumed()) {
                context.Restore();
                return;
            }

            if (context.input[context.ptr] != '"') {
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
            output.Add(new StyleToken(StyleTokenType.String, substring, context.line, context.column));
            context.Advance(length);

            TryConsumeWhiteSpace(context);
        }

        private static void TryReadValue(TokenizerContext context, List<StyleToken> output) {
            if (context.IsConsumed()) return;
            if (context.input[context.ptr] != '=') return;
            context.Save();

            context.Advance();
            TryConsumeWhiteSpace(context);
            int start = context.ptr;

            while (context.HasMore() && context.input[context.ptr] != ';' && context.input[context.ptr] != '\n') {
                if (context.input[context.ptr] == '/' && context.ptr + 1 < context.input.Length && context.input[context.ptr + 1] == '/') {
                    break;
                }

                context.Advance();
            }

            if (context.IsConsumed()) {
                context.Restore();
                return;
            }

            string value = context.input.Substring(start, context.ptr - start);
            output.Add(new StyleToken(StyleTokenType.Value, value, context.line, context.column));
            TryConsumeWhiteSpace(context);
        }

        // todo take optional file / line number for error message
        public static List<StyleToken> Tokenize(string input, List<StyleToken> retn = null) {
            List<StyleToken> output = retn ?? new List<StyleToken>();

            TokenizerContext context = new TokenizerContext(input);
            TryConsumeWhiteSpace(context);
            while (context.ptr < input.Length) {
                int start = context.ptr;

                TryConsumeComment(context);

                TryReadCharacters(context, "@", StyleTokenType.At, output);
                TryReadCharacters(context, ":", StyleTokenType.Colon, output);
                TryReadCharacters(context, "==", StyleTokenType.Equals, output);
                TryReadCharacters(context, "!=", StyleTokenType.NotEquals, output);
                TryReadCharacters(context, "!", StyleTokenType.BooleanNot, output);
                TryReadCharacters(context, "=", StyleTokenType.EqualSign, output);
                TryReadCharacters(context, ">", StyleTokenType.GreaterThan, output);
                TryReadCharacters(context, "<", StyleTokenType.LessThan, output);
                TryReadCharacters(context, "&&", StyleTokenType.BooleanAnd, output);
                TryReadCharacters(context, "||", StyleTokenType.BooleanOr, output);
                TryReadCharacters(context, "$", StyleTokenType.Dollar, output);
                TryReadCharacters(context, "+", StyleTokenType.Plus, output);
                // If the next character is a minus sign then we want to try to parse a single number token
                TryReadDigit(context, output);
                TryReadCharacters(context, "-", StyleTokenType.Minus, output);
                TryReadCharacters(context, "/", StyleTokenType.Divide, output);
                TryReadCharacters(context, "*", StyleTokenType.Times, output);
                TryReadCharacters(context, "%", StyleTokenType.Mod, output);
                TryReadCharacters(context, "?", StyleTokenType.QuestionMark, output);

                TryReadCharacters(context, ".", StyleTokenType.Dot, output);
                TryReadCharacters(context, ",", StyleTokenType.Comma, output);
                TryReadCharacters(context, "(", StyleTokenType.ParenOpen, output);
                TryReadCharacters(context, ")", StyleTokenType.ParenClose, output);
                TryReadCharacters(context, "[", StyleTokenType.BracketOpen, output);
                TryReadCharacters(context, "]", StyleTokenType.BracketClose, output);
                TryReadCharacters(context, "{", StyleTokenType.BracesOpen, output);
                TryReadCharacters(context, "}", StyleTokenType.BracesClose, output);

                TryReadHashColor(context, output);
                TryReadDigit(context, output);
                TryReadString(context, output);
                TryReadIdentifier(context, output);
                TryConsumeWhiteSpace(context);

                TryReadCharacters(context, ";\n", StyleTokenType.EndStatement, output);
                TryReadCharacters(context, ";", StyleTokenType.EndStatement, output);
                TryReadCharacters(context, "\n", StyleTokenType.EndStatement, output);

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

        private static string PrintTokenList(List<StyleToken> tokens) {
            string retn = "\n";
            for (int i = 0; i < tokens.Count; i++) {
                retn += tokens[i].ToString();
                if (i != tokens.Count - 1) {
                    retn += ", \n";
                }
            }

            return retn;
        }

    }

}