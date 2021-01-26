//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using UIForia.Compilers;
//using UIForia.Compilers.ExpressionResolvers;
//using UIForia.Exceptions;
//using UIForia.Expressions;
//using UnityEngine;
//
//#pragma warning disable 0660
//#pragma warning disable 0661
//#pragma warning disable 0649
//
//[TestFixture]
//public class ExpressionCompilerTests {
//
//    public class Thing {
//
//        public float value;
//
//        public Thing(float value) {
//            this.value = value;
//        }
//
//        public string Stuff() {
//            return value.ToString();
//        }
//
//        public override string ToString() {
//            return value.ToString();
//        }
//
//    }
//
//    public class Indexable {
//
//        public string[] val;
//
//        public string this[int i] {
//            get => val[i];
//            set => val[i] = value;
//        }
//
//    }
//
//    private class TestType0 {
//
//        public const string ConstantFieldValue = "some constant value";
//        public ConvertThing convertThing;
//        public string value;
//        public int ValueProp { get; set; }
//        public object obj1;
//        public object obj2;
//        public List<Vector3> vectors;
//        public List<Thing> things;
//        public float[] floats;
//        public Indexable indexable;
//        public Thing thing;
//
//        public Thing thingProp { get; set; }
//
//        public Vector3 structThing;
//        public readonly float notWritable;
//
//        public float nonWritableProper {
//            set { }
//        }
//
//        public string StringValueProp => value;
//
//        public string GetValue() {
//            return value;
//        }
//
//        public string GetValue(string val) {
//            return value + val;
//        }
//
//        public string GetValue(string val, string val1) {
//            return value + val + val1;
//        }
//
//        public string GetValue(string val, string val1, string val2) {
//            return value + val + val1 + val2;
//        }
//
//
//        public string GetValue(string val, string val1, string val2, string val3) {
//            return value + val + val1 + val2 + val3;
//        }
//
//        public string GetValue(string val, string val1, string val2, string val3, string val4) {
//            return value + val + val1 + val2 + val3 + val4;
//        }
//
//        public Func<string, string> GetSomeFunc() {
//            return (string str) => str;
//        }
//
//        public ConvertThing ConvertThingProperty {
//            get => convertThing;
//            set => convertThing = value;
//        }
//
//    }
//
//    public class TestType1 {
//
//        public static float s_FloatVal;
//
//    }
//
//    [Test]
//    public void Compile_BasicField() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expr = compiler.Compile<string>(typeof(TestType0), "value");
//        Assert.AreEqual("Matt", expr.Evaluate(new ExpressionContext(new TestType0() {value = "Matt"})));
//    }
//
//    [Test]
//    public void Compile_BasicField_Linq() {
//        ExpressionCompiler compiler = new ExpressionCompiler(allowLinq: true);
//        Expression<string> expr = compiler.Compile<string>(typeof(TestType0), "value");
//        Assert.AreEqual("Matt", expr.Evaluate(new ExpressionContext(new TestType0() {value = "Matt"})));
//    }
//
//    [Test]
//    public void Compile_BasicProperty() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<int> expr = compiler.Compile<int>(typeof(TestType0), "ValueProp");
//        int value = expr.Evaluate(
//            new ExpressionContext(
//                new TestType0() {
//                    ValueProp = 11
//                }
//            )
//        );
//        Assert.AreEqual(11, value);
//    }
//
//    [Test]
//    public void Compile_BasicProperty_Linq() {
//        ExpressionCompiler compiler = new ExpressionCompiler(true);
//        Expression<int> expr = compiler.Compile<int>(typeof(TestType0), "ValueProp");
//        int value = expr.Evaluate(
//            new ExpressionContext(
//                new TestType0() {
//                    ValueProp = 11
//                }
//            )
//        );
//        Assert.AreEqual(11, value);
//    }
//
//    [Test]
//    public void Compile_ImplicitlyCastProperty() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<float> expr = compiler.Compile<float>(typeof(TestType0), "ValueProp");
//        float value = expr.Evaluate(
//            new ExpressionContext(
//                new TestType0() {
//                    ValueProp = 11
//                }
//            )
//        );
//        Assert.AreEqual(11, value);
//    }
//
//    private class TestAliasResolver<T> : ExpressionAliasResolver {
//
//        public T value;
//
//        public TestAliasResolver(string aliasName, T value) : base(aliasName) {
//            this.value = value;
//        }
//
//        public override Expression CompileAsValueExpression(CompilerContext context) {
//            return new ConstantExpression<T>(value);
//        }
//
//    }
//
//    private struct ConvertThing {
//
//        public float floatVal;
//        public int intVal;
//        public double doubleVal;
//        public string stringVal;
//        public bool boolVal;
//        public static float s_StaticVal = 0;
//
//        public Vector3 vectorProp { get; set; }
//
//        public static implicit operator ConvertThing(bool f) {
//            return new ConvertThing() {boolVal = f};
//        }
//
//        public static implicit operator ConvertThing(string f) {
//            return new ConvertThing() {stringVal = f};
//        }
//
//        public static implicit operator ConvertThing(float f) {
//            return new ConvertThing() {floatVal = f};
//        }
//
//        public static implicit operator ConvertThing(int f) {
//            return new ConvertThing() {intVal = f};
//        }
//
//        public static implicit operator ConvertThing(double f) {
//            return new ConvertThing() {doubleVal = f};
//        }
//
//        public static ConvertThing operator +(ConvertThing a, float b) {
//            a.floatVal += b;
//            return a;
//        }
//
//        public static bool operator >(ConvertThing a, int b) {
//            return a.intVal > b;
//        }
//
//        public static bool operator <(ConvertThing a, int b) {
//            return a.intVal < b;
//        }
//
//
//        public static bool operator >(ConvertThing a, float b) {
//            return a.floatVal > b;
//        }
//
//        public static bool operator <(ConvertThing a, float b) {
//            return a.floatVal < b;
//        }
//
//        public static bool operator ==(ConvertThing a, float b) {
//            return a.floatVal == b;
//        }
//
//        public static bool operator !=(ConvertThing a, float b) {
//            return !(a == b);
//        }
//
//        public static string operator !(ConvertThing a) {
//            return a.intVal + a.stringVal;
//        }
//
//        public static float operator -(ConvertThing a) {
//            return -a.floatVal;
//        }
//
//    }
//
//    [Test]
//    public void Compile_AliasAsValue() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        compiler.AddAliasResolver(new TestAliasResolver<float>("$alias", 5f));
//        Expression<float> expr = compiler.Compile<float>(typeof(TestType0), "$alias");
//        float value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(5f, value);
//    }
//
//    [Test]
//    public void Compile_AliasAsValue_Cast() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        compiler.AddAliasResolver(new TestAliasResolver<int>("$alias", 5));
//        Expression<float> expr = compiler.Compile<float>(typeof(TestType0), "$alias");
//        float value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(5f, value);
//    }
//
//    [Test]
//    public void Compile_IntConstant() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<int> expr = compiler.Compile<int>(typeof(TestType0), "5");
//        int value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(5, value);
//    }
//
//    [Test]
//    public void Compile_FloatConstant() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<float> expr = compiler.Compile<float>(typeof(TestType0), "5");
//        float value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(5f, value);
//    }
//
//    [Test]
//    public void Compile_DoubleConstant() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<double> expr = compiler.Compile<double>(typeof(TestType0), "5");
//        double value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(5.0, value);
//    }
//
//    [Test]
//    public void Compile_StringConstant() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expr = compiler.Compile<string>(typeof(TestType0), "'matt'");
//        string value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual("matt", value);
//    }
//
//    [Test]
//    public void Compile_BooleanConstant() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "false");
//        bool value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(false, value);
//        expr = compiler.Compile<bool>(typeof(TestType0), "true");
//        value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(true, value);
//    }
//
//    [Test]
//    public void Compile_ImplicitlyConvertibleStringConstant() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<ConvertThing> expr = compiler.Compile<ConvertThing>(typeof(TestType0), "'matt'");
//        ConvertThing value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual("matt", value.stringVal);
//    }
//
//    [Test]
//    public void Compile_ImplicitlyConvertibleFloatConst() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<ConvertThing> expr = compiler.Compile<ConvertThing>(typeof(TestType0), "5.1f");
//        ConvertThing value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(5.1f, value.floatVal);
//    }
//
//    [Test]
//    public void Compile_ImplicitlyConvertibleBoolConst() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<ConvertThing> expr = compiler.Compile<ConvertThing>(typeof(TestType0), "true");
//        ConvertThing value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(true, value.boolVal);
//    }
//
//    [Test]
//    public void Compile_ImplicitlyConvertibleIntConst() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<ConvertThing> expr = compiler.Compile<ConvertThing>(typeof(TestType0), "5");
//        ConvertThing value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(5, value.intVal);
//    }
//
//    [Test]
//    public void Compile_ImplicitlyConvertibleDoubleConst() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<ConvertThing> expr = compiler.Compile<ConvertThing>(typeof(TestType0), "5.12");
//        ConvertThing value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(5.12, value.doubleVal);
//    }
//
//    [Test]
//    public void Compile_BasicTypeOf() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<Type> expr = compiler.Compile<Type>(typeof(TestType0), "typeof(string)");
//        Type value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(typeof(string), value);
//    }
//
//    [Test]
//    public void Compile_SimpleBinaryAddition() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<float> expr = compiler.Compile<float>(typeof(TestType0), "5f + 5f");
//        float value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(10f, value);
//        expr = compiler.Compile<float>(typeof(TestType0), "5f + 5");
//        value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(10f, value);
//    }
//
//    [Test]
//    public void Compile_ImplicitCastBinaryAddition() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<ConvertThing> expr = compiler.Compile<ConvertThing>(typeof(TestType0), "5f + 5f");
//        ConvertThing value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual(10, value.floatVal);
//    }
//
//    [Test]
//    public void Compile_ImplicitCastOverloadedAddition() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<ConvertThing> expr = compiler.Compile<ConvertThing>(typeof(TestType0), "convertThing + 5f");
//        TestType0 t0 = new TestType0();
//        t0.convertThing = 5f;
//        ConvertThing value = expr.Evaluate(new ExpressionContext(t0));
//        Assert.AreEqual(10, value.floatVal);
//    }
//
//    [Test]
//    public void Compile_LiteralStringConcat() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expr = compiler.Compile<string>(null, "'string1' + 'string2'");
//        string value = expr.Evaluate(new ExpressionContext(null));
//        Assert.AreEqual("string1string2", value);
//        Assert.IsTrue(expr.IsConstant());
//    }
//
//    [Test]
//    public void Compile_FieldStringConcat() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expr = compiler.Compile<string>(typeof(TestType0), "value + 'string2'");
//        TestType0 t0 = new TestType0();
//        t0.value = "string1";
//        string value = expr.Evaluate(new ExpressionContext(t0));
//        Assert.AreEqual("string1string2", value);
//    }
//
//    [Test]
//    public void Compile_NumericComparison() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "5 > 6");
//        Assert.IsFalse(expr.Evaluate(new ExpressionContext(null)));
//
//        expr = compiler.Compile<bool>(typeof(TestType0), "5 >= 6");
//        Assert.IsFalse(expr.Evaluate(new ExpressionContext(null)));
//
//        expr = compiler.Compile<bool>(typeof(TestType0), "5 < 6");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(null)));
//
//        expr = compiler.Compile<bool>(typeof(TestType0), "5 <= 6");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(null)));
//
//        expr = compiler.Compile<bool>(typeof(TestType0), "5 <= 5");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(null)));
//
//        expr = compiler.Compile<bool>(typeof(TestType0), "5 <= 3");
//        Assert.IsFalse(expr.Evaluate(new ExpressionContext(null)));
//    }
//
//    [Test]
//    public void Compile_ComparisonOverload() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        TestType0 t0 = new TestType0();
//        t0.convertThing.intVal = 41;
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "convertThing > 6");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(t0)));
//
//        expr = compiler.Compile<bool>(typeof(TestType0), "convertThing > 6");
//        t0.convertThing.intVal = -1;
//        Assert.IsFalse(expr.Evaluate(new ExpressionContext(t0)));
//
//        t0.convertThing.floatVal = 81.5f;
//        expr = compiler.Compile<bool>(typeof(TestType0), "convertThing > 6f");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(t0)));
//
//        t0.convertThing.floatVal = -41;
//        expr = compiler.Compile<bool>(typeof(TestType0), "convertThing > 6f");
//        Assert.IsFalse(expr.Evaluate(new ExpressionContext(t0)));
//    }
//
//    [Test]
//    public void Compile_BooleanEquality() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "true == true");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(null)));
//    }
//
//    [Test]
//    public void Compile_NumericEquality() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "5 == 5");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(null)));
//    }
//
//    [Test]
//    public void Compile_StringEquality() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "'x' == 'x'");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(null)));
//    }
//
//    [Test]
//    public void Compile_ObjectEquality() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        TestType0 t0 = new TestType0();
//        t0.obj1 = new object[0];
//        t0.obj2 = t0.obj1;
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "obj1 == obj2");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(t0)));
//    }
//
//    [Test]
//    public void Compile_BooleanInequality() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "false != true");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(null)));
//    }
//
//    [Test]
//    public void Compile_NumericInequality() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "5 != 5");
//        Assert.IsFalse(expr.Evaluate(new ExpressionContext(null)));
//    }
//
//    [Test]
//    public void Compile_StringInequality() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "'x' != 'x'");
//        Assert.IsFalse(expr.Evaluate(new ExpressionContext(null)));
//    }
//
//    [Test]
//    public void Compile_ObjectInequality() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        TestType0 t0 = new TestType0();
//        t0.obj1 = new object[0];
//        t0.obj2 = t0.obj1;
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "obj1 != obj2");
//        Assert.IsFalse(expr.Evaluate(new ExpressionContext(t0)));
//    }
//
//    [Test]
//    public void Compile_OverloadedEquality() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        TestType0 t0 = new TestType0();
//        t0.convertThing.floatVal = 10f;
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "convertThing == 10f");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(t0)));
//    }
//
//    [Test]
//    public void Compile_OverloadedInequality() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        TestType0 t0 = new TestType0();
//        t0.convertThing.floatVal = 10f;
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "convertThing != 90f");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(t0)));
//    }
//
//    [Test]
//    public void Compile_EqualNull() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        TestType0 t0 = new TestType0();
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "value == null");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(t0)));
//    }
//
//    [Test]
//    public void Compile_NotEqualNull() {
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        TestType0 t0 = new TestType0();
//        t0.value = "hello";
//        Expression<bool> expr = compiler.Compile<bool>(typeof(TestType0), "value != null");
//        Assert.IsTrue(expr.Evaluate(new ExpressionContext(t0)));
//    }
//
//    [Test]
//    public void Compile_AndOrBoolBool() {
//        TestType0 target = new TestType0();
//        target.obj1 = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expression = compiler.Compile<bool>(typeof(TestType0), "true && true");
//        Assert.IsInstanceOf<OperatorExpression_AndOrBool>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        expression = compiler.Compile<bool>(typeof(TestType0), "true && false");
//        Assert.IsInstanceOf<OperatorExpression_AndOrBool>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//
//        expression = compiler.Compile<bool>(typeof(TestType0), "false && true");
//        Assert.IsInstanceOf<OperatorExpression_AndOrBool>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//
//        expression = compiler.Compile<bool>(typeof(TestType0), "false && false");
//        Assert.IsInstanceOf<OperatorExpression_AndOrBool>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//
//        expression = compiler.Compile<bool>(typeof(TestType0), "true || true");
//        Assert.IsInstanceOf<OperatorExpression_AndOrBool>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        expression = compiler.Compile<bool>(typeof(TestType0), "true || false");
//        Assert.IsInstanceOf<OperatorExpression_AndOrBool>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        expression = compiler.Compile<bool>(typeof(TestType0), "false || true");
//        Assert.IsInstanceOf<OperatorExpression_AndOrBool>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        expression = compiler.Compile<bool>(typeof(TestType0), "false || false");
//        Assert.IsInstanceOf<OperatorExpression_AndOrBool>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AndOrObjectBool() {
//        TestType0 target = new TestType0();
//        target.obj1 = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expression = compiler.Compile<bool>(typeof(TestType0), "obj1 && true");
//        Assert.IsInstanceOf<OperatorExpression_AndOrObjectBool<object>>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        target.obj1 = new TestType0();
//        expression = compiler.Compile<bool>(typeof(TestType0), "obj1 && false");
//        Assert.IsInstanceOf<OperatorExpression_AndOrObjectBool<object>>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//
//        target.obj1 = null;
//        expression = compiler.Compile<bool>(typeof(TestType0), "obj1 && true");
//        Assert.IsInstanceOf<OperatorExpression_AndOrObjectBool<object>>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//
//        target.obj1 = null;
//        expression = compiler.Compile<bool>(typeof(TestType0), "obj1 && false");
//        Assert.IsInstanceOf<OperatorExpression_AndOrObjectBool<object>>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//
//        target.obj1 = new TestType0();
//        expression = compiler.Compile<bool>(typeof(TestType0), "obj1 || true");
//        Assert.IsInstanceOf<OperatorExpression_AndOrObjectBool<object>>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        target.obj1 = new TestType0();
//        expression = compiler.Compile<bool>(typeof(TestType0), "obj1 || false");
//        Assert.IsInstanceOf<OperatorExpression_AndOrObjectBool<object>>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        target.obj1 = null;
//        expression = compiler.Compile<bool>(typeof(TestType0), "obj1 || true");
//        Assert.IsInstanceOf<OperatorExpression_AndOrObjectBool<object>>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        target.obj1 = null;
//        expression = compiler.Compile<bool>(typeof(TestType0), "obj1 || false");
//        Assert.IsInstanceOf<OperatorExpression_AndOrObjectBool<object>>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AndOrBoolObject() {
//        TestType0 target = new TestType0();
//        target.obj1 = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expression = compiler.Compile<bool>(typeof(TestType0), "true && obj1");
//        Assert.IsInstanceOf<OperatorExpression_AndOrBoolObject<object>>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        target.obj1 = null;
//        expression = compiler.Compile<bool>(typeof(TestType0), ("true && obj1"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrBoolObject<object>>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//
//        target.obj1 = new TestType0();
//        expression = compiler.Compile<bool>(typeof(TestType0), ("false && obj1"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrBoolObject<object>>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//
//        target.obj1 = null;
//        expression = compiler.Compile<bool>(typeof(TestType0), ("false && obj1"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrBoolObject<object>>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//
//        target.obj1 = new TestType0();
//        expression = compiler.Compile<bool>(typeof(TestType0), ("true || obj1"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrBoolObject<object>>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        target.obj1 = null;
//        expression = compiler.Compile<bool>(typeof(TestType0), ("true || obj1"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrBoolObject<object>>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        target.obj1 = new TestType0();
//        expression = compiler.Compile<bool>(typeof(TestType0), ("false || obj1"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrBoolObject<object>>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        target.obj1 = null;
//        expression = compiler.Compile<bool>(typeof(TestType0), ("false || obj1"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrBoolObject<object>>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AndOrObjectObject() {
//        TestType0 target = new TestType0();
//        target.obj1 = new TestType0();
//        target.obj2 = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expression = compiler.Compile<bool>(typeof(TestType0), "obj1 && obj2");
//        Assert.IsInstanceOf<OperatorExpression_AndOrObject<object, object>>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        target.obj1 = new TestType0();
//        target.obj2 = null;
//        expression = compiler.Compile<bool>(typeof(TestType0), ("obj1 && obj2"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrObject<object, object>>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//
//        target.obj1 = null;
//        target.obj2 = new TestType0();
//        expression = compiler.Compile<bool>(typeof(TestType0), ("obj1 && obj2"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrObject<object, object>>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//
//        target.obj1 = null;
//        target.obj2 = null;
//        expression = compiler.Compile<bool>(typeof(TestType0), ("obj1 && obj2"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrObject<object, object>>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//
//        target.obj1 = new TestType0();
//        target.obj2 = new TestType0();
//        expression = compiler.Compile<bool>(typeof(TestType0), ("obj1 || obj2"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrObject<object, object>>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        target.obj1 = new TestType0();
//        target.obj2 = null;
//        expression = compiler.Compile<bool>(typeof(TestType0), ("obj1 || obj2"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrObject<object, object>>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        target.obj1 = null;
//        target.obj2 = new TestType0();
//        expression = compiler.Compile<bool>(typeof(TestType0), ("obj1 || obj2"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrObject<object, object>>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//
//        target.obj1 = null;
//        target.obj2 = null;
//        expression = compiler.Compile<bool>(typeof(TestType0), ("obj1 || obj2"));
//        Assert.IsInstanceOf<OperatorExpression_AndOrObject<object, object>>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//    }
//
//
//    [Test]
//    public void Compile_StringNotWithNull() {
//        TestType0 target = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expression = compiler.Compile<bool>(typeof(TestType0), "!value");
//        Assert.IsInstanceOf<UnaryExpression_StringBoolean>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_StringNotWithEmpty() {
//        TestType0 target = new TestType0();
//        target.value = string.Empty;
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expression = compiler.Compile<bool>(typeof(TestType0), "!value");
//        Assert.IsInstanceOf<UnaryExpression_StringBoolean>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_StringNotWithValue() {
//        TestType0 target = new TestType0();
//        target.value = "yup";
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expression = compiler.Compile<bool>(typeof(TestType0), "!value");
//        Assert.IsInstanceOf<UnaryExpression_StringBoolean>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_ObjectNotWithNull() {
//        TestType0 target = new TestType0();
//        target.obj1 = null;
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expression = compiler.Compile<bool>(typeof(TestType0), "!obj1");
//        Assert.IsInstanceOf<UnaryExpression_ObjectBoolean>(expression);
//        Assert.AreEqual(true, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_ObjectNotWithValue() {
//        TestType0 target = new TestType0();
//        target.obj1 = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<bool> expression = compiler.Compile<bool>(typeof(TestType0), "!obj1");
//        Assert.IsInstanceOf<UnaryExpression_ObjectBoolean>(expression);
//        Assert.AreEqual(false, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_OverloadedNot() {
//        TestType0 target = new TestType0();
//        target.convertThing.intVal = 5;
//        target.convertThing.stringVal = "times";
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expression = compiler.Compile<string>(typeof(TestType0), "!convertThing");
//        Assert.AreEqual("5times", expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_UnaryMinusNumeric() {
//        TestType0 target = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<float> expression = compiler.Compile<float>(typeof(TestType0), "-(5f)");
//        Assert.IsInstanceOf<UnaryExpression_Minus_Float>(expression);
//        Assert.AreEqual(-5f, expression.Evaluate(ctx));
//
//        Expression<double> expressionD = compiler.Compile<double>(typeof(TestType0), "-(5.0)");
//        Assert.IsInstanceOf<UnaryExpression_Minus_Double>(expressionD);
//        Assert.AreEqual(-5.0, expressionD.Evaluate(ctx));
//
//        Expression<int> expressionI = compiler.Compile<int>(typeof(TestType0), "-(5)");
//        Assert.IsInstanceOf<UnaryExpression_Minus_Int>(expressionI);
//        Assert.AreEqual(-5, expressionI.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_UnaryMinusOverload() {
//        TestType0 target = new TestType0();
//        target.convertThing.floatVal = 5;
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<float> expression = compiler.Compile<float>(typeof(TestType0), "-convertThing");
//        Assert.AreEqual(-5f, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_Field() {
//        TestType0 target = new TestType0();
//        target.convertThing.floatVal = 5;
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<float> expression = compiler.Compile<float>(typeof(TestType0), "convertThing.floatVal");
//        Assert.AreEqual(5f, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_Property() {
//        TestType0 target = new TestType0();
//        target.convertThing.vectorProp = new Vector3(5, 5, 5);
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<Vector3> expression = compiler.Compile<Vector3>(typeof(TestType0), "convertThing.vectorProp");
//        Assert.AreEqual(new Vector3(5, 5, 5), expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_PropertyHead() {
//        TestType0 target = new TestType0();
//        target.convertThing.vectorProp = new Vector3(5, 5, 5);
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<float> expression = compiler.Compile<float>(typeof(TestType0), "ConvertThingProperty.vectorProp.x");
//        Assert.AreEqual(5f, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_StaticField() {
//        TestType0 target = new TestType0();
//        ConvertThing.s_StaticVal = 11f;
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        compiler.AddNamespace("UnityEngine");
//        Expression<float> expression = compiler.Compile<float>(typeof(TestType0), "ConvertThingProperty.s_StaticVal");
//        Assert.AreEqual(11f, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_StaticTypeRefProperty() {
//        TestType0 target = new TestType0();
//        ConvertThing.s_StaticVal = 11f;
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        compiler.AddNamespace("UnityEngine");
//        Expression<Vector3> expression = compiler.Compile<Vector3>(typeof(TestType0), "Vector3.up");
//        Assert.AreEqual(new Vector3(0, 1, 0), expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_StaticTypeRefField() {
//        TestType0 target = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        TestType1.s_FloatVal = 6;
//        Expression<float> expression = compiler.Compile<float>(typeof(TestType0), "TestType1.s_FloatVal");
//        Assert.AreEqual(6f, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_ListIndex() {
//        TestType0 target = new TestType0();
//        target.vectors = new List<Vector3>();
//        target.vectors.Add(Vector2.up);
//        target.vectors.Add(Vector2.down);
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<Vector3> expression = compiler.Compile<Vector3>(typeof(TestType0), "vectors[1]");
//        Assert.AreEqual(new Vector3(0, -1, 0), expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_ArrayIndex() {
//        TestType0 target = new TestType0();
//        target.floats = new float[2];
//        target.floats[0] = 11f;
//        target.floats[1] = 12f;
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<float> expression = compiler.Compile<float>(typeof(TestType0), "floats[1]");
//        Assert.AreEqual(12f, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_IndexList_Field() {
//        TestType0 target = new TestType0();
//        target.vectors = new List<Vector3>();
//        target.vectors.Add(Vector2.up);
//        target.vectors.Add(Vector2.down);
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<float> expression = compiler.Compile<float>(typeof(TestType0), "vectors[1].y");
//        Assert.AreEqual(-1f, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_IndexList_ExpressionIndex() {
//        TestType0 target = new TestType0();
//        target.convertThing.intVal = 2;
//        target.vectors = new List<Vector3>();
//        target.vectors.Add(Vector2.up);
//        target.vectors.Add(Vector2.down);
//        target.vectors.Add(Vector2.right);
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<float> expression = compiler.Compile<float>(typeof(TestType0), "vectors[convertThing.intVal].x");
//        Assert.AreEqual(1f, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_IndexList_ExpressionIndexOutOfBounds() {
//        TestType0 target = new TestType0();
//        target.convertThing.intVal = 2;
//        target.vectors = new List<Vector3>();
//        target.vectors.Add(Vector2.up);
//        target.vectors.Add(Vector2.down);
//        target.vectors.Add(Vector2.right);
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<float> expression = compiler.Compile<float>(typeof(TestType0), "vectors[10].x");
//        Assert.AreEqual(0f, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_TernaryExpression_Literals() {
//        TestType0 target = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<int> expression = compiler.Compile<int>(typeof(TestType0), " 1 > 2 ? 5 : 6");
//        Assert.IsInstanceOf<OperatorExpression_Ternary<int>>(expression);
//        Assert.AreEqual(6, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_TernaryExpression_Lookup() {
//        TestType0 target = new TestType0();
//        target.value = "matt";
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<int> expression = compiler.Compile<int>(typeof(TestType0), " value.Length > 2 ? 5 : 6");
//        Assert.IsInstanceOf<OperatorExpression_Ternary<int>>(expression);
//        Assert.AreEqual(5, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_Method0Args() {
//        TestType0 target = new TestType0();
//        target.value = "0";
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expression = compiler.Compile<string>(typeof(TestType0), "GetValue()");
//        Assert.IsInstanceOf<AccessExpression<string, TestType0>>(expression);
//        Assert.AreEqual("0", expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_Method1Arg() {
//        TestType0 target = new TestType0();
//        target.value = "0";
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expression = compiler.Compile<string>(typeof(TestType0), "GetValue('1')");
//        Assert.IsInstanceOf<AccessExpression<string, TestType0>>(expression);
//        Assert.AreEqual("01", expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_Method2Args() {
//        TestType0 target = new TestType0();
//        target.value = "0";
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expression = compiler.Compile<string>(typeof(TestType0), "GetValue('1', '2')");
//        Assert.IsInstanceOf<AccessExpression<string, TestType0>>(expression);
//        Assert.AreEqual("012", expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_Method3Args() {
//        TestType0 target = new TestType0();
//        target.value = "0";
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expression = compiler.Compile<string>(typeof(TestType0), "GetValue('1', '2', '3')");
//        Assert.IsInstanceOf<AccessExpression<string, TestType0>>(expression);
//        Assert.AreEqual("0123", expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_Method4Args() {
//        TestType0 target = new TestType0();
//        target.value = "0";
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expression = compiler.Compile<string>(typeof(TestType0), "GetValue('1', '2', '3', '4')");
//        Assert.IsInstanceOf<AccessExpression<string, TestType0>>(expression);
//        Assert.AreEqual("01234", expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_Method5Args() {
//        CompileException ex = Assert.Throws<CompileException>(() => { new ExpressionCompiler().Compile<string>(typeof(TestType0), "GetValue('1', '2', '3', '4', '5')"); });
////        Assert.AreEqual(CompileExceptions.TooManyArgumentsException("GetValue", 5).Message, ex.Message);
//    }
//
//    [Test]
//    public void Compile_AccessExpression_MethodChained() {
//        TestType0 target = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expression = compiler.Compile<string>(typeof(TestType0), "GetSomeFunc()('funky')");
//        Assert.IsInstanceOf<AccessExpression<string, TestType0>>(expression);
//        Assert.AreEqual("funky", expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_MethodOnField() {
//        TestType0 target = new TestType0();
//        target.value = "matt";
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expression = compiler.Compile<string>(typeof(TestType0), "value.ToString()");
//        Assert.IsInstanceOf<AccessExpression<string, TestType0>>(expression);
//        Assert.AreEqual("matt", expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_MethodOnProperty() {
//        TestType0 target = new TestType0();
//        target.value = "matt";
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expression = compiler.Compile<string>(typeof(TestType0), "StringValueProp.ToString()");
//        Assert.IsInstanceOf<AccessExpression<string, TestType0>>(expression);
//        Assert.AreEqual("matt", expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_MethodOnIndex() {
//        TestType0 target = new TestType0();
//        target.things = new List<Thing>();
//        target.things.Add(new Thing(5));
//        target.things.Add(new Thing(6));
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expression = compiler.Compile<string>(typeof(TestType0), "things[1].Stuff()");
//        Assert.IsInstanceOf<AccessExpression<string, TestType0>>(expression);
//        Assert.AreEqual((6f).ToString(), expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_AccessExpression_ConstantValue() {
//        TestType0 target = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string> expression = compiler.Compile<string>(typeof(TestType0), "ConstantFieldValue");
//        Assert.IsInstanceOf<ConstantExpression<string>>(expression);
//        Assert.AreEqual(TestType0.ConstantFieldValue, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void Compile_ListLiteral() {
//        TestType0 target = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        Expression<string[]> expression = compiler.Compile<string[]>(typeof(TestType0), "['a', 'b', 'c']");
//        Assert.IsInstanceOf<ArrayLiteralExpression<string>>(expression);
//        Assert.AreEqual(new[] {"a", "b", "c"}, expression.Evaluate(ctx));
//    }
//
//    [Test]
//    public void CompileWriteTarget_RootField() {
//        TestType0 target = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        WriteTargetExpression<string> expression = compiler.CompileWriteTarget<string>(typeof(TestType0), "value");
//        expression.Assign(ctx, "this is a new value");
//        Assert.AreEqual(target.value, "this is a new value");
//    }
//
//    [Test]
//    public void CompileWriteTarget_RootProperty() {
//        TestType0 target = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        WriteTargetExpression<int> expression = compiler.CompileWriteTarget<int>(typeof(TestType0), "ValueProp");
//        Assert.AreEqual(target.ValueProp, 0);
//        expression.Assign(ctx, 1111);
//        Assert.AreEqual(target.ValueProp, 1111);
//    }
//
//    [Test]
//    public void CompileWriteTarget_ArrayIndexedField() {
//        TestType0 target = new TestType0();
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        target.floats = new float[3];
//        WriteTargetExpression<float> expression = compiler.CompileWriteTarget<float>(typeof(TestType0), "floats[1]");
//        Assert.AreEqual(target.floats[1], 0f);
//        expression.Assign(ctx, 3f);
//        Assert.AreEqual(target.floats[1], 3f);
//    }
//
//    [Test]
//    public void CompileWriteTarget_IndexedField() {
//        TestType0 target = new TestType0();
//        target.indexable = new Indexable();
//        target.indexable.val = new string[] {"hi", "there", "guten tag"};
//
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        WriteTargetExpression<string> expression = compiler.CompileWriteTarget<string>(typeof(TestType0), "indexable[1]");
//        Assert.AreEqual("there", target.indexable[1]);
//        expression.Assign(ctx, "here");
//        Assert.AreEqual("here", target.indexable[1]);
//    }
//
//    [Test]
//    public void CompileWriteTarget_DotFieldAccess() {
//        TestType0 target = new TestType0();
//        target.thing = new Thing(0);
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        WriteTargetExpression<float> expression = compiler.CompileWriteTarget<float>(typeof(TestType0), "thing.value");
//        Assert.AreEqual(0, target.thing.value);
//        expression.Assign(ctx, 12f);
//        Assert.AreEqual(12f, target.thing.value);
//    }
//
//    [Test]
//    public void CompileWriteTarget_DotPropertyAccess() {
//        TestType0 target = new TestType0();
//        target.thingProp = new Thing(0);
//        ExpressionContext ctx = new ExpressionContext(target);
//        ExpressionCompiler compiler = new ExpressionCompiler();
//        WriteTargetExpression<float> expression = compiler.CompileWriteTarget<float>(typeof(TestType0), "thingProp.value");
//        Assert.AreEqual(0, target.thingProp.value);
//        expression.Assign(ctx, 12f);
//        Assert.AreEqual(12f, target.thingProp.value);
//    }
//
//    [Test]
//    public void CompileWriteTarget_ThrowsForStructTargets() {
//        CompileException ex = Assert.Throws<CompileException>(() => {
//            ExpressionCompiler compiler = new ExpressionCompiler();
//            compiler.CompileWriteTarget<float>(typeof(TestType0), "structThing.x");
//        });
//        Assert.AreEqual("structThing.x", ex.expression);
//    }
//
//    [Test]
//    public void CompileWriteTarget_ErrorOnNonWritable() {
//        CompileException ex0 = Assert.Throws<CompileException>(() => {
//            ExpressionCompiler compiler = new ExpressionCompiler();
//            compiler.CompileWriteTarget<float>(typeof(TestType0), "notWritable");
//        });
//        Assert.AreEqual("notWritable", ex0.expression);
//
//        CompileException ex1 = Assert.Throws<CompileException>(() => {
//            ExpressionCompiler compiler = new ExpressionCompiler();
//            compiler.CompileWriteTarget<float>(typeof(TestType0), "notWritableProp");
//        });
//        Assert.AreEqual("notWritableProp", ex1.expression);
//    }
//
//   
//
//}