using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UIForia.Util;

[TestFixture]
public class ReflectionUtilTests {

    private class RefUtilClass1 : IDoSomething {

        public string value;

        public RefUtilClass1(string value) {
            this.value = value;
        }

        public string GetValue() {
            return value;
        }

        public string GetValueWithArg(string arg) {
            return arg + value;
        }

        public virtual string CallVirtMethod() {
            return "base";
        }
    }

    private class RefUtilClass1Extends : RefUtilClass1 {

        public RefUtilClass1Extends(string value) : base(value) { }

        public override string CallVirtMethod() {
            return "extends";
        }
        
    }
    
    private class RefUtilClass1ExtendsAgain : RefUtilClass1Extends {

        public RefUtilClass1ExtendsAgain(string value) : base(value) { }

    }

    private class RefUtilClass2 : IDoSomething {

        public string value;

        public RefUtilClass2(string value) {
            this.value = value;
        }

        public string GetValue() {
            return value;
        }

    }


    private interface IDoSomething {

        string GetValue();

    }
    
    private interface IDoSomething2 {

        string GetValue();

    }

    [Test]
    public void TestOpenDelegate() {
        RefUtilClass1 one = new RefUtilClass1("one");
        RefUtilClass1 two = new RefUtilClass1("two");
        MethodInfo info = typeof(RefUtilClass1).GetMethod("GetValue");
        Func<RefUtilClass1, string> openDelegate = ReflectionUtil.CreateOpenDelegate<Func<RefUtilClass1, string>>(info);
        Assert.AreEqual("one", openDelegate(one));
        Assert.AreEqual("two", openDelegate(two));
    }

    [Test]
    public void GetOpenDelegateFromType() {
        MethodInfo info1 = typeof(RefUtilClass1).GetMethod("GetValue");
        MethodInfo info2 = typeof(RefUtilClass1).GetMethod("GetValueWithArg");
        Type openDelegateType1 = ReflectionUtil.GetOpenDelegateType(info1);
        Type openDelegateType2 = ReflectionUtil.GetOpenDelegateType(info2);
        Assert.AreEqual(typeof(Func<RefUtilClass1, string>), openDelegateType1);
        Assert.AreEqual(typeof(Func<RefUtilClass1, string, string>), openDelegateType2);
    }

    [Test]
    public void TestOpenDelegateChild() {
        RefUtilClass1 one = new RefUtilClass1("one");
        RefUtilClass1 two = new RefUtilClass1Extends("two");
        MethodInfo info1 = typeof(RefUtilClass1).GetMethod("GetValue");
        MethodInfo info2 = typeof(RefUtilClass1Extends).GetMethod("GetValue");
        Func<RefUtilClass1, string> d1 = (Func<RefUtilClass1, string>)ReflectionUtil.GetDelegate(info1);
        Func<RefUtilClass1, string> d2 =  (Func<RefUtilClass1, string>)ReflectionUtil.GetDelegate(info2);
        Assert.AreEqual(d1, d2);
        
        Assert.AreEqual("one", d1(one));
        Assert.AreEqual("two", d1(two));
        
    }
    
    [Test]
    public void TestOpenDelegateAncestor() {
        RefUtilClass1 one = new RefUtilClass1("one");
        RefUtilClass1Extends two = new RefUtilClass1Extends("two");
        RefUtilClass1ExtendsAgain three = new RefUtilClass1ExtendsAgain("three");
        
        MethodInfo info1 = typeof(RefUtilClass1).GetMethod("GetValue");
        MethodInfo info2 = typeof(RefUtilClass1Extends).GetMethod("GetValue");
        MethodInfo info3 = typeof(RefUtilClass1ExtendsAgain).GetMethod("GetValue");
        
        Func<RefUtilClass1, string> d1 = (Func<RefUtilClass1, string>)ReflectionUtil.GetDelegate(info1);
        Func<RefUtilClass1, string> d2 =  (Func<RefUtilClass1, string>)ReflectionUtil.GetDelegate(info2);
        Func<RefUtilClass1, string> d3 =  (Func<RefUtilClass1, string>)ReflectionUtil.GetDelegate(info3);
        
        Assert.AreEqual(d1, d2);
        Assert.AreEqual(d2, d3);
        
        Assert.AreEqual("one", d1(one));
        Assert.AreEqual("two", d1(two));
        Assert.AreEqual("three", d1(three));
        
    }
    
