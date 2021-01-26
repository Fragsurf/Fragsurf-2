using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing.Expressions;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Templates;
using UIForia.Text;
using UIForia.Util;
using Unity.Profiling;
using UnityEngine;

namespace UIForia.Parsing {

    // namespaced elements
    // using declarations

    // <Using namespace=""/>
    // <Slot:Name>
    // <SlotContent:Name>
    // <Layout:Element>
    // <Transclude:
    // <Slot:Children>
    // <Dynamic:ElementType type="" data="">
    // <Repeat:
    // <LazyLoad:
    // <Virtual:
    // <NameSpace.Whatever.Element>
    // <Const
    // <RecursiveConst: 
    // <ConstTree
    // <Shadow:

    public class XMLTemplateParser {

        public static ProfilerMarker profilerMarker = new ProfilerMarker("UIForia::TemplateParser");
        
        private readonly XmlParserContext parserContext;
        private readonly Dictionary<string, TemplateShell> parsedFiles;
        private TemplateSettings settings;

        private class CustomNamespaceReader : XmlNamespaceManager {

            public CustomNamespaceReader([NotNull] XmlNameTable nameTable) : base(nameTable) { }

            public override string LookupNamespace(string prefix) {
                return prefix;
            }

        }

        public XMLTemplateParser(TemplateSettings settings) {
            this.parsedFiles = new Dictionary<string, TemplateShell>(37);
            XmlNamespaceManager nameSpaceManager = new CustomNamespaceReader(new NameTable());
            this.parserContext = new XmlParserContext(null, nameSpaceManager, null, XmlSpace.None);
            this.settings = settings;
        }

        // first time we get a file parse request we need to create the shell 
        // then, as more templates from that shell are requested, return them bit by bit

        private TemplateShell ParseOuterShell(string filePath, string source) {
            XElement root = XElement.Load(new XmlTextReader(source, XmlNodeType.Element, parserContext), LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);

            root.MergeTextNodes();

            TemplateShell retn = new TemplateShell(filePath);

            IEnumerable<XElement> styleElements = root.GetChildren("Style");
            IEnumerable<XElement> usingElements = root.GetChildren("Using");
            IEnumerable<XElement> contentElements = root.GetChildren("Contents");
            IEnumerable<XElement> elementDefElements = root.GetChildren("Element");

            foreach (XElement usingElement in usingElements) {
                retn.usings.Add(ParseUsing(usingElement));
            }

            BuildNamespaceListFromUsings(retn.usings, retn.referencedNamespaces);

            foreach (XElement styleElement in styleElements) {
                retn.styles.Add(ParseStyleSheet(filePath, styleElement));
            }

            XElement[] array = contentElements.ToArray();

            foreach (XElement contentElement in array) {
                XAttribute attr = contentElement.GetAttribute("id");

                string templateId = null;

                if (attr != null) {
                    templateId = attr.Value.Trim();
                }

                if (retn.HasContentNode(templateId)) {
                    throw new ArgumentException("Multiple templates found with id: " + templateId);
                }

                retn.unprocessedContentNodes.Add(new RawTemplateContent() {
                    templateId = templateId,
                    type = ParsedTemplateType.FromCode,
                    content = contentElement,
                    elementDefinition = null,
                });
            }

            foreach (XElement elementDef in elementDefElements) {
                XAttribute attr = elementDef.GetAttribute("id");

                string templateId = null;

                if (attr != null) {
                    templateId = attr.Value.Trim();
                    if (!IsValidIdentifier(templateId)) {
                        throw new ParseException($"Element definitions require an id attribute. `{templateId}` in file `{retn.filePath}` is not a valid identifier");
                    }

                    if (!IsUniqueUsingIdentifier(retn.unprocessedContentNodes, templateId)) {
                        throw new ParseException($"Element definitions require an id that is unique in its file. `{templateId}` was already registered in {retn.filePath}");
                    }
                }
                else {
                    int line = ((IXmlLineInfo) elementDef).LineNumber;
                    throw new ParseException($"Element definitions require an id attribute but Element definition at `{retn.filePath} line {line}` did not declare one");
                }

                XElement template = elementDef.GetChild("Template");

                retn.unprocessedContentNodes.Add(new RawTemplateContent() {
                    templateId = templateId,
                    type = ParsedTemplateType.Dynamic,
                    content = template,
                    elementDefinition = elementDef
                });
            }

            return retn;
        }

