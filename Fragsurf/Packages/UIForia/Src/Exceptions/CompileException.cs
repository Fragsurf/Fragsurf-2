using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UIForia.Compilers;
using UIForia.Parsing;
using UIForia.Parsing.Expressions;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Text;
using UIForia.UIInput;
using UIForia.Util;

namespace UIForia.Exceptions {

    public class CompileException : Exception {

        private string fileName = "";
        public string expression = "";

        public CompileException(string message = null) : base(message) { }


        public CompileException(StyleASTNode node, string message = null) :
            base($"Compile error for style token at line {node.line}, column {node.column}, node type '{node.type}'\n{message}") { }

        public CompileException(string fileName, StyleASTNode node, string message = null) :
            base($"Compile error for style token at line {node.line}, column {node.column}, node type '{node.type}'\n{message}") {
            this.fileName = fileName;
        }

        public void SetFileName(string name) {
            this.fileName = "Error in file " + name + ": ";
        }

        public override string Message {
            get {
                string retn = fileName + "\n" + base.Message;

                if (!string.IsNullOrEmpty(expression)) {
                    retn += "\nExpression was: " + expression;
                }

                return retn;
            }
        }

        public void SetExpression(string input) {
            expression = input;
        }

        // todo -- add more debug info to these and make them actually useful. These are basically placeholder but need help to be really useful to people

        public static CompileException MissingAliasResolver(string aliasName) {
            return new CompileException($"No alias resolvers were registered that could handle alias {aliasName}");
        }
        
        public static CompileException NoStatementsRootBlock() {
            return new CompileException($"Cannot compile the lambda because there are not statements emitted in the main block of the function");
        }

        public static CompileException InvalidActionArgumentCount(IList<ParameterExpression> parameters, Type[] genericArguments) {
            return new CompileException($"Cannot compile the action because the declared parameter count {parameters.Count} is not the same as the required signatures parameter count {genericArguments.Length}");
        }

        public static CompileException MissingBinaryOperator(OperatorType opType, Type a, Type b) {
            return new CompileException($"Missing operator: the binary operator {opType} is not defined for the types {a} and {b}");
        }

        public static CompileException UnresolvedIdentifier(string identifierNodeName) {
            return new CompileException($"Unable to resolve the variable or parameter {identifierNodeName}");
        }

        public static CompileException InvalidTargetType(Type expected, Type actual) {
            return new CompileException($"Expected expression to be compatible with type {expected} but got {actual} which was not convertible");
        }

        public static CompileException InvalidAccessExpression() {
            return new CompileException("Expected access expression to have more symbols, the last expression is not a valid terminal");
        }

        public static CompileException InvalidIndexOrInvokeOperator() {
            return new CompileException("Index or Invoke operations are not valid on static types or namespaces");
        }

        public static CompileException UnknownStaticOrConstMember(Type type, string memberName) {
            return new CompileException($"Unable to find a field or property with the name {memberName} on type {type}");
        }

        public static CompileException AccessNonReadableStaticOrConstField(Type type, string memberName) {
            return new CompileException($"Unable to read static or const field {memberName} on type {type} because it is not marked as public");
        }

        public static CompileException AccessNonReadableStaticProperty(Type type, string memberName) {
            return new CompileException($"Unable to read static property {memberName} on type {type} because has no read accessor");
        }

        public static CompileException AccessNonReadableProperty(Type type, PropertyInfo propertyInfo) {
            return new CompileException($"Unable to read {(propertyInfo.GetMethod.IsStatic ? "static" : "instance")} property {propertyInfo.Name} on type {type} because has no public read accessor");
        }

        public static CompileException AccessNonPublicStaticProperty(Type type, string memberName) {
            return new CompileException($"Unable to read static property {memberName} on type {type} because it's read accessor is not public");
        }

        public static CompileException AccessNonReadableField(Type type, FieldInfo fieldInfo) {
            if (fieldInfo.IsStatic) {
                return new CompileException($"Unable to read static field {fieldInfo.Name} on type {type} because it is not marked as public");
            }
            else {
                return new CompileException($"Unable to read instance field {fieldInfo.Name} on type {type} because it is not marked as public");
            }
        }
        