    [Test]
    public void TestOpenDelegateWorksWithInterface() {
        MethodInfo info1 = typeof(RefUtilClass1).GetMethod("GetValue");
        MethodInfo info2 = typeof(RefUtilClass2).GetMethod("GetValue");
        Delegate d1 = ReflectionUtil.GetDelegate(info1);
        Delegate d2 = ReflectionUtil.GetDelegate(info2);
        Assert.AreEqual(d1, d2);
    }

    [Test]
    public void TestCallsVirtualMethod() {
        RefUtilClass1 one = new RefUtilClass1("one");
        RefUtilClass1Extends two = new RefUtilClass1Extends("two");
        MethodInfo info1 = typeof(RefUtilClass1Extends).GetMethod("CallVirtMethod");
        Func<RefUtilClass1, string> d1 = (Func<RefUtilClass1, string>)ReflectionUtil.GetDelegate(info1);
        
        Assert.AreEqual("base", d1(one));
        Assert.AreEqual("extends", d1(two));

    }

    [Test]
    public void GenerateArrayGetterSetter() {
        float[] array = new float[4];
        Func<float[], int, float, float> callback = (Func<float[], int, float, float>) ReflectionUtil.CreateArraySetter(typeof(float[]));
        callback.Invoke(array, 1, 5f);
        Func<float[], int, float> getter = (Func<float[], int, float>) ReflectionUtil.CreateArrayGetter(typeof(float[]));
        Assert.AreEqual(5f, getter(array, 1), 5f);
    }

    [Test]
    public void GenerateIndexSetterGetter() {
        List<string> list = new List<string>();
        list.Add("hello");
        list.Add("world");
        var y = (Func<List<string>,int, string, string>)ReflectionUtil.CreateIndexSetter(typeof(List<string>));
        var z = (Func<List<string>,int, string>)ReflectionUtil.CreateIndexGetter(typeof(List<string>));
        y.Invoke(list, 0, "hallochen");
        Assert.AreEqual(list[0], "hallochen");
        Assert.AreEqual(z.Invoke(list, 0), "hallochen");
    }

    private class Ref1 : IList<float>, IList {

        public IEnumerator<float> GetEnumerator() {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(float item) {
            throw new NotImplementedException();
        }

        public int Add(object value) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(object value) {
            throw new NotImplementedException();
        }

        public int IndexOf(object value) {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value) {
            throw new NotImplementedException();
        }

        public void Remove(object value) {
            throw new NotImplementedException();
        }

        public bool Contains(float item) {
            throw new NotImplementedException();
        }

        public void CopyTo(float[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public bool Remove(float item) {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsSynchronized { get; }
        public object SyncRoot { get; }
        public bool IsReadOnly { get; }
        
        object IList.this[int index] {
            get => null;
            set { }
        }

        public int IndexOf(float item) {
            throw new NotImplementedException();
        }

        public void Insert(int index, float item) {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        public bool IsFixedSize { get; }

        public float this[int index] {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

    }

    private class Ref2 : IList {

        public IEnumerator GetEnumerator() {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsSynchronized { get; }
        public object SyncRoot { get; }

        public int Add(object value) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(object value) {
            throw new NotImplementedException();
        }

        public int IndexOf(object value) {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value) {
            throw new NotImplementedException();
        }

        public void Remove(object value) {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        public bool IsFixedSize { get; }
        public bool IsReadOnly { get; }

        public object this[int index] {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

    }

    [Test]
    public void GetListElementType() {
        Assert.AreEqual(typeof(float), ReflectionUtil.GetArrayElementTypeOrThrow(typeof(Ref1)));
    }
    
    [Test]
    public void GetListElementTypeIList() {
        Assert.AreEqual(typeof(object), ReflectionUtil.GetArrayElementTypeOrThrow(typeof(Ref2)));
    }
    
    [Test]
    public void GetListElementTypeArray() {
        Assert.AreEqual(typeof(string), ReflectionUtil.GetArrayElementTypeOrThrow(typeof(string[])));
    }
}