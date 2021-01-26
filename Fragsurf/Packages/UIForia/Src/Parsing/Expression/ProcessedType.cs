using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Parsing.Expressions;
using UIForia.Util;

namespace UIForia.Parsing {

    [DebuggerDisplay("{rawType.Name}")]
    public class ProcessedType {

        public readonly Type rawType;
        public readonly TemplateAttribute templateAttr;
        public readonly bool requiresUpdateFn;
        private object rawCtorFn;
        private StructList<PropertyChangeHandlerDesc> methods;
        private Func<ProcessedType, UIElement, UIElement> constructionFn;
        public readonly string tagName;
        public string namespaceName;
        public int id;
        public int references;
        public bool requiresBeforePropertyUpdates;
        public bool requiresAfterPropertyUpdates;
        public bool requiresOnEnable;
        public bool isDynamic;

        public ProcessedType(Type rawType, TemplateAttribute templateAttr, string tagName = null) {
            this.id = -1; // set by TypeProcessor
            this.rawType = rawType;
            this.templateAttr = templateAttr;
            this.tagName = tagName;
            this.requiresUpdateFn = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnUpdate)));
            this.requiresOnEnable = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnEnable)));
            this.requiresBeforePropertyUpdates = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnBeforePropertyBindings)));
            this.requiresAfterPropertyUpdates = ReflectionUtil.IsOverride(rawType.GetMethod(nameof(UIElement.OnAfterPropertyBindings)));
            this.namespaceName = rawType.Namespace;
        }

        public struct PropertyChangeHandlerDesc {

            public MethodInfo methodInfo;
            public string memberName;

        }

        public bool IsUnresolvedGeneric { get; set; }

        public void GetChangeHandlers(string memberName, StructList<PropertyChangeHandlerDesc> retn) {
            if (methods == null) {
                MethodInfo[] candidates = ReflectionUtil.GetInstanceMethods(rawType);
                for (int i = 0; i < candidates.Length; i++) {
                    IEnumerable<OnPropertyChanged> attrs = candidates[i].GetCustomAttributes<OnPropertyChanged>();
                    methods = methods ?? new StructList<PropertyChangeHandlerDesc>();
                    foreach (OnPropertyChanged a in attrs) {
                        methods.Add(new PropertyChangeHandlerDesc() {
                            methodInfo = candidates[i],
                            memberName = a.propertyName
                        });
                    }
                }
            }

            if (methods == null) {
                return;
            }

            for (int i = 0; i < methods.size; i++) {
                if (methods.array[i].memberName == memberName) {
                    retn.Add(methods[i]);
                }
            }
        }
        
        public void ValidateAttributes(StructList<AttributeDefinition> attributes) { }

        public ProcessedType Reference() {
            references++;
            return this;
        }

        internal ConstructorInfo constructorInfo;
        public ConstructorInfo GetConstructor() {
            if (constructorInfo != null) {
                return constructorInfo;
            }

            constructorInfo = rawType.GetConstructor(Type.EmptyTypes);
            if (constructorInfo == null) {
                UnityEngine.Debug.LogError(rawType + "doesn't define a parameterless public constructor. This is a requirement for it to be used templates");
            }

            return constructorInfo;
        }

    }

}