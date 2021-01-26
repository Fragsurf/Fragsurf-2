using System;
using NUnit.Framework;
using UIForia.Compilers;
using UIForia.Exceptions;
using UnityEngine;

[TestFixture]
public class TestLinqCompilerErrors {

    [Test]
    public void CompileMethod_MissingMethodOverload() {
        LinqCompiler compiler = new LinqCompiler();
        compiler.SetSignature<int>();
        compiler.AddNamespace("UnityEngine");
        CompileException exception = Assert.Throws<CompileException>(() =>
            compiler.Return("Color.red.GetHashCode(14, 15)")
        );
        string expected = CompileException.UnresolvedInstanceMethodOverload(typeof(Color), "GetHashCode", new Type[] {typeof(int), typeof(int)}).Message;
        Assert.AreEqual(expected, exception.Message);
    }

}