        public static CompileException InvalidLambdaArgument() {
            return new CompileException($"Type mismatch with Lambda Argument");
        }
        
        public static CompileException NoImplicitConversion(Type a, Type b) {
            return new CompileException($"Implicit conversion exists between types {a} and {b}. Please use casing to fix this");
        }
        
        public static CompileException NoSuchProperty(Type type, string fieldOrPropertyName) {
            return new CompileException($"Type {type} has no field or property {fieldOrPropertyName}");
        }

        public static CompileException UnresolvedType(TypeLookup typeLookup, IReadOnlyList<string> searchedNamespaces = null) {
            string retn = string.Empty;
            if (searchedNamespaces != null) {
                retn += " searched in the following namespaces: ";
                for (int i = 0; i < searchedNamespaces.Count - 1; i++) {
                    retn += searchedNamespaces[i] + ",";
                }

                retn += searchedNamespaces[searchedNamespaces.Count - 1];
            }

            return new CompileException($"Unable to resolve type {typeLookup}, are you missing a namespace?{retn}");
        }

        public static CompileException InvalidNamespaceOperation(string namespaceName, Type type) {
            return new CompileException($"Resolved namespace {namespaceName} but {type} is not a valid next token");
        }

        public static CompileException UnknownEnumValue(Type type, string value) {
            return new CompileException($"Unable to enum value {value} on type {type}");
        }
        
        public static CompileException UnresolvedStaticMethod(Type type, string value) {
            return new CompileException($"Unable to find a public method on {type} with the name {value}");
        }
        
        public static CompileException NonPublicType(Type type) {
            return new CompileException($"The type {type} is not public and cannot be used in expressions.");
        }

        public static CompileException SignatureNotDefined() {
            return new CompileException($"The signature must be set before calling builder methods on {nameof(LinqCompiler)}");
        }
        public static CompileException UnresolvedMethodOverload(Type type, string methodName, Type[] inputTypeArguments) {
            string argumentTypeString = "";
            for (int i = 0; i < inputTypeArguments.Length; i++) {
                argumentTypeString += inputTypeArguments[i].FullName;
                if (i != inputTypeArguments.Length - 1) {
                    argumentTypeString += ", ";
                }
            }
            return new CompileException($"Unable to find a public method '{methodName}' on type {type} with a signature matching ({argumentTypeString})");
        }

         public static CompileException UnresolvedInstanceMethodOverload(Type type, string methodName, Type[] inputTypeArguments) {
            string argumentTypeString = "";
            for (int i = 0; i < inputTypeArguments.Length; i++) {
                argumentTypeString += inputTypeArguments[i].FullName;
                if (i != inputTypeArguments.Length - 1) {
                    argumentTypeString += ", ";
                }
            }
            return new CompileException($"Unable to find a public instance method '{methodName}' on type {type} with a signature matching ({argumentTypeString})");
        }
        
        public static CompileException UnresolvedConstructor(Type type, Type[] arguments) {
            string BuildArgumentList() {
                if (arguments == null || arguments.Length == 0) {
                    return "no arguments";
                }

                string retn = "arguments (";
                for (int i = 0; i < arguments.Length; i++) {
                    retn += arguments[i];
                    if (i != arguments.Length - 1) {
                        retn += ", ";
                    }
                }

                retn += ")";
                return retn;
            }

            return new CompileException($"Unable to find a suitable constructor on type {type} that accepts {BuildArgumentList()}");
        }

        public static CompileException UnresolvedMethod(Type type, string methodName) {
            return new CompileException($"Unable to find a method called `{methodName}` on type {type}");
        }

        public static CompileException UnresolvedFieldOrProperty(Type type, string fieldOrPropertyName) {
            return new CompileException($"Unable to find a field or property called `{fieldOrPropertyName}` on type {type}");
        }

        public static CompileException UnresolvedGenericElement(ProcessedType processedType, TemplateNodeDebugData data) {
            return new CompileException($"Unable to resolve the concrete type for " + processedType.rawType + $"\n\nFailed parsing {data.tagName} at {data.fileName}:{data.lineInfo}\n" +
                                        $"You can try to fix this by providing the type explicitly. (add an attribute generic:type=\"your,types,here\"");
        }

