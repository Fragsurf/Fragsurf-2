// using System;
// using System.Collections.Generic;
// using System.Linq.Expressions;
// using System.Reflection;
// using System.Reflection.Emit;
// using UIForia.Compilers;
// using UIForia.Exceptions;
// using UIForia.Parsing;
// using UIForia.Parsing.Expressions;
// using UIForia.Parsing.Expressions.AstNodes;
// using UnityEngine.Assertions;
//
// namespace UIForia.Util {
//
//     public class ClassBuilder {
//
//         private readonly AssemblyName assemblyName;
//         private readonly AssemblyBuilder assemblyBuilder;
//         private readonly ModuleBuilder moduleBuilder;
//         private readonly Dictionary<string, Type> typeMap;
//         private readonly LinqCompiler linqCompiler;
//         private static Dictionary<Type, TypeData> s_DynamicTypeData = new Dictionary<Type, TypeData>();
//
//         public ClassBuilder() {
//             this.linqCompiler = new LinqCompiler();
//             this.assemblyName = new AssemblyName("UIForia.Generated");
//             this.assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
//             this.moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
//             this.typeMap = new Dictionary<string, Type>();
//         }
//
//         public void Reset() {
//             typeMap.Clear();
//         }
//
//         public Type CreateGenericRuntimeType(string id, Type baseType, ReflectionUtil.GenericTypeDefinition[] generics, IList<ReflectionUtil.FieldDefinition> fieldDefinitions, IList<string> namespaces) {
//             if (typeMap.ContainsKey(id)) {
//                 return null; //todo -- exception
//             }
//
//             TypeBuilder typeBuilder = moduleBuilder.DefineType(
//                 id,
//                 TypeAttributes.Public |
//                 TypeAttributes.Class |
//                 TypeAttributes.AutoClass |
//                 TypeAttributes.AnsiClass |
//                 TypeAttributes.BeforeFieldInit |
//                 TypeAttributes.AutoLayout,
//                 baseType
//             );
//
//             string[] typeNames = new string[generics.Length];
//
//             for (int i = 0; i < generics.Length; i++) {
//                 typeNames[i] = generics[i].name;
//             }
//
//             GenericTypeParameterBuilder[] typeParams = typeBuilder.DefineGenericParameters(typeNames);
//
//             for (int i = 0; i < fieldDefinitions.Count; i++) {
//                 // string typeName = fieldDefinitions[i].fieldType;
//                 //
//                 // Type fieldType = ResolveFieldTypeFromGenerics(typeName, typeParams);
//                 //
//                 // if (fieldType == null) {
//                 //     fieldType = TypeProcessor.ResolveTypeExpression(null, namespaces, typeName);
//                 // }
//                 //
//                 // typeBuilder.DefineField(fieldDefinitions[i].fieldName, fieldType, FieldAttributes.Public);
//             }
//
//             Type retn = typeBuilder.CreateType();
//             typeMap[id] = retn;
//             return retn;
//         }
//
//         // private static Type ResolveFieldTypeFromGenerics(string fieldType, GenericTypeParameterBuilder[] typeParams) {
//         //     for (int i = 0; i < typeParams.Length; i++) {
//         //         if (fieldType == typeParams[i].Name) {
//         //             return typeParams[i];
//         //         }
//         //     }
//         //
//         //     return null;
//         // }
//         //
//         // private static void GenerateMethodCallIL(FieldBuilder fieldBuilder, MethodInfo methodInfo, ILGenerator il, Type[] parameters) {
//         //     il.Emit(OpCodes.Ldsfld, fieldBuilder);
//         //
//         //     int argIdx = 0;
//         //     // first 3 args can go directly in. assume we always get arg0 as 'this'
//         //     for (argIdx = 0; argIdx < parameters.Length; argIdx++) {
//         //         switch (argIdx) {
//         //             case 0:
//         //                 il.Emit(OpCodes.Ldarg_0);
//         //                 break;
//         //             case 1:
//         //                 il.Emit(OpCodes.Ldarg_1);
//         //                 break;
//         //             case 2:
//         //                 il.Emit(OpCodes.Ldarg_2);
//         //                 break;
//         //             case 3:
//         //                 il.Emit(OpCodes.Ldarg_3);
//         //                 break;
//         //             default:
//         //                 il.Emit(OpCodes.Ldarg_S);
//         //                 break;
//         //         }
//         //     }
//         //
//         //     il.EmitCall(OpCodes.Callvirt, methodInfo, null);
//         //
//         //     if (methodInfo.ReturnType == typeof(void)) {
//         //         il.Emit(OpCodes.Nop);
//         //     }
//         //     // else {
//         //     //     il.Emit(OpCodes.Stloc_0);
//         //     //     il.Emit(OpCodes.Br_S, targetInstruction);
//         //     //     il.MarkLabel(targetInstruction);
//         //     //     il.Emit(OpCodes.Ldloc_0);
//         //     // }
//         //
//         //     il.Emit(OpCodes.Ret);
//         // }
//
//         // todo -- bring this back properly, keeping this code for later reference
//         // public Type CreateRuntimeType(string id, Type baseType, IList<ReflectionUtil.FieldDefinition> fields, IList<ReflectionUtil.MethodDefinition> methods, IList<string> namespaces) {
//         //     if (typeMap.ContainsKey(id)) {
//         //         return null;
//         //     }
//         //
//         //     TypeBuilder typeBuilder = moduleBuilder.DefineType(
//         //         id,
//         //         TypeAttributes.Public |
//         //         TypeAttributes.Class |
//         //         TypeAttributes.AutoClass |
//         //         TypeAttributes.AnsiClass |
//         //         TypeAttributes.BeforeFieldInit |
//         //         TypeAttributes.AutoLayout,
//         //         baseType
//         //     );
//         //
//         //     typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
//         //
//         //     if (fields != null) {
//         //         for (int i = 0; i < fields.Count; i++) {
//         //             Type fieldType = TypeProcessor.ResolveType(fields[i].fieldType, (IReadOnlyList<string>) namespaces);
//         //
//         //             if (fields[i].isStatic) {
//         //                 typeBuilder.DefineField(fields[i].fieldName, fieldType, FieldAttributes.Public  | FieldAttributes.Static);
//         //             }
//         //             else {
//         //                 typeBuilder.DefineField(fields[i].fieldName, fieldType, FieldAttributes.Public);
//         //             }
//         //             
//         //         }
//         //     }
//         //
//         //     const MethodAttributes k_MethodAttributes = MethodAttributes.Public | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
//         //
//         //     StructList<ExpressionData> expressionData = StructList<ExpressionData>.Get();
//         //
//         //     if (methods != null) {
//         //         for (int i = 0; i < methods.Count; i++) {
//         //             ReflectionUtil.MethodDefinition methodDefinition = methods[i];
//         //
//         //             Type returnType = null;
//         //
//         //             if (methodDefinition.returnType.typeName != null && methodDefinition.returnType.typeName != "void") {
//         //                 returnType = TypeProcessor.ResolveType(methodDefinition.returnType, (IReadOnlyList<string>) namespaces);
//         //             }
//         //
//         //             // todo -- allocate less
//         //             ResolvedParameter[] signature = new ResolvedParameter[methodDefinition.arguments.Length];
//         //             Type[] staticSignature = new Type[signature.Length + 1];
//         //             Type[] signatureTypes = new Type[methodDefinition.arguments.Length];
//         //
//         //             // because this type doesn't exist yet, making a delegate the references it fails upon type creation.
//         //             // instead we resort to treating 'this' as an object type and invoking methods via cast.
//         //             // this won't be the case for production code though
//         //             staticSignature[0] = typeof(object);
//         //
//         //             for (int j = 0; j < methodDefinition.arguments.Length; j++) {
//         //                 ref LambdaArgument argument = ref methodDefinition.arguments[j];
//         //                 TypeLookup? typeLookup = argument.type;
//         //                 
//         //                 Assert.IsTrue(typeLookup.HasValue);
//         //                 
//         //                 Type type = TypeProcessor.ResolveType(typeLookup.Value, (IReadOnlyList<string>) namespaces);
//         //
//         //                 if (type == null) {
//         //                     throw new CompileException($"Unable to resolve type for {typeLookup.Value.ToString()}");
//         //                 }
//         //                 
//         //                 signatureTypes[j] = type;
//         //                 signature[j] = new ResolvedParameter() {
//         //                     type = type,
//         //                     name = argument.identifier
//         //                 };
//         //                 staticSignature[j + 1] = signature[j].type;
//         //             }
//         //
//         //             Type fnType = ReflectionUtil.GetClosedDelegateType(staticSignature, returnType ?? typeof(void));
//         //
//         //             FieldBuilder fnField = typeBuilder.DefineField("__" + methodDefinition.methodName, fnType, FieldAttributes.Private | FieldAttributes.Static);
//         //
//         //             MethodAttributes methodAttributes = k_MethodAttributes;
//         //             
//         //             if (methodDefinition.isStatic) {
//         //                 methodAttributes |= MethodAttributes.Static;
//         //                 throw new CompileException("Static methods are not yet supported in Dynamic element types");
//         //             }
//         //             
//         //             MethodBuilder method = typeBuilder.DefineMethod(
//         //                 methodDefinition.methodName,
//         //                 methodAttributes,
//         //                 CallingConventions.Standard,
//         //                 returnType,
//         //                 signatureTypes
//         //             );
//         //
//         //             GenerateMethodCallIL(fnField, fnType.GetMethod("Invoke"), method.GetILGenerator(), staticSignature);
//         //
//         //             expressionData.Add(new ExpressionData() {
//         //                 body = methodDefinition.body,
//         //                 signature = signature,
//         //                 returnType = returnType,
//         //                 name = methodDefinition.methodName,
//         //                 isStatic = methodDefinition.isStatic
//         //             });
//         //         }
//         //     }
//         //
//         //     Type retn = typeBuilder.CreateType();
//         //     typeMap[id] = retn;
//         //
//         //     FieldInfo[] fieldInfos = retn.GetFields(BindingFlags.NonPublic | BindingFlags.Static);
//         //
//         //     TypeData typeData = default;
//         //     typeData.fieldData = new StructList<FieldData>();
//         //     typeData.methodData = new StructList<MethodData>();
//         //
//         //     if (methods != null) {
//         //
//         //         for (int i = 0; i < methods.Count; i++) {
//         //             linqCompiler.Reset();
//         //             ref ExpressionData exprData = ref expressionData.array[i];
//         //
//         //             LightList<Parameter> parameters = LightList<Parameter>.Get();
//         //
//         //
//         //             if (!exprData.isStatic) {
//         //                 parameters.Add(new Parameter(typeof(object), "__thisObject", ParameterFlags.NeverNull | ParameterFlags.NeverOutOfBounds));
//         //             }
//         //
//         //             for (int j = 0; j < exprData.signature.Length; j++) {
//         //                 ref ResolvedParameter parameter = ref exprData.signature[j];
//         //                 parameters.Add(new Parameter(parameter.type, parameter.name));
//         //             }
//         //
//         //             MethodData methodData = new MethodData() {
//         //                 isStatic = false,
//         //                 methodName = exprData.name,
//         //                 returnType = exprData.returnType,
//         //                 signature = exprData.signature
//         //             };
//         //                 
//         //             typeData.methodData.Add(methodData);
//         //             
//         //
//         //             linqCompiler.SetSignature(parameters, exprData.returnType);
//         //
//         //             if (!methodData.isStatic) {
//         //                 Expression castExpr = Expression.Convert(parameters[0], retn);
//         //                 ParameterExpression thisRef = linqCompiler.AddVariable(retn, "_this");
//         //                 linqCompiler.RawExpression(Expression.Assign(thisRef, castExpr));
//         //                 linqCompiler.SetImplicitContext(thisRef);
//         //             }
//         //             else {
//         //                 linqCompiler.SetImplicitStaticContext(retn);
//         //             }
//         //
//         //             linqCompiler.StatementList(exprData.body);
//         //
//         //             LambdaExpression lambda = linqCompiler.BuildLambda();
//         //
//         //             Delegate fn = lambda.Compile();
//         //
//         //             string fnName = "__" + exprData.name;
//         //
//         //             FieldData fieldData = new FieldData() {
//         //                 lambdaValue = lambda,
//         //                 fieldName = fnName
//         //             };
//         //
//         //             typeData.fieldData.Add(fieldData);
//         //             
//         //             FieldInfo fieldInfo = null;
//         //             for (int j = 0; j < fieldInfos.Length; j++) {
//         //                 if (fieldInfos[j].Name == "__" + exprData.name) {
//         //                     fieldInfo = fieldInfos[j];
//         //                     break;
//         //                 }
//         //             }
//         //
//         //             fieldInfo.SetValue(null, fn);
//         //         }
//         //     }
//         //
//         //     s_DynamicTypeData[retn] = typeData;
//         //     expressionData.Release();
//         //     return retn;
//         // }
//
//         public static TypeData GetDynamicTypeData(Type type) {
//             if (s_DynamicTypeData.TryGetValue(type, out TypeData retn)) {
//                 return retn;
//             }
//
//             return default;
//         }
//
//         public struct FieldData {
//
//             public string fieldName;
//             public LambdaExpression lambdaValue;
//
//         }
//
//         public struct MethodData {
//
//             public string methodName;
//             public Type returnType;
//             public bool isStatic;
//             public ResolvedParameter[] signature;
//
//         }
//         
//         public struct LambdaField {
//
//             public readonly string fnName;
//             public readonly LambdaExpression lambdaExpression;
//
//             public LambdaField(string fnName, LambdaExpression lambdaExpression) {
//                 this.fnName = fnName;
//                 this.lambdaExpression = lambdaExpression;
//             }
//
//         }
//
//         public struct TypeData {
//
//             public StructList<FieldData> fieldData;
//             public StructList<MethodData> methodData;
//
//             public FieldData GetFieldData(string fieldName) {
//                 if (fieldData == null) return default;
//
//                 for (int i = 0; i < fieldData.size; i++) {
//                     ref FieldData f = ref fieldData.array[i];
//                     if (f.fieldName == fieldName) {
//                         return f;
//                     }
//                 }
//
//                 return default;
//             }
//
//         }
//
//         private struct ExpressionData {
//
//             public ResolvedParameter[] signature;
//             public Type returnType;
//             public BlockNode body;
//             public string name;
//             public bool isStatic;
//
//         }
//
//         public struct ResolvedParameter {
//
//             public Type type;
//             public string name;
//
//         }
//
//         public Type GetCreatedType(string id) {
//             Type type = null;
//             typeMap.TryGetValue(id, out type);
//             return type;
//         }
//
//         public bool TryCreateInstance<T>(string id, out T instance) {
//             if (typeMap.TryGetValue(id, out Type toCreate)) {
//                 instance = (T) Activator.CreateInstance(toCreate);
//                 return true;
//             }
//
//             instance = default;
//             return false;
//         }
//
//     }
//
// }