        private static bool IsUniqueUsingIdentifier(StructList<RawTemplateContent> contents, string templateId) {
            for (int i = 0; i < contents.size; i++) {
                if (contents.array[i].templateId == templateId) {
                    return false;
                }
            }

            return true;
        }

        internal void Parse(TemplateRootNode templateRootNode, ProcessedType processedType) {
            profilerMarker.Begin();
            TemplateAttribute templateAttr = processedType.templateAttr;

            string filePath = templateAttr.filePath;

            if (parsedFiles.TryGetValue(filePath, out TemplateShell rootNode)) {
                ParseContentTemplate(templateRootNode, rootNode, processedType);
                return;
            }

            TemplateShell shell = ParseOuterShell(templateAttr.filePath, templateAttr.source);

            parsedFiles.Add(filePath, shell);

            ParseContentTemplate(templateRootNode, shell, processedType);
            profilerMarker.End();
        }

        internal TemplateShell GetOuterTemplateShell(TemplateAttribute templateAttribute) {
            return GetOuterTemplateShell(templateAttribute.filePath, templateAttribute.source);
        }

        internal TemplateShell GetOuterTemplateShell(string filePath, string source) {
            if (parsedFiles.TryGetValue(filePath, out TemplateShell rootNode)) {
                return rootNode;
            }

            source = source ?? settings.TryReadFile(settings.GetTemplatePath(filePath));
            TemplateShell shell = ParseOuterShell(filePath, source);
            parsedFiles.Add(filePath, shell);
            return shell;
        }

        // this might be getting called too many times since im not sure im caching the result
        private void ParseContentTemplate(TemplateRootNode templateRootNode, TemplateShell shell, ProcessedType processedType) {
            XElement root = shell.GetElementTemplateContent(processedType.templateAttr.templateId);

            if (root == null) {
                throw new TemplateNotFoundException(processedType.templateAttr.filePath, processedType.templateAttr.templateId);
            }

            IXmlLineInfo xmlLineInfo = root;

            StructList<AttributeDefinition> attributes = StructList<AttributeDefinition>.Get();
            StructList<AttributeDefinition> injectedAttributes = StructList<AttributeDefinition>.Get();

            ParseAttributes(shell, "Contents", root.Attributes(), attributes, injectedAttributes, out string genericTypeResolver, out string requireType);

            if (attributes.size == 0) {
                StructList<AttributeDefinition>.Release(ref attributes);
            }

            if (injectedAttributes.size == 0) {
                StructList<AttributeDefinition>.Release(ref injectedAttributes);
            }

            templateRootNode.attributes = ValidateRootAttributes(shell.filePath, attributes);
            templateRootNode.lineInfo = new TemplateLineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
            templateRootNode.genericTypeResolver = genericTypeResolver;
            templateRootNode.requireType = requireType; // always null I think
            ParseChildren(templateRootNode, templateRootNode, root.Nodes());
        }

        private StructList<AttributeDefinition> ValidateRootAttributes(string fileName, StructList<AttributeDefinition> attributes) {
            if (attributes == null) return null;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                // contents should remove `id` attr
                if (attr.type == AttributeType.Attribute && attr.key == "id") {
                    attributes.RemoveAt(i--);
                    continue;
                }

                if (attr.type == AttributeType.Conditional) {
                    Debug.LogError($"<Contents> cannot contain conditional bindings. Ignoring {attr.rawValue} in file {fileName} line {attr.line}");
                    attributes.RemoveAt(i--);
                    continue;
                }

                if (attr.type == AttributeType.Property) {
                    Debug.LogError($"<Contents> cannot contain property bindings. Ignoring {attr.rawValue} in file {fileName} line {attr.line}");
                    attributes.RemoveAt(i--);
                }
            }

