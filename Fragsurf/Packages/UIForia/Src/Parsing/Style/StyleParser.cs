using System;
using System.Collections.Generic;
using UIForia.Animation;
using UIForia.Exceptions;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Parsing.Style.Tokenizer;
using UIForia.Rendering;
using UIForia.Text;
using UIForia.Util;
using Unity.Profiling;
using UnityEngine;

namespace UIForia.Parsing.Style {

    public struct StyleParser {

        public static ProfilerMarker profilerMarker = new ProfilerMarker("UIForia::StyleParser");
        
        private StyleTokenStream tokenStream;

        /// <summary>
        /// Contains all top level nodes that are in the style file. 
        /// </summary>
        private LightList<StyleASTNode> nodes;

        private Stack<StyleASTNode> expressionStack;
        private Stack<StyleOperatorNode> operatorStack;
        private Stack<AttributeNodeContainer> groupExpressionStack;
        private Stack<StyleOperatorType> groupOperatorStack;

        private StyleParser(StyleTokenStream stream) {
            tokenStream = stream;
            nodes = LightList<StyleASTNode>.Get();
            operatorStack = StackPool<StyleOperatorNode>.Get();
            expressionStack = StackPool<StyleASTNode>.Get();
            groupExpressionStack = StackPool<AttributeNodeContainer>.Get();
            groupOperatorStack = StackPool<StyleOperatorType>.Get();
        }

        private static StyleTokenStream FromString(string input) {
            return new StyleTokenStream(StyleTokenizer.Tokenize(input, ListPool<StyleToken>.Get()));
        }

        public static LightList<StyleASTNode> Parse(string input) {
            return new StyleParser(FromString(input)).Parse();
        }

        private void Release() {
            tokenStream.Release();
            StackPool<StyleOperatorNode>.Release(operatorStack);
            StackPool<StyleASTNode>.Release(expressionStack);
        }

        private LightList<StyleASTNode> Parse() {
            if (!tokenStream.HasMoreTokens) {
                return nodes;
            }

            ParseLoop();
            Release();

            return nodes;
        }

        private void ParseLoop() {
            while (tokenStream.HasMoreTokens) {
                ParseNextRoot();
            }
        }

        private void ParseNextRoot() {
            switch (tokenStream.Current.styleTokenType) {
                case StyleTokenType.Style:
                    tokenStream.Advance();
                    ParseStyle();
                    break;

                case StyleTokenType.Sound:
                    tokenStream.Advance();
                    ParseSound();
                    break;

                case StyleTokenType.Animation:
                    tokenStream.Advance();
                    ParseAnimation();
                    break;

                case StyleTokenType.SpriteSheet:
                    tokenStream.Advance();
                    ParseSpriteSheet();
                    break;

                case StyleTokenType.Import:
                    ParseImportNode();
                    break;

                case StyleTokenType.Export:
                    ParseExportNode();
                    break;

                case StyleTokenType.Const:
                    nodes.Add(ParseConstNode());
                    AdvanceIfTokenType(StyleTokenType.EndStatement);
                    break;

                case StyleTokenType.Cursor:
                    tokenStream.Advance();
                    break;

                default:
                    throw new ParseException(tokenStream.Current, $"Did not expect token {tokenStream.Current.value} of type {tokenStream.Current.styleTokenType} here at line");
            }
        }

        /// <summary>
        /// Takes on all the things after a 'style' keyword on the root level of a style file.
        /// </summary>
        /// <exception cref="ParseException"></exception>
        private void ParseStyle() {
            string identifier = null;
            string tagName = null;
            StyleToken initialStyleToken = tokenStream.Current;
            switch (initialStyleToken.styleTokenType) {
                // <TagName> { ... }
                case StyleTokenType.LessThan:
                    tokenStream.Advance();
                    AssertTokenType(StyleTokenType.Identifier);
                    tagName = tokenStream.Current.value;
                    tokenStream.Advance();
                    if (tokenStream.Current.styleTokenType == StyleTokenType.LessThan) {
                        tagName += tokenStream.Current.value;
                        tokenStream.Advance();
                        tagName += tokenStream.Current.value;
                        AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
                        tagName += tokenStream.Current.value;
                        AssertTokenTypeAndAdvance(StyleTokenType.GreaterThan);
                    }

                    AssertTokenTypeAndAdvance(StyleTokenType.GreaterThan);
                    break;

                // styleId { ... }
                case StyleTokenType.Identifier:
                    identifier = tokenStream.Current.value;
                    tokenStream.Advance();
                    break;

                default:
                    throw new ParseException(initialStyleToken, $"Expected style definition or tag name but found {initialStyleToken.styleTokenType}");
            }

            StyleRootNode styleRootNode = StyleASTNodeFactory.StyleRootNode(identifier, tagName);
            styleRootNode.WithLocation(initialStyleToken);
            nodes.Add(styleRootNode);

            // we just read an element name or style name
            // now move forward and expect an open curly brace

            // next there should be one of those:
            // - property
            // - state
            // - attribute with or without boolean modifier
            // - expression with constants
            ParseStyleGroupBody(styleRootNode);
        }

