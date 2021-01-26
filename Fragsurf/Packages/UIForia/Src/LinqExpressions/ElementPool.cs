using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using UIForia.Elements;
using UIForia.Util;
using UnityEngine;

namespace UIForia.LinqExpressions {

    public class Gen<T> { }

    public static class LinqExpressions {

        private static readonly List<Expression> s_ExpressionList = new List<Expression>(24);

        private static object[] empty = { };

        public static object ResetInstance(object instance, ConstructorInfo constructorInfo, Action<object> clear) {
            clear(instance);
            return constructorInfo.Invoke(instance, empty);
        }

        public static object CreateInstance(object instance, Type type, Action<object> clear) {
            var constructor = type.GetConstructor(new Type[0]);
            if (constructor == null && !type.IsValueType) {
                throw new NotSupportedException($"Type '{type.FullName}' doesn't have a parameterless constructor");
            }

            if (instance != null) {
                clear(instance);
            }

            var emptyInstance = instance ?? FormatterServices.GetUninitializedObject(type);

            return constructor?.Invoke(emptyInstance, new object[0]) ?? emptyInstance;
        }
        
        public static object CreateInstance(Type type, ConstructorInfo constructor, object[] args) {
//            var emptyInstance = FormatterServices.GetUninitializedObject(type);

            return constructor.Invoke(args);
        }

        public static bool IsAutoProperty(PropertyInfo prop, FieldInfo[] fieldInfos) {
            if (!prop.CanWrite || !prop.CanRead) {
                return false;
            }

            string target = "<" + prop.Name + ">";

            for (int i = 0; i < fieldInfos.Length; i++) {
                if (fieldInfos[i].Name.StartsWith(target)) {
                    return true;
                }
            }

            return false;
        }

        public static void ClearEventInvocations(object obj, string eventName) {
            FieldInfo fi = GetEventField(obj.GetType(), eventName);
            if (fi == null) return;
            fi.SetValue(obj, null);
        }

        private static FieldInfo GetEventField(Type type, string eventName) {
            FieldInfo field = null;
            while (type != null) {
                /* Find events defined as field */
                field = type.GetField(eventName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null && (field.FieldType == typeof(MulticastDelegate) || field.FieldType.IsSubclassOf(typeof(MulticastDelegate))))
                    break;

                /* Find events defined as property { add; remove; } */
                field = type.GetField("EVENT_" + eventName.ToUpper(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null)
                    break;
                type = type.BaseType;
            }

            return field;
        }

        private static readonly MethodInfo s_FieldSetValue = typeof(FieldInfo).GetMethod(nameof(FieldInfo.SetValue), new[] {typeof(object), typeof(object)});

        // todo -- we need to know if this being compiled for dev or not.
        // In production mode we generate valid C# code and thus need to know how to assign
        // to auto properties (direct assign if public, reflection if not) & clear events (reflection)
        public static Action<object> CompileClear(Type type) {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "element");

            UnaryExpression converted = Expression.Convert(parameterExpression, type);

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            Assembly assembly = typeof(Expression).Assembly;
            Type cheating = assembly.GetType("System.Linq.Expressions.AssignBinaryExpression");
            // this is bypassing the IsInitOnly check done internally by Expression.Assign
            var ctor = cheating.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance)[0];
//            ctor.GetMethodBody().LocalVariables[0
            for (int i = 0; i < fields.Length; i++) {
                Type fieldType = fields[i].FieldType;
                if (fields[i].IsInitOnly) {
//                    object defaultValue = fieldType.IsValueType ? Activator.CreateInstance(fieldType) : null;
//                    // use reflection to set the value since the compiler will complain about setting a read only field
//                    s_ExpressionList.Add(Expression.Call(
//                        Expression.Constant(fields[i]),
//                        s_FieldSetValue,
//                        parameterExpression,
//                        Expression.Convert(Expression.Constant(defaultValue), typeof(object)))
//                    );
                    // compiler will bitch about this if we actually generate code that tries to assign to read only fields....
                    // for dev and non AOT platforms this is perfectly fine to do.
                    var ohSoBad = CreateInstance(cheating, ctor, new object[] {
                        Expression.Field(converted, fields[i]),
                        Expression.Default(fieldType)
                    });
                    s_ExpressionList.Add((BinaryExpression)ohSoBad);
                }
                else {
                    s_ExpressionList.Add(Expression.Assign(Expression.Field(converted, fields[i]), Expression.Default(fieldType)));
                }
            }

            Expression blockExpr = Expression.Block(s_ExpressionList);
            s_ExpressionList.Clear();

            return Expression.Lambda<Action<object>>(blockExpr, parameterExpression).Compile();
        }

    }

}