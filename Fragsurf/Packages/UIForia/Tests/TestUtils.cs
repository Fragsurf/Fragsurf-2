using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Mono.Linq.Expressions;
using NUnit.Framework;
using UIForia.Elements;

namespace Tests {

    public static class TestUtils {

        [Flags]
        public enum TestEnum {

            One = 1 << 0,
            Two = 1 << 1,
            Three = 1 << 2

        }

        public static T As<T>(object thing) {
            return (T) thing;
        }

        public static T AssertInstanceOfAndReturn<T>(object target) {
            Assert.IsInstanceOf<T>(target);
            return (T) target;
        }

        public class TestUIElementType : UIElement {

            public int intValue;

        }

     

        public class FakeRootElement : UIElement {

            public int arg0CallCount;

            public string[] arg1Params;
            public string[] arg2Params;
            public string[] arg3Params;
            public string[] arg4Params;

            public Action<string> evt1Handler;
            public Func<string, string> evt1Handler_Func;

            public FakeRootElement() {
                evt1Handler = HandleSomeEventArg1;
                evt1Handler_Func = HandleSomeFuncEventArg1;
            }

            public void HandleSomeEventArg0() {
                arg0CallCount++;
            }

            public void HandleSomeEventArg1(string val) {
                arg1Params = new[] {val};
            }

            public string HandleSomeFuncEventArg1(string val) {
                arg1Params = new[] {val};
                return val;
            }

            public void HandleSomeEventArg2(string arg0, string arg1) {
                arg2Params = new[] {arg0, arg1};
            }

            public void HandleSomeEventArg3(string arg0, string arg1, string arg2) {
                arg3Params = new[] {arg0, arg1, arg2};
            }

            public void HandleSomeEventArg4(string arg0, string arg1, string arg2, string arg3) {
                arg4Params = new[] {arg0, arg1, arg2, arg3};
            }

        }

        public class FakeElement : UIElement {
            
            public event Func<string, string> onFuncEvt1;

            public event Action<string> onEvt1;
//            public event Action<string, string> onEvt2;
//            public event Action<string, string, string> onEvt3;
//            public event Action<string, string, string, string> onEvt4;
            
            public delegate void SomeDelegateArg0();

            public delegate void SomeDelegateArg1(string arg0);

            public delegate void SomeDelegateArg2(string arg0, string arg1);

            public delegate void SomeDelegateArg3(string arg0, string arg1, string arg2);

            public delegate void SomeDelegateArg4(string arg0, string arg1, string arg2, string arg3);

            public event SomeDelegateArg0 onSomeEventArg0;
//            public event SomeDelegateArg1 onSomeEventArg1;
            public event SomeDelegateArg2 onSomeEventArg2;
            public event SomeDelegateArg3 onSomeEventArg3;
            public event SomeDelegateArg4 onSomeEventArg4;


            public void InvokeEvtArg0() {
                onSomeEventArg0?.Invoke();
            }

            public void InvokeEvtArg1(string arg0) {
                onEvt1?.Invoke(arg0);
            }

            public void InvokeEvtArg2(string arg0, string arg1) {
                onSomeEventArg2?.Invoke(arg0, arg1);
            }

            public void InvokeEvtArg3(string arg0, string arg1, string arg2) {
                onSomeEventArg3?.Invoke(arg0, arg1, arg2);
            }

            public void InvokeEvtArg4(string arg0, string arg1, string arg2, string arg3) {
                onSomeEventArg4?.Invoke(arg0, arg1, arg2, arg3);
            }

            public void InvokeFuncEvtArg1(string str) {
                onFuncEvt1?.Invoke(str);
            }

        }

        public static void AssertStringsEqual(string a, string b) {
            string[] splitA = a.Trim().Split('\n');
            string[] splitB = b.Trim().Split('\n');

            Assert.AreEqual(splitA.Length, splitB.Length);

            for (int i = 0; i < splitA.Length; i++) {
                Assert.AreEqual(splitA[i].Trim(), splitB[i].Trim());
            }
        }

        public static string PrintCode(IList<Expression> expressions) {
            string retn = "";
            for (int i = 0; i < expressions.Count; i++) {
                retn += expressions[i].ToCSharpCode();
                if (i != expressions.Count - 1) {
                    retn += "\n";
                }
            }

            return retn;
        }

        public static string PrintCode(Expression expression) {
            return expression.ToCSharpCode();
        }
    }

}