        private void ParseSound() {
            StyleToken soundNameToken = tokenStream.Current;
            SoundRootNode soundRootNode = StyleASTNodeFactory.SoundRootNode(soundNameToken);
            soundRootNode.WithLocation(soundNameToken);
            tokenStream.Advance();
            AssertTokenTypeAndAdvance(StyleTokenType.BracesOpen);
            SoundParseLoop(soundRootNode);
            nodes.Add(soundRootNode);
        }

        private void SoundParseLoop(SoundRootNode soundRootNode) {
            while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.BracesClose)) {
                ParseSoundProperty(soundRootNode);
            }
        }

        private void ParseAnimation() {
            StyleToken initialStyleToken = tokenStream.Current;
            AnimationRootNode animRoot = StyleASTNodeFactory.AnimationRootNode(initialStyleToken);
            animRoot.WithLocation(initialStyleToken);
            tokenStream.Advance();
            AssertTokenTypeAndAdvance(StyleTokenType.BracesOpen);
            AnimationParseLoop(animRoot);
            nodes.Add(animRoot);
        }

        private void AnimationParseLoop(AnimationRootNode rootNode) {
            while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.BracesClose)) {
                AssertTokenTypeAndAdvance(StyleTokenType.BracketOpen);

                string identifier = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
                AssertTokenTypeAndAdvance(StyleTokenType.BracketClose);
                AssertTokenTypeAndAdvance(StyleTokenType.BracesOpen);
                if (identifier == "variables") {
                    ParseAnimationVariables(rootNode);
                }
                else if (identifier == "keyframes") {
                    ParseAnimationKeyFrames(rootNode);
                }
                else if (identifier == "options") {
                    ParseAnimationOptions(rootNode);
                }
                else if (identifier == "triggers") {
                    
                }
            }
        }

        private void ParseSpriteSheet() {
            StyleToken initialStyleToken = tokenStream.Current;
            SpriteSheetNode rootNode = StyleASTNodeFactory.SpriteSheetNode(initialStyleToken);
            rootNode.WithLocation(initialStyleToken);
            tokenStream.Advance();
            AssertTokenTypeAndAdvance(StyleTokenType.BracesOpen);
            SpriteSheetParseLoop(rootNode);
            nodes.Add(rootNode);
        }

        private void SpriteSheetParseLoop(SpriteSheetNode rootNode) {
            while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.BracesClose)) {
                StyleToken typeToken = tokenStream.Current;
                string optionName = AssertTokenTypeAndAdvance(StyleTokenType.Identifier).ToLower();
                bool typeFound = false;
                for (int index = 0; index < s_SpriteSheetOptionNames.Length; index++) {
                    string name = s_SpriteSheetOptionNames[index].Item1;
                    if (name == optionName) {
                        AssertTokenTypeAndAdvance(StyleTokenType.EqualSign);
                        StyleToken variableToken = tokenStream.Current;

                        PropertyNode propertyNode = StyleASTNodeFactory.PropertyNode(s_SpriteSheetOptionNames[index].Item2);
                        propertyNode.AddChildNode(ParsePropertyValue());
                        propertyNode.WithLocation(variableToken);

                        rootNode.AddChildNode(propertyNode);

                        typeFound = true;

                        break;
                    }
                }

                if (!typeFound) {
                    throw new ParseException(typeToken, $"{optionName} is not a supported spritesheet option. Valid values are: {FormatOptionList(s_SpriteSheetOptionNames)}\n");
                }

                AssertTokenTypeAndAdvance(StyleTokenType.EndStatement);
            }
        }

        public static readonly ValueTuple<string, Type>[] s_SupportedVariableTypes = {
            ValueTuple.Create("float", typeof(float)),
            ValueTuple.Create("int", typeof(int)),
            ValueTuple.Create("Color", typeof(Color)),
            ValueTuple.Create("UIMeasurement", typeof(UIMeasurement)),
            ValueTuple.Create("Measurement", typeof(UIMeasurement)),
            ValueTuple.Create("UIFixedLength", typeof(UIFixedLength)),
            ValueTuple.Create("FixedLength", typeof(UIFixedLength)),
            ValueTuple.Create("TransformOffset", typeof(OffsetMeasurement)),
            ValueTuple.Create("Offset", typeof(OffsetMeasurement)),
        };

        public static readonly ValueTuple<string, string>[] s_AnimationOptionNames = {
            ValueTuple.Create(
                nameof(AnimationOptions.loopTime).ToLower(),
                nameof(AnimationOptions.loopTime)),
            ValueTuple.Create(
                nameof(AnimationOptions.iterations).ToLower(),
                nameof(AnimationOptions.iterations)),
            ValueTuple.Create(
                nameof(AnimationOptions.delay).ToLower(),
                nameof(AnimationOptions.delay)),
            ValueTuple.Create(
                nameof(AnimationOptions.duration).ToLower(),
                nameof(AnimationOptions.duration)),
            ValueTuple.Create(
                nameof(AnimationOptions.forwardStartDelay).ToLower(),
                nameof(AnimationOptions.forwardStartDelay)),
            ValueTuple.Create(
                nameof(AnimationOptions.reverseStartDelay).ToLower(),
                nameof(AnimationOptions.reverseStartDelay)),
            ValueTuple.Create(
                nameof(AnimationOptions.direction).ToLower(),
                nameof(AnimationOptions.direction)),
            ValueTuple.Create(
                nameof(AnimationOptions.loopType).ToLower(),
                nameof(AnimationOptions.loopType)),
            ValueTuple.Create(
                nameof(AnimationOptions.timingFunction).ToLower(),
                nameof(AnimationOptions.timingFunction)),
        };

        public static readonly ValueTuple<string, string>[] s_SpriteSheetOptionNames = {
            ValueTuple.Create(
                nameof(AnimationOptions.iterations).ToLower(),
                nameof(AnimationOptions.iterations)),
            ValueTuple.Create(
                nameof(AnimationOptions.delay).ToLower(),
                nameof(AnimationOptions.delay)),
            ValueTuple.Create(
                nameof(AnimationOptions.duration).ToLower(),
                nameof(AnimationOptions.duration)),
            ValueTuple.Create(
                nameof(AnimationOptions.direction).ToLower(),
                nameof(AnimationOptions.direction)),
            ValueTuple.Create(
                nameof(AnimationOptions.loopType).ToLower(),
                nameof(AnimationOptions.loopType)),
            ValueTuple.Create(
                nameof(AnimationOptions.forwardStartDelay).ToLower(),
                nameof(AnimationOptions.forwardStartDelay)),
            ValueTuple.Create(
                nameof(AnimationOptions.reverseStartDelay).ToLower(),
                nameof(AnimationOptions.reverseStartDelay)),
            ValueTuple.Create(
                nameof(AnimationOptions.fps).ToLower(),
                nameof(AnimationOptions.fps)),
            ValueTuple.Create(
                nameof(AnimationOptions.endFrame).ToLower(),
                nameof(AnimationOptions.endFrame)),
            ValueTuple.Create(
                nameof(AnimationOptions.startFrame).ToLower(),
                nameof(AnimationOptions.startFrame)),
            ValueTuple.Create(
                nameof(AnimationOptions.pathPrefix).ToLower(),
                nameof(AnimationOptions.pathPrefix))
        };

        private void ParseAnimationVariables(AnimationRootNode rootNode) {
            while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.BracesClose)) {
                StyleToken typeToken = tokenStream.Current;
                string typeIdentifier = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
                bool typeFound = false;
                for (int index = 0; index < s_SupportedVariableTypes.Length; index++) {
                    (string name, Type type) = s_SupportedVariableTypes[index];
                    if (name == typeIdentifier) {
                        string variableName = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
                        AssertTokenTypeAndAdvance(StyleTokenType.EqualSign);
                        StyleToken variableToken = tokenStream.Current;

                        VariableDefinitionNode varNode = new VariableDefinitionNode();
                        varNode.name = variableName;
                        varNode.variableType = type;
                        varNode.value = ParsePropertyValue();
                        varNode.WithLocation(variableToken);

                        rootNode.AddVariableNode(varNode);

                        typeFound = true;

                        break;
                    }
                }

                if (!typeFound) {
                    throw new ParseException(typeToken, "Unsupported Type; please read the manual!");
                }

                AssertTokenTypeAndAdvance(StyleTokenType.EndStatement);
            }
        }

        private void ParseAnimationOptions(AnimationRootNode rootNode) {
            while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.BracesClose)) {
                StyleToken typeToken = tokenStream.Current;
                string optionName = AssertTokenTypeAndAdvance(StyleTokenType.Identifier).ToLower();
                bool typeFound = false;
                for (int index = 0; index < s_AnimationOptionNames.Length; index++) {
                    string name = s_AnimationOptionNames[index].Item1;
                    if (name == optionName) {
                        AssertTokenTypeAndAdvance(StyleTokenType.EqualSign);
                        StyleToken variableToken = tokenStream.Current;

                        AnimationOptionNode optionNode = StyleASTNodeFactory.AnimationOptionNode(s_AnimationOptionNames[index].Item2, ParsePropertyValue());
                        optionNode.WithLocation(variableToken);

                        rootNode.AddOptionNode(optionNode);

                        typeFound = true;

                        break;
                    }
                }

                if (!typeFound) {
                    throw new ParseException(typeToken, $"{optionName} is not a supported animation option. Valid values are: {FormatOptionList(s_AnimationOptionNames)}\n");
                }

                AssertTokenTypeAndAdvance(StyleTokenType.EndStatement);
            }
        }

        private void ParseSoundProperty(SoundRootNode soundRootNode) {
            string propertyName = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
            AssertTokenTypeAndAdvance(StyleTokenType.EqualSign);
            StyleToken variableToken = tokenStream.Current;

            SoundPropertyNode propertyNode = StyleASTNodeFactory.SoundPropertyNode(propertyName, ParsePropertyValue());
            propertyNode.WithLocation(variableToken);

            soundRootNode.AddChildNode(propertyNode);
            AssertTokenTypeAndAdvance(StyleTokenType.EndStatement);
        }

        private string FormatOptionList(ValueTuple<string, string>[] values) {
            string retn = "";
            for (int i = 0; i < values.Length; i++) {
                retn += "\n" + values[i].Item2;
            }

            return retn;
        }

        private void ParseAnimationKeyFrames(AnimationRootNode rootNode) {
            while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.BracesClose)) {
                string value = AssertTokenTypeAndAdvance(StyleTokenType.Number);
                AssertTokenTypeAndAdvance(StyleTokenType.Mod);

                KeyFrameNode keyFrameNode = StyleASTNodeFactory.KeyFrameNode(int.Parse(value));

                while (AdvanceIfTokenType(StyleTokenType.Comma)) {
                    keyFrameNode.keyframes.Add(int.Parse(AssertTokenTypeAndAdvance(StyleTokenType.Number)));
                    AssertTokenTypeAndAdvance(StyleTokenType.Mod);
                }

                AssertTokenTypeAndAdvance(StyleTokenType.BracesOpen);

                while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.BracesClose)) {
                    ParseProperties(keyFrameNode);
                }

                rootNode.AddKeyFrameNode(keyFrameNode);
            }
        }

        private void ParseExportNode() {
            StyleToken exportToken = tokenStream.Current;
            tokenStream.Advance();
            // export statement must be followed by const keyword

            // now let's find out which value we're assigning
            nodes.Add(StyleASTNodeFactory.ExportNode(ParseConstNode()).WithLocation(exportToken));
            AdvanceIfTokenType(StyleTokenType.EndStatement);
        }

        private ConstNode ParseConstNode() {
            StyleToken constToken = tokenStream.Current;
            AssertTokenTypeAndAdvance(StyleTokenType.Const);
            // const name
            string variableName = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
            AssertTokenTypeAndAdvance(StyleTokenType.EqualSign);

            ConstNode constNode = StyleASTNodeFactory.ConstNode(variableName, ParsePropertyValue());
            constNode.WithLocation(constToken);
            return constNode;
        }

        private void ParseImportNode() {
            StyleToken importToken = tokenStream.Current;
            AssertTokenTypeAndAdvance(StyleTokenType.Import);
            string source = AssertTokenTypeAndAdvance(StyleTokenType.String);
            AssertTokenTypeAndAdvance(StyleTokenType.As);

            string alias = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
            AssertTokenTypeAndAdvance(StyleTokenType.EndStatement);

            nodes.Add(StyleASTNodeFactory.ImportNode(alias, source).WithLocation(importToken));
        }

        private void ParseStyleGroupBody(StyleNodeContainer styleRootNode) {
            AssertTokenTypeAndAdvance(StyleTokenType.BracesOpen);

            while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.BracesClose)) {
                switch (tokenStream.Current.styleTokenType) {
                    case StyleTokenType.Not: {
                        groupOperatorStack.Push(StyleOperatorType.Not);
                        tokenStream.Advance();
                        ParseAttributeOrExpressionGroup();
                        break;
                    }

                    case StyleTokenType.And:
                        tokenStream.Advance();
                        if (AdvanceIfTokenType(StyleTokenType.Not)) {
                            groupOperatorStack.Push(StyleOperatorType.Not);
                        }

                        ParseAttributeOrExpressionGroup();
                        break;

                    case StyleTokenType.BracketOpen:
                        tokenStream.Advance();
                        if (!TryParseCommand(styleRootNode)) {
                            ParseStateOrAttributeGroup(styleRootNode);
                        }

                        break;

                    case StyleTokenType.Cursor:
                        // special case here: we are out of words and need to use the
                        // cursor token for the property AND the top level definition ¯\_(ツ)_/¯
                        ParseProperty(styleRootNode);
                        break;

                    case StyleTokenType.Identifier:
                        ParseProperty(styleRootNode);
                        break;

                    case StyleTokenType.Run:
                        tokenStream.Advance();
                        styleRootNode.AddChildNode(ParseRunNode(RunCommandType.Enter, RunAction.Run));
                        break;

                    case StyleTokenType.Stop:
                        tokenStream.Advance();
                        styleRootNode.AddChildNode(ParseRunNode(RunCommandType.Enter, RunAction.Stop));
                        break;

                    case StyleTokenType.Pause:
                        tokenStream.Advance();
                        styleRootNode.AddChildNode(ParseRunNode(RunCommandType.Enter, RunAction.Pause));
                        break;

                    case StyleTokenType.BracesOpen: {
                        // At this point only unconsumed attribute/expression group bodies are allowed

                        if (groupExpressionStack.Count > 1) {
                            throw new ParseException(tokenStream.Current, "There was a problem, I somehow made an error parsing a combined style group...");
                        }

                        if (groupExpressionStack.Count == 1) {
                            AttributeNodeContainer attributeNodeContainer = groupExpressionStack.Pop();
                            ParseStyleGroupBody(attributeNodeContainer);
                            styleRootNode.AddChildNode(attributeNodeContainer);
                        }
                        else {
                            throw new ParseException(tokenStream.Current, "Expected an attribute style group body. Braces are in a weird position!");
                        }

                        break;
                    }

                    default:
                        throw new ParseException(tokenStream.Current, "Expected either a boolean group operator (not / and), the start" + " of a group (an open bracket) or a regular property identifier but found " + tokenStream.Current.styleTokenType + " with value " + tokenStream.Current.value);
                }
            }
        }

        private void ParseStateOrAttributeGroup(StyleNodeContainer styleRootNode) {
            switch (tokenStream.Current.styleTokenType) {
                // this is the state group
                case StyleTokenType.Identifier:
                    StyleStateContainer stateGroupRootNode = StyleASTNodeFactory.StateGroupRootNode(tokenStream.Current);

                    tokenStream.Advance();
                    AssertTokenTypeAndAdvance(StyleTokenType.BracketClose);
                    AssertTokenTypeAndAdvance(StyleTokenType.BracesOpen);

                    ParseProperties(stateGroupRootNode);

                    AssertTokenTypeAndAdvance(StyleTokenType.BracesClose);

                    styleRootNode.AddChildNode(stateGroupRootNode);

                    break;

                case StyleTokenType.AttributeSpecifier:
                    ParseAttributeGroup();
                    break;

                default:
                    throw new ParseException(tokenStream.Current, "Expected either a group state identifier (hover etc.)" + " or an attribute identifier (attr:...)");
            }
        }

        private void ParseProperties(StyleNodeContainer styleRootNode) {
            while (tokenStream.HasMoreTokens && tokenStream.Current.styleTokenType != StyleTokenType.BracesClose) {
                ParseProperty(styleRootNode);
            }
        }

        private void ParseProperty(StyleNodeContainer styleRootNode, bool parsingKeyframes = false) {
            StyleToken propertyNodeToken = tokenStream.Current;
            string propertyName;
            if (AdvanceIfTokenType(StyleTokenType.Cursor)) {
                propertyName = propertyNodeToken.value;
            }
            else if (AdvanceIfTokenType(StyleTokenType.Run)) {
                styleRootNode.AddChildNode(ParseRunNode(RunCommandType.Enter, RunAction.Run));
                return;
            }
            else if (AdvanceIfTokenType(StyleTokenType.Pause)) {
                styleRootNode.AddChildNode(ParseRunNode(RunCommandType.Enter, RunAction.Pause));
                return;
            }
            else if (AdvanceIfTokenType(StyleTokenType.Stop)) {
                styleRootNode.AddChildNode(ParseRunNode(RunCommandType.Enter, RunAction.Stop));
                return;
            }
            else if (AdvanceIfTokenType(StyleTokenType.BracketOpen)) {
                if (TryParseCommand(styleRootNode)) {
                    return;
                }

                throw new ParseException(tokenStream.Current, "Not sure what you tried here but at this point only [enter] and [exit] run animation would be legal.");
            }
            else {
                propertyName = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);

                if (propertyName == "material" || propertyName == "Material") {

                    if (tokenStream.Current == StyleTokenType.Colon) {
                        tokenStream.Advance();

                        string materialName = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);

                        AssertTokenTypeAndAdvance(StyleTokenType.Dot);

                        string materialPropertyName = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);

                        AssertTokenTypeAndAdvance(StyleTokenType.EqualSign);

                        TextUtil.StringBuilder.Clear();

                        while (tokenStream.HasMoreTokens && tokenStream.Current != StyleTokenType.EndStatement) {
                            TextUtil.StringBuilder.Append(tokenStream.Current.value);
                            tokenStream.Advance();
                        }

                        tokenStream.Advance();

                        MaterialPropertyNode materialPropertyNode = new MaterialPropertyNode {
                            materialName = materialName,
                            identifier = materialPropertyName,
                            value = TextUtil.StringBuilder.ToString()
                        };

                        TextUtil.StringBuilder.Clear();

                        materialPropertyNode.WithLocation(propertyNodeToken);
                        styleRootNode.AddChildNode(materialPropertyNode);
                        return;

                    }

                }

            }

            AssertTokenTypeAndAdvance(StyleTokenType.EqualSign);

            PropertyNode propertyNode = StyleASTNodeFactory.PropertyNode(propertyName);
            propertyNode.WithLocation(propertyNodeToken);

            while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.EndStatement)) {
                propertyNode.AddChildNode(ParsePropertyValue());
                // we just ignore the comma for now
                AdvanceIfTokenType(StyleTokenType.Comma);
            }

            styleRootNode.AddChildNode(propertyNode);
        }

        /// <summary>
        /// Consumes the stream only if the current token belongs to a command definition.
        /// </summary>
        /// <param name="styleRootNode"></param>
        /// <returns>true in case the current token belongs to a command definition. False if it does not.</returns>
        private bool TryParseCommand(StyleNodeContainer styleRootNode) {
            switch (tokenStream.Current.styleTokenType) {
                case StyleTokenType.Exit:
                    tokenStream.Advance();
                    AssertTokenTypeAndAdvance(StyleTokenType.BracketClose);
                    styleRootNode.AddChildNode(ParseRunNode(RunCommandType.Exit, ParseCommandAttribute()));
                    return true;

                case StyleTokenType.Enter:
                    tokenStream.Advance();
                    AssertTokenTypeAndAdvance(StyleTokenType.BracketClose);
                    styleRootNode.AddChildNode(ParseRunNode(RunCommandType.Enter, ParseCommandAttribute()));
                    return true;

                case StyleTokenType.EnterExit:
                    tokenStream.Advance();
                    AssertTokenTypeAndAdvance(StyleTokenType.BracketClose);
                    styleRootNode.AddChildNode(ParseRunNode(RunCommandType.EnterExit, ParseCommandAttribute()));
                    return true;

                default:
                    return false;
            }
        }

        
        private RunAction ParseCommandAttribute() {
            StyleTokenType styleTokenType = tokenStream.Current.styleTokenType;
            tokenStream.Advance();

            switch (styleTokenType) {
                case StyleTokenType.Run: return RunAction.Run;
                case StyleTokenType.Pause: return RunAction.Pause;
                case StyleTokenType.Stop: return RunAction.Stop;

                default:
                    throw new ParseException(tokenStream.Previous,
                        $"The following command actions are supported: run, pause and stop. Found '{tokenStream.Previous.value}' instead.");
            }
        }

        private RunNode ParseRunNode(RunCommandType cmdType, RunAction runAction) {
            StyleToken runCommandToken = tokenStream.Current;
            CommandNode command;
            if (AdvanceIfTokenType(StyleTokenType.Animation)) {
                command = StyleASTNodeFactory.AnimationCommandNode(ParseIdentifierInParentheses(), cmdType, runAction);
            }
            else if (AdvanceIfTokenType(StyleTokenType.Sound)) {
                command = StyleASTNodeFactory.SoundCommandNode(ParseIdentifierInParentheses(), cmdType, runAction);
            }
            else {
                throw new ParseException(runCommandToken,
                    $"Only animation and sound run commands are supported. Found {runCommandToken}");
            }

            command.WithLocation(tokenStream.Current);
            AssertTokenTypeAndAdvance(StyleTokenType.EndStatement);

            RunNode runNode = StyleASTNodeFactory.RunNode(command);
            runNode.WithLocation(runCommandToken);
            return runNode;
        }

        private StyleASTNode ParseIdentifierInParentheses() {
            AssertTokenTypeAndAdvance(StyleTokenType.ParenOpen);

            StyleASTNode identifier;
            switch (tokenStream.Current.styleTokenType) {
                case StyleTokenType.Identifier:
                    identifier = StyleASTNodeFactory.IdentifierNode(tokenStream.Current.value);
                    break;

                case StyleTokenType.At:
                    identifier = ParseConstReference();
                    break;

                default:
                    throw new ParseException(tokenStream.Current, "Was expecting an identifier or a reference.");
            }

            tokenStream.Advance();
            // todo: add support for parameters here

            AssertTokenTypeAndAdvance(StyleTokenType.ParenClose);

            return identifier;
        }

        private StyleASTNode ParsePropertyValue() {
            StyleToken propertyToken = tokenStream.Current;
            StyleASTNode propertyValue;

            switch (tokenStream.Current.styleTokenType) {
                case StyleTokenType.Number:
                    StyleLiteralNode value = StyleASTNodeFactory.NumericLiteralNode(tokenStream.Current.value).WithLocation(propertyToken) as StyleLiteralNode;
                    tokenStream.Advance();
                    if (!IsAnyTokenType(StyleTokenType.EndStatement, StyleTokenType.Number, StyleTokenType.Comma, StyleTokenType.ParenClose)) {
                        UnitNode unit = ParseUnit().WithLocation(tokenStream.Previous) as UnitNode;
                        propertyValue = StyleASTNodeFactory.MeasurementNode(value, unit);
                    }
                    else {
                        propertyValue = value;
                    }

                    break;

                case StyleTokenType.String:

                    propertyValue = StyleASTNodeFactory.StringLiteralNode(tokenStream.Current.value);
                    tokenStream.Advance();

                    if (tokenStream.Current == StyleTokenType.BracesOpen) {
                        TextUtil.StringBuilder.Clear();
                        StyleLiteralNode n = (StyleLiteralNode) propertyValue;

                        TextUtil.StringBuilder.Append("\"");
                        TextUtil.StringBuilder.Append(n.rawValue);
                        TextUtil.StringBuilder.Append("\" ");

                        while (tokenStream.HasMoreTokens && tokenStream.Current != StyleTokenType.BracesClose) {
                            TextUtil.StringBuilder.Append(tokenStream.Current.value);
                            tokenStream.Advance();
                        }

                        TextUtil.StringBuilder.Append(tokenStream.Current.value);

                        tokenStream.Advance();
                        n.rawValue = TextUtil.StringBuilder.ToString();
                        TextUtil.StringBuilder.Clear();

                    }

                    break;

                case StyleTokenType.Identifier:
                    string identifier = tokenStream.Current.value;
                    tokenStream.Advance();
                    if (tokenStream.Current.styleTokenType == StyleTokenType.ParenOpen) {
                        tokenStream.Advance();
                        propertyValue = ParsePropertyFunction(identifier);
                    }
                    else {
                        propertyValue = StyleASTNodeFactory.IdentifierNode(identifier);
                    }

                    break;

                case StyleTokenType.Rgba:
                    propertyValue = ParseRgba();
                    break;

                case StyleTokenType.Rgb:
                    propertyValue = ParseRgb();
                    break;

                case StyleTokenType.HashColor:
                    propertyValue = StyleASTNodeFactory.ColorNode(tokenStream.Current.value);
                    tokenStream.Advance();
                    break;

                case StyleTokenType.Url:
                    tokenStream.Advance();
                    AssertTokenTypeAndAdvance(StyleTokenType.ParenOpen);

                    StyleASTNode url;
                    StyleASTNode spriteName = null;
                    if (tokenStream.Current.styleTokenType == StyleTokenType.String) {
                        url = ParseLiteralOrReference(StyleTokenType.String);
                    }
                    else {
                        url = ParseLiteralOrReference(StyleTokenType.Identifier);
                    }

                    if (url is StyleIdentifierNode urlIdentifier) {
                        // todo -- this doesn't handle spaces!
                        while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.ParenClose)) {
                            // advancing tokens no matter the type. We want to concatenate all identifiers and slashes of a path again.
                            urlIdentifier.name += tokenStream.Current.value;
                            spriteName = ParseSpriteName();
                            tokenStream.Advance();
                        }
                    }
                    else if (url is StyleLiteralNode || url is ConstReferenceNode) {
                        spriteName = ParseSpriteName();
                        AssertTokenTypeAndAdvance(StyleTokenType.ParenClose);
                    }
                    else {
                        throw new CompileException(url, "URL could not be parsed.");
                    }

                    propertyValue = StyleASTNodeFactory.UrlNode(url, spriteName);
                    break;

                case StyleTokenType.At:
                    propertyValue = ParseConstReference();
                    break;

                case StyleTokenType.Dollar:
                    propertyValue = ParseVariableReference();
                    break;

                default:
                    throw new ParseException(propertyToken, "Expected a property value but found no valid token.");
            }

            return propertyValue.WithLocation(propertyToken);
        }

        private StyleFunctionNode ParsePropertyFunction(string identifier) {
            StyleFunctionNode functionNode = StyleASTNodeFactory.StyleFunctionNode(identifier);

            while (tokenStream.HasMoreTokens && !AdvanceIfTokenType(StyleTokenType.ParenClose)) {
                functionNode.AddChildNode(ParsePropertyValue());
                // we just ignore the comma for now
                AdvanceIfTokenType(StyleTokenType.Comma);
            }

            return functionNode;
        }

        private StyleASTNode ParseSpriteName() {
            if (AdvanceIfTokenType(StyleTokenType.Comma)) {
                return ParseLiteralOrReference(StyleTokenType.String);
            }

            return null;
        }

        private UnitNode ParseUnit() {
            StyleToken styleToken = tokenStream.Current;
            string value = styleToken.value;

            tokenStream.Advance();

            switch (styleToken.styleTokenType) {
                case StyleTokenType.Identifier:
                    return StyleASTNodeFactory.UnitNode(value);

                case StyleTokenType.Mod:
                    return StyleASTNodeFactory.UnitNode(value);

                default:
                    throw new ParseException(styleToken, "Expected a token that looks like a unit but this didn't.");
            }
        }

        private StyleASTNode ParseConstReference() {
            AdvanceIfTokenType(StyleTokenType.At);
            ConstReferenceNode constReferenceNode = StyleASTNodeFactory.ConstReferenceNode(AssertTokenTypeAndAdvance(StyleTokenType.Identifier));

            while (tokenStream.HasMoreTokens && AdvanceIfTokenType(StyleTokenType.Dot)) {
                constReferenceNode.AddChildNode(StyleASTNodeFactory.DotAccessNode(AssertTokenTypeAndAdvance(StyleTokenType.Identifier)).WithLocation(tokenStream.Previous));
            }

            return constReferenceNode;
        }

        private StyleASTNode ParseVariableReference() {
            AdvanceIfTokenType(StyleTokenType.Dollar);
            VariableReferenceNode refNode = new VariableReferenceNode(AssertTokenTypeAndAdvance(StyleTokenType.Identifier));

            while (tokenStream.HasMoreTokens && AdvanceIfTokenType(StyleTokenType.Dot)) {
                refNode.AddChildNode(StyleASTNodeFactory.DotAccessNode(AssertTokenTypeAndAdvance(StyleTokenType.Identifier)).WithLocation(tokenStream.Previous));
            }

            return refNode;
        }

        private StyleASTNode ParseRgba() {
            AssertTokenTypeAndAdvance(StyleTokenType.Rgba);
            AssertTokenTypeAndAdvance(StyleTokenType.ParenOpen);

            StyleASTNode red = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.Comma);

            StyleASTNode green = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.Comma);

            StyleASTNode blue = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.Comma);

            StyleASTNode alpha = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.ParenClose);

            return StyleASTNodeFactory.RgbaNode(red, green, blue, alpha);
        }

        private StyleASTNode ParseRgb() {
            AssertTokenTypeAndAdvance(StyleTokenType.Rgb);
            AssertTokenTypeAndAdvance(StyleTokenType.ParenOpen);

            StyleASTNode red = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.Comma);

            StyleASTNode green = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.Comma);

            StyleASTNode blue = ParseLiteralOrReference(StyleTokenType.Number);
            AssertTokenTypeAndAdvance(StyleTokenType.ParenClose);

            return StyleASTNodeFactory.RgbNode(red, green, blue);
        }

        private StyleASTNode ParseLiteralOrReference(StyleTokenType literalType) {
            StyleToken currentToken = tokenStream.Current;
            if (AdvanceIfTokenType(StyleTokenType.At)) {
                return ParseConstReference().WithLocation(currentToken);
            }

            string value = AssertTokenTypeAndAdvance(literalType);
            switch (literalType) {
                case StyleTokenType.String:
                    return StyleASTNodeFactory.StringLiteralNode(value).WithLocation(currentToken);

                case StyleTokenType.Number:
                    return StyleASTNodeFactory.NumericLiteralNode(value).WithLocation(currentToken);

                case StyleTokenType.Boolean:
                    return StyleASTNodeFactory.BooleanLiteralNode(value).WithLocation(currentToken);

                case StyleTokenType.Identifier:
                    return StyleASTNodeFactory.IdentifierNode(value).WithLocation(currentToken);
            }

            throw new ParseException(currentToken, $"Please add support for this type: {literalType}!");
        }

        private void AssertTokenType(StyleTokenType styleTokenType) {
            if (tokenStream.Current.styleTokenType != styleTokenType) {
                throw new ParseException(tokenStream.Current, $"Parsing stylesheet failed. Expected token '{styleTokenType}' but got '{tokenStream.Current.styleTokenType}'");
            }
        }

        private string AssertTokenTypeAndAdvance(StyleTokenType styleTokenType) {
            if (tokenStream.Current.styleTokenType != styleTokenType) {
                throw new ParseException(tokenStream.Current, $"Parsing stylesheet failed. Expected token '{styleTokenType}' but got '{tokenStream.Current.styleTokenType}'");
            }

            tokenStream.Advance();
            return tokenStream.Previous.value;
        }

        private bool AdvanceIfTokenType(StyleTokenType styleTokenType) {
            if (tokenStream.Current.styleTokenType == styleTokenType) {
                tokenStream.Advance();
                return true;
            }

            return false;
        }

        private void ParseAttributeOrExpressionGroup() {
            AssertTokenTypeAndAdvance(StyleTokenType.BracketOpen);

            switch (tokenStream.Current.styleTokenType) {
                case StyleTokenType.AttributeSpecifier:
                    ParseAttributeGroup();
                    break;

                case StyleTokenType.Dollar:
                    ParseStyleExpression();
                    // todo add style expression
                    break;
            }
        }

        private void ParseAttributeGroup() {
            AssertTokenTypeAndAdvance(StyleTokenType.AttributeSpecifier);
            AssertTokenTypeAndAdvance(StyleTokenType.Colon);
            StyleToken attributeToken = tokenStream.Current;
            string attributeIdentifier = AssertTokenTypeAndAdvance(StyleTokenType.Identifier);
            string attributeValue = null;

            if (AdvanceIfTokenType(StyleTokenType.EqualSign)) {
                attributeValue = tokenStream.Current.value;
                tokenStream.Advance();
            }

            bool invert = groupOperatorStack.Count > 0 && groupOperatorStack.Pop() == StyleOperatorType.Not;

            AttributeNodeContainer andAttribute = groupExpressionStack.Count > 0 ? groupExpressionStack.Pop() : null;
            AttributeNodeContainer attributeNodeContainer = StyleASTNodeFactory.AttributeGroupRootNode(attributeIdentifier, attributeValue, invert, andAttribute);
            attributeNodeContainer.WithLocation(attributeToken);

            groupExpressionStack.Push(attributeNodeContainer);

            AssertTokenTypeAndAdvance(StyleTokenType.BracketClose);
        }

        private void ParseStyleExpression() {
            switch (tokenStream.Current.styleTokenType) {
                case StyleTokenType.Plus:
                    StyleASTNodeFactory.OperatorNode(StyleOperatorType.Plus);
                    break;
            }

            AssertTokenTypeAndAdvance(StyleTokenType.BracketClose);
        }

        private bool ParseOperatorExpression(out StyleOperatorNode operatorNode) {
            tokenStream.Save();

            if (!tokenStream.Current.IsOperator) {
                tokenStream.Restore();
                operatorNode = default;
                return false;
            }

            tokenStream.Advance();

            switch (tokenStream.Previous.styleTokenType) {
                case StyleTokenType.Plus:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.Plus);
                    return true;

                case StyleTokenType.Minus:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.Minus);
                    return true;

                case StyleTokenType.Times:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.Times);
                    return true;

                case StyleTokenType.Divide:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.Divide);
                    return true;

                case StyleTokenType.Mod:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.Mod);
                    return true;

                case StyleTokenType.BooleanAnd:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.BooleanAnd);
                    return true;

                case StyleTokenType.BooleanOr:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.BooleanOr);
                    return true;

                case StyleTokenType.Equals:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.Equals);
                    return true;

                case StyleTokenType.NotEquals:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.NotEquals);
                    return true;

                case StyleTokenType.GreaterThan:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.GreaterThan);
                    return true;

                case StyleTokenType.GreaterThanEqualTo:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.GreaterThanEqualTo);
                    return true;

                case StyleTokenType.LessThan:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.LessThan);
                    return true;

                case StyleTokenType.LessThanEqualTo:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.LessThanEqualTo);
                    return true;

                case StyleTokenType.QuestionMark:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.TernaryCondition);
                    return true;

                case StyleTokenType.Colon:
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.TernarySelection);
                    return true;

                case StyleTokenType.As: {
                    operatorNode = StyleASTNodeFactory.OperatorNode(StyleOperatorType.As);
                    return true;
                }

                default:
                    throw new Exception("Unknown op type");
            }
        }

        private bool IsAnyTokenType(StyleTokenType type0, StyleTokenType type1, StyleTokenType type2 = 0, StyleTokenType type3 = 0, StyleTokenType type4 = 0, StyleTokenType type5 = 0) {
            StyleTokenType tokenType = tokenStream.Current.styleTokenType;
            return tokenType == type0 || tokenType == type1 || tokenType == type2 || tokenType == type3 || tokenType == type4 || tokenType == type5;
        }

    }

}