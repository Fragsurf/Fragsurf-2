using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UIForia.Util;

// ReSharper disable HeapView.CanAvoidClosure

[TestFixture]
public class SkipTreeTests {

    [DebuggerDisplay("{name}")]
    public class Item : IHierarchical {

        private static int idGenerator = 0;
        
        public int id;
        public Item parent;
        public string name;

        public Item(Item parent = null, string name = null) {
            this.id = idGenerator++;
            this.parent = parent;
            this.name = name;
            if (this.name == null) {
                this.name = "__NO_NAME_";
            }
        }

        public int UniqueId => id;
        public IHierarchical Element => this;
        public IHierarchical Parent => parent;

    }

    [Test]
    public void AddNodeToRoot() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(one, "two");
        var three = new Item(one, "three");
        tree.AddItem(two);
        tree.AddItem(three);
        string[] output = new string[2];
        int i = 0;
        tree.TraversePreOrder((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "two", "three" }, output);
    }
    
    [Test]
    public void TraversePostOrder() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(one, "two");
        var three = new Item(one, "three");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        string[] output = new string[3];
        int i = 0;
        tree.TraversePostOrder((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "three", "two", "one" }, output);
    }

    [Test]
    public void AddParentAfterChildren() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(one, "two");
        var three = new Item(one, "three");
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(one);
        string[] output = new string[3];
        int i = 0;
        tree.TraversePreOrder((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "two", "three" }, output);
    }

    [Test]
    public void MissingParentInTree() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(one, "two");
        var three = new Item(two, "three");
        var four = new Item(two, "four");
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(one);
        string[] output = new string[3];
        int i = 0;
        tree.TraversePreOrder((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "three", "four" }, output);
    }

    [Test]
    public void RemoveAnElement() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(one, "two");
        var three = new Item(one, "three");
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(one);
        tree.RemoveItem(one);
        string[] output = new string[2];
        int i = 0;
        tree.TraversePreOrder((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "two", "three" }, output);
    }

    [Test]
    public void RemoveAnElementWithSiblings() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        tree.RemoveItem(two);
        string[] output = new string[5];
        int i = 0;
        tree.TraversePreOrder((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "four", "five", "six", "three" }, output);
        Assert.AreEqual(tree.Size, 5);
    }

    [Test]
    public void RemoveAnElementHierarchy_NodeInTree() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        tree.RemoveHierarchy(two);
        string[] output = new string[2];
        int i = 0;
        tree.TraversePreOrder((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "three" }, output);
        Assert.AreEqual(2, tree.Size);
    }

    [Test]
    public void RemoveAnElementHierarchy_NodeNotInTree() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
