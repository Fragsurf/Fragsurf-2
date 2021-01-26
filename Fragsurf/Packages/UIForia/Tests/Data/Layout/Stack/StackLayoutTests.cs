using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace Layout {

    public class StackLayoutTests {

        [Template("Data/Layout/Stack/Stack_ComputeContentSize.xml")]
        public class Stack_ComputeContentSize : UIElement { }

        [Test]
        public void ComputeContentSize() {
            MockApplication mockView = MockApplication.Setup<Stack_ComputeContentSize>();
            Stack_ComputeContentSize root = (Stack_ComputeContentSize) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 150, 150), root.layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 0, 150, 150), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 0, 150, 150), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 0, 150, 150), root[2].layoutResult.AllocatedRect);

            Assert.AreEqual(new Rect(0, 0, 150, 150), root[0].layoutResult.LocalRect);
            Assert.AreEqual(new Rect(0, 0, 100, 100), root[1].layoutResult.LocalRect);
            Assert.AreEqual(new Rect(0, 0, 50, 50), root[2].layoutResult.LocalRect);
        }

        [Template("Data/Layout/Stack/Stack_AlignItems.xml")]
        public class Stack_AlignItems : UIElement { }

        [Test]
        public void AlignItems() {
            MockApplication mockView = MockApplication.Setup<Stack_AlignItems>();
            Stack_AlignItems root = (Stack_AlignItems) mockView.RootElement;

            root.style.SetAlignItemsHorizontal(ItemAlignment.Center, StyleState.Normal);
            root.style.SetAlignItemsVertical(ItemAlignment.Center, StyleState.Normal);

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 300, 300), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 0, 300, 300), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 0, 300, 300), root[2].layoutResult.AllocatedRect);

            Assert.AreEqual(new Rect(75, 75, 150, 150), root[0].layoutResult.LocalRect);
            Assert.AreEqual(new Rect(100, 100, 100, 100), root[1].layoutResult.LocalRect);
            Assert.AreEqual(new Rect(125, 125, 50, 50), root[2].layoutResult.LocalRect);

            root.style.SetAlignItemsHorizontal(ItemAlignment.Start, StyleState.Normal);
            root.style.SetAlignItemsVertical(ItemAlignment.Start, StyleState.Normal);
            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 300, 300), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 0, 300, 300), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 0, 300, 300), root[2].layoutResult.AllocatedRect);

            Assert.AreEqual(new Rect(0, 0, 150, 150), root[0].layoutResult.LocalRect);
            Assert.AreEqual(new Rect(0, 0, 100, 100), root[1].layoutResult.LocalRect);
            Assert.AreEqual(new Rect(0, 0, 50, 50), root[2].layoutResult.LocalRect);

            root.style.SetAlignItemsHorizontal(ItemAlignment.End, StyleState.Normal);
            root.style.SetAlignItemsVertical(ItemAlignment.End, StyleState.Normal);
            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 300, 300), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 0, 300, 300), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 0, 300, 300), root[2].layoutResult.AllocatedRect);

            Assert.AreEqual(new Rect(150, 150, 150, 150), root[0].layoutResult.LocalRect);
            Assert.AreEqual(new Rect(200, 200, 100, 100), root[1].layoutResult.LocalRect);
            Assert.AreEqual(new Rect(250, 250, 50, 50), root[2].layoutResult.LocalRect);
        }

    }

}