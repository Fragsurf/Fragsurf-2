using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Templates;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace UIForia.Compilers {

    public class TemplateCompiler {

        internal const string k_InputEventParameterName = "__evt";
        private static readonly char[] s_StyleSeparator = {' '};

        private readonly UIForiaLinqCompiler enabledCompiler;
        private readonly UIForiaLinqCompiler createdCompiler;
        private readonly UIForiaLinqCompiler updateCompiler;
        private readonly UIForiaLinqCompiler lateCompiler;
        private readonly UIForiaLinqCompiler typeResolver;
        private bool resolvingTypeOnly;

        private Expression changeHandlerCurrentValue;
        private Expression changeHandlerPreviousValue;
        private Expression currentEvent;

        private readonly CompiledTemplateData templateData;
        private readonly Dictionary<Type, CompiledTemplate> templateMap;
        private readonly TemplateCache templateCache;

        private int contextId = 1;
        private int NextContextId => contextId++;
        private SlotAttributeData slotScope;

        private readonly LightStack<LightStack<ContextVariableDefinition>> contextStack;

        internal static readonly DynamicStyleListTypeWrapper s_DynamicStyleListTypeWrapper = new DynamicStyleListTypeWrapper();
        internal static readonly RepeatKeyFnTypeWrapper s_RepeatKeyFnTypeWrapper = new RepeatKeyFnTypeWrapper();

        internal static readonly MethodInfo s_CreateFromPool = typeof(Application).GetMethod(nameof(Application.CreateElementFromPoolWithType));
        internal static readonly MethodInfo s_LinqBindingNode_Get = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.Get), BindingFlags.Static | BindingFlags.Public);
        internal static readonly MethodInfo s_LinqBindingNode_GetSlotModifyNode = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetSlotModifyNode), BindingFlags.Static | BindingFlags.Public);
        internal static readonly MethodInfo s_LinqBindingNode_GetSlotNode = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetSlotNode), BindingFlags.Static | BindingFlags.Public);
        internal static readonly FieldInfo s_StructList_ElementAttr_Array = typeof(StructList<ElementAttribute>).GetField("array");

        internal static readonly FieldInfo s_SlotElement_SlotId = typeof(UISlotBase).GetField(nameof(UISlotBase.slotId));

        internal static readonly FieldInfo s_RepeatElement_IndexVar = typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.indexVarId));
        internal static readonly FieldInfo s_RepeatElement_ItemVar = typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.itemVarId));
        internal static readonly FieldInfo s_RepeatElement_Scope = typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.scope));
        internal static readonly FieldInfo s_RepeatElement_ContextRoot = typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.templateContextRoot));
        internal static readonly FieldInfo s_RepeatElement_TemplateSpawnId = typeof(UIRepeatElement).GetField(nameof(UIRepeatElement.templateSpawnId));

        internal static readonly ConstructorInfo s_TemplateScope_Ctor = typeof(TemplateScope).GetConstructor(new[] {typeof(Application)});
        internal static readonly FieldInfo s_TemplateScope_ApplicationField = typeof(TemplateScope).GetField(nameof(TemplateScope.application));
        internal static readonly FieldInfo s_TemplateScope_InnerContext = typeof(TemplateScope).GetField(nameof(TemplateScope.innerSlotContext));
        internal static readonly MethodInfo s_TemplateScope_AddSlotForward = typeof(TemplateScope).GetMethod(nameof(TemplateScope.AddSlotForward));
        internal static readonly MethodInfo s_TemplateScope_AddSlotOverride = typeof(TemplateScope).GetMethod(nameof(TemplateScope.AddSlotOverride));
        internal static readonly MethodInfo s_TemplateScope_SetParentScopeList = typeof(TemplateScope).GetMethod(nameof(TemplateScope.SetParentScope));
        internal static readonly MethodInfo s_TemplateScope_GetOverrideScope = typeof(TemplateScope).GetMethod(nameof(TemplateScope.GetOverrideScope));
        internal static readonly MethodInfo s_TemplateScope_Clone = typeof(TemplateScope).GetMethod(nameof(TemplateScope.Clone));

        internal static readonly ConstructorInfo s_ElementAttributeCtor = typeof(ElementAttribute).GetConstructor(new[] {typeof(string), typeof(string)});
        internal static readonly FieldInfo s_ElementAttributeList = typeof(UIElement).GetField("attributes", BindingFlags.Public | BindingFlags.Instance);
        internal static readonly FieldInfo s_TextElement_Text = typeof(UITextElement).GetField(nameof(UITextElement.text), BindingFlags.Instance | BindingFlags.Public);
        internal static readonly MethodInfo s_TextElement_SetText = typeof(UITextElement).GetMethod(nameof(UITextElement.SetText), BindingFlags.Instance | BindingFlags.Public);

        internal static readonly FieldInfo s_UIElement_StyleSet = typeof(UIElement).GetField(nameof(UIElement.style), BindingFlags.Instance | BindingFlags.Public);
        internal static readonly PropertyInfo s_UIElement_Application = typeof(UIElement).GetProperty(nameof(UIElement.application), BindingFlags.Instance | BindingFlags.Public);
        internal static readonly MethodInfo s_UIElement_OnUpdate = typeof(UIElement).GetMethod(nameof(UIElement.OnUpdate), BindingFlags.Instance | BindingFlags.Public);
        internal static readonly MethodInfo s_UIElement_OnBeforePropertyBindings = typeof(UIElement).GetMethod(nameof(UIElement.OnBeforePropertyBindings), BindingFlags.Instance | BindingFlags.Public);
        internal static readonly MethodInfo s_UIElement_OnAfterPropertyBindings = typeof(UIElement).GetMethod(nameof(UIElement.OnAfterPropertyBindings), BindingFlags.Instance | BindingFlags.Public);
        internal static readonly MethodInfo s_UIElement_SetAttribute = typeof(UIElement).GetMethod(nameof(UIElement.SetAttribute), BindingFlags.Instance | BindingFlags.Public);
        internal static readonly MethodInfo s_UIElement_SetEnabled = typeof(UIElement).GetMethod(nameof(UIElement.internal__dontcallmeplease_SetEnabledIfBinding), BindingFlags.Instance | BindingFlags.Public);
        internal static readonly FieldInfo s_UIElement_Parent = typeof(UIElement).GetField(nameof(UIElement.parent), BindingFlags.Instance | BindingFlags.Public);

        internal static readonly MethodInfo s_StyleSet_InternalInitialize = typeof(UIStyleSet).GetMethod(nameof(UIStyleSet.internal_Initialize), BindingFlags.Instance | BindingFlags.Public);
        internal static readonly MethodInfo s_StyleSet_SetBaseStyles = typeof(UIStyleSet).GetMethod(nameof(UIStyleSet.SetBaseStyles), BindingFlags.Instance | BindingFlags.Public);
        internal static readonly MethodInfo s_StyleSet_AddBaseStyle = typeof(UIStyleSet).GetMethod(nameof(UIStyleSet.internal_AddBaseStyle));

        internal static readonly MethodInfo s_TemplateMetaData_GetStyleById = typeof(TemplateMetaData).GetMethod(nameof(TemplateMetaData.GetStyleById), BindingFlags.Instance | BindingFlags.Public);

        internal static readonly MethodInfo s_LightList_UIStyleGroupContainer_Get = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.Get), BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo s_LightList_UIStyleGroupContainer_Release = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.Release), BindingFlags.Public | BindingFlags.Instance);
        internal static readonly MethodInfo s_LightList_UIStyleGroupContainer_Add = typeof(LightList<UIStyleGroupContainer>).GetMethod(nameof(LightList<UIStyleGroupContainer>.Add), BindingFlags.Public | BindingFlags.Instance);

        internal static readonly MethodInfo s_Application_CreateSlot = typeof(Application).GetMethod(nameof(Application.CreateSlot), BindingFlags.Public | BindingFlags.Instance);
        internal static readonly MethodInfo s_Application_HydrateTemplate = typeof(Application).GetMethod(nameof(Application.HydrateTemplate), BindingFlags.Public | BindingFlags.Instance);
        internal static readonly MethodInfo s_Application_GetTemplateMetaData = typeof(Application).GetMethod(nameof(Application.GetTemplateMetaData), BindingFlags.Public | BindingFlags.Instance);
        internal static readonly PropertyInfo s_Application_GetTemplateMetaDataArray = typeof(Application).GetProperty(nameof(Application.zz_Internal_TemplateMetaData), BindingFlags.Public | BindingFlags.Instance);

        internal static readonly MethodInfo s_InputHandlerGroup_AddMouseEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddMouseEvent));
        internal static readonly MethodInfo s_InputHandlerGroup_AddDragCreator = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddDragCreator));
        internal static readonly MethodInfo s_InputHandlerGroup_AddDragEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddDragEvent));
        internal static readonly MethodInfo s_InputHandlerGroup_AddKeyboardEvent = typeof(InputHandlerGroup).GetMethod(nameof(InputHandlerGroup.AddKeyboardEvent));

        internal static readonly PropertyInfo s_Element_IsEnabled = typeof(UIElement).GetProperty(nameof(UIElement.isEnabled));
        internal static readonly PropertyInfo s_Element_IsEnabledAndNeedsUpdate = typeof(UIElement).GetProperty(nameof(UIElement.__internal_isEnabledAndNeedsUpdate));
        internal static readonly FieldInfo s_UIElement_BindingNode = typeof(UIElement).GetField(nameof(UIElement.bindingNode));

        internal static readonly MethodInfo s_LinqBindingNode_CreateLocalContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.CreateLocalContextVariable));
        internal static readonly MethodInfo s_LinqBindingNode_GetContextVariable = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetContextVariable));
        internal static readonly MethodInfo s_LinqBindingNode_GetRepeatItem = typeof(LinqBindingNode).GetMethod(nameof(LinqBindingNode.GetRepeatItem));
        internal static readonly FieldInfo s_LinqBindingNode_ReferencedContext = typeof(LinqBindingNode).GetField(nameof(LinqBindingNode.referencedContexts));

        internal static readonly MethodInfo s_EventUtil_Subscribe = typeof(EventUtil).GetMethod(nameof(EventUtil.Subscribe));

        internal static readonly MethodInfo s_DynamicStyleList_Flatten = typeof(DynamicStyleList).GetMethod(nameof(DynamicStyleList.Flatten));

        internal static readonly Expression s_StringBuilderExpr = Expression.Field(null, typeof(StringUtil), nameof(StringUtil.s_CharStringBuilder));
        internal static readonly Expression s_StringBuilderClear = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod("Clear"));
        internal static readonly Expression s_StringBuilderToString = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod("ToString", Type.EmptyTypes));
        internal static readonly MethodInfo s_StringBuilder_AppendString = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(string)});
        internal static readonly MethodInfo s_StringBuilder_AppendInt16 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(short)});
        internal static readonly MethodInfo s_StringBuilder_AppendInt32 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(int)});
        internal static readonly MethodInfo s_StringBuilder_AppendInt64 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(long)});
        internal static readonly MethodInfo s_StringBuilder_AppendUInt16 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(ushort)});
        internal static readonly MethodInfo s_StringBuilder_AppendUInt32 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(uint)});
        internal static readonly MethodInfo s_StringBuilder_AppendUInt64 = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(ulong)});
        internal static readonly MethodInfo s_StringBuilder_AppendFloat = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(float)});
        internal static readonly MethodInfo s_StringBuilder_AppendDouble = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(double)});
        internal static readonly MethodInfo s_StringBuilder_AppendDecimal = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(decimal)});
        internal static readonly MethodInfo s_StringBuilder_AppendByte = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(byte)});
        internal static readonly MethodInfo s_StringBuilder_AppendSByte = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(sbyte)});
        internal static readonly MethodInfo s_StringBuilder_AppendBool = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(bool)});
        internal static readonly MethodInfo s_StringBuilder_AppendChar = typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.Append), new[] {typeof(char)});
        private MaterialDatabase materialDatabase;

        private TemplateCompiler(TemplateSettings settings, MaterialDatabase materialDatabase) {
            this.templateCache = new TemplateCache(settings);
            this.templateMap = new Dictionary<Type, CompiledTemplate>();
            this.templateData = new CompiledTemplateData(settings);
            this.updateCompiler = new UIForiaLinqCompiler();
            this.createdCompiler = new UIForiaLinqCompiler();
            this.enabledCompiler = new UIForiaLinqCompiler();
            this.lateCompiler = new UIForiaLinqCompiler();
            this.typeResolver = new UIForiaLinqCompiler();
            this.materialDatabase = materialDatabase;

            Func<string, LinqCompiler, Expression> resolveAlias = ResolveAlias;

            this.createdCompiler.resolveAlias = resolveAlias;
            this.enabledCompiler.resolveAlias = resolveAlias;
            this.updateCompiler.resolveAlias = resolveAlias;
            this.lateCompiler.resolveAlias = resolveAlias;
            this.typeResolver.resolveAlias = resolveAlias;

            this.contextStack = new LightStack<LightStack<ContextVariableDefinition>>();
        }

        public static CompiledTemplateData CompileTemplates(Type appRootType, TemplateSettings templateSettings) {

            TypeProcessor.ClearDynamics();

            MaterialDatabase materialDatabase = MaterialAssetBuilder.BuildMaterialDatabase(templateSettings.materialAssets);

            TemplateCompiler instance = new TemplateCompiler(templateSettings, materialDatabase);

            CompiledTemplateData compiledTemplateData = instance.CompileRoot(appRootType, templateSettings.dynamicallyCreatedTypes);

            compiledTemplateData.materialDatabase = materialDatabase;

            return compiledTemplateData;
        }

        private CompiledTemplateData CompileRoot(Type appRootType, List<Type> dynamicallyCreatedTypes) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (!typeof(UIElement).IsAssignableFrom(appRootType)) {
                throw new ArgumentException($"You can only create elements which are subclasses of UIElement. {appRootType} does not inherit from UIElement");
            }

            if (typeof(UIContainerElement).IsAssignableFrom(appRootType)) {
                throw new ArgumentException($"You can only create elements which are subclasses of UIElement and are not subclasses of UITerminalElement, UITextElement or UIContainerElement. {appRootType} inherits from UIContainerElement");
            }

            if (typeof(UITerminalElement).IsAssignableFrom(appRootType)) {
                throw new ArgumentException($"You can only create elements which are subclasses of UIElement and are not subclasses of UITerminalElement, UITextElement or UIContainerElement. {appRootType} inherits from UITerminalElement");
            }

            if (typeof(UITextElement).IsAssignableFrom(appRootType)) {
                throw new ArgumentException($"You can only create elements which are subclasses of UIElement and are not subclasses of UITerminalElement, UITextElement or UIContainerElement. {appRootType} inherits from UITextElement");
            }

            ProcessedType appRoot = TypeProcessor.GetProcessedType(appRootType);

            TemplateRootNode templateRootNode = templateCache.GetParsedTemplate(appRoot);

            Compile(templateRootNode, true);

            if (dynamicallyCreatedTypes != null) {
                for (int i = 0; i < dynamicallyCreatedTypes.Count; i++) {
                    Type type = dynamicallyCreatedTypes[i];

                    if (type == null) continue;

                    if (type.IsGenericTypeDefinition) {
                        throw new ArgumentException($"You can only create elements dynamically which are of concrete class types. {type} is a generic type definition and cannot be compiled");
                    }

                    if (!typeof(UIElement).IsAssignableFrom(type)) {
                        throw new ArgumentException($"You can only create elements which are subclasses of UIElement. {type} does not inherit from UIElement");
                    }

                    ProcessedType processedType = TypeProcessor.GetProcessedType(type);
                    CompiledTemplate template = GetCompiledTemplate(processedType, true);

                    templateData.AddDynamicTemplate(type, processedType.id, template.templateId);
                }
            }

            stopwatch.Stop();
            Debug.Log("Compiled UIForia templates in " + stopwatch.Elapsed.TotalSeconds.ToString("F2") + " seconds");
            return templateData;
        }

        private CompiledTemplate GetCompiledTemplate(ProcessedType processedType, bool isRoot = false) {
            if (templateMap.TryGetValue(processedType.rawType, out CompiledTemplate retn)) {
                return retn;
            }

            TemplateRootNode templateRootNode = templateCache.GetParsedTemplate(processedType);

            CompiledTemplate compiledTemplate = Compile(templateRootNode, isRoot);

            return compiledTemplate;
        }

        private CompilationContext CompileTemplateMetaData(TemplateRootNode templateRootNode) {
            CompiledTemplate compiledTemplate = templateData.CreateTemplate(templateRootNode.templateShell.filePath, templateRootNode.templateName);

            LightList<string> namespaces = new LightList<string>(4);

            if (templateRootNode.templateShell.usings != null) {
                for (int i = 0; i < templateRootNode.templateShell.usings.size; i++) {
                    if (templateRootNode.templateShell.usings.array[i].type == UsingDeclarationType.Namespace) {
                        namespaces.Add(templateRootNode.templateShell.usings.array[i].name);
                    }
                }
            }

            ProcessedType processedType = templateRootNode.processedType;

            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            compiledTemplate.elementType = processedType;

            compiledTemplate.namespaces = namespaces;

            CompilationContext ctx = new CompilationContext(templateRootNode) {
                namespaces = namespaces,
                rootType = processedType,
                rootParam = rootParam,
                templateScope = scopeParam,
                applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField),
                compiledTemplate = compiledTemplate,
                ContextExpr = rootParam
            };

            ctx.Initialize(rootParam);

            for (int i = 0; i < templateRootNode.templateShell.styles.size; i++) {
                ref StyleDefinition styleDef = ref templateRootNode.templateShell.styles.array[i];

                StyleSheet sheet = templateData.ImportStyleSheet(styleDef, materialDatabase, templateRootNode.templateShell.filePath);

                if (sheet != null) {
                    ctx.AddStyleSheet(styleDef.alias, sheet);
                }
            }

            if (ctx.styleSheets != null && ctx.styleSheets.size > 0) {
                compiledTemplate.templateMetaData.styleReferences = ctx.styleSheets.ToArray();
            }

            templateMap[processedType.rawType] = compiledTemplate;

            return ctx;
        }

        private CompiledTemplate Compile(TemplateRootNode templateRootNode, bool isRoot = false) {
            CompilationContext ctx = CompileTemplateMetaData(templateRootNode);
            contextStack.Push(new LightStack<ContextVariableDefinition>());

            ProcessedType processedType = templateRootNode.processedType;

            if (!processedType.rawType.IsNested && !processedType.rawType.IsPublic) {
                throw new CompileException($"{processedType.rawType} is not public, but must be in order to be used in a template. {templateRootNode.TemplateNodeDebugData}");
            }

            if (isRoot) {
                ctx.Comment("new " + TypeNameGenerator.GetTypeName(processedType.rawType));

                Expression createRootExpression = CreateElement(ctx, processedType, Expression.Default(typeof(UIElement)), templateRootNode.ChildCount, CountRealAttributes(templateRootNode.attributes), ctx.compiledTemplate.templateId);
                ctx.Assign(ctx.rootParam, createRootExpression);
                ProcessAttrsAndVisitChildren(ctx, templateRootNode);
            }
            else {
                VisitChildren(ctx, templateRootNode);
            }

            ctx.templateRootNode = templateRootNode;

            ctx.Return(ctx.rootParam);
            ctx.compiledTemplate.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), (ParameterExpression) ctx.rootParam, (ParameterExpression) ctx.templateScope);
            contextStack.Pop();
            return ctx.compiledTemplate;
        }

        private static Type ResolveRequiredType(IList<string> namespaces, string typeName, Type rootType) {
            if (typeName != null) {
                Type requiredType = TypeProcessor.ResolveTypeExpression(rootType, namespaces, typeName);

                if (requiredType == null) {
                    throw new CompileException($"Unable to resolve required child type `{typeName}`");
                }
                else if (!requiredType.IsInterface && !typeof(UIElement).IsAssignableFrom(requiredType)) {
                    throw new CompileException($"When requiring an explicit child type, that type must either be an interface or a subclass of UIElement. {requiredType} was neither");
                }

                return requiredType;
            }

            return null;
        }

        private void VisitChildren(CompilationContext ctx, TemplateNode templateNode, Type requiredType = null) {
            if (templateNode.ChildCount == 0) {
                return;
            }

            ctx.PushScope();

            for (int i = 0; i < templateNode.ChildCount; i++) {
                Visit(ctx, templateNode[i], requiredType);
            }

            ctx.PopScope();
        }

        private Expression Visit(CompilationContext ctx, TemplateNode templateNode, Type requiredType) {
            if (templateNode is RepeatNode repeatNode) {
                // todo -- fix loop hole with required type
                return CompileRepeatNode(ctx, repeatNode);
            }

            if (templateNode.processedType.IsUnresolvedGeneric) {
                templateNode.processedType = ResolveGenericElementType(ctx.namespaces, ctx.templateRootNode.ElementType, templateNode);
            }

            if (requiredType != null) {
                if (!requiredType.IsAssignableFrom(templateNode.processedType.rawType)) {
                    throw new CompileException($"Expected element that can be assigned to {requiredType} but {templateNode.processedType.rawType} (<{templateNode.processedType.tagName}>) is not.");
                }
            }

            switch (templateNode) {
                case TextNode textNode:
                    return CompileTextNode(ctx, textNode);

                case ContainerNode containerNode:
                    return CompileContainerNode(ctx, containerNode);

                case SlotNode slotNode:
                    return CompileSlotDefinition(ctx, slotNode);

                case TerminalNode terminalNode:
                    return CompileTerminalNode(ctx, terminalNode);

                case ExpandedTemplateNode expandedTemplateNode:
                    return CompileExpandedNode(ctx, expandedTemplateNode);
            }

            return null;
        }

        private Expression CompileRepeatNode(CompilationContext ctx, RepeatNode repeatNode) {
            if (repeatNode.HasProperty("count")) {
                if (repeatNode.HasProperty("list") || repeatNode.HasProperty("start") || repeatNode.HasProperty("end")) {
                    throw CompileException.UnresolvedRepeatType("count", "list", "start", "end");
                }

                return CompileRepeatCount(ctx, repeatNode);
            }
            else if (repeatNode.HasProperty("list")) {
                if (repeatNode.HasProperty("count")) {
                    throw CompileException.UnresolvedRepeatType("count", "");
                }

                return CompileRepeatList(ctx, repeatNode);
            }
            // else if (repeatNode.HasProperty("page") || repeatNode.HasProperty("pageSize")) { }

            throw new NotImplementedException("<Repeat> must have either a `count` or a `list` property");
        }

        private Expression CompileRepeatCount(CompilationContext ctx, RepeatNode repeatNode) {
            ParameterExpression nodeExpr = ctx.ElementExpr;

            repeatNode.processedType = TypeProcessor.GetProcessedType(typeof(UIRepeatCountElement));

            ctx.CommentNewLineBefore("new " + TypeNameGenerator.GetTypeName(typeof(UIRepeatCountElement)));
            ctx.Assign(nodeExpr, CreateElement(ctx, repeatNode.processedType, ctx.ParentExpr, 0, CountRealAttributes(repeatNode.attributes), ctx.compiledTemplate.templateId));

            MemberExpression templateSpawnIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), s_RepeatElement_TemplateSpawnId);
            MemberExpression templateRootContext = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), s_RepeatElement_ContextRoot);
            MemberExpression scopeVar = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), s_RepeatElement_Scope);
            MemberExpression indexVarIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), s_RepeatElement_IndexVar);

            StructList<ContextAliasActions> mods = CompileBindings(ctx, repeatNode, repeatNode.attributes).contextModifications;

            int spawnId = CompileRepeatTemplate(ctx, repeatNode, RepeatType.Count, out int _, out int indexVarId);

            UndoContextMods(mods);

            ctx.Assign(templateSpawnIdField, Expression.Constant(spawnId));
            ctx.Assign(templateRootContext, ctx.rootParam);
            ctx.Assign(scopeVar, ExpressionFactory.CallInstanceUnchecked(ctx.templateScope, s_TemplateScope_Clone));
            ctx.Assign(indexVarIdField, Expression.Constant(indexVarId));

            return nodeExpr;
        }

        private Expression CompileRepeatList(CompilationContext ctx, RepeatNode repeatNode) {
            ParameterExpression nodeExpr = ctx.ElementExpr;

            repeatNode.processedType = TypeProcessor.GetProcessedType(typeof(UIRepeatElement<>));
            repeatNode.processedType = ResolveGenericElementType(ctx.namespaces, ctx.templateRootNode.ElementType, repeatNode);

            ctx.CommentNewLineBefore("new " + TypeNameGenerator.GetTypeName(typeof(UIRepeatCountElement)));
            ctx.Assign(nodeExpr, CreateElement(ctx, repeatNode.processedType, ctx.ParentExpr, 0, CountRealAttributes(repeatNode.attributes), ctx.compiledTemplate.templateId));

            MemberExpression templateSpawnIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), s_RepeatElement_TemplateSpawnId);
            MemberExpression templateRootContext = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), s_RepeatElement_ContextRoot);
            MemberExpression scopeVar = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), s_RepeatElement_Scope);
            MemberExpression itemVarIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), s_RepeatElement_ItemVar);
            MemberExpression indexVarIdField = Expression.Field(ExpressionFactory.Convert(nodeExpr, typeof(UIRepeatElement)), s_RepeatElement_IndexVar);

            BindingOutput bindingOutput = CompileBindings(ctx, repeatNode, repeatNode.attributes);

            int spawnId = CompileRepeatTemplate(ctx, repeatNode, RepeatType.List, out int itemVarId, out int indexVarId);

            UndoContextMods(bindingOutput.contextModifications);

            ctx.Assign(templateSpawnIdField, Expression.Constant(spawnId));
            ctx.Assign(templateRootContext, ctx.rootParam);
            ctx.Assign(scopeVar, ExpressionFactory.CallInstanceUnchecked(ctx.templateScope, s_TemplateScope_Clone));
            ctx.Assign(indexVarIdField, Expression.Constant(indexVarId));
            ctx.Assign(itemVarIdField, Expression.Constant(itemVarId));

            return nodeExpr;
        }

        private Expression CompileTerminalNode(CompilationContext ctx, TerminalNode terminalNode) {
            throw new NotImplementedException();
        }

        private ContextVariableDefinition[] CloneContextStack() {
            return contextStack.Peek().ToArray();
        }

        private Expression CompileSlotDefinition(CompilationContext parentContext, SlotNode slotNode) {
            // we want to try to resolve the slot name. if we can't fall back, if fallback id is -1 then don't add a child
            CompiledSlot compiledSlot = templateData.CreateSlot(parentContext.compiledTemplate.filePath, parentContext.compiledTemplate.templateName, slotNode.slotName, slotNode.slotType);

            compiledSlot.overrideDepth = 0;

            parentContext.compiledTemplate.AddSlot(compiledSlot);

            StructList<AttributeDefinition> attributes = new StructList<AttributeDefinition>();
            StructList<AttributeDefinition> exposedAttributes = StructList<AttributeDefinition>.Get();

            if (slotNode.attributes != null) {
                SlotAttributeData attrData = new SlotAttributeData();
                attrData.slotDepth = 0;
                attrData.slotContextType = parentContext.rootType;
                attrData.namespaces = parentContext.namespaces.Clone();
                attrData.contextStack = CloneContextStack();
                attrData.templateMetaData = parentContext.compiledTemplate.templateMetaData;

                for (int i = 0; i < slotNode.attributes.size; i++) {
                    AttributeDefinition attrCopy = slotNode.attributes.array[i];
                    attrCopy.slotAttributeData = attrData;
                    attributes.Add(attrCopy);
                    if (attrCopy.type == AttributeType.Expose) {
                        exposedAttributes.Add(attrCopy);
                    }
                }
            }

            if (slotNode.injectedAttributes != null) {
                SlotAttributeData attrData = new SlotAttributeData();
                attrData.slotDepth = 0;
                attrData.slotContextType = parentContext.rootType;
                attrData.namespaces = parentContext.namespaces.Clone();
                attrData.contextStack = CloneContextStack();
                attrData.templateMetaData = parentContext.compiledTemplate.templateMetaData;

                for (int i = 0; i < slotNode.injectedAttributes.size; i++) {
                    ref AttributeDefinition injectedAttr = ref slotNode.injectedAttributes.array[i];
                    injectedAttr.slotAttributeData = attrData;
                }
            }

            compiledSlot.overrideDepth = 0;
            compiledSlot.originalAttributes = attributes;
            compiledSlot.injectedAttributes = slotNode.injectedAttributes;
            compiledSlot.rootElementType = parentContext.rootType.rawType;
            compiledSlot.scopedVariables = CloneContextStack();
            compiledSlot.exposedAttributes = exposedAttributes.ToArray();

            exposedAttributes.Release();

            Expression nodeExpr = parentContext.ElementExpr;

            Expression slotNameExpr = Expression.Constant(slotNode.slotName);

            parentContext.Assign(parentContext.ElementExpr, Expression.Call(
                parentContext.applicationExpr,
                s_Application_CreateSlot,
                slotNameExpr,
                parentContext.templateScope,
                Expression.Constant(compiledSlot.slotId),
                parentContext.rootParam,
                parentContext.ParentExpr
            ));

            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression parentParam = Expression.Parameter(typeof(UIElement), "parent");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            CompilationContext ctx = new CompilationContext(parentContext.templateRootNode);

            ParameterExpression slotRootParam = ctx.GetVariable(slotNode.processedType.rawType, "slotRoot");
            ctx.rootType = parentContext.rootType;
            ctx.rootParam = rootParam;
            ctx.templateScope = scopeParam;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.Initialize(slotRootParam);
            ctx.compiledTemplate = parentContext.compiledTemplate;
            ctx.ContextExpr = rootParam;
            ctx.namespaces = parentContext.namespaces;

            // if (compiledSlot.slotType == SlotType.Modify) {
            //     ctx.Return(Expression.Default(slotNode.processedType.rawType));
            //     compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, parentParam, scopeParam);
            // }
            // else {
            Expression createRootExpression = CreateElement(ctx, slotNode.processedType, parentParam, slotNode.ChildCount, CountRealAttributes(slotNode.attributes), parentContext.compiledTemplate.templateId);

            ctx.Assign(slotRootParam, Expression.Convert(createRootExpression, slotNode.processedType.rawType));
            ctx.Assign(Expression.Field(slotRootParam, s_SlotElement_SlotId), Expression.Constant(slotNode.slotName));
            StructList<ContextAliasActions> contextMods = CompileBindings(ctx, slotNode, attributes).contextModifications;

            VisitChildren(ctx, slotNode);

            UndoContextMods(contextMods);

            ctx.Return(slotRootParam);

            compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, parentParam, scopeParam);
            // }

            compiledSlot.requiredChildType = ResolveRequiredType(ctx.namespaces, slotNode.requireType, ctx.rootType.rawType);

            return nodeExpr;
        }

        // compile slot attributes
        // each attribute needs a depth attached to it to reference that context, not just inner/outer like regular templates
        // might need to store referenced namespaces too
        // might need to store context stack per level
        private CompiledSlot CompileSlotOverride(CompilationContext parentContext, SlotNode slotOverrideNode, CompiledSlot toOverride, Type type = null) {
            if (type == null) type = slotOverrideNode.processedType.rawType;

            // if (slotOverrideNode.slotType == SlotType.Forward && toOverride.slotType == SlotType.Modify) {
            //     throw new CompileException("Forwarding modified slots is not yet implemented");
            // }

            CompiledSlot compiledSlot = templateData.CreateSlot(parentContext.compiledTemplate.filePath, parentContext.compiledTemplate.templateName, slotOverrideNode.slotName, slotOverrideNode.slotType);

            compiledSlot.rootElementType = parentContext.rootType.rawType;
            compiledSlot.scopedVariables = CloneContextStack();
            compiledSlot.exposedAttributes = slotOverrideNode.GetAttributes(AttributeType.Expose);
            compiledSlot.overrideDepth = toOverride.overrideDepth + 1;

            parentContext.compiledTemplate.AddSlot(compiledSlot);

            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression parentParam = Expression.Parameter(typeof(UIElement), "parent");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            SlotAttributeData slotAttributeData = new SlotAttributeData();
            slotAttributeData.slotDepth = compiledSlot.overrideDepth;
            slotAttributeData.slotContextType = parentContext.rootType;
            slotAttributeData.namespaces = parentContext.namespaces.Clone();
            slotAttributeData.contextStack = CloneContextStack();
            slotAttributeData.templateMetaData = parentContext.compiledTemplate.templateMetaData;

            StructList<AttributeDefinition> attributes = AttributeMerger.MergeSlotAttributes(toOverride.originalAttributes, slotAttributeData, slotOverrideNode.attributes);

            compiledSlot.originalAttributes = attributes;

            CompilationContext ctx = new CompilationContext(parentContext.templateRootNode);

            ParameterExpression slotRootParam = ctx.GetVariable(type, "slotRoot");
            ctx.rootType = parentContext.rootType;
            ctx.rootParam = rootParam;
            ctx.templateScope = scopeParam;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.compiledTemplate = parentContext.compiledTemplate;
            ctx.ContextExpr = Expression.Field(scopeParam, s_TemplateScope_InnerContext);
            ctx.namespaces = parentContext.namespaces;

            ctx.Initialize(slotRootParam);

            Expression createRootExpression = CreateElement(ctx, TypeProcessor.GetProcessedType(type), parentParam, slotOverrideNode.ChildCount, CountRealAttributes(attributes), parentContext.compiledTemplate.templateId);

            ctx.Assign(slotRootParam, Expression.Convert(createRootExpression, type));
            ctx.Assign(Expression.Field(slotRootParam, s_SlotElement_SlotId), Expression.Constant(slotOverrideNode.slotName));

            LightList<ExposedVariableData> exposedVariableDataList = new LightList<ExposedVariableData>();

            if (toOverride.exposedVariableDataList != null) {
                exposedVariableDataList.AddRange(toOverride.exposedVariableDataList);
            }

            ExposedVariableData exposedVariableData = new ExposedVariableData();
            exposedVariableData.rootType = toOverride.rootElementType;
            exposedVariableData.scopedVariables = toOverride.scopedVariables;
            exposedVariableData.exposedAttrs = toOverride.exposedAttributes ?? new AttributeDefinition[0];

            exposedVariableDataList.Add(exposedVariableData);

            compiledSlot.exposedVariableDataList = exposedVariableDataList;

            SlotNode node = slotOverrideNode;

            if (toOverride.injectedAttributes != null) {
                for (int i = 0; i < node.ChildCount; i++) {
                    node[i].isModified = true;
                    node[i].attributes = AttributeMerger.MergeModifySlotAttributes(node[i].attributes, toOverride.injectedAttributes);
                }
            }

            // if (toOverride.slotType == SlotType.Modify) {
            //     // modify slots have no attributes, they push all attributes onto their children
            //     for (int i = 0; i < node.ChildCount; i++) {
            //         node[i].isModified = true;
            //         node[i].attributes = AttributeMerger.MergeModifySlotAttributes(node[i].attributes, toOverride.originalAttributes);
            //     }
            //
            //     VisitChildren(ctx, node, toOverride.requiredChildType);
            // }
            // else {
            StructList<ContextAliasActions> contextMods = CompileBindings(ctx, node, attributes, exposedVariableDataList).contextModifications;

            if (slotOverrideNode.slotType == SlotType.Override) {
                ctx.Assign(ctx.templateScope, ExpressionFactory.CallInstanceUnchecked(ctx.templateScope, s_TemplateScope_GetOverrideScope));
            }

            VisitChildren(ctx, node, toOverride.requiredChildType);

            UndoContextMods(contextMods);
            // }

            ctx.Return(slotRootParam);

            compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, parentParam, scopeParam);

            return compiledSlot;
        }

        private enum RepeatType {

            Count,
            List,

        }

        private int CompileRepeatTemplate(CompilationContext parentContext, RepeatNode repeatNode, RepeatType repeatType, out int itemVarId, out int indexVarId) {
            CompiledSlot compiledSlot = templateData.CreateSlot(parentContext.compiledTemplate.filePath, parentContext.compiledTemplate.templateName, "__template__", SlotType.Template);

            parentContext.compiledTemplate.AddSlot(compiledSlot);

            ParameterExpression rootParam = Expression.Parameter(typeof(UIElement), "root");
            ParameterExpression parentParam = Expression.Parameter(typeof(UIElement), "parent");
            ParameterExpression scopeParam = Expression.Parameter(typeof(TemplateScope), "scope");

            CompilationContext ctx = new CompilationContext(parentContext.templateRootNode);

            itemVarId = -1;
            indexVarId = NextContextId;
            // todo -- maybe call a static method on type that returns the context definitions
            if (repeatType != RepeatType.Count) {
                itemVarId = NextContextId;
                contextStack.Peek().Push(new ContextVariableDefinition() {
                    name = repeatNode.GetItemVariableName(),
                    id = itemVarId,
                    type = repeatNode.processedType.rawType.GetGenericArguments()[0],
                    variableType = AliasResolverType.RepeatItem
                });
            }

            contextStack.Peek().Push(new ContextVariableDefinition() {
                name = repeatNode.GetIndexVariableName(),
                id = indexVarId,
                type = typeof(int),
                variableType = AliasResolverType.RepeatIndex
            });

            ctx.rootType = parentContext.rootType;
            ctx.rootParam = rootParam;
            ctx.templateScope = scopeParam;
            ctx.applicationExpr = Expression.Field(scopeParam, s_TemplateScope_ApplicationField);
            ctx.compiledTemplate = parentContext.compiledTemplate;
            ctx.ContextExpr = rootParam;
            ctx.namespaces = parentContext.namespaces;

            ctx.Initialize(parentParam);

            ctx.PushScope();

            if (repeatNode.ChildCount != 1) {
                ctx.Assign(ctx.ElementExpr, CreateElement(ctx, TypeProcessor.GetProcessedType(typeof(RepeatMultiChildContainerElement)), ctx.ParentExpr, repeatNode.ChildCount, 0, ctx.compiledTemplate.templateId));
                // need to create a binding node since we implicitly create this node instead of visiting it.
                ctx.AddStatement(ExpressionFactory.CallStaticUnchecked(s_LinqBindingNode_Get,
                        ctx.applicationExpr,
                        ctx.rootParam,
                        ctx.ElementExpr,
                        ctx.ContextExpr,
                        Expression.Constant(-1),
                        Expression.Constant(-1),
                        Expression.Constant(-1),
                        Expression.Constant(-1)
                    )
                );
                VisitChildren(ctx, repeatNode);
                ctx.Return(ctx.ElementExpr);
            }
            else {
                ctx.Return(Visit(ctx, repeatNode.children[0], null));
            }

            contextStack.Peek().Pop();

            if (repeatType != RepeatType.Count) {
                contextStack.Peek().Pop();
            }

            compiledSlot.templateFn = Expression.Lambda(ctx.Finalize(typeof(UIElement)), rootParam, parentParam, scopeParam);

            return compiledSlot.slotId;
        }

        private Expression CompileTextNode(CompilationContext ctx, TextNode textNode) {
            ParameterExpression nodeExpr = ctx.ElementExpr;
            ProcessedType processedType = textNode.processedType;

            ctx.CommentNewLineBefore("new " + TypeNameGenerator.GetTypeName(processedType.rawType) + " " + textNode.lineInfo);

            ctx.Assign(nodeExpr, CreateElement(ctx, textNode));

            // ((UITextElement)element).text = "string value";
            if (textNode.textExpressionList != null && textNode.textExpressionList.size > 0) {
                if (textNode.IsTextConstant()) {
                    ctx.Assign(Expression.MakeMemberAccess(Expression.Convert(nodeExpr, typeof(UITextElement)), s_TextElement_Text), Expression.Constant(textNode.GetStringContent()));
                }
            }

            ProcessAttrsAndVisitChildren(ctx, textNode);

            return nodeExpr;
        }

        private void ProcessAttrsAndVisitChildren(CompilationContext ctx, TemplateNode node, LightList<ExposedVariableData> exposedVariableData = null) {
            StructList<ContextAliasActions> contextMods = CompileBindings(ctx, node, node.attributes, exposedVariableData).contextModifications;

            VisitChildren(ctx, node);

            UndoContextMods(contextMods);
        }

        private void UndoContextMods(StructList<ContextAliasActions> mods) {
            if (mods == null || mods.size == 0) {
                return;
            }

            for (int i = 0; i < mods.size; i++) {
                ContextAliasActions mod = mods.array[i];
                if (mod.modType == ModType.Alias) {
                    ContextVariableDefinition definition = FindContextByName(mod.name);
                    // remove from name list
                    // assert is first
                    definition.nameList.RemoveLast();
                }
                else {
                    contextStack.Peek().Pop();
                }
            }
        }

        private ContextVariableDefinition FindContextByName(string name) {
            if (slotScope == null) {
                LightStack<ContextVariableDefinition> stack = contextStack.Peek();
                for (int j = stack.size - 1; j >= 0; j--) {
                    ContextVariableDefinition definition = stack.array[j];
                    if (definition.GetName() == name) {
                        return definition;
                    }
                }
            }
            else {
                for (int i = 0; i < slotScope.contextStack.Length; i++) {
                    if (slotScope.contextStack[i].GetName() == name) {
                        return slotScope.contextStack[i];
                    }
                }
            }

            return null;
        }

        private Expression CompileContainerNode(CompilationContext ctx, ContainerNode containerNode) {
            ParameterExpression nodeExpr = ctx.ElementExpr;
            ProcessedType processedType = containerNode.processedType;

            ctx.CommentNewLineBefore("new " + TypeNameGenerator.GetTypeName(processedType.rawType));

            ctx.Assign(nodeExpr, CreateElement(ctx, processedType, ctx.ParentExpr, containerNode.ChildCount, CountRealAttributes(containerNode.attributes), ctx.compiledTemplate.templateId));

            ProcessAttrsAndVisitChildren(ctx, containerNode);

            return nodeExpr;
        }

        private static int CountRealAttributes(StructList<AttributeDefinition> attributes) {
            if (attributes == null) return 0;

            int count = 0;

            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Attribute) {
                    count++;
                }
            }

            return count;
        }

        private Expression CompileExpandedNode(CompilationContext ctx, ExpandedTemplateNode expandedTemplateNode) {
            ProcessedType templateType = expandedTemplateNode.processedType;

            TemplateRootNode innerRoot = templateCache.GetParsedTemplate(templateType);

            CompiledTemplate innerTemplate = GetCompiledTemplate(templateType);

            ParameterExpression nodeExpr = ctx.ElementExpr;

            StructList<AttributeDefinition> attributes = AttributeMerger.MergeExpandedAttributes(innerRoot.attributes, expandedTemplateNode.attributes);

            ctx.CommentNewLineBefore("new " + TypeNameGenerator.GetTypeName(templateType.rawType) + " " + expandedTemplateNode.lineInfo);
            ctx.Assign(nodeExpr, CreateElement(ctx, expandedTemplateNode.processedType, ctx.ParentExpr, innerRoot.ChildCount, CountRealAttributes(attributes), ctx.compiledTemplate.templateId));

            bool hasForwardOrOverrides = expandedTemplateNode.slotOverrideNodes != null && expandedTemplateNode.slotOverrideNodes.size > 0;

            ParameterExpression hydrateScope = ctx.GetVariable<TemplateScope>("hydrateScope");

            Expression templateScopeCtor = Expression.New(s_TemplateScope_Ctor, ctx.applicationExpr);

            ctx.Assign(hydrateScope, templateScopeCtor);

            ctx.innerTemplate = innerTemplate;

            BindingOutput result = CompileBindings(ctx, expandedTemplateNode, attributes);

            bool didOverride = false;

            if (hasForwardOrOverrides) {
                for (int i = 0; i < expandedTemplateNode.slotOverrideNodes.size; i++) {
                    SlotNode node = expandedTemplateNode.slotOverrideNodes.array[i];

                    CompiledSlot toOverride = innerTemplate.GetCompiledSlot(node.slotName);

                    if (toOverride == null) {
                        throw new CompileException($"Error compiling {node.TemplateNodeDebugData}: No slot called {node.slotName} was found in template for {innerTemplate.elementType.tagName} to {node.slotType}");
                    }

                    Assert.IsNotNull(toOverride, "toOverride != null");

                    CompiledSlot compiledSlot = CompileSlotOverride(ctx, node, toOverride);

                    switch (node.slotType) {
                        case SlotType.Define:
                            // technically this can't happen, should be part of implicit <override:Children/> where it is legal.
                            break;

                        case SlotType.Forward: {
                            ctx.AddStatement(Expression.Call(
                                hydrateScope,
                                s_TemplateScope_AddSlotForward,
                                ctx.templateScope,
                                Expression.Constant(compiledSlot.slotName), // todo -- alias as needed
                                ctx.rootParam,
                                Expression.Constant(compiledSlot.slotId))
                            );
                            break;
                        }

                        case SlotType.Override: {
                            didOverride = true;
                            ctx.AddStatement(Expression.Call(
                                hydrateScope,
                                s_TemplateScope_AddSlotOverride,
                                Expression.Constant(compiledSlot.slotName), // todo -- alias as needed
                                ctx.rootParam,
                                Expression.Constant(compiledSlot.slotId))
                            );
                            break;
                        }

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            if (didOverride) {
                ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(hydrateScope, s_TemplateScope_SetParentScopeList, ctx.templateScope));
            }

            ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_Application_HydrateTemplate, Expression.Constant(innerTemplate.templateId), nodeExpr, hydrateScope));

            UndoContextMods(result.contextModifications);

            ctx.innerTemplate = null;

            return nodeExpr;
        }

        private void InitializeCompilers(LightList<string> namespaces, Type rootType, Type elementType) {
            updateCompiler.Reset();
            enabledCompiler.Reset();
            createdCompiler.Reset();
            lateCompiler.Reset();

            Parameter p0 = new Parameter(typeof(UIElement), "__root", ParameterFlags.NeverNull);
            Parameter p1 = new Parameter(typeof(UIElement), "__element", ParameterFlags.NeverNull);

            updateCompiler.SetSignature(p0, p1);
            enabledCompiler.SetSignature(p0, p1);
            createdCompiler.SetSignature(p0, p1);
            lateCompiler.SetSignature(p0, p1);

            updateCompiler.Setup(rootType, elementType, namespaces);
            enabledCompiler.Setup(rootType, elementType, namespaces);
            createdCompiler.Setup(rootType, elementType, namespaces);
            lateCompiler.Setup(rootType, elementType, namespaces);
        }

        private static void InitializeAttributes(CompilationContext ctx, StructList<AttributeDefinition> attributes) {
            if (attributes == null) return;

            int attrIdx = 0;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                if (attr.type == AttributeType.Attribute) {
                    // targetElement_x.attributeList.array[x] = new ElementAttribute("key", "value"); will be empty string for attributes that are bound
                    MemberExpression listAccess = Expression.MakeMemberAccess(ctx.ElementExpr, s_ElementAttributeList);
                    MemberExpression arrayAccess = Expression.MakeMemberAccess(listAccess, s_StructList_ElementAttr_Array);
                    IndexExpression arrayIndex = Expression.ArrayAccess(arrayAccess, Expression.Constant(attrIdx++));

                    if ((attr.flags & AttributeFlags.Const) != 0) {
                        ctx.Assign(arrayIndex, Expression.New(s_ElementAttributeCtor, Expression.Constant(attr.key), Expression.Constant(attr.value)));
                    }
                    else {
                        ctx.Assign(arrayIndex, Expression.New(s_ElementAttributeCtor, Expression.Constant(attr.key), Expression.Constant(string.Empty)));
                    }
                }
            }
        }

        private struct StyleRefInfo {

            public int styleId;
            public string styleName;
            public TemplateMetaData templateMetaData;

        }

        private struct ChangeHandlerDefinition {

            public bool wasHandled;
            public AttributeDefinition attributeDefinition;
            public ContextVariableDefinition variableDefinition;

        }

        private static void GatherChangeHandlers(StructList<AttributeDefinition> attributes, ref StructList<ChangeHandlerDefinition> handlers) {
            if (attributes == null) {
                return;
            }

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];
                if (attr.type == AttributeType.ChangeHandler) {
                    handlers = handlers ?? StructList<ChangeHandlerDefinition>.Get();
                    handlers.Add(new ChangeHandlerDefinition() {
                        attributeDefinition = attr
                    });
                }
            }
        }

        // Binding order
        // - conditional
        // - BeforePropertyUpdates() -- if declared
        // - properties & context vars, in declared order 
        // - AfterPropertyUpdates() -- currently called Update()
        // - attributes
        // - styles
        // - AfterBindings()
        // - sync & change

        private struct BindingOutput {

            public bool hasBindingNode;
            public StructList<ContextAliasActions> contextModifications;

        }

        private BindingOutput CompileBindings(CompilationContext ctx, TemplateNode templateNode, StructList<AttributeDefinition> attributes, LightList<ExposedVariableData> exposedVariableData = null) {
            StructList<ContextAliasActions> contextModifications = null;

            StructList<ChangeHandlerDefinition> changeHandlerDefinitions = null;

            BindingOutput retn = default;

            // for template roots (which are not the app root!) we dont want to generate bindings in their own template definition functions
            // instead we let the usage site do that for us. We still need to provide context variables to our template, probably in a dry-run fashion.

            try {
                GatherChangeHandlers(attributes, ref changeHandlerDefinitions);

                InitializeCompilers(ctx.namespaces, ctx.templateRootNode.ElementType, templateNode.processedType.rawType);

                InitializeAttributes(ctx, attributes);

                CompileExposedData(exposedVariableData, ref contextModifications);

                CompileConditionalBindings(templateNode, attributes);

                CompileBeforePropertyUpdates(templateNode.processedType);

                CompileAliases(attributes, ref contextModifications);

                CompilePropertyBindingsAndContextVariables(ctx, templateNode.processedType, attributes, changeHandlerDefinitions, ref contextModifications);

                CompileTextBinding(templateNode);

                CompileRemainingChangeHandlerStores(templateNode.processedType.rawType, changeHandlerDefinitions);

                CompileEnabledThisFrame(templateNode.processedType);

                CompileAfterPropertyUpdates(templateNode.processedType);

                CompileAttributeBindings(attributes);

                CompileInstanceStyleBindings(attributes);

                CompileStyleBindings(ctx, templateNode.tagName, attributes);

                // CompileAfterStyleBindings();

                CompileInputHandlers(templateNode.processedType, attributes);

                CompileCheckChangeHandlers(changeHandlerDefinitions);

                retn.hasBindingNode = BuildBindings(ctx, templateNode, exposedVariableData?.size ?? 0);

                changeHandlerDefinitions?.Release();
            }
            catch (CompileException exception) {
                exception.SetFileName($"{ctx.compiledTemplate.filePath} {templateNode.TemplateNodeDebugData.lineInfo} <{templateNode.TemplateNodeDebugData.tagName}>");
                throw;
            }
            catch (TypeResolutionException typeResolutionException) {
                throw new CompileException($"Error in file {ctx.compiledTemplate.filePath} at line {templateNode.TemplateNodeDebugData.lineInfo} while compiling <{templateNode.TemplateNodeDebugData.tagName}>: " + typeResolutionException.Message);
            }

            retn.contextModifications = contextModifications;
            return retn;
        }

        private void CompileEnabledThisFrame(ProcessedType processed) {
            if (processed.requiresOnEnable) {
                // ParameterExpression element = updateCompiler.GetElement();

            }
        }

        private void CompileCheckChangeHandlers(StructList<ChangeHandlerDefinition> changeHandlers) {
            if (changeHandlers == null) return;

            for (int i = 0; i < changeHandlers.size; i++) {
                CompileChangeHandlerCheck(changeHandlers.array[i]);
            }
        }

        private void CompileRemainingChangeHandlerStores(Type type, StructList<ChangeHandlerDefinition> changeHandlers) {
            if (changeHandlers == null) return;
            for (int i = 0; i < changeHandlers.size; i++) {
                ref ChangeHandlerDefinition handler = ref changeHandlers.array[i];

                if (handler.wasHandled) {
                    continue;
                }

                MemberExpression member = Expression.PropertyOrField(updateCompiler.GetCastElement(), handler.attributeDefinition.key);

                CompileChangeHandlerStore(type, member, ref handler);
            }
        }

        private void CompileChangeHandlerCheck(ChangeHandlerDefinition changeHandler) {
            ContextVariableDefinition variableDefinition = changeHandler.variableDefinition;
            ref AttributeDefinition attr = ref changeHandler.attributeDefinition;

            SetImplicitContext(lateCompiler, changeHandler.attributeDefinition);

            // late update reads from context variable and compares
            Expression access = Expression.MakeMemberAccess(lateCompiler.GetCastElement(), s_UIElement_BindingNode);
            Expression call = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_GetContextVariable, Expression.Constant(variableDefinition.id));
            lateCompiler.Comment(attr.key);
            Expression cast = Expression.Convert(call, variableDefinition.contextVarType);
            Expression target = lateCompiler.AddVariable(variableDefinition.contextVarType, "changeHandler_" + attr.key);
            lateCompiler.Assign(target, cast);
            Expression oldValue = Expression.Field(target, variableDefinition.contextVarType.GetField(nameof(ContextVariable<object>.value)));
            Expression newValue = Expression.PropertyOrField(lateCompiler.GetCastElement(), attr.key);
            string attrValue = attr.value;
            lateCompiler.IfNotEqual(oldValue, newValue, () => {
                // __castRoot.HandleChange();
                ASTNode astNode = ExpressionParser.Parse(attrValue);

                changeHandlerCurrentValue = newValue;
                changeHandlerPreviousValue = oldValue;

                if (astNode.type == ASTNodeType.LambdaExpression) {
                    throw new NotImplementedException("We do not support lambda syntax for onChange handlers yet");
                }

                // assume its a method, probably doesn't have to be once we support assignment
                lateCompiler.Statement(attrValue);

                changeHandlerCurrentValue = null;
                changeHandlerPreviousValue = null;
            });
        }

        private void CompileExposedData(LightList<ExposedVariableData> exposedDataList, ref StructList<ContextAliasActions> contextModifications) {
            if (exposedDataList == null) {
                return;
            }

            for (int x = 0; x < exposedDataList.size; x++) {
                ExposedVariableData exposedData = exposedDataList.array[x];

                if (exposedData.scopedVariables.Length == 0 && exposedData.exposedAttrs.Length == 0) {
                    continue;
                }

                // exposer needs to write & update

                contextModifications = contextModifications ?? StructList<ContextAliasActions>.Get();

                for (int i = 0; i < exposedData.scopedVariables.Length; i++) {
                    contextStack.Peek().Push(exposedData.scopedVariables[i]);
                }

                if (exposedData.exposedAttrs.Length != 0) {
                    // ParameterExpression innerSlotContext_update = updateCompiler.AddVariable(exposedData.rootType, "__innerContext");

                    // updateCompiler.Assign(innerSlotContext_update, Expression.Convert(idx, exposedData.rootType));
                    // updateCompiler.SetImplicitContext(innerSlotContext_update);

                    for (int i = 0; i < exposedData.exposedAttrs.Length; i++) {
                        ref AttributeDefinition attr = ref exposedData.exposedAttrs[i];
                        // bindingNode.CreateContextVariable<string>(id);
                        ContextVariableDefinition variableDefinition = new ContextVariableDefinition();
                        updateCompiler.SetupAttributeData(attr);
                        SetImplicitContext(updateCompiler, attr);
                        Type expressionType = updateCompiler.GetExpressionType(attr.value);

                        variableDefinition.name = attr.key;
                        variableDefinition.id = NextContextId;
                        variableDefinition.type = expressionType;
                        variableDefinition.variableType = AliasResolverType.ContextVariable;

                        MethodCallExpression createVariable = CreateLocalContextVariableExpression(variableDefinition, out Type contextVarType);

                        CompileAssignContextVariable(updateCompiler, attr, contextVarType, variableDefinition.id);

                        contextStack.Peek().Push(variableDefinition);

                        contextModifications.Add(new ContextAliasActions() {
                            modType = ModType.Context,
                            name = variableDefinition.name
                        });

                        createdCompiler.RawExpression(createVariable);
                    }
                }

                for (int i = 0; i < exposedData.scopedVariables.Length; i++) {
                    contextStack.Peek().RemoveWhere(exposedData.scopedVariables[i], (closure, item) => item.id == closure.id);
                }
            }
        }

        private void CompileConditionalBindings(TemplateNode templateNode, StructList<AttributeDefinition> attributes) {
            if (attributes == null) {
                return;
            }

            bool found = false;
            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                if (attr.type != AttributeType.Conditional) {
                    continue;
                }

                if (found) { // todo -- is this bad? with slot merging maybe not
                    throw CompileException.MultipleConditionalBindings(templateNode.TemplateNodeDebugData);
                }

                if ((attr.flags & AttributeFlags.Const) != 0) {
                    CompileConditionalBinding(createdCompiler, attr);
                }
                else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                    CompileConditionalBinding(enabledCompiler, attr);
                }
                else {
                    CompileConditionalBinding(updateCompiler, attr);
                }

                found = true;
            }
        }

        private void CompileBeforePropertyUpdates(ProcessedType processedType) {
            if (processedType.requiresBeforePropertyUpdates) {
                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateCompiler.GetCastElement(), s_UIElement_OnBeforePropertyBindings));
            }
        }

        private void CompileAliases(StructList<AttributeDefinition> attributes, ref StructList<ContextAliasActions> contextModifications) {
            if (attributes == null) return;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                if (attr.type != AttributeType.Alias) {
                    continue;
                }

                contextModifications = contextModifications ?? StructList<ContextAliasActions>.Get();
                contextModifications.Add(new ContextAliasActions() {
                    modType = ModType.Alias,
                    name = attr.key
                });

                ContextVariableDefinition contextVar = FindContextByName(attr.value.Trim());

                if (contextVar == null) {
                    throw CompileException.UnknownAlias(attr.key);
                }

                contextVar.PushAlias(attr.key);
            }
        }

        private void CompileAfterPropertyUpdates(ProcessedType processedType) {
            if (processedType.requiresAfterPropertyUpdates) {
                ParameterExpression element = updateCompiler.GetCastElement();

                updateCompiler.IfEqual(Expression.MakeMemberAccess(element, s_Element_IsEnabledAndNeedsUpdate), Expression.Constant(true), () => { updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateCompiler.GetCastElement(), s_UIElement_OnAfterPropertyBindings)); });
            }

            if (processedType.requiresUpdateFn) {
                ParameterExpression element = updateCompiler.GetCastElement();

                updateCompiler.IfEqual(Expression.MakeMemberAccess(element, s_Element_IsEnabledAndNeedsUpdate), Expression.Constant(true), () => { updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateCompiler.GetCastElement(), s_UIElement_OnUpdate)); });
            }
        }

        private void CompilePropertyBindingsAndContextVariables(CompilationContext ctx, ProcessedType processedType, StructList<AttributeDefinition> attributes, StructList<ChangeHandlerDefinition> changeHandlers,
            ref StructList<ContextAliasActions> contextModifications) {
            if (attributes == null) return;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                switch (attr.type) {
                    case AttributeType.Context:
                        CompileContextVariable(attr, ref contextModifications);
                        break;

                    case AttributeType.Property: {
                        if (ReflectionUtil.IsEvent(processedType.rawType, attr.key, out EventInfo eventInfo)) {
                            CompileEventBinding(createdCompiler, attr, eventInfo);
                            continue;
                        }

                        if ((attr.flags & AttributeFlags.Sync) != 0) {
                            ContextVariableDefinition ctxVar = CompilePropertyBinding(updateCompiler, processedType, attr, changeHandlers);
                            CompilePropertyBindingSync(lateCompiler, attr, ctxVar);
                        }
                        else if ((attr.flags & AttributeFlags.Const) != 0) {
                            CompilePropertyBinding(createdCompiler, processedType, attr, changeHandlers);
                        }
                        else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                            CompilePropertyBinding(enabledCompiler, processedType, attr, changeHandlers);
                        }
                        else {
                            CompilePropertyBinding(updateCompiler, processedType, attr, changeHandlers);
                        }

                        break;
                    }
                }
            }
        }

        private void CompileContextVariable(in AttributeDefinition attr, ref StructList<ContextAliasActions> contextModifications) {

            createdCompiler.SetupAttributeData(attr);
            SetImplicitContext(createdCompiler, attr);

            Type expressionType = createdCompiler.GetExpressionType(attr.value);

            contextModifications = contextModifications ?? StructList<ContextAliasActions>.Get();

            contextModifications.Add(new ContextAliasActions() {
                modType = ModType.Context,
                name = attr.key
            });

            LightStack<ContextVariableDefinition> ctxStack = contextStack.Peek();

            ContextVariableDefinition variableDefinition = new ContextVariableDefinition();

            variableDefinition.name = attr.key;
            variableDefinition.id = NextContextId;
            variableDefinition.type = expressionType;
            variableDefinition.variableType = AliasResolverType.ContextVariable;

            ctxStack.Push(variableDefinition);

            Type type = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), expressionType);
            ReflectionUtil.TypeArray3[0] = typeof(int);
            ReflectionUtil.TypeArray3[1] = typeof(string);
            ReflectionUtil.TypeArray3[2] = expressionType;
            ConstructorInfo ctor = type.GetConstructor(ReflectionUtil.TypeArray3);

            Expression contextVariable = Expression.New(ctor, Expression.Constant(variableDefinition.id), Expression.Constant(attr.key), Expression.Default(expressionType));
            Expression access = Expression.MakeMemberAccess(createdCompiler.GetCastElement(), s_UIElement_BindingNode);
            Expression createVariable = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_CreateLocalContextVariable, contextVariable);

            createdCompiler.RawExpression(createVariable);

            if ((attr.flags & AttributeFlags.Const) != 0) {
                SetImplicitContext(createdCompiler, attr);
                CompileAssignContextVariable(createdCompiler, attr, type, variableDefinition.id);
            }
            else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                SetImplicitContext(enabledCompiler, attr);
                CompileAssignContextVariable(enabledCompiler, attr, type, variableDefinition.id);
            }
            else {
                SetImplicitContext(updateCompiler, attr);
                CompileAssignContextVariable(updateCompiler, attr, type, variableDefinition.id);
            }
        }

        private void CompileAttributeBindings(StructList<AttributeDefinition> attributes) {
            if (attributes == null) return;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];

                if (attr.type != AttributeType.Attribute) {
                    continue;
                }

                if ((attr.flags & AttributeFlags.Const) != 0) {
                    /* no op */
                }
                else if ((attr.flags & AttributeFlags.EnableOnly) != 0) {
                    CompileAttributeBinding(enabledCompiler, attr);
                }
                else {
                    CompileAttributeBinding(updateCompiler, attr);
                }
            }
        }

        private void CompileInstanceStyleBindings(StructList<AttributeDefinition> attributes) {
            if (attributes == null) return;

            for (int i = 0; i < attributes.size; i++) {
                ref AttributeDefinition attr = ref attributes.array[i];
                if (attr.type == AttributeType.InstanceStyle) {
                    CompileInstanceStyleBinding(updateCompiler, attr);
                }
            }
        }

        private struct StyleExpression {

            public TemplateMetaData templateMetaData;
            public StructList<TextExpression> expressions;
            public AttributeDefinition attribute;
            public bool fromInnerContext;

        }

        private void CompileStyleBindings(CompilationContext ctx, string tagName, StructList<AttributeDefinition> attributes) {
            LightList<StyleRefInfo> styleIds = LightList<StyleRefInfo>.Get();

            StyleSheetReference[] styleRefs = ctx.innerTemplate?.templateMetaData.styleReferences;

            if (styleRefs != null) {
                for (int i = 0; i < styleRefs.Length; i++) {
                    if (styleRefs[i].styleSheet.TryResolveStyleByTagName(tagName, out int id)) {
                        id = ctx.innerTemplate.templateMetaData.ResolveStyleByIdSlow(id);
                        styleIds.Add(new StyleRefInfo() {styleId = id, styleName = "implicit:<" + tagName + ">", templateMetaData = ctx.innerTemplate.templateMetaData});
                    }
                }
            }

            styleRefs = ctx.compiledTemplate.templateMetaData.styleReferences;

            if (styleRefs != null) {
                for (int i = 0; i < styleRefs.Length; i++) {
                    if (styleRefs[i].styleSheet.TryResolveStyleByTagName(tagName, out int id)) {
                        id = ctx.compiledTemplate.templateMetaData.ResolveStyleByIdSlow(id);
                        styleIds.Add(new StyleRefInfo() {styleId = id, styleName = "implicit:<" + tagName + ">", templateMetaData = ctx.compiledTemplate.templateMetaData});
                    }
                }
            }

            StructList<TextExpression> list = StructList<TextExpression>.Get();
            StructList<StyleExpression> styleExpressions = StructList<StyleExpression>.Get();

            if (attributes != null) {
                for (int i = 0; i < attributes.size; i++) {
                    ref AttributeDefinition attr = ref attributes.array[i];

                    if (attr.type != AttributeType.Style) {
                        continue;
                    }

                    StyleExpression styleExpression = default;
                    styleExpression.attribute = attr;

                    if (attr.slotAttributeData != null) {
                        styleExpression.templateMetaData = attr.slotAttributeData.templateMetaData;
                    }

                    else if ((attr.flags & AttributeFlags.InnerContext) != 0) {
                        styleExpression.templateMetaData = ctx.innerTemplate.templateMetaData;
                        styleExpression.fromInnerContext = true;
                    }
                    else {
                        styleExpression.templateMetaData = ctx.compiledTemplate.templateMetaData;
                    }

                    styleExpression.expressions = StructList<TextExpression>.Get();

                    TextTemplateProcessor.ProcessTextExpressions(attr.value, styleExpression.expressions);

                    list.AddRange(styleExpression.expressions);
                    styleExpressions.Add(styleExpression);
                }
            }

            if (TextTemplateProcessor.TextExpressionIsConstant(list)) {
                CompileStaticSharedStyles(ctx, styleExpressions, styleIds);
            }
            else {
                CompileDynamicSharedStyles(ctx, styleExpressions, styleIds);
            }

            styleExpressions.Release();
            list.Release();
        }

        private void CompileStaticSharedStyles(CompilationContext ctx, StructList<StyleExpression> styleExpressionGroups, LightList<StyleRefInfo> styleIds) {
            for (int i = 0; i < styleExpressionGroups.size; i++) {
                StyleExpression styleExpression = styleExpressionGroups.array[i];

                StructList<TextExpression> expressionList = styleExpression.expressions;

                for (int k = 0; k < expressionList.size; k++) {
                    TextExpression textExpression = expressionList.array[k];

                    string text = textExpression.text;
                    string[] splitStyles = text.Split(s_StyleSeparator);

                    for (int s = 0; s < splitStyles.Length; s++) {
                        string styleName = splitStyles[s];

                        int styleId = styleExpression.templateMetaData.ResolveStyleNameSlow(styleName);

                        if (styleId >= 0) {
                            styleIds.Add(new StyleRefInfo() {styleId = styleId, styleName = textExpression.text, templateMetaData = styleExpression.templateMetaData});
                        }
                    }
                }
            }

            if (styleIds.Count > 0) {
                Expression styleSet = Expression.Field(ctx.ElementExpr, s_UIElement_StyleSet);

                for (int i = 0; i < styleIds.size; i++) {
                    ref StyleRefInfo styleRefInfo = ref styleIds.array[i];

                    MethodInfo method = typeof(Application).GetMethod(nameof(Application.GetTemplateMetaData));
                    int metaDataId = styleRefInfo.templateMetaData.id;

                    MethodCallExpression call = ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, method, Expression.Constant(metaDataId));
                    MethodCallExpression expr = ExpressionFactory.CallInstanceUnchecked(call, s_TemplateMetaData_GetStyleById, Expression.Constant(styleRefInfo.styleId));

                    ctx.AddStatement(ExpressionFactory.CallInstanceUnchecked(styleSet, s_StyleSet_AddBaseStyle, expr));
                    ctx.InlineComment(styleRefInfo.styleName + " -> from template " + styleRefInfo.templateMetaData.filePath);
                }

                MemberExpression style = Expression.Field(ctx.ElementExpr, s_UIElement_StyleSet);
                MethodCallExpression initStyle = ExpressionFactory.CallInstanceUnchecked(style, s_StyleSet_InternalInitialize);
                ctx.AddStatement(initStyle);
            }

            styleIds.Release();
        }

        private void CompileDynamicSharedStyles(CompilationContext ctx, StructList<StyleExpression> styleExpressionGroups, LightList<StyleRefInfo> styleIds) {
            updateCompiler.SetNullCheckingEnabled(false);

            ParameterExpression styleList = ctx.GetVariable<LightList<UIStyleGroupContainer>>("styleList");
            ctx.Assign(styleList, ExpressionFactory.CallStaticUnchecked(s_LightList_UIStyleGroupContainer_Get));

            Parameter updateStyleParam = new Parameter<LightList<UIStyleGroupContainer>>("styleList");
            ParameterExpression updateStyleList = updateCompiler.AddVariable(updateStyleParam, ExpressionFactory.CallStaticUnchecked(s_LightList_UIStyleGroupContainer_Get));

            // this makes sure we always use implicit styles
            for (int i = 0; i < styleIds.size; i++) {
                ref StyleRefInfo styleRefInfo = ref styleIds.array[i];

                Expression target = ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_Application_GetTemplateMetaData, Expression.Constant(styleRefInfo.templateMetaData.id));
                MethodCallExpression expr = ExpressionFactory.CallInstanceUnchecked(target, s_TemplateMetaData_GetStyleById, Expression.Constant(styleRefInfo.styleId));
                MethodCallExpression addCall = ExpressionFactory.CallInstanceUnchecked(styleList, s_LightList_UIStyleGroupContainer_Add, expr);

                ctx.AddStatement(addCall);
                ctx.InlineComment(styleRefInfo.styleName + " -> from template " + styleRefInfo.templateMetaData.filePath);
            }

            updateCompiler.SetNullCheckingEnabled(false);

            for (int i = 0; i < styleExpressionGroups.size; i++) {
                StyleExpression styleExpression = styleExpressionGroups.array[i];

                StructList<TextExpression> expressionList = styleExpression.expressions;

                updateCompiler.SetupAttributeData(styleExpression.attribute);
                updateCompiler.SetImplicitContext(styleExpression.fromInnerContext ? updateCompiler.GetCastElement() : updateCompiler.GetCastRoot());

                ParameterExpression templateMetaDataExpr = updateCompiler.AddVariable(typeof(TemplateMetaData[]), "metaData");
                MemberExpression application = Expression.Property(updateCompiler.GetElement(), s_UIElement_Application);
                updateCompiler.Assign(templateMetaDataExpr, Expression.Property(application, s_Application_GetTemplateMetaDataArray));

                Expression templateContext = Expression.ArrayIndex(templateMetaDataExpr, Expression.Constant(styleExpression.templateMetaData.id));
                for (int k = 0; k < expressionList.size; k++) {
                    TextExpression textExpression = expressionList.array[k];

                    if (textExpression.isExpression) {
                        Expression dynamicStyleList = updateCompiler.TypeWrapStatement(s_DynamicStyleListTypeWrapper, typeof(DynamicStyleList), textExpression.text);

                        updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(dynamicStyleList, s_DynamicStyleList_Flatten, templateContext, updateStyleList));
                    }

                    else {
                        string text = textExpression.text;
                        string[] splitStyles = text.Split(s_StyleSeparator);

                        for (int s = 0; s < splitStyles.Length; s++) {
                            string styleName = splitStyles[s];

                            int styleId = styleExpression.templateMetaData.ResolveStyleNameSlow(styleName);

                            if (styleId >= 0) {
                                MethodCallExpression expr = ExpressionFactory.CallInstanceUnchecked(templateContext, s_TemplateMetaData_GetStyleById, Expression.Constant(styleId));
                                MethodCallExpression addCall = ExpressionFactory.CallInstanceUnchecked(updateStyleList, s_LightList_UIStyleGroupContainer_Add, expr);
                                updateCompiler.RawExpression(addCall);
                                ctx.InlineComment(styleName + " -> from template " + styleExpression.templateMetaData.filePath);
                            }
                        }
                    }
                }
            }

            MemberExpression styleSet = Expression.Field(updateCompiler.GetElement(), s_UIElement_StyleSet);
            MethodCallExpression setBaseStyles = ExpressionFactory.CallInstanceUnchecked(styleSet, s_StyleSet_SetBaseStyles, updateStyleList);

            updateCompiler.RawExpression(setBaseStyles);
            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(updateStyleList, s_LightList_UIStyleGroupContainer_Release));

            MemberExpression style = Expression.Field(ctx.ElementExpr, s_UIElement_StyleSet);
            MethodCallExpression initStyle = ExpressionFactory.CallInstanceUnchecked(style, s_StyleSet_InternalInitialize);
            ctx.AddStatement(initStyle);
            styleIds.Release();
            updateCompiler.SetNullCheckingEnabled(true);
        }

        private void CompileMouseHandlerFromAttribute(in InputHandler handler) {
            if (!handler.methodInfo.IsPublic) {
                throw new CompileException($"{handler.methodInfo.DeclaringType}.{handler.methodInfo} must be marked as public in order to be referenced in a template expression");
            }

            LightList<Parameter> parameters = LightList<Parameter>.Get();

            parameters.Add(new Parameter<MouseInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
            LinqCompiler closure = createdCompiler.CreateClosure(parameters, typeof(void));

            currentEvent = parameters[0];

            if (handler.useEventParameter) {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo, currentEvent));
            }
            else {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo));
            }

            LambdaExpression lambda = closure.BuildLambda();

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddMouseEvent,
                Expression.Constant(handler.descriptor.handlerType),
                Expression.Constant(handler.descriptor.modifiers),
                Expression.Constant(handler.descriptor.requiresFocus),
                Expression.Constant(handler.descriptor.eventPhase),
                lambda
            );

            createdCompiler.RawExpression(expression);
            closure.Release();
            parameters.Release();
        }

        private void CompileKeyboardHandlerFromAttribute(in InputHandler handler) {
            if (!handler.methodInfo.IsPublic) {
                throw new CompileException($"{handler.methodInfo.DeclaringType}.{handler.methodInfo} must be marked as public in order to be referenced in a template expression");
            }

            LightList<Parameter> parameters = LightList<Parameter>.Get();

            parameters.Add(new Parameter<KeyboardInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
            LinqCompiler closure = createdCompiler.CreateClosure(parameters, typeof(void));

            if (handler.useEventParameter) {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo, parameters[0]));
            }
            else {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo));
            }

            LambdaExpression lambda = closure.BuildLambda();

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddKeyboardEvent,
                Expression.Constant(handler.descriptor.handlerType),
                Expression.Constant(handler.descriptor.modifiers),
                Expression.Constant(handler.descriptor.requiresFocus),
                Expression.Constant(handler.descriptor.eventPhase),
                Expression.Constant(handler.keyCode),
                Expression.Constant(handler.character),
                lambda
            );

            createdCompiler.RawExpression(expression);
            closure.Release();
            parameters.Release();
        }

        private void CompileDragHandlerFromAttribute(in InputHandler handler) {
            if (!handler.methodInfo.IsPublic) {
                throw new CompileException($"{handler.methodInfo.DeclaringType}.{handler.methodInfo} must be marked as public in order to be referenced in a template expression");
            }

            LightList<Parameter> parameters = LightList<Parameter>.Get();

            parameters.Add(new Parameter<DragEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
            LinqCompiler closure = createdCompiler.CreateClosure(parameters, typeof(void));

            if (handler.useEventParameter) {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo, parameters[0].expression));
            }
            else {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo));
            }

            LambdaExpression lambda = closure.BuildLambda();

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddDragEvent,
                Expression.Constant(handler.descriptor.handlerType),
                Expression.Constant(handler.descriptor.modifiers),
                Expression.Constant(handler.descriptor.requiresFocus),
                Expression.Constant(handler.descriptor.eventPhase),
                lambda
            );

            createdCompiler.RawExpression(expression);
            closure.Release();
            parameters.Release();
        }

        private void CompileDragCreateFromAttribute(in InputHandler handler) {
            if (!handler.methodInfo.IsPublic) {
                throw new CompileException($"{handler.methodInfo.DeclaringType}.{handler.methodInfo} must be marked as public in order to be referenced in a template expression");
            }

            LightList<Parameter> parameters = LightList<Parameter>.Get();

            parameters.Add(new Parameter<MouseInputEvent>(k_InputEventParameterName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));

            LinqCompiler closure = createdCompiler.CreateClosure(parameters, typeof(DragEvent));

            if (handler.useEventParameter) {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo, parameters[0].expression));
            }
            else {
                closure.RawExpression(ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetCastElement(), handler.methodInfo));
            }

            LambdaExpression lambda = closure.BuildLambda();

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddDragCreator,
                Expression.Constant(handler.descriptor.modifiers),
                Expression.Constant(handler.descriptor.requiresFocus),
                Expression.Constant(handler.descriptor.eventPhase),
                lambda
            );

            createdCompiler.RawExpression(expression);
            closure.Release();
            parameters.Release();
        }

        private void CompileInputHandlers(ProcessedType processedType, StructList<AttributeDefinition> attributes) {
            StructList<InputHandler> handlers = InputCompiler.CompileInputAnnotations(processedType.rawType);

            const InputEventType k_KeyboardType = InputEventType.KeyDown | InputEventType.KeyUp | InputEventType.KeyHeldDown;
            const InputEventType k_DragType = InputEventType.DragCancel | InputEventType.DragDrop | InputEventType.DragEnter | InputEventType.DragEnter | InputEventType.DragExit | InputEventType.DragHover | InputEventType.DragMove;

            bool hasHandlers = false;

            if (handlers != null) {
                hasHandlers = true;

                for (int i = 0; i < handlers.size; i++) {
                    ref InputHandler handler = ref handlers.array[i];

                    if (handler.descriptor.handlerType == InputEventType.DragCreate) {
                        CompileDragCreateFromAttribute(handler);
                    }
                    else if ((handler.descriptor.handlerType & k_DragType) != 0) {
                        CompileDragHandlerFromAttribute(handler);
                    }
                    else if ((handler.descriptor.handlerType & k_KeyboardType) != 0) {
                        CompileKeyboardHandlerFromAttribute(handler);
                    }
                    else {
                        CompileMouseHandlerFromAttribute(handler);
                    }
                }
            }

            const AttributeType k_InputType = AttributeType.Controller | AttributeType.Mouse | AttributeType.Key | AttributeType.Touch | AttributeType.Drag;

            if (attributes != null) {
                for (int i = 0; i < attributes.size; i++) {
                    ref AttributeDefinition attr = ref attributes.array[i];
                    if ((attr.type & k_InputType) == 0) {
                        continue;
                    }

                    hasHandlers = true;
                    createdCompiler.SetupAttributeData(attr);
                    switch (attr.type) {
                        case AttributeType.Mouse:
                            CompileMouseInputBinding(createdCompiler, attr);
                            break;

                        case AttributeType.Key:
                            CompileKeyboardInputBinding(createdCompiler, attr);
                            break;

                        case AttributeType.Drag:
                            CompileDragBinding(createdCompiler, attr);
                            break;
                    }
                }
            }

            if (!hasHandlers) {
                return;
            }

            // Application.InputSystem.RegisterKeyboardHandler(element);
            ParameterExpression elementVar = createdCompiler.GetElement();
            MemberExpression app = Expression.Property(elementVar, typeof(UIElement).GetProperty(nameof(UIElement.application)));
            MemberExpression inputSystem = Expression.Property(app, typeof(Application).GetProperty(nameof(Application.InputSystem)));
            MethodInfo method = typeof(InputSystem).GetMethod(nameof(InputSystem.RegisterKeyboardHandler));
            createdCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(inputSystem, method, elementVar));
        }

        private bool BuildBindings(CompilationContext ctx, TemplateNode templateNode, int slotContextSize) {
            int createdBindingId = -1;
            int enabledBindingId = -1;
            int updateBindingId = -1;
            int lateBindingId = -1;

            // we always have 4 statements because of Initialize(), so only consider compilers with more than 4 statements

            if (createdCompiler.StatementCount > 0) {
                CompiledBinding createdBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnCreate);
                createdBinding.bindingFn = createdCompiler.BuildLambda();
                createdBindingId = createdBinding.bindingId;
                ctx.compiledTemplate.AddBinding(createdBinding);
            }

            if (enabledCompiler.StatementCount > 0) {
                CompiledBinding enabledBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnEnable);
                enabledBinding.bindingFn = enabledCompiler.BuildLambda();
                enabledBindingId = enabledBinding.bindingId;
                ctx.compiledTemplate.AddBinding(enabledBinding);
            }

            if (updateCompiler.StatementCount > 0) {
                CompiledBinding updateBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnUpdate);
                updateBinding.bindingFn = updateCompiler.BuildLambda();
                updateBindingId = updateBinding.bindingId;
                ctx.compiledTemplate.AddBinding(updateBinding);
            }

            if (lateCompiler.StatementCount > 0) {
                CompiledBinding lateBinding = templateData.AddBinding(templateNode, CompiledBindingType.OnLateUpdate);
                lateBinding.bindingFn = lateCompiler.BuildLambda();
                lateBindingId = lateBinding.bindingId;
                ctx.compiledTemplate.AddBinding(lateBinding);
            }

            if (templateNode is SlotNode slotNode) {
                ctx.AddStatement(Expression.Call(null, s_LinqBindingNode_GetSlotNode,
                        ctx.applicationExpr,
                        ctx.rootParam,
                        ctx.ElementExpr,
                        ctx.ContextExpr,
                        Expression.Constant(createdBindingId),
                        Expression.Constant(enabledBindingId),
                        Expression.Constant(updateBindingId),
                        Expression.Constant(lateBindingId),
                        Expression.Constant(slotNode.slotName),
                        ctx.templateScope,
                        Expression.Constant(slotContextSize)
                    )
                );
            }
            else if (templateNode.isModified) {
                ctx.AddStatement(Expression.Call(null, s_LinqBindingNode_GetSlotModifyNode,
                        ctx.applicationExpr,
                        ctx.rootParam,
                        ctx.ElementExpr,
                        ctx.ContextExpr,
                        Expression.Constant(createdBindingId),
                        Expression.Constant(enabledBindingId),
                        Expression.Constant(updateBindingId),
                        Expression.Constant(lateBindingId)
                    )
                );
            }
            else {
                ctx.AddStatement(ExpressionFactory.CallStaticUnchecked(s_LinqBindingNode_Get,
                        ctx.applicationExpr,
                        ctx.rootParam,
                        ctx.ElementExpr,
                        ctx.ContextExpr,
                        Expression.Constant(createdBindingId),
                        Expression.Constant(enabledBindingId),
                        Expression.Constant(updateBindingId),
                        Expression.Constant(lateBindingId)
                    )
                );
            }

            return true;
        }

        private MethodCallExpression CreateLocalContextVariableExpression(ContextVariableDefinition definition, out Type contextVarType) {
            Type type = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), definition.type);
            ReflectionUtil.TypeArray3[0] = typeof(int);
            ReflectionUtil.TypeArray3[1] = typeof(string);
            ReflectionUtil.TypeArray3[2] = definition.type;
            ConstructorInfo ctor = type.GetConstructor(ReflectionUtil.TypeArray3);

            Expression contextVariable = Expression.New(ctor, Expression.Constant(definition.id), Expression.Constant(definition.name), Expression.Default(definition.type));
            Expression access = Expression.MakeMemberAccess(createdCompiler.GetElement(), s_UIElement_BindingNode);
            contextVarType = type;
            definition.contextVarType = type;
            return ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_CreateLocalContextVariable, contextVariable);
        }

        private void CompileTextBinding(TemplateNode templateNode) {
            if (!(templateNode is TextNode textNode)) {
                return;
            }

            updateCompiler.TeardownAttributeData();

            if (textNode.textExpressionList != null && textNode.textExpressionList.size > 0 && !textNode.IsTextConstant()) {
                updateCompiler.AddNamespace("UIForia.Util");
                updateCompiler.AddNamespace("UIForia.Text");
                StructList<TextExpression> expressionParts = textNode.textExpressionList;

                MemberExpression textValueExpr = Expression.Field(updateCompiler.GetCastElement(), s_TextElement_Text);
                updateCompiler.RawExpression(s_StringBuilderClear);

                for (int i = 0; i < expressionParts.size; i++) {
                    if (expressionParts[i].isExpression) {
                        updateCompiler.SetImplicitContext(updateCompiler.GetCastRoot());

                        updateCompiler.BeginIsolatedSection();
                        Expression val = updateCompiler.Value(expressionParts[i].text);
                        if (val.Type.IsEnum) {
                            MethodCallExpression toString = ExpressionFactory.CallInstanceUnchecked(val, val.Type.GetMethod("ToString", Type.EmptyTypes));
                            updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendString, toString));
                            continue;
                        }

                        switch (Type.GetTypeCode(val.Type)) {
                            case TypeCode.Boolean:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendBool, val));
                                break;

                            case TypeCode.Byte:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendByte, val));
                                break;

                            case TypeCode.Char:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendChar, val));
                                break;

                            case TypeCode.Decimal:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendDecimal, val));
                                break;

                            case TypeCode.Double:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendDouble, val));
                                break;

                            case TypeCode.Int16:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendInt16, val));
                                break;

                            case TypeCode.Int32:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendInt32, val));
                                break;

                            case TypeCode.Int64:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendInt64, val));
                                break;

                            case TypeCode.SByte:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendSByte, val));
                                break;

                            case TypeCode.Single:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendFloat, val));
                                break;

                            case TypeCode.String:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendString, val));
                                break;

                            case TypeCode.UInt16:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendUInt16, val));
                                break;

                            case TypeCode.UInt32:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendUInt32, val));
                                break;

                            case TypeCode.UInt64:
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendUInt64, val));
                                break;

                            default:
                                MethodCallExpression toString = ExpressionFactory.CallInstanceUnchecked(val, val.Type.GetMethod("ToString", Type.EmptyTypes));
                                updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendString, toString));
                                break;
                        }
                        
                        updateCompiler.EndIsolatedSection();
                    }
                    else {
                        updateCompiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, s_StringBuilder_AppendString, Expression.Constant(expressionParts[i].text)));
                    }
                }

                // todo -- this needs to check the TextInfo for equality or whitespace mutations will be ignored and we will return false from equal!!!
                Expression e = updateCompiler.GetCastElement();
                Expression condition = ExpressionFactory.CallInstanceUnchecked(s_StringBuilderExpr, typeof(CharStringBuilder).GetMethod(nameof(CharStringBuilder.EqualsString), new[] {typeof(string)}), textValueExpr);
                condition = Expression.Equal(condition, Expression.Constant(false));
                ConditionalExpression ifCheck = Expression.IfThen(condition, Expression.Block(ExpressionFactory.CallInstanceUnchecked(e, s_TextElement_SetText, s_StringBuilderToString)));

                updateCompiler.RawExpression(ifCheck);
                updateCompiler.RawExpression(s_StringBuilderClear);
            }
        }

        public static void CompileAssignContextVariable(UIForiaLinqCompiler compiler, in AttributeDefinition attr, Type contextVarType, int varId, string varPrefix = null, Expression value = null) {
            //ContextVariable<T> ctxVar = (ContextVariable<T>)__castElement.bindingNode.GetContextVariable(id);
            //ctxVar.value = expression;
            Expression access = Expression.MakeMemberAccess(compiler.GetElement(), s_UIElement_BindingNode);
            Expression call = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_GetContextVariable, Expression.Constant(varId));
            compiler.Comment(attr.key);
            Expression cast = Expression.Convert(call, contextVarType);
            ParameterExpression target = compiler.AddVariable(contextVarType, (varPrefix ?? "ctxVar_") + attr.key);
            compiler.Assign(target, cast);
            MemberExpression valueField = Expression.Field(target, contextVarType.GetField(nameof(ContextVariable<object>.value)));
            compiler.Assign(valueField, value ?? compiler.Value(attr.value));
        }

        private void CompileKeyboardInputBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            LambdaExpression lambda = BuildInputTemplateBinding<KeyboardInputEvent>(createdCompiler, attr);

            InputHandlerDescriptor descriptor = InputCompiler.ParseKeyboardDescriptor(attr.key);

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(compiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddKeyboardEvent,
                Expression.Constant(descriptor.handlerType),
                Expression.Constant(descriptor.modifiers),
                Expression.Constant(descriptor.requiresFocus),
                Expression.Constant(descriptor.eventPhase),
                Expression.Constant(KeyCodeUtil.AnyKey),
                Expression.Constant('\0'),
                lambda
            );

            compiler.RawExpression(expression);
        }

        private void CompileDragBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            InputHandlerDescriptor descriptor = InputCompiler.ParseDragDescriptor(attr.key);
            if (descriptor.handlerType == InputEventType.DragCreate) {
                CompileDragCreateBinding(attr, descriptor);
            }
            else {
                CompileDragEventBinding(attr, descriptor);
            }
        }

        private void CompileDragEventBinding(in AttributeDefinition attr, in InputHandlerDescriptor descriptor) {
            LambdaExpression lambda = BuildInputTemplateBinding<DragEvent>(createdCompiler, attr);

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddDragEvent,
                Expression.Constant(descriptor.handlerType),
                Expression.Constant(descriptor.modifiers),
                Expression.Constant(descriptor.requiresFocus),
                Expression.Constant(descriptor.eventPhase),
                lambda
            );

            createdCompiler.RawExpression(expression);
        }

        private void SetupForAttribute(in AttributeDefinition attr) {
            slotScope = attr.slotAttributeData;
        }

        private void TeardownAttributeData(in AttributeDefinition attr) {
            slotScope = null;
        }

        private LambdaExpression BuildInputTemplateBinding<T>(UIForiaLinqCompiler compiler, in AttributeDefinition attr, Type returnType = null) {
            SetupForAttribute(attr);
            compiler.SetupAttributeData(attr);
            SetImplicitContext(compiler, attr);

            ASTNode astNode = ExpressionParser.Parse(attr.value);
            string eventName = k_InputEventParameterName;

            if (astNode.type == ASTNodeType.LambdaExpression) {
                LambdaExpressionNode n = (LambdaExpressionNode) astNode;
                if (n.signature.size == 1) {
                    LambdaArgument signature = n.signature.array[0];
                    eventName = signature.identifier;
                    if (signature.type != null) {
                        Debug.LogWarning("Input handler lambda should not define a type");
                    }
                }
                else if (n.signature.size > 1) {
                    throw CompileException.InvalidInputHandlerLambda(attr, n.signature.size);
                }

                astNode = n.body;
            }

            LinqCompiler closure = compiler.CreateClosure(new Parameter<T>(eventName, ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds), returnType ?? typeof(void));

            currentEvent = closure.GetParameterAtIndex(0);

            try {
                if (returnType == null) {
                    closure.Statement(astNode);
                }
                else {
                    closure.Return(astNode);
                }
            }
            catch (CompileException exception) {
                exception.SetExpression(attr.rawValue + " at " + attr.line + ": " + attr.column);
                throw;
            }

            currentEvent = null;

            LambdaExpression lambda = closure.BuildLambda();
            closure.Release();

            TeardownAttributeData(attr);

            return lambda;
        }

        private void CompileMouseInputBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            // todo -- eliminate generated closure by passing in template root and element from input system and doing casting as normal in the callback

            LambdaExpression lambda = BuildInputTemplateBinding<MouseInputEvent>(compiler, attr);

            InputHandlerDescriptor descriptor = InputCompiler.ParseMouseDescriptor(attr.key);

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(compiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddMouseEvent,
                Expression.Constant(descriptor.handlerType),
                Expression.Constant(descriptor.modifiers),
                Expression.Constant(descriptor.requiresFocus),
                Expression.Constant(descriptor.eventPhase),
                lambda
            );

            compiler.RawExpression(expression);
        }

        private void CompileDragCreateBinding(in AttributeDefinition attr, in InputHandlerDescriptor descriptor) {
            LambdaExpression lambda = BuildInputTemplateBinding<MouseInputEvent>(createdCompiler, attr, typeof(DragEvent));

            MethodCallExpression expression = ExpressionFactory.CallInstanceUnchecked(createdCompiler.GetInputHandlerGroup(), s_InputHandlerGroup_AddDragCreator,
                Expression.Constant(descriptor.modifiers),
                Expression.Constant(descriptor.requiresFocus),
                Expression.Constant(descriptor.eventPhase),
                lambda
            );

            createdCompiler.RawExpression(expression);
        }

        private static void CompileEventBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr, EventInfo eventInfo) {
            bool hasReturnType = ReflectionUtil.IsFunc(eventInfo.EventHandlerType);
            Type[] eventHandlerTypes = eventInfo.EventHandlerType.GetGenericArguments();
            Type returnType = hasReturnType ? eventHandlerTypes[eventHandlerTypes.Length - 1] : null;

            int parameterCount = eventHandlerTypes.Length;
            if (hasReturnType) {
                parameterCount--;
            }

            compiler.SetupAttributeData(attr);

            SetImplicitContext(compiler, attr);
            LightList<Parameter> parameters = LightList<Parameter>.Get();

            IEnumerable<AliasGenericParameterAttribute> attrNameAliases = eventInfo.GetCustomAttributes<AliasGenericParameterAttribute>();
            for (int i = 0; i < parameterCount; i++) {
                string argName = "arg" + i;
                foreach (AliasGenericParameterAttribute a in attrNameAliases) {
                    if (a.parameterIndex == i) {
                        argName = a.aliasName;
                        break;
                    }
                }

                parameters.Add(new Parameter(eventHandlerTypes[i], argName));
            }

            ASTNode astNode = ExpressionParser.Parse(attr.value);

            if (astNode.type == ASTNodeType.Identifier) {
                IdentifierNode idNode = (IdentifierNode) astNode;

                if (ReflectionUtil.IsField(compiler.rootElementType, idNode.name, out FieldInfo fieldInfo)) {
                    if (eventInfo.EventHandlerType.IsAssignableFrom(fieldInfo.FieldType)) {
                        LinqCompiler closure = compiler.CreateClosure(parameters, returnType);
                        string statement = fieldInfo.Name + "(";

                        for (int i = 0; i < parameters.size; i++) {
                            statement += parameters.array[i].name;
                            if (i != parameters.size - 1) {
                                statement += ", ";
                            }
                        }

                        statement += ")";
                        closure.Statement(statement);
                        LambdaExpression lambda = closure.BuildLambda();
                        ParameterExpression evtFn = compiler.AddVariable(lambda.Type, "evtFn");
                        compiler.Assign(evtFn, lambda);
                        compiler.CallStatic(s_EventUtil_Subscribe, compiler.GetCastElement(), Expression.Constant(attr.key), evtFn);
                        closure.Release();
                        LightList<Parameter>.Release(ref parameters);

                        return;
                    }
                }

                if (ReflectionUtil.IsProperty(compiler.rootElementType, idNode.name, out PropertyInfo propertyInfo)) {
                    if (eventInfo.EventHandlerType.IsAssignableFrom(propertyInfo.PropertyType)) {
                        LinqCompiler closure = compiler.CreateClosure(parameters, returnType);
                        string statement = propertyInfo.Name + "(";

                        for (int i = 0; i < parameters.size; i++) {
                            statement += parameters.array[i].name;
                            if (i != parameters.size - 1) {
                                statement += ", ";
                            }
                        }

                        statement += ")";
                        closure.Statement(statement);
                        LambdaExpression lambda = closure.BuildLambda();
                        ParameterExpression evtFn = compiler.AddVariable(lambda.Type, "evtFn");
                        compiler.Assign(evtFn, lambda);
                        compiler.CallStatic(s_EventUtil_Subscribe, compiler.GetCastElement(), Expression.Constant(attr.key), evtFn);
                        closure.Release();
                        LightList<Parameter>.Release(ref parameters);

                        return;
                    }
                }

                if (ReflectionUtil.IsMethod(compiler.rootElementType, idNode.name, out MethodInfo methodInfo)) {
                    LinqCompiler closure = compiler.CreateClosure(parameters, returnType);

                    string statement = idNode.name + "(";

                    for (int i = 0; i < parameters.size; i++) {
                        statement += parameters.array[i].name;
                        if (i != parameters.size - 1) {
                            statement += ", ";
                        }
                    }

                    statement += ")";
                    closure.Statement(statement);
                    LambdaExpression lambda = closure.BuildLambda();
                    ParameterExpression evtFn = compiler.AddVariable(lambda.Type, "evtFn");
                    compiler.Assign(evtFn, lambda);
                    compiler.CallStatic(s_EventUtil_Subscribe, compiler.GetCastElement(), Expression.Constant(attr.key), evtFn);
                    closure.Release();
                    LightList<Parameter>.Release(ref parameters);

                    return;
                }

                LightList<Parameter>.Release(ref parameters);

                throw new CompileException($"Error compiling event handler {attr.DebugData}. {idNode.name} is not assignable to type {eventInfo.EventHandlerType}");
            }

            else if (astNode.type == ASTNodeType.AccessExpression) {
                MemberAccessExpressionNode accessNode = (MemberAccessExpressionNode) astNode;
                LinqCompiler closure = compiler.CreateClosure(parameters, returnType);
                string statement = string.Empty;

                if (accessNode.parts[accessNode.parts.size - 1] is InvokeNode) {
                    statement = attr.value;
                }
                else {
                    statement = attr.value + "(";

                    for (int i = 0; i < parameters.size; i++) {
                        statement += parameters.array[i].name;
                        if (i != parameters.size - 1) {
                            statement += ", ";
                        }
                    }

                    statement += ")";
                }

                closure.Statement(statement);
                LambdaExpression lambda = closure.BuildLambda();
                ParameterExpression evtFn = compiler.AddVariable(lambda.Type, "evtFn");
                compiler.Assign(evtFn, lambda);
                compiler.CallStatic(s_EventUtil_Subscribe, compiler.GetCastElement(), Expression.Constant(attr.key), evtFn);
                closure.Release();
                LightList<Parameter>.Release(ref parameters);
            }
            else {
                LinqCompiler closure = compiler.CreateClosure(parameters, returnType);

                closure.Statement(eventInfo.EventHandlerType, astNode);
                LambdaExpression lambda = closure.BuildLambda();
                ParameterExpression evtFn = compiler.AddVariable(lambda.Type, "evtFn");
                compiler.Assign(evtFn, lambda);
                compiler.CallStatic(s_EventUtil_Subscribe, compiler.GetCastElement(), Expression.Constant(attr.key), evtFn);
                closure.Release();
            }

            LightList<Parameter>.Release(ref parameters);
        }

        private static void CompileConditionalBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            // cannot have more than 1 conditional    
            try {
                compiler.SetupAttributeData(attr);

                ParameterExpression element = compiler.GetElement();

                compiler.SetupAttributeData(attr);

                SetImplicitContext(compiler, attr);

                compiler.CommentNewLineBefore($"if=\"{attr.value}\"");
                MethodCallExpression setEnabled = ExpressionFactory.CallInstanceUnchecked(element, s_UIElement_SetEnabled, compiler.Value(attr.value));
                compiler.RawExpression(setEnabled);

                // if(!element.isEnabled) return
                // compiler.IfEqual(Expression.MakeMemberAccess(element, s_Element_IsEnabled), Expression.Constant(false), () => { compiler.Return(); });
            }
            catch (Exception e) {
                Debug.LogError(e);
            }

            // compiler.EndIsolatedSection();
        }

        private static void CompileAttributeBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            // __castElement.SetAttribute("attribute-name", computedValue);
            compiler.CommentNewLineBefore($"{attr.key}=\"{attr.value}\"");

            compiler.SetupAttributeData(attr);

            SetImplicitContext(compiler, attr);

            ParameterExpression element = compiler.GetElement();
            Expression value = compiler.TypedValue(typeof(string), attr.StrippedValue);

            if (value.Type != typeof(string)) {
                value = ExpressionFactory.CallInstanceUnchecked(value, value.Type.GetMethod("ToString", Type.EmptyTypes));
            }

            compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(element, s_UIElement_SetAttribute, Expression.Constant(attr.key), value));
        }

        private void CompileInstanceStyleBinding(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            StyleState styleState = StyleState.Normal;

            string key = attr.key;

            if ((attr.flags & AttributeFlags.StyleStateActive) == AttributeFlags.StyleStateActive) {
                styleState = StyleState.Active;
            }
            else if ((attr.flags & AttributeFlags.StyleStateFocus) == AttributeFlags.StyleStateFocus) {
                styleState = StyleState.Focused;
            }
            else if ((attr.flags & AttributeFlags.StyleStateHover) == AttributeFlags.StyleStateHover) {
                styleState = StyleState.Hover;
            }

            ParameterExpression castElement = compiler.GetCastElement();

            MemberExpression field = Expression.Field(castElement, s_UIElement_StyleSet);

            compiler.BeginIsolatedSection();

            compiler.SetupAttributeData(attr);

            SetImplicitContext(compiler, attr);

            compiler.CommentNewLineBefore($"style.{attr.key}=\"{attr.value}\"");

            if (!char.IsUpper(key[0])) {
                char[] keyChars = key.ToCharArray();
                keyChars[0] = char.ToUpper(keyChars[0]);
                key = new string(keyChars);
            }

            MethodInfo method = typeof(UIStyleSet).GetMethod("Set" + key);

            if (method == null) {
                throw CompileException.UnknownStyleMapping();
            }

            ParameterInfo[] parameters = method.GetParameters();

            Expression value = compiler.Value(attr.value);

            // hack! for some reason because the type can be by ref (via in) it doesn't report as a generic type
            if (parameters[0].ParameterType.FullName.Contains("System.Nullable")) {
                if (!value.Type.IsNullableType()) {
                    Type targetType = parameters[0].ParameterType.GetGenericArguments()[0];

                    if (targetType.IsByRef) {
                        targetType = targetType.GetElementType();
                    }

                    value = Expression.Convert(value, ReflectionUtil.CreateGenericType(typeof(Nullable<>), targetType));
                }
            }

            compiler.RawExpression(Expression.Call(field, method, value, Expression.Constant(styleState)));

            compiler.EndIsolatedSection();
        }

        private static bool HasTypeWrapper(Type type, out ITypeWrapper typeWrapper) {
            if (type == typeof(RepeatItemKey)) {
                typeWrapper = s_RepeatKeyFnTypeWrapper;
                return true;
            }

            typeWrapper = null;
            return false;
        }

        private Expression GetContextVariableValue(UIForiaLinqCompiler compiler, ContextVariableDefinition ctxVar, string prefix) {
            ParameterExpression el = compiler.GetElement();
            Expression access = Expression.MakeMemberAccess(el, s_UIElement_BindingNode);
            Expression call = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_GetContextVariable, Expression.Constant(ctxVar.id));
            Type varType = ReflectionUtil.CreateGenericType(typeof(ContextVariable<>), ctxVar.type);

            UnaryExpression convert = Expression.Convert(call, varType);
            ParameterExpression variable = compiler.AddVariable(ctxVar.type, prefix + ctxVar.GetName());

            compiler.Assign(variable, Expression.MakeMemberAccess(convert, varType.GetField("value")));
            return variable;
        }

        private ContextVariableDefinition CompilePropertyBinding(UIForiaLinqCompiler compiler, ProcessedType processedType, in AttributeDefinition attr, StructList<ChangeHandlerDefinition> changeHandlerAttrs) {
            LHSStatementChain left;
            Expression right = null;
            ParameterExpression castElement = compiler.GetCastElement();
            compiler.CommentNewLineBefore($"{attr.key}=\"{attr.value}\"");
            compiler.BeginIsolatedSection();
            try {
                compiler.SetupAttributeData(attr);
                compiler.SetImplicitContext(castElement);
                left = compiler.AssignableStatement(attr.key);
            }
            catch (Exception e) {
                TeardownAttributeData(attr);
                compiler.EndIsolatedSection();
                Debug.LogError(e);
                return null;
            }

            //castElement.value = root.value
            SetImplicitContext(compiler, attr);

            if (ReflectionUtil.IsFunc(left.targetExpression.Type)) {
                Type[] generics = left.targetExpression.Type.GetGenericArguments();
                Type target = generics[generics.Length - 1];
                if (HasTypeWrapper(target, out ITypeWrapper wrapper)) {
                    right = compiler.TypeWrapStatement(wrapper, left.targetExpression.Type, attr.value);
                }
            }

            if (right == null) {
                Expression accessor = compiler.AccessorStatement(left.targetExpression.Type, attr.value);

                if (accessor is ConstantExpression) {
                    right = accessor;
                }
                else {
                    right = compiler.AddVariable(left.targetExpression.Type, "__right");
                    compiler.Assign(right, accessor);
                }
            }

            // todo -- I can figure out if a value is constant using IsConstant(expr), use this information to push the expression onto the const compiler

            CompileChangeHandlerPropertyBindingStore(processedType.rawType, attr, changeHandlerAttrs, right);

            if ((attr.flags & AttributeFlags.Const) == 0) {
                StructList<ProcessedType.PropertyChangeHandlerDesc> changeHandlers = StructList<ProcessedType.PropertyChangeHandlerDesc>.Get();
                processedType.GetChangeHandlers(attr.key, changeHandlers);

                bool isProperty = ReflectionUtil.IsProperty(castElement.Type, attr.key);

                // if there is a change handler or the member is a property we need to check for changes
                // otherwise field values can be assigned w/o checking
                if (changeHandlers.size > 0 || isProperty) {
                    ParameterExpression old = compiler.AddVariable(left.targetExpression.Type, "__oldVal");
                    compiler.RawExpression(Expression.Assign(old, left.targetExpression));
                    compiler.IfNotEqual(left, right, () => {
                        compiler.Assign(left, right);
                        for (int j = 0; j < changeHandlers.size; j++) {
                            MethodInfo methodInfo = changeHandlers.array[j].methodInfo;
                            ParameterInfo[] parameters = methodInfo.GetParameters();

                            if (!methodInfo.IsPublic) {
                                throw CompileException.NonPublicPropertyChangeHandler(methodInfo.Name, right.Type);
                            }

                            if (parameters.Length == 0) {
                                compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(castElement, methodInfo));
                                continue;
                            }

                            if (parameters.Length == 1 && parameters[0].ParameterType == right.Type) {
                                compiler.RawExpression(ExpressionFactory.CallInstanceUnchecked(castElement, methodInfo, old));
                                continue;
                            }

                            throw CompileException.UnresolvedPropertyChangeHandler(methodInfo.Name, right.Type); // todo -- better error message
                        }
                    });
                }
                else {
                    compiler.Assign(left, right);
                }

                changeHandlers.Release();
            }
            else {
                compiler.Assign(left, right);
            }

            ContextVariableDefinition ctxVar = null;
            if ((attr.flags & AttributeFlags.Sync) != 0) {
                ctxVar = new ContextVariableDefinition();
                ctxVar.id = NextContextId;
                ctxVar.name = "sync_" + attr.key;
                ctxVar.type = left.targetExpression.Type;
                ctxVar.variableType = AliasResolverType.ContextVariable;

                MethodCallExpression createVariable = CreateLocalContextVariableExpression(ctxVar, out Type contextVarType);

                createdCompiler.RawExpression(createVariable);

                ctxVar.contextVarType = contextVarType;

                CompileAssignContextVariable(compiler, attr, ctxVar.contextVarType, ctxVar.id, "sync_", right);
            }

            TeardownAttributeData(attr);
            compiler.EndIsolatedSection();
            return ctxVar;
        }

        private void CompilePropertyBindingSync(UIForiaLinqCompiler compiler, in AttributeDefinition attr, ContextVariableDefinition ctxVar) {
            ParameterExpression castElement = compiler.GetCastElement();
            ParameterExpression castRoot = compiler.GetCastRoot();

            compiler.BeginIsolatedSection();
            compiler.SetupAttributeData(attr);

            Expression right = null;

            //castElement.value = root.value
            SetImplicitContext(compiler, attr);

            LHSStatementChain assignableStatement = compiler.AssignableStatement(attr.value);

            Expression accessor = compiler.AccessorStatement(assignableStatement.targetExpression.Type, attr.value);

            if (accessor is ConstantExpression) {
                right = accessor;
            }
            else {
                right = compiler.AddVariable(assignableStatement.targetExpression.Type, "__right");
                compiler.Assign(right, accessor);
            }

            compiler.SetImplicitContext(castRoot);
            Expression expr = GetContextVariableValue(compiler, ctxVar, "");
            compiler.SetImplicitContext(castElement);
            string key = attr.key;
            FieldInfo fieldInfo = castElement.Type.GetField(key);
            if (fieldInfo != null) {
                compiler.IfEqual(expr, right, () => { compiler.Assign(assignableStatement, Expression.Field(castElement, fieldInfo)); });
            }
            else {
                PropertyInfo propertyInfo = castElement.Type.GetProperty(key);
                if (propertyInfo != null) {
                    compiler.IfEqual(expr, right, () => { compiler.Assign(assignableStatement, Expression.Property(castElement, propertyInfo)); });
                }
            }

            compiler.EndIsolatedSection();
        }

        private void CompileChangeHandlerPropertyBindingStore(Type type, in AttributeDefinition attr, StructList<ChangeHandlerDefinition> changeHandlers, Expression value) {
            if (changeHandlers == null) return;

            for (int i = 0; i < changeHandlers.size; i++) {
                ref ChangeHandlerDefinition handler = ref changeHandlers.array[i];

                if (handler.attributeDefinition.key != attr.key) {
                    continue;
                }

                if (handler.wasHandled) {
                    return;
                }

                CompileChangeHandlerStore(type, value, ref handler);

                return;
            }
        }

        // todo accept compiler? or always use update?
        private void CompileChangeHandlerStore(Type type, Expression value, ref ChangeHandlerDefinition changeHandler) {
            ref AttributeDefinition attr = ref changeHandler.attributeDefinition;
            createdCompiler.SetupAttributeData(attr);
            updateCompiler.SetupAttributeData(attr);

            SetImplicitContext(createdCompiler, attr);
            SetImplicitContext(updateCompiler, attr);

            Type fieldOrPropertyType = ReflectionUtil.ResolveFieldOrPropertyType(type, attr.key);

            if (fieldOrPropertyType == null) {
                throw CompileException.UnresolvedFieldOrProperty(type, attr.key);
            }

            // create a local context variable
            ContextVariableDefinition variableDefinition = new ContextVariableDefinition();
            variableDefinition.name = attr.key;
            variableDefinition.id = NextContextId;
            variableDefinition.type = fieldOrPropertyType;
            variableDefinition.variableType = AliasResolverType.ChangeHandlerStorage;

            MethodCallExpression createVariable = CreateLocalContextVariableExpression(variableDefinition, out Type contextVarType);

            changeHandler.wasHandled = true;
            changeHandler.variableDefinition = variableDefinition;

            createdCompiler.RawExpression(createVariable);

            Expression access = Expression.MakeMemberAccess(updateCompiler.GetCastElement(), s_UIElement_BindingNode);
            Expression call = ExpressionFactory.CallInstanceUnchecked(access, s_LinqBindingNode_GetContextVariable, Expression.Constant(variableDefinition.id));
            updateCompiler.Comment(attr.key);
            Expression cast = Expression.Convert(call, contextVarType);
            ParameterExpression target = updateCompiler.AddVariable(contextVarType, "changeHandler_" + attr.key);
            updateCompiler.Assign(target, cast);
            MemberExpression valueField = Expression.Field(target, contextVarType.GetField(nameof(ContextVariable<object>.value)));
            updateCompiler.Assign(valueField, value);
        }

        private Expression ResolveAlias(string aliasName, LinqCompiler compiler) {
            if (aliasName == "oldValue") {
                if (changeHandlerPreviousValue == null) {
                    throw new CompileException("Invalid use of $oldValue, this alias is only available when used inside of an onChange handler");
                }

                return changeHandlerPreviousValue;
            }

            if (aliasName == "newValue") {
                if (changeHandlerCurrentValue == null) {
                    throw new CompileException("Invalid use of $newValue, this alias is only available when used inside of an onChange handler");
                }

                return changeHandlerCurrentValue;
            }

            if (aliasName == "element") {
                return ((UIForiaLinqCompiler) compiler).GetCastElement();
            }

            if (aliasName == "parent") {
                // todo -- should return the parent but ignore intrinsic elements like RepeatMulitChildContainer
                UIForiaLinqCompiler c = compiler as UIForiaLinqCompiler;
                System.Diagnostics.Debug.Assert(c != null, nameof(c) + " != null");
                return Expression.Field(c.GetElement(), s_UIElement_Parent);
            }

            if (aliasName == "evt") {
                if (currentEvent == null) {
                    throw new CompileException("Invalid use of $evt, this alias is only available when used inside of an event handler");
                }

                return currentEvent;
            }

            if (aliasName == "event") {
                if (currentEvent == null) {
                    throw new CompileException("Invalid use of $event, this alias is only available when used inside of an event handler");
                }

                return currentEvent;
            }

            if (aliasName == "root" || aliasName == "this") {
                return ((UIForiaLinqCompiler) compiler).GetCastRoot();
            }

            ContextVariableDefinition contextVar = FindContextByName(aliasName);

            if (contextVar != null) {
                if (resolvingTypeOnly) {
                    return contextVar.ResolveType(compiler);
                }

                return contextVar.Resolve(compiler);
            }

            throw CompileException.UnknownAlias(aliasName);
        }

        private Expression CreateElement(CompilationContext ctx, TemplateNode node) {
            return CreateElement(ctx, node.processedType, ctx.ParentExpr, node.ChildCount, CountRealAttributes(node.attributes), ctx.compiledTemplate.templateId);
        }

        private Expression CreateElement(CompilationContext ctx, ProcessedType processedType, Expression parentExpression, int childCount, int attrCount, int templateId) {
            return ExpressionFactory.CallInstanceUnchecked(ctx.applicationExpr, s_CreateFromPool,
                Expression.New(processedType.GetConstructor()),
                parentExpression,
                Expression.Constant(childCount),
                Expression.Constant(attrCount),
                Expression.Constant(templateId)
            );
        }

        private ProcessedType ResolveGenericElementType(IList<string> namespaces, Type rootType, TemplateNode templateNode) {
            ProcessedType processedType = templateNode.processedType;

            Type generic = processedType.rawType;
            Type[] arguments = processedType.rawType.GetGenericArguments();
            Type[] resolvedTypes = new Type[arguments.Length];

            if (templateNode.genericTypeResolver != null) {
                string replaceSpec = templateNode.genericTypeResolver.Replace("[", "<").Replace("]", ">");

                int ptr = 0;
                int rangeStart = 0;
                int depth = 0;

                LightList<string> strings = LightList<string>.Get();

                while (ptr != replaceSpec.Length) {
                    char c = replaceSpec[ptr];
                    switch (c) {
                        case '<':
                            depth++;
                            break;

                        case '>':
                            depth--;
                            break;

                        case ',': {
                            if (depth == 0) {
                                strings.Add(replaceSpec.Substring(rangeStart, ptr));
                                rangeStart = ptr;
                            }

                            break;
                        }
                    }

                    ptr++;
                }

                if (rangeStart != ptr) {
                    strings.Add(replaceSpec.Substring(rangeStart, ptr));
                }

                if (arguments.Length != strings.size) {
                    throw new CompileException($"Unable to resolve generic type of tag <{templateNode.tagName}>. Expected {arguments.Length} arguments but was only provided {strings.size} {templateNode.genericTypeResolver}");
                }

                for (int i = 0; i < strings.size; i++) {
                    if (ExpressionParser.TryParseTypeName(strings[i], out TypeLookup typeLookup)) {
                        Type type = TypeProcessor.ResolveType(typeLookup, (IReadOnlyList<string>) namespaces);

                        if (type == null) {
                            throw CompileException.UnresolvedType(typeLookup, (IReadOnlyList<string>) namespaces);
                        }

                        resolvedTypes[i] = type;
                    }
                    else {
                        throw new CompileException($"Unable to resolve generic type of tag <{templateNode.tagName}>. Failed to parse generic specifier {strings[i]}. Original expression = {templateNode.genericTypeResolver}");
                    }
                }

                strings.Release();
                Type createdType = ReflectionUtil.CreateGenericType(processedType.rawType, resolvedTypes);
                return TypeProcessor.AddResolvedGenericElementType(createdType, processedType.templateAttr, processedType.tagName);
            }

            typeResolver.Reset();
            resolvingTypeOnly = true;
            typeResolver.SetSignature(new Parameter(rootType, "__root", ParameterFlags.NeverNull));
            typeResolver.SetImplicitContext(typeResolver.GetParameter("__root"));
            typeResolver.resolveAlias = ResolveAlias;
            typeResolver.Setup(rootType, null, (LightList<string>) namespaces);

            if (templateNode.attributes == null) {
                throw CompileException.UnresolvedGenericElement(processedType, templateNode.TemplateNodeDebugData);
            }

            for (int i = 0; i < templateNode.attributes.size; i++) {
                ref AttributeDefinition attr = ref templateNode.attributes.array[i];

                if (attr.type != AttributeType.Property) continue;

                if (ReflectionUtil.IsField(generic, attr.key, out FieldInfo fieldInfo)) {
                    if (fieldInfo.FieldType.IsGenericParameter || fieldInfo.FieldType.IsGenericType || fieldInfo.FieldType.IsConstructedGenericType) {
                        if (ValidForGenericResolution(fieldInfo.FieldType)) {
                            HandleType(fieldInfo.FieldType, attr);
                        }
                    }
                }
                else if (ReflectionUtil.IsProperty(generic, attr.key, out PropertyInfo propertyInfo)) {
                    if (propertyInfo.PropertyType.IsGenericParameter || propertyInfo.PropertyType.IsGenericType || propertyInfo.PropertyType.IsConstructedGenericType) {
                        HandleType(propertyInfo.PropertyType, attr);
                    }
                }
            }

            for (int i = 0; i < arguments.Length; i++) {
                if (resolvedTypes[i] == null) {
                    throw CompileException.UnresolvedGenericElement(processedType, templateNode.TemplateNodeDebugData);
                }
            }

            Type newType = ReflectionUtil.CreateGenericType(processedType.rawType, resolvedTypes);
            ProcessedType retn = TypeProcessor.AddResolvedGenericElementType(newType, processedType.templateAttr, processedType.tagName);
            resolvingTypeOnly = false;
            return retn;

            bool ValidForGenericResolution(Type checkType) {
                if (checkType.IsConstructedGenericType) {
                    Type[] args = checkType.GetGenericArguments();
                    for (int i = 0; i < args.Length; i++) {
                        if (args[i].IsConstructedGenericType) {
                            return false;
                        }
                    }
                }

                return true;
            }

            int GetTypeIndex(Type[] _args, string name) {
                for (int i = 0; i < _args.Length; i++) {
                    if (_args[i].Name == name) {
                        return i;
                    }
                }

                return -1;
            }

            void HandleType(Type inputType, in AttributeDefinition attr) {
                if (!inputType.ContainsGenericParameters) {
                    return;
                }

                // List<IOption<T>> options
                // resolve T
                // options = (<IList<IOption<string>>)someExpression();

                // find out if we have a generic argument in our field
                // either field type is generic 
                // or field is a constructed generic type
                // if constructed 
                // recurse both sides
                // class StringList : List<String> {} 
                // have generic type defintion and expression
                // solve for generic type defs by extracting 'T' arguments from expression

                // dont care about type checking yet

                // need to 'step into' constructed types until non constructed found

                if (inputType.IsConstructedGenericType) {
                    if (ReflectionUtil.IsAction(inputType) || ReflectionUtil.IsFunc(inputType)) {
                        return;
                    }

                    Type expressionType = typeResolver.GetExpressionType(attr.value);

                    Type[] typeArgs = expressionType.GetGenericArguments();
                    Type[] memberGenericArgs = inputType.GetGenericArguments();

                    Assert.AreEqual(memberGenericArgs.Length, typeArgs.Length);

                    for (int a = 0; a < memberGenericArgs.Length; a++) {
                        string genericName = memberGenericArgs[a].Name;
                        int typeIndex = GetTypeIndex(arguments, genericName);

                        if (typeIndex == -1) {
                            throw new CompileException(templateNode.TemplateNodeDebugData.tagName + templateNode.TemplateNodeDebugData.lineInfo);
                        }

                        Assert.IsTrue(typeIndex != -1);

                        if (resolvedTypes[typeIndex] != null) {
                            if (resolvedTypes[typeIndex] != typeArgs[a]) {
                                throw CompileException.DuplicateResolvedGenericArgument(templateNode.tagName, inputType.Name, resolvedTypes[typeIndex], typeArgs[a]);
                            }
                        }

                        resolvedTypes[typeIndex] = typeArgs[a];
                    }
                }
                else {
                    string genericName = inputType.Name;
                    int typeIndex = GetTypeIndex(arguments, genericName);
                    Assert.IsTrue(typeIndex != -1);

                    Type type = typeResolver.GetExpressionType(attr.value);
                    if (resolvedTypes[typeIndex] != null) {
                        if (resolvedTypes[typeIndex] != type) {
                            throw CompileException.DuplicateResolvedGenericArgument(templateNode.tagName, inputType.Name, resolvedTypes[typeIndex], type);
                        }
                    }

                    resolvedTypes[typeIndex] = type;
                }
            }
        }

        private static void SetImplicitContext(UIForiaLinqCompiler compiler, in AttributeDefinition attr) {
            if ((attr.flags & AttributeFlags.InnerContext) != 0) {
                compiler.SetImplicitContext(compiler.GetCastElement());
            }
            else {
                compiler.SetImplicitContext(compiler.GetCastRoot());
            }
        }

        private enum ModType {

            Alias,
            Context

        }

        private struct ContextAliasActions {

            public ModType modType;
            public string name;

        }

    }

}