//        two not in tree
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        tree.RemoveHierarchy(two);
        string[] output = new string[2];
        int i = 0;
        tree.TraversePreOrder((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "three" }, output);
        Assert.AreEqual(2, tree.Size);
    }

    [Test]
    public void DisableHierarchy_NodeInTree() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        tree.DisableHierarchy(two);
        string[] output = new string[2];
        int i = 0;
        tree.TraversePreOrder((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "three" }, output);
        Assert.AreEqual(6, tree.Size);
    }

    [Test]
    public void DisableHierarchy_NodeNotInTree() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
        // two not in tree!
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        tree.DisableHierarchy(two);
        string[] output = new string[2];
        int i = 0;
        tree.TraversePreOrder((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "three" }, output);
        Assert.AreEqual(5, tree.Size);
    }

    [Test]
    public void EnableHierarchy_NodeInTree() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        tree.DisableHierarchy(two);
        tree.EnableHierarchy(two);
        string[] output = new string[6];
        int i = 0;
        tree.TraversePreOrder((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "two", "four", "five", "six", "three" }, output);
        Assert.AreEqual(6, tree.Size);
    }

    [Test]
    public void EnableHierarchy_NodeNotInTree() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
        // two not in tree!
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        tree.DisableHierarchy(five);
        tree.DisableHierarchy(four);
        string[] output = new string[5];
        int i = 0;
        tree.EnableHierarchy(two);
        tree.TraversePreOrder((item) => {
            output[i++] = item.name;
        });
        Assert.AreEqual(new[] { "one", "three", "four", "five", "six" }, output);
        Assert.AreEqual(5, tree.Size);
    }

    [Test]
    public void Event_AddItem() {
        int callCount = 0;
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        tree.onTreeChanged += (changeType) => {
            if (changeType == SkipTree<Item>.TreeChangeType.ItemAdded) {
                callCount++;
            }
        };
        tree.AddItem(one);
        tree.AddItem(two);
        Assert.AreEqual(2, callCount);
    }

    [Test]
    public void Event_RemoveItem() {
        int callCount = 0;
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        tree.onTreeChanged += (changeType) => {
            if (changeType == SkipTree<Item>.TreeChangeType.ItemRemoved) {
                callCount++;
            }
        };
        tree.AddItem(one);
        tree.AddItem(two);
        tree.RemoveItem(two);
        Assert.AreEqual(1, callCount);
    }

    [Test]
    public void Event_EnableHierarchy() {
        int callCount = 0;
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        tree.onTreeChanged += (changeType) => {
            if (changeType == SkipTree<Item>.TreeChangeType.HierarchyEnabled) {
                callCount++;
            }
        };
        tree.AddItem(one);
        tree.AddItem(two);
        tree.DisableHierarchy(two);
        tree.EnableHierarchy(two);
        Assert.AreEqual(1, callCount);
    }

    [Test]
    public void Event_DisableHierarchy() {
        int callCount = 0;
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        tree.onTreeChanged += (changeType) => {
            if (changeType == SkipTree<Item>.TreeChangeType.HierarchyDisabled) {
                callCount++;
            }
        };
        tree.AddItem(one);
        tree.AddItem(two);
        tree.DisableHierarchy(two);
        Assert.AreEqual(1, callCount);
    }

    [Test]
    public void Event_RemoveHierarchy() {
        int callCount = 0;
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        tree.onTreeChanged += (changeType) => {
            if (changeType == SkipTree<Item>.TreeChangeType.HierarchyRemoved) {
                callCount++;
            }
        };
        tree.AddItem(one);
        tree.AddItem(two);
        tree.DisableHierarchy(two);
        tree.RemoveHierarchy(two);
        Assert.AreEqual(1, callCount);
    }

    [Test]
    public void Event_Clear() {
        int callCount = 0;
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        tree.onTreeChanged += (changeType) => {
            if (changeType == SkipTree<Item>.TreeChangeType.Cleared) {
                callCount++;
            }
        };
        tree.AddItem(one);
        tree.AddItem(two);
        tree.Clear();
        Assert.AreEqual(1, callCount);
    }

    [Test]
    public void GetTraversableTree_Root() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);

        SkipTree<Item>.TreeNode traverseTree = tree.GetTraversableTree();

        Stack<SkipTree<Item>.TreeNode> stack = new Stack<SkipTree<Item>.TreeNode>();

        stack.Push(traverseTree);
        string[] output = new string[6];
        int count = 0;

        while (stack.Count > 0) {
            var current = stack.Pop();
            if (current.item != null) {
                output[count++] = current.item.name;
            }
            for (int i = 0; i < current.children.Length; i++) {
                stack.Push(current.children[i]);
            }

        }

        Assert.AreEqual(new string[] {
            "three",
            "two",
            "six",
            "five",
            "four",
            "one"
        }, output);

    }

    [Test]
    public void GetTraversableTree_Subtree() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(two, "six");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);

        SkipTree<Item>.TreeNode traverseTree = tree.GetTraversableTree(two);

        Stack<SkipTree<Item>.TreeNode> stack = new Stack<SkipTree<Item>.TreeNode>();

        stack.Push(traverseTree);
        string[] output = new string[4];
        int count = 0;

        while (stack.Count > 0) {
            var current = stack.Pop();
            output[count++] = current.item.name;
            for (int i = 0; i < current.children.Length; i++) {
                stack.Push(current.children[i]);
            }

        }

        Assert.AreEqual(new string[] {
            "two",
            "six",
            "five",
            "four",
        }, output);

    }

    [Test]
    public void SetSiblingIndex() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(null, "three");
        var four = new Item(null, "four");
        var five = new Item(null, "five");
        var six = new Item(null, "six");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        
        Assert.AreEqual(0, tree.GetSiblingIndex(one));
        Assert.AreEqual(1, tree.GetSiblingIndex(two));
        Assert.AreEqual(2, tree.GetSiblingIndex(three));
        Assert.AreEqual(3, tree.GetSiblingIndex(four));
        Assert.AreEqual(4, tree.GetSiblingIndex(five));
        Assert.AreEqual(5, tree.GetSiblingIndex(six));
        
        tree.SetSiblingIndex(two, 0);
        Assert.AreEqual(1, tree.GetSiblingIndex(one));
        Assert.AreEqual(0, tree.GetSiblingIndex(two));

        tree.SetSiblingIndex(five, 3);

        Assert.AreEqual(1, tree.GetSiblingIndex(one));
        Assert.AreEqual(0, tree.GetSiblingIndex(two));
        Assert.AreEqual(2, tree.GetSiblingIndex(three));
        Assert.AreEqual(3, tree.GetSiblingIndex(five));
        Assert.AreEqual(4, tree.GetSiblingIndex(four));
        Assert.AreEqual(5, tree.GetSiblingIndex(six));
        
        tree.SetSiblingIndex(six, -1);
        Assert.AreEqual(0, tree.GetSiblingIndex(six));
        Assert.AreEqual(1, tree.GetSiblingIndex(two));
        Assert.AreEqual(2, tree.GetSiblingIndex(one));
        Assert.AreEqual(3, tree.GetSiblingIndex(three));
        Assert.AreEqual(4, tree.GetSiblingIndex(five));
        Assert.AreEqual(5, tree.GetSiblingIndex(four));
        
        tree.SetSiblingIndex(six, 10);
        Assert.AreEqual(0, tree.GetSiblingIndex(two));
        Assert.AreEqual(1, tree.GetSiblingIndex(one));
        Assert.AreEqual(2, tree.GetSiblingIndex(three));
        Assert.AreEqual(3, tree.GetSiblingIndex(five));
        Assert.AreEqual(4, tree.GetSiblingIndex(four));
        Assert.AreEqual(5, tree.GetSiblingIndex(six));
    }

    [Test]
    public void OhForFuckSakesNotAgain() {
        SkipTree<Item> tree = new SkipTree<Item>();
        var one = new Item(null, "one");
        var two = new Item(null, "two");
        var three = new Item(two, "three");
        var four = new Item(two, "four");
        var five = new Item(two, "five");
        var six = new Item(null, "six");
        tree.AddItem(one);
        tree.AddItem(two);
        tree.AddItem(three);
        tree.AddItem(four);
        tree.AddItem(five);
        tree.AddItem(six);
        string[] output = new string[3];
        int i = 0;
        tree.ConditionalTraversePreOrder((item) => {
            output[i++] = item.name;
            return item.name != "two";
        });
        Assert.AreEqual(new [] {
            "one", "two", "six"
        }, output);
        
    }
    
}