            return attributes;
        }

        private void BuildNamespaceListFromUsings(StructList<UsingDeclaration> usings, LightList<string> namespaces) {
            if (usings == null) return;
            for (int i = 0; i < usings.size; i++) {
                if (usings.array[i].name != null) {
                    namespaces.Add(usings.array[i].name);
                }
            }
        }

        private TemplateNode ParseElementTag(TemplateRootNode templateRoot, TemplateNode parent, string namespacePath, string tagName, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo) {
            ProcessedType processedType;
            TemplateNode node = null;

            string lowerNamespace = namespacePath.ToLower();

            if (lowerNamespace == "define") {
                processedType = TypeProcessor.GetProcessedType(typeof(UISlotDefinition));
                node = new SlotNode(templateRoot, parent, processedType, attributes, templateLineInfo, tagName, SlotType.Define);
                templateRoot.AddSlot((SlotNode) node);
                parent.AddChild(node);
                return node;
            }

            if (lowerNamespace == "override") {
                processedType = TypeProcessor.GetProcessedType(typeof(UISlotOverride));
                node = new SlotNode(templateRoot, parent, processedType, attributes, templateLineInfo, tagName, SlotType.Override);
                if (!(parent is ExpandedTemplateNode expanded)) {
                    throw ParseException.InvalidSlotOverride("override", parent.TemplateNodeDebugData, node.TemplateNodeDebugData);
                }

                expanded.AddSlotOverride((SlotNode) node);
                return node;
            }

            if (lowerNamespace == "forward") {
                processedType = TypeProcessor.GetProcessedType(typeof(UISlotForward));
                node = new SlotNode(templateRoot, parent, processedType, attributes, templateLineInfo, tagName, SlotType.Forward);
                if (!(parent is ExpandedTemplateNode)) {
                    throw ParseException.InvalidSlotOverride("forward", parent.TemplateNodeDebugData, node.TemplateNodeDebugData);
                }

                templateRoot.AddSlot((SlotNode) node);
                parent.AddChild(node);
                return node;
            }

            if (string.Equals(tagName, "Repeat", StringComparison.Ordinal)) {
                node = new RepeatNode(templateRoot, parent, null, attributes, templateLineInfo);
                parent.AddChild(node);
                return node;
            }

            if (string.IsNullOrEmpty(lowerNamespace) && string.Equals(tagName, "Children", StringComparison.Ordinal)) {
                throw new ParseException($"Error parsing file {templateRoot.templateShell.filePath} on line {templateLineInfo}: <Children> tag is not supported. Please use an appropriate prefix `forward`, `override`, or `define`");
            }

            if (namespacePath == "UIForia") namespacePath = "UIForia.Elements";

            processedType = ResolveTagName(tagName, namespacePath, templateRoot.templateShell);

            if (processedType == null) {
                throw ParseException.UnresolvedTagName(templateRoot.templateShell.filePath, templateLineInfo, namespacePath + ":" + tagName);
            }

            processedType.ValidateAttributes(attributes);

            if (typeof(UIContainerElement).IsAssignableFrom(processedType.rawType)) {
                node = new ContainerNode(templateRoot, parent, processedType, attributes, templateLineInfo);
            }
            else if (typeof(UITextElement).IsAssignableFrom(processedType.rawType)) {
                node = new TextNode(templateRoot, parent, string.Empty, processedType, attributes, templateLineInfo);
            }
            else if (typeof(UITextSpanElement).IsAssignableFrom(processedType.rawType)) {
                throw new NotImplementedException();
            }
            else if (typeof(UITerminalElement).IsAssignableFrom(processedType.rawType)) {
                node = new TerminalNode(templateRoot, parent, processedType, attributes, templateLineInfo);
            }
            else if (typeof(UIElement).IsAssignableFrom(processedType.rawType)) {
                node = new ExpandedTemplateNode(templateRoot, parent, processedType, attributes, templateLineInfo);
            }

            if (node == null) {
                throw new ParseException("Unresolved tag name: " + tagName);
            }

            node.tagName = tagName;
            node.namespaceName = namespacePath;

            parent.AddChild(node);

            return node;
        }

