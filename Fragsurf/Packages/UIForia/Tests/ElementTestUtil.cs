using System;
using NUnit.Framework;
using UIForia.Elements;

namespace Tests {

    public enum TestEnableFlags {

        Enabled,
        Disabled,
        EnabledSelf,
        DisabledSelf,
        DisabledAncestor,
        EnabledAncestor

    }

    public struct TypeAssert {

        public readonly Type parentType;
        public readonly TypeAssert[] childTypes;
        public readonly TestEnableFlags flags;

        public TypeAssert(Type parentType, TestEnableFlags flags = 0, TypeAssert[] childTypes = null) {
            this.parentType = parentType;
            this.childTypes = childTypes ?? new TypeAssert[0];
            this.flags = flags;
        }

    }

    public static class ElementTestUtil {

        public static T AssertElementType<T>(UIElement element) where T : UIElement {
            Assert.IsInstanceOf<T>(element);
            return (T) element;
        }

        public static void AssertHasAttribute(UIElement element, string attrName, string attrValue = null) {
            string attr = element.GetAttribute(attrName);
            Assert.IsNotNull(attr);
            if (attrValue != null) {
                Assert.AreEqual(attrValue, attr);
            }
        }

        public static void AssertHierarchy(UIElement element, TypeAssert assertRoot, int depth = 0) {
            Assert.AreEqual(element.GetType(), assertRoot.parentType);
            if (element.children.Count != assertRoot.childTypes.Length) {
                Assert.Fail("Child Count did not match at depth: " + depth);
            }

            for (int i = 0; i < element.children.Count; i++) {
                if (element.children[i].GetType() != assertRoot.childTypes[i].parentType) {
                    Assert.Fail($"Types did not match for child number {i} at depth {depth}. {element.children[i].GetType()} is not {assertRoot.childTypes[i].parentType}");
                }

                AssertHierarchy(element.children[i], assertRoot.childTypes[i], depth + 1);
            }
        }

        public static void AssertHierarchyFlags(UIElement element, TypeAssert assertRoot, int depth = 0) {
            Assert.AreEqual(element.GetType(), assertRoot.parentType);

            switch (assertRoot.flags) {
                case TestEnableFlags.Disabled:
                    Assert.IsTrue(element.isDisabled);
                    Assert.IsFalse(element.isEnabled);
                    break;
                case TestEnableFlags.DisabledSelf:
                    Assert.IsTrue(element.isSelfDisabled);
                    Assert.IsFalse(element.isSelfEnabled);
                    break;
                case TestEnableFlags.DisabledAncestor:
                    Assert.IsTrue(element.isDisabled);
                    Assert.IsTrue(element.isSelfEnabled);
                    Assert.IsFalse(element.isEnabled);
                    break;
                case TestEnableFlags.Enabled:
                    Assert.IsTrue(element.isEnabled);
                    Assert.IsTrue(element.isSelfEnabled);
                    Assert.IsFalse(element.isSelfDisabled);
                    break;
                case TestEnableFlags.EnabledAncestor:
                    break;
                case TestEnableFlags.EnabledSelf:
                    Assert.IsTrue(element.isSelfEnabled);
                    break;
            }

            if (element.children.Count != assertRoot.childTypes.Length) {
                Assert.Fail("Child Count did not match at depth: " + depth);
            }

            for (int i = 0; i < element.children.Count; i++) {
                if (element.children[i].GetType() != assertRoot.childTypes[i].parentType) {
                    Assert.Fail("Types did not match for child number " + i + " at depth " + depth);
                }

                AssertHierarchy(element.children[i], assertRoot.childTypes[i], depth + 1);
            }
        }

    }

}