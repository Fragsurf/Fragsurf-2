using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Elements;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Compilers {

    public class UIForiaLinqCompiler : LinqCompiler {

        private static readonly LightList<UIForiaLinqCompiler> s_CompilerPool = new LightList<UIForiaLinqCompiler>();

        private ParameterExpression elementParameter;
        private ParameterExpression rootParameter;
        private ParameterExpression castElementParameter;
        private ParameterExpression castRootParameter;
        private MemberExpression inputHandlerGroup;
        public SlotAttributeData attributeData;

        public Type elementType;
        public Type rootElementType;

        private const string k_CastElement = "__castElement";
        private const string k_CastRoot = "__castRoot";
        private UIForiaLinqCompiler parent;
        private bool attrMode;
        private LightList<string> originalNamespaces;
        
        private static readonly FieldInfo s_UIElement_InputHandlerGroup = typeof(UIElement).GetField(nameof(UIElement.inputHandlers), BindingFlags.Instance | BindingFlags.Public);

        public void Setup(Type rootElementType, Type elementType, LightList<string> originalNamespaces) {
            this.rootElementType = rootElementType;
            this.elementType = elementType;
            this.originalNamespaces = originalNamespaces;
            elementParameter = null;
            rootParameter = null;
            castElementParameter = null;
            castRootParameter = null;
            inputHandlerGroup = null;
            castSlotContext = null;
            slotContext = null;
        }

        protected override void SetupClosure(LinqCompiler p) {
            this.parent = p as UIForiaLinqCompiler;
        }

        public MemberExpression GetInputHandlerGroup() {
            if (inputHandlerGroup == null) {
                inputHandlerGroup = Expression.Field(GetElement(), s_UIElement_InputHandlerGroup);
                Assign(inputHandlerGroup, Expression.New(typeof(InputHandlerGroup)));
            }

            return inputHandlerGroup;
        }

        public ParameterExpression GetElement() {
            if (elementParameter == null) {
                if (parent != null) return parent.GetElement();
                elementParameter = GetParameter("__element");
            }

            return elementParameter;
        }

        private ParameterExpression slotContext;
        private ParameterExpression castSlotContext;

        public ParameterExpression GetRoot() {
            if (attributeData != null) {
                if (slotContext == null) {
                    string variableName = "__slotCtx" + attributeData.slotDepth;
                    ParameterExpression variable = GetVariable(variableName);
                    if (variable != null) {
                        slotContext = variable;
                        return slotContext;
                    }
                    Parameter p = new Parameter(attributeData.slotContextType.rawType, variableName, ParameterFlags.NeverNull);
                    MemberExpression bindingNode = Expression.Field(GetElement(), TemplateCompiler.s_UIElement_BindingNode);
                    MemberExpression referenceArray = Expression.Field(bindingNode, TemplateCompiler.s_LinqBindingNode_ReferencedContext);
                    BinaryExpression index = Expression.ArrayIndex(referenceArray, Expression.Constant(attributeData.slotDepth));
                    slotContext = AddVariableUnchecked(p, ExpressionFactory.Convert(index, attributeData.slotContextType.rawType));
                }

                return slotContext;
            }


            if (rootParameter == null) {
                rootParameter = GetParameter("__root");
            }

            return rootParameter;
        }

        public ParameterExpression GetCastElement() {
            if (castElementParameter == null) {
                if (parent != null) return parent.GetCastElement();
                Parameter p = new Parameter(elementType, k_CastElement, ParameterFlags.NeverNull);
                castElementParameter = AddVariableUnchecked(p, ExpressionFactory.Convert(GetElement(), elementType));
            }

            return castElementParameter;
        }

        public ParameterExpression GetCastRoot() {
            if (attributeData != null) {
                if (castSlotContext == null) {
                    
                    string variableName = "__slotCtx" + attributeData.slotDepth + "_cast";
                    ParameterExpression variable = GetVariable(variableName);
                    if (variable != null) {
                        castSlotContext = variable;
                        return variable;
                    }
                    Parameter p = new Parameter(attributeData.slotContextType.rawType, variableName, ParameterFlags.NeverNull);
                    MemberExpression bindingNode = Expression.Field(GetElement(), TemplateCompiler.s_UIElement_BindingNode);
                    MemberExpression referenceArray = Expression.Field(bindingNode, TemplateCompiler.s_LinqBindingNode_ReferencedContext);
                    BinaryExpression index = Expression.ArrayIndex(referenceArray, Expression.Constant(attributeData.slotDepth));
                    castSlotContext = AddVariableUnchecked(p, Expression.Convert(index, attributeData.slotContextType.rawType));
                }

                return castSlotContext;
            }

            if (castRootParameter == null) {
                Parameter p = new Parameter(rootElementType, k_CastRoot, ParameterFlags.NeverNull);
                castRootParameter = AddVariableUnchecked(p, ExpressionFactory.Convert(GetRoot(), rootElementType));
            }

            return castRootParameter;
        }

        protected override LinqCompiler CreateNested() {
            if (s_CompilerPool.size == 0) {
                return new UIForiaLinqCompiler();
            }

            return s_CompilerPool.RemoveLast();
        }

        public override void Release() {
            Reset();
            s_CompilerPool.Add(this);
        }

        public void SetupAttributeData(in AttributeDefinition attr) {
            this.attributeData = attr.slotAttributeData;
            this.castSlotContext = null;
            this.slotContext = null;
            SetNamespaces(attr.templateShell.referencedNamespaces);
        }

        public void TeardownAttributeData() {
            this.attributeData = null;
            this.castSlotContext = null;
            this.slotContext = null;
            SetNamespaces(originalNamespaces);
        }

    }

}