        private ProcessedType GetDynamicElementType(TemplateShell templateShell, string tagName) {
            for (int i = 0; i < templateShell.unprocessedContentNodes.size; i++) {
                ref RawTemplateContent node = ref templateShell.unprocessedContentNodes.array[i];

                if (node.type != ParsedTemplateType.Dynamic || node.templateId != tagName) {
                    continue;
                }

//                node.processedType = node.processedType ?? CreateDynamicElementType(templateShell, node);

                return node.processedType;
            }

            return null;
        }

        private ProcessedType ResolveTagName(string tagName, string namespacePath, TemplateShell templateShell) {
            ProcessedType retn = GetDynamicElementType(templateShell, tagName);

            if (retn != null) {
                return retn;
            }

            for (int i = 0; i < templateShell.usings.size; i++) {
                UsingDeclaration usingDef = templateShell.usings.array[i];
                if (usingDef.type == UsingDeclarationType.Element && usingDef.name == tagName) {
                    int index = usingDef.pathName.IndexOf("#", StringComparison.Ordinal);
                    if (index != -1) {
                        string path = usingDef.pathName.Substring(0, index);
                        string id = usingDef.pathName.Substring(index + 1);
                        TemplateShell shell = GetOuterTemplateShell(path, null);
                        if (shell == null) {
                            throw new ParseException($"Error in file {templateShell.filePath} line {usingDef.lineNumber}. Unable to find template file at path `{path}`");
                        }

                        return GetDynamicElementType(shell, id);
                    }
                    else {
                        TemplateShell shell = GetOuterTemplateShell(usingDef.pathName, null);
                        if (shell == null) {
                            throw new ParseException($"Error in file {templateShell.filePath} line {usingDef.lineNumber}. Unable to find template file at path `{usingDef.pathName}`");
                        }

                        return GetDynamicElementType(shell, tagName);
                    }
                }
            }

            return TypeProcessor.ResolveTagName(tagName, namespacePath, templateShell.referencedNamespaces);
        }

