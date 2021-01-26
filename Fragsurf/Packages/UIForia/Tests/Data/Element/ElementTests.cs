using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;

namespace ElementTests {

    public class ElementTests {

        [Template("Data/Element/ElementTests_Depth.xml")]
        public class DepthThing : UIElement {

            public UIElement g1;
            public UIElement g2;
            public UIElement g1_1;
            public UIElement g2_1;
            public UIElement g2_2;

            public override void OnCreate() {
                g1 = FindById("g1");
                g1_1 = FindById("g1_child1");
                g2 = FindById("g2");
                g2_1 = FindById("g2_child1");
                g2_2 = FindById("g2_child2");
            }

        }

        [Template("Data/Element/ElementTests_Depth.xml#depth_thing_child")]
        public class DepthThingChild : UIElement { }

        [Test]
        public void ProperDepth() {
            MockApplication app = MockApplication.Setup<DepthThing>();
            DepthThing root = (DepthThing) app.RootElement;
            Assert.AreEqual(1, root.hierarchyDepth);
            Assert.AreEqual(2, root.g1.hierarchyDepth);
            Assert.AreEqual(3, root.g1_1.hierarchyDepth);
            Assert.AreEqual(2, root.g2.hierarchyDepth);
            Assert.AreEqual(3, root.g2_1.hierarchyDepth);
            Assert.AreEqual(3, root.g2_2.hierarchyDepth);
        }

        [Test]
        public void ProperDepthNested() {
            MockApplication app = MockApplication.Setup<DepthThing>();
            DepthThing root = (DepthThing) app.RootElement;
            DepthThingChild child = root.FindFirstByType<DepthThingChild>();
            Assert.AreEqual(2, child.hierarchyDepth);
            Assert.AreEqual(3, child.FindById("g1").hierarchyDepth);
            Assert.AreEqual(4, child.FindById("g1_child1").hierarchyDepth);
            Assert.AreEqual(3, child.FindById("g2").hierarchyDepth);
            Assert.AreEqual(4, child.FindById("g2_child1").hierarchyDepth);
            Assert.AreEqual(4, child.FindById("g2_child2").hierarchyDepth);
        }

        [Test]
        public void ProperSiblingIndex() {
            MockApplication app = MockApplication.Setup<DepthThing>();
            DepthThing root = (DepthThing) app.RootElement;
            Assert.AreEqual(0, root.siblingIndex);
            Assert.AreEqual(0, root.g1.siblingIndex);
            Assert.AreEqual(0, root.g1_1.siblingIndex);
            Assert.AreEqual(1, root.g2.siblingIndex);
            Assert.AreEqual(0, root.g2_1.siblingIndex);
            Assert.AreEqual(1, root.g2_2.siblingIndex);
        }

    }

}