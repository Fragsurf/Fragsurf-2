using System;
using System.Text;
using UIForia.Util;

namespace UIForia.Compilers {

    public static class TypeNameGenerator {

        public static string GetTypeName(Type type) {
            if (type == typeof(void)) return "void";
            StringBuilder builder = new StringBuilder();
            GetTypeName(type, builder);
            return builder.ToString();
        }
        
        public static void GetTypeName(Type type, StringBuilder builder, bool genericName = false) {
            if (type == null || type == typeof(void)) {
                builder.Append("void");
                return;
            }

            if (type.IsArray) {
                VisitArrayType(type, builder);
                return;
            }

            if (type.IsGenericParameter) {
                builder.Append(GetPrintableTypeName(type));
                return;
            }

            if (type.IsGenericType && type.IsGenericTypeDefinition) {
                VisitGenericTypeDefinition(type, builder);
                return;
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition) {
                VisitGenericTypeInstance(type, builder, genericName);
                return;
            }

            builder.Append(GetSimpleTypeName(type));
        }

        private static string CleanGenericName(Type type) {
            string name = GetPrintableTypeName(type);
            int position = name.LastIndexOf("`");
            if (position == -1) {
                return name;
            }

            return name.Substring(0, position);
        }

        private static string GetSimpleTypeName(Type type) {
            if (type == typeof(void))
                return "void";
            if (type == typeof(object))
                return "object";

            if (type.IsEnum) {
                return GetPrintableTypeName(type);
            }

            switch (Type.GetTypeCode(type)) {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Byte:
                    return "byte";
                case TypeCode.Char:
                    return "char";
                case TypeCode.Decimal:
                    return "decimal";
                case TypeCode.Double:
                    return "double";
                case TypeCode.Int16:
                    return "short";
                case TypeCode.Int32:
                    return "int";
                case TypeCode.Int64:
                    return "long";
                case TypeCode.SByte:
                    return "sbyte";
                case TypeCode.Single:
                    return "float";
                case TypeCode.String:
                    return "string";
                case TypeCode.UInt16:
                    return "ushort";
                case TypeCode.UInt32:
                    return "uint";
                case TypeCode.UInt64:
                    return "ulong";
                default:
                    return GetPrintableTypeName(type);
            }
        }

        private static string GetPrintableTypeName(Type type) {
            string typeName = type.FullName ?? type.Name;

            return typeName.Contains("+") 
                ? typeName.Replace("+", ".") 
                : typeName;
        }

        private static void VisitGenericTypeInstance(Type type, StringBuilder builder, bool genericName) {
            Type[] genericArguments = type.GetGenericArguments();
            int argIndx = 0;

            // namespace.basetype
            // for each type in chain that has generic arguments
            // replace `{arg count} with < ,? > until no more args
            // UIForia.Test.NamespaceTest.SomeNamespace.NamespaceTestClass+SubType1`1+NestedSubType1`1[System.Int32,System.Int32]

            if (type.IsNullableType()) {
                GetTypeName(type.GetGenericArguments()[0], builder);
                builder.Append("?");
                return;
            }

            string typeName = type.ToString();

            bool printedGenerics = false;
            for (int i = 0; i < typeName.Length; i++) {
                if (typeName[i] == '`') {
                    printedGenerics = true;
                    i++;
                    int count = int.Parse(typeName[i].ToString());
                    builder.Append("<");
                    for (int c = 0; c < count; c++) {
                        GetTypeName(genericArguments[argIndx++], builder);

                        if (c != count - 1) {
                            builder.Append(", ");
                        }
                    }

                    builder.Append(">");
                }
                else {
                    switch (typeName[i]) {
                        case '[':
                            if (printedGenerics) {
                                return;
                            }
                            // weird case where runtime generated type names are not prefixed with standard `x where x = generic parameter count
                            if (genericArguments.Length != 0) {
                                builder.Append("<");
                                if (genericName) {
                                    // todo -- wont handle things like Element<string, T> where generics are mixed with concrete 
                                    Type[] args = type.GetGenericTypeDefinition().GetGenericArguments();
                                    for (int c = 0; c < args.Length; c++) {
                                        builder.Append(args[c].Name);
                                        if (c != args.Length - 1) {
                                            builder.Append(", ");
                                        }
                                    }
                                }
                                else {
                                    for (int c = 0; c < genericArguments.Length; c++) {
                                        GetTypeName(genericArguments[c], builder);
                                        if (c != genericArguments.Length - 1) {
                                            builder.Append(", ");
                                        }
                                    }

                                }
                                builder.Append(">");
                            }
                            return;

                        case '+':
                            builder.Append(".");
                            break;

                        default:
                            builder.Append(typeName[i]);
                            break;
                    }

                }
            }
        }

        private static void VisitArrayType(Type type, StringBuilder builder) {
            GetTypeName(type.GetElementType(), builder);
            builder.Append("[");
            for (int i = 1; i < type.GetArrayRank(); i++) {
                builder.Append(",");
            }

            builder.Append("]");
        }

        private static void VisitGenericTypeDefinition(Type type, StringBuilder builder) {
            builder.Append(CleanGenericName(type));
            builder.Append("<");
            Type[] genericArguments = type.GetGenericArguments();
            
            for (int c = 0; c < genericArguments.Length; c++) {
                GetTypeName(genericArguments[c], builder);
                if (c != genericArguments.Length - 1) {
                    builder.Append(", ");
                }
            }
            
            //for (int i = 1; i < arity; i++) {
            //    builder.Append(",");
            //}

            builder.Append(">");
        }

    }

}