        // private static ProcessedType CreateDynamicElementType(TemplateShell templateShell, RawTemplateContent node) {
        //     XElement rootNode = node.elementDefinition;
        //
        //     if (!IsValidIdentifier(node.templateId)) {
        //         throw new ParseException($"Expected a valid identifier for template id but `{node.templateId}` is not valid. Please use only letters or numbers (except for first character)");
        //     }
        //
        //     XAttribute generics = rootNode.GetAttribute("generic");
        //
        //     Type type;
        //     if (generics != null) {
        //         string[] genericNames = generics.Value.Split(StringUtil.s_SplitComma, StringSplitOptions.RemoveEmptyEntries);
        //
        //         ReflectionUtil.GenericTypeDefinition[] genericTypeDefinitions = new ReflectionUtil.GenericTypeDefinition[genericNames.Length];
        //
        //         for (int i = 0; i < genericNames.Length; i++) {
        //             genericNames[i] = genericNames[i].Trim();
        //             if (!IsValidIdentifier(genericNames[i])) {
        //                 throw new ParseException("Invalid generic name: " + genericNames[i] + ". Please use only letters or numbers (except for first character)");
        //             }
        //
        //             genericTypeDefinitions[i].name = genericNames[i];
        //         }
        //
        //         List<ReflectionUtil.FieldDefinition> fieldDefinitions = new List<ReflectionUtil.FieldDefinition>();
        //
        //         string typeName = node.templateId + "_" + Guid.NewGuid().ToString().Replace("-", "_");
        //
        //         type = ReflectionUtil.CreateGenericRuntimeType(typeName, typeof(UIElement), genericTypeDefinitions, fieldDefinitions, templateShell.referencedNamespaces);
        //     }
        //     else {
        //         List<ReflectionUtil.FieldDefinition> fieldDefinitions = new List<ReflectionUtil.FieldDefinition>();
        //         List<ReflectionUtil.MethodDefinition> methodDefinitions = new List<ReflectionUtil.MethodDefinition>();
        //
        //         XCData cdata = rootNode.GetCDataChild();
        //
        //         if (cdata != null) {
        //             string contents = cdata.Value.Trim();
        //             var typeBodyParser = new TypeBodyParser();
        //             IXmlLineInfo lineInfo = cdata;
        //
        //             TypeBodyNode astNode = typeBodyParser.Parse(contents, templateShell.filePath, lineInfo.LineNumber);
        //
        //             for (int i = 0; i < astNode.nodes.size; i++) {
        //                 ASTNode n = astNode.nodes.array[i];
        //                 if (n is FieldNode fieldNode) {
        //                     
        //                     ReflectionUtil.FieldDefinition fieldDefinition = new ReflectionUtil.FieldDefinition(fieldNode.typeLookup, fieldNode.name, fieldNode.isStatic);
        //
        //                     fieldDefinitions.Add(fieldDefinition);
        //                 }
        //                 else if (n is MethodNode methodNode) {
        //                         
        //                     ReflectionUtil.MethodDefinition methodDefinition = new ReflectionUtil.MethodDefinition() {
        //                         arguments = methodNode.signatureList,
        //                         returnType = methodNode.returnTypeLookup,
        //                         body = methodNode.body,
        //                         methodName = methodNode.name,
        //                         isStatic = methodNode.isStatic,
        //                     };
        //                     
        //                     methodDefinitions.Add(methodDefinition);
        //                 }
        //             }
        //         }
        //
        //         string typeName = node.templateId + "_" + Guid.NewGuid().ToString().Replace("-", "_");
        //
        //         type = ReflectionUtil.CreateType(typeName, typeof(UIElement), fieldDefinitions, methodDefinitions, templateShell.referencedNamespaces);
        //     }
        //
        //     TemplateAttribute templateAttribute = new TemplateAttribute(TemplateType.File, templateShell.filePath + "#" + node.templateId);
        //
        //     ProcessedType processedType = new ProcessedType(type, templateAttribute, node.templateId) {
        //         IsUnresolvedGeneric = generics != null
        //     };
        //
        //     if (generics == null) {
        //         processedType.Reference();
        //     }
        //
        //     TypeProcessor.AddDynamicElementType(processedType);
        //
        //     processedType.isDynamic = true;
        //
        //     return processedType;
        // }

        private static bool IsValidIdentifier(string input) {
            char first = input[0];

            if (!char.IsLetter(first) && first != '_') return false;

            for (int i = 1; i < input.Length; i++) {
                char c = input[i];
                if (!(char.IsLetterOrDigit(c) || c == '_')) {
                    return false;
                }
            }

            return true;
        }

        private static void CreateOrUpdateTextNode(TemplateRootNode templateRootRoot, TemplateNode parent, string textContent, in TemplateLineInfo templateLineInfo) {
            if (parent is TextNode textParent) {
                if (parent.ChildCount == 0) {
                    TextTemplateProcessor.ProcessTextExpressions(textContent, textParent.textExpressionList);
                }
                else {
                    TextNode node = new TextNode(templateRootRoot, parent, textContent, TypeProcessor.GetProcessedType(typeof(UITextElement)), null, templateLineInfo);
                    TextTemplateProcessor.ProcessTextExpressions(textContent, node.textExpressionList);
                    parent.AddChild(node);
                }
            }
            else {
                TextNode node = new TextNode(templateRootRoot, parent, textContent, TypeProcessor.GetProcessedType(typeof(UITextElement)), null, templateLineInfo);
                TextTemplateProcessor.ProcessTextExpressions(textContent, node.textExpressionList);
                parent.AddChild(node);
            }
        }

