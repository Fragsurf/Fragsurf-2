using System;
using System.Collections.Generic;
using NUnit.Framework;
using UIForia.Util;

[TestFixture]
public class LightListTests {

    [Test]
    public void Insert() {
        LightList<int> ints = new LightList<int>();
        ints.Add(0);
        ints.Add(1);
        ints.Add(3);
        ints.Add(4);
        ints.Insert(2, 2);
        Assert.AreEqual(5, ints.Count);
        for (int i = 0; i < ints.Count; i++) {
            Assert.AreEqual(i, ints[i]);
        }
    }
    
    [Test]
    public void InsertRange() {
        LightList<int> ints = new LightList<int>();
        ints.Add(0);
        ints.Add(1);
        ints.Add(4);
        ints.Add(5);
        LightList<int> other = new LightList<int>();
        other.Add(2);
        other.Add(3);
        ints.InsertRange(2, other);
        Assert.AreEqual(6, ints.Count);
        for (int i = 0; i < ints.Count; i++) {
            Assert.AreEqual(i, ints[i]);
        }
    }

    [Test]
    public void InsertRangeFromZero() {
        LightList<int> ints = new LightList<int>();
        ints.Add(0);
        ints.Add(1);
        ints.Add(4);
        ints.Add(5);
        LightList<int> other = new LightList<int>();
        other.Add(2);
        other.Add(3);
        ints.InsertRange(0, other);
        Assert.AreEqual(6, ints.Count);
        Assert.AreEqual(2, ints[0]);
        Assert.AreEqual(3, ints[1]);
        Assert.AreEqual(0, ints[2]);
        Assert.AreEqual(1, ints[3]);
        Assert.AreEqual(4, ints[4]);
        Assert.AreEqual(5, ints[5]);
    }
    
    [Test]
    public void InsertRangeFromEnd() {
        LightList<int> ints = new LightList<int>();
        ints.Add(0);
        ints.Add(1);
        ints.Add(4);
        ints.Add(5);
        LightList<int> other = new LightList<int>();
        other.Add(2);
        other.Add(3);
        ints.InsertRange(ints.Count, other);
        Assert.AreEqual(6, ints.Count);
        Assert.AreEqual(0, ints[0]);
        Assert.AreEqual(1, ints[1]);
        Assert.AreEqual(4, ints[2]);
        Assert.AreEqual(5, ints[3]);
        Assert.AreEqual(2, ints[4]);
        Assert.AreEqual(3, ints[5]);
    }

    [Test]
    public void InsertRangeFromEndMinus1() {
        LightList<int> ints = new LightList<int>();
        ints.Add(0);
        ints.Add(1);
        ints.Add(4);
        ints.Add(5);
        LightList<int> other = new LightList<int>();
        other.Add(2);
        other.Add(3);
        ints.InsertRange(ints.Count - 1, other);
        Assert.AreEqual(6, ints.Count);
        Assert.AreEqual(0, ints[0]);
        Assert.AreEqual(1, ints[1]);
        Assert.AreEqual(4, ints[2]);
        Assert.AreEqual(2, ints[3]);
        Assert.AreEqual(3, ints[4]);
        Assert.AreEqual(5, ints[5]);
    }
    
    [Test]
    public void ShiftRight3() {
        LightList<int> ints = new LightList<int>();
        ints.Add(0);
        ints.Add(1);
        ints.Add(2);
        ints.Add(3);
        ints.Add(4);
        ints.Add(5);
        ints.ShiftRight(3, 3);
        Assert.AreEqual(9, ints.Count);
        Assert.AreEqual(0, ints[0]);
        Assert.AreEqual(1, ints[1]);
        Assert.AreEqual(2, ints[2]);
        Assert.AreEqual(0, ints[3]);
        Assert.AreEqual(0, ints[4]);
        Assert.AreEqual(0, ints[5]);
        Assert.AreEqual(3, ints[6]);
        Assert.AreEqual(4, ints[7]);
        Assert.AreEqual(5, ints[8]);
    }
    
    [Test]
    public void ShiftLeft3() {
        LightList<int> ints = new LightList<int>();
        ints.Add(0);
        ints.Add(1);
        ints.Add(2);
        ints.Add(3);
        ints.Add(4);
        ints.Add(5);
        ints.ShiftLeft(3, 3);
        Assert.AreEqual(3, ints.Count);
        Assert.AreEqual(3, ints[0]);
        Assert.AreEqual(4, ints[1]);
        Assert.AreEqual(5, ints[2]);
        Assert.AreEqual(0, ints[3]);
        Assert.AreEqual(0, ints[4]);
        Assert.AreEqual(0, ints[5]);
    }
    
    [Test]
    public void ShiftLeft2() {
        LightList<int> ints = new LightList<int>();
        ints.Add(0);
        ints.Add(1);
        ints.Add(2);
        ints.Add(3);
        ints.Add(4);
        ints.Add(5);
        ints.Add(6);
        ints.Add(7);
        
        ints.ShiftLeft(3, 2);
        Assert.AreEqual(6, ints.Count);
        
        Assert.AreEqual(0, ints[0]);
        Assert.AreEqual(3, ints[1]);
        Assert.AreEqual(4, ints[2]);
        Assert.AreEqual(5, ints[3]);
        Assert.AreEqual(6, ints[4]);
        Assert.AreEqual(7, ints[5]);
    }


    [Test]
    public void Sort() {
        LightList<int> ints = new LightList<int>();
        ints.Add(4);
        ints.Add(0);
        ints.Add(2);
        ints.Add(6);
        ints.Add(3);
        ints.Add(1);
        ints.Add(7);
        ints.Add(5);
        Comparison<int> cmp = (a, b) => a > b ? 1 : -1;
        ints.Sort(cmp);
        
        for (int i = 0; i < ints.Count; i++) {
            Assert.AreEqual(i, ints[i]);
        }
    }
}