        public static CompileException GenericElementMissingResolver(ProcessedType processedType) {
            return new CompileException($"{processedType.rawType} requires a class attribute of type {nameof(GenericElementTypeResolvedByAttribute)} in order to be used in a template");
        }

        public static CompileException UnresolvableGenericElement(ProcessedType processedType, string value) {
            return new CompileException($"{processedType.rawType} requires a class attribute of type {nameof(GenericElementTypeResolvedByAttribute)} in order to be used in a template and a value that is not null or default also declared in the template");
        }

        public static CompileException UnmatchedSlot(string slotName, string path) {
            return new CompileException($"Unable to find a matching slot with the name {slotName} in template {path}");
        }

        public static CompileException InvalidInputHandlerLambda(in AttributeDefinition attr, int signatureSize) {
            return new CompileException($"Input handler lambda is invalid. Expected 0 or 1 arguments for handler {attr.value} but found {signatureSize}");
        }

        public static CompileException UnknownStyleState(in AttributeNodeDebugData data, string s) {
            return new CompileException($"file: {data.fileName}{data.lineInfo}\nUnable to handle style state declaration '{s}' Expected 'active', 'focus', or 'hover'");
        }

        public static CompileException UnresolvedRepeatType(string provided, params string[] others) {
            return new CompileException("Unable to determine repeat type: " + provided + " was provided but is not legal in combination with " + StringUtil.ListToString(others));
        }

        public static CompileException UnresolvedPropertyChangeHandler(string methodInfoName, Type propertyType) {
            return new CompileException($"Unable to use {methodInfoName} as a property change handler. Please be sure the signature either accepts no arguments or only 1 argument with a type matching the type of the property it is bound to: {propertyType}");
        }
   
        public static CompileException NonPublicPropertyChangeHandler(string methodInfoName, Type propertyType) {
            return new CompileException($"Unable to use {methodInfoName} as a property change handler because it is not public");
        }
   
        public static CompileException UnknownAlias(string aliasName) {
            return new CompileException($"Unknown alias `{aliasName}`");
        }

        public static CompileException DuplicateResolvedGenericArgument(string tagName, string argName, Type original, Type duplicate) {
            return new CompileException($"When attempting to resolve generic element tag {tagName}, {argName} was resolved first to {original} and later to {duplicate}. Ensure multiple usages of {argName} resolve to the same type.");
        }

        public static CompileException MultipleConditionalBindings(TemplateNodeDebugData data) {
            return new CompileException($"Encountered multiple conditional bindings (if) on element {data.tagName} in file: {data.fileName} {data.lineInfo}. Only one conditional binding is permitted per element");
        }

        public static CompileException UnknownStyleMapping() {
            return new CompileException($"Unknown style mapping");
        }

        public static CompileException InvalidInputAnnotation(string methodName, Type type, Type annotationType, Type expectedParameterType, Type actualParameterType) {
           return new CompileException($"Method {methodName} in type {type.Name} is annotated with {annotationType.Name} which expects 0 or 1 arguments of type {expectedParameterType} but was declared with {actualParameterType} which is invalid");
        }

        public static CompileException TooManyInputAnnotationArguments(string methodName, Type type, Type annotationType, Type expectedParameterType, int parameterCount) {
            return new CompileException($"Method {methodName} in type {type.Name} is annotated with {annotationType.Name} which expects 0 or 1 arguments of type {expectedParameterType} but was declared with {parameterCount} arguments which is invalid");
        }
        
        public static CompileException InvalidDragCreatorAnnotationReturnType(string methodName, Type type, Type returnType) {
            return new CompileException($"Method {methodName} in type {type.Name} is annotated with {nameof(OnDragCreateAttribute)} which expects a return type assignable to {nameof(DragEvent)}. The method returns {returnType} which is invalid");
        }

        public static CompileException NonAccessibleOrStatic(Type type, string methodName) {
            return new CompileException($"Method {methodName} in type {type.Name} is either not public or is a static method and cannot be used in expression");
        }

    }
    

}