        private void ParseChildren(TemplateRootNode templateRoot, TemplateNode parent, IEnumerable<XNode> nodes) {
            string textContext = string.Empty;
            foreach (XNode node in nodes) {
                switch (node.NodeType) {
                    case XmlNodeType.Text: {
                        XText textNode = (XText) node;

                        if (string.IsNullOrWhiteSpace(textNode.Value)) {
                            continue;
                        }

                        textContext += textNode.Value.Trim();

                        continue;
                    }

                    case XmlNodeType.Element: {
                        XElement element = (XElement) node;

                        if (textContext.Length > 0) {
                            IXmlLineInfo textLineInfo = element.PreviousNode;
                            CreateOrUpdateTextNode(templateRoot, parent, textContext, new TemplateLineInfo(textLineInfo.LineNumber, textLineInfo.LinePosition));
                            textContext = string.Empty;
                        }

                        string tagName = element.Name.LocalName;
                        string namespaceName = element.Name.NamespaceName;

                        StructList<AttributeDefinition> attributes = StructList<AttributeDefinition>.Get();
                        StructList<AttributeDefinition> injectedAttributes = StructList<AttributeDefinition>.Get();

                        ParseAttributes(templateRoot.templateShell, tagName, element.Attributes(), attributes, injectedAttributes, out string genericTypeResolver, out string requireType);

                        if (attributes.size == 0) {
                            StructList<AttributeDefinition>.Release(ref attributes);
                        }

                        if (injectedAttributes.size == 0) {
                            StructList<AttributeDefinition>.Release(ref injectedAttributes);
                        }

                        IXmlLineInfo lineInfo = element;
                        TemplateNode p = ParseElementTag(templateRoot, parent, namespaceName, tagName, attributes, new TemplateLineInfo(lineInfo.LineNumber, lineInfo.LinePosition));

                        p.genericTypeResolver = genericTypeResolver;
                        p.requireType = requireType;

                        if (p is SlotNode slotNode) {
                            slotNode.injectedAttributes = injectedAttributes;
                        }
                        else if (injectedAttributes != null) {
                            throw new ParseException("Only slot nodes can have injected attributes");
                        }

                        ParseChildren(templateRoot, p, element.Nodes());

                        continue;
                    }

                    case XmlNodeType.Comment:
                        continue;
                }

                throw new TemplateParseException(node, $"Unable to handle node type: {node.NodeType}");
            }

            if (textContext.Length != 0) {
                CreateOrUpdateTextNode(templateRoot, parent, textContext, parent.lineInfo); // todo -- line info probably wrong
            }
        }

        private static void HandleAttribute(TemplateShell templateShell, string tagName, string prefix, string name, int line, int column, string value, StructList<AttributeDefinition> attributes) {
            AttributeType attributeType = AttributeType.Property;
            AttributeFlags flags = 0;

            // once:if=""
            // enable:if=""
            // todo -- not valid everywhere
            if (name.Contains(".once") || name.Contains(".const")) {
                name = name.Replace(".once", "");
                name = name.Replace(".const", "");
                flags |= AttributeFlags.Const;
            }

            // todo -- validate this syntax
            if (name.Contains(".enable")) {
                name = name.Replace(".enable", "");
                flags |= AttributeFlags.EnableOnly;
            }

            if (name == "if") {
                attributeType = AttributeType.Conditional;
            }
            else if (prefix == string.Empty) {
                if (name == "style") {
                    attributeType = AttributeType.Style;
                    name = "style";
                }
                else if (name.StartsWith("style.")) {
                    attributeType = AttributeType.InstanceStyle;
                    name = name.Substring("style.".Length);
                }
            }
            else {
                switch (prefix) {
                    case "property":
                        break;

                    case "attr": {
                        attributeType = AttributeType.Attribute;
                        if (value[0] != '{' || value[value.Length - 1] != '}') {
                            flags |= AttributeFlags.Const;
                        }

                        break;
                    }

                    case "slot": {
                        attributeType = AttributeType.Slot;
                        break;
                    }

                    case "mouse":
                        attributeType = AttributeType.Mouse;
                        break;

                    case "key":
                        attributeType = AttributeType.Key;
                        break;

                    case "drag":
                        attributeType = AttributeType.Drag;
                        break;

                    case "onChange":
                        attributeType = AttributeType.ChangeHandler;
                        break;

                    case "touch":
                        attributeType = AttributeType.Touch;
                        break;

                    case "controller":
                        attributeType = AttributeType.Controller;
                        break;

                    case "style":
                        attributeType = AttributeType.InstanceStyle;
                        if (name.Contains(".")) {
                            if (name.StartsWith("hover.")) {
                                flags |= AttributeFlags.StyleStateHover;
                                name = name.Substring("hover.".Length);
                            }
                            else if (name.StartsWith("focus.")) {
                                flags |= AttributeFlags.StyleStateFocus;
                                name = name.Substring("focus.".Length);
                            }
                            else if (name.StartsWith("active.")) {
                                flags |= AttributeFlags.StyleStateActive;
                                name = name.Substring("active.".Length);
                            }
                            else {
                                throw CompileException.UnknownStyleState(new AttributeNodeDebugData(templateShell.filePath, tagName, new TemplateLineInfo(line, column), value), name.Split('.')[0]);
                            }
                        }

                        break;

                    case "evt":
                        attributeType = AttributeType.Event;
                        break;

                    case "ctx":

                        attributeType = AttributeType.Context;

                        if (name == "element" || name == "parent" || name == "root" || name == "evt") {
                            throw new ParseException($"`{name} is a reserved name and cannot be used as a context variable name");
                        }

                        break;

                    case "var":
                        attributeType = AttributeType.ImplicitVariable;

                        if (name == "element" || name == "parent" || name == "root" || name == "evt") {
                            throw new ParseException($"`{name} is a reserved name and cannot be used as a context variable name");
                        }

                        break;

                    case "sync":
                        attributeType = AttributeType.Property;
                        flags |= AttributeFlags.Sync;
                        break;

                    case "expose":
                        attributeType = AttributeType.Expose;
                        if (name == "element" || name == "parent" || name == "root" || name == "evt") {
                            throw new ParseException($"`{name} is a reserved name and cannot be used as a context variable name");
                        }

                        break;

                    case "alias":
                        attributeType = AttributeType.Alias;
                        if (name == "element" || name == "parent" || name == "root" || name == "evt") {
                            throw new ParseException($"`{name} is a reserved name and cannot be used as a context variable name");
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException("Unknown attribute prefix: " + prefix);
                }
            }

            string raw = string.Empty;
            if (!string.IsNullOrEmpty(prefix)) {
                TextUtil.StringBuilder.Append(prefix);
                TextUtil.StringBuilder.Append(":");
                TextUtil.StringBuilder.Append(name);
                TextUtil.StringBuilder.Append("=\"");
                TextUtil.StringBuilder.Append(value);
                TextUtil.StringBuilder.Append("\"");
                raw = TextUtil.StringBuilder.ToString();
                TextUtil.StringBuilder.Clear();
            }
            else {
                TextUtil.StringBuilder.Append(name);
                TextUtil.StringBuilder.Append("=\"");
                TextUtil.StringBuilder.Append(value);
                TextUtil.StringBuilder.Append("\"");
                raw = TextUtil.StringBuilder.ToString();
                TextUtil.StringBuilder.Clear();
            }

            attributes.Add(new AttributeDefinition(raw, attributeType, flags, name, value, templateShell, line, column));
        }

        private static void ParseAttributes(TemplateShell templateShell, string tagName, IEnumerable<XAttribute> xmlAttributes, StructList<AttributeDefinition> attributes, StructList<AttributeDefinition> injectedAttributes, out string genericTypeResolver, out string requireType) {
            genericTypeResolver = null;
            requireType = null;

            foreach (XAttribute attr in xmlAttributes) {
                string prefix = attr.Name.NamespaceName;
                string name = attr.Name.LocalName.Trim();

                int line = ((IXmlLineInfo) attr).LineNumber;
                int column = ((IXmlLineInfo) attr).LinePosition;

                if (name == "id" && string.IsNullOrEmpty(prefix)) {
                    prefix = "attr";
                }

                if (prefix == "generic" && name == "type") {
                    genericTypeResolver = attr.Value;
                    continue;
                }

                if (prefix == "require" && name == "type") {
                    requireType = attr.Value;
                    continue;
                }

                if (prefix.StartsWith("inject.")) {
                    prefix = prefix.Substring("inject.".Length);
                    HandleAttribute(templateShell, tagName, prefix, name, line, column, attr.Value, injectedAttributes);
                    continue;
                }

                HandleAttribute(templateShell, tagName, prefix, name, line, column, attr.Value, attributes);
            }
        }

        private static UsingDeclaration ParseUsing(XElement element) {
            XAttribute namespaceAttr = element.GetAttribute("namespace");
            XAttribute elementAttr = element.GetAttribute("element");
            XAttribute pathAttr = element.GetAttribute("path");

            if (elementAttr != null || pathAttr != null) {
                if (elementAttr == null) {
                    throw new CompileException("<Using> tag requires `element` attribute if `path` is provided");
                }

                if (pathAttr == null) {
                    throw new CompileException("<Using> tag requires `path` attribute if `element` is provided");
                }

                return new UsingDeclaration() {
                    name = elementAttr.Value.Trim(),
                    pathName = pathAttr.Value.Trim(),
                    type = UsingDeclarationType.Element,
                    lineNumber = ((IXmlLineInfo) element).LineNumber
                };
            }

            if (namespaceAttr == null) {
                throw new TemplateParseException(element, "<Using/> tags require a 'namespace' attribute");
            }

            string value = namespaceAttr.Value.Trim();
            if (string.IsNullOrEmpty(value)) {
                throw new TemplateParseException(element, "<Using/> tags require a 'namespace' attribute with a value");
            }

            return new UsingDeclaration() {
                name = value,
                type = UsingDeclarationType.Namespace,
                lineNumber = ((IXmlLineInfo) element).LineNumber
            };
        }

        private StyleDefinition ParseStyleSheet(string templateId, XElement styleElement) {
            XAttribute aliasAttr = styleElement.GetAttribute("as");
            XAttribute importPathAttr = styleElement.GetAttribute("path") ?? styleElement.GetAttribute("src");

            string rawText = string.Empty;
            // styles can have either a class path or a body
            foreach (XNode node in styleElement.Nodes()) {
                switch (node.NodeType) {
                    case XmlNodeType.Text:
                        rawText += ((XText) node).Value;
                        continue;

                    case XmlNodeType.Element:
                        throw new TemplateParseException(node, "<Style> can only have text children, no elements");

                    case XmlNodeType.Comment:
                        continue;
                }

                throw new TemplateParseException(node, $"Unable to handle node type: {node.NodeType}");
            }

            string alias = StyleDefinition.k_EmptyAliasName;
            if (aliasAttr != null && !string.IsNullOrEmpty(aliasAttr.Value)) {
                alias = aliasAttr.Value.Trim();
            }

            // if we have a body, expect import path to be null
            if (!string.IsNullOrEmpty(rawText) && !string.IsNullOrWhiteSpace(rawText)) {
                if (importPathAttr != null && !string.IsNullOrEmpty(importPathAttr.Value)) {
                    throw new TemplateParseException(styleElement, "Expected 'path' or 'src' to be null when a body is provided to a style tag");
                }

                return new StyleDefinition(alias, templateId + ".style", rawText);
            }

            // if we have no body then expect path to be set
            if (importPathAttr == null || string.IsNullOrEmpty(importPathAttr.Value)) {
                throw new TemplateParseException(styleElement, "Expected 'path' or 'src' to be provided when a body is not provided in a style tag");
            }

            return new StyleDefinition(alias, importPathAttr.Value.Trim());
        }

    }

}