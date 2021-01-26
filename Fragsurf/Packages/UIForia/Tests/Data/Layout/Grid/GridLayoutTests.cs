using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UnityEngine;

namespace Layout {

    public class GridLayoutTest {

        [Template("Data/Layout/Grid/GridLayout_RowSizeMinContent.xml")]
        public class GridLayout_RowSize_MinContent : UIElement { }

        [Test]
        public void RowSize_MinContent() {
            MockApplication mockView = MockApplication.Setup<GridLayout_RowSize_MinContent>();
            GridLayout_RowSize_MinContent root = (GridLayout_RowSize_MinContent) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 400), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 400), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 400, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 400, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 500, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 500, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_ColSizeMaxContent.xml")]
        public class GridLayout_ColSizeMaxContent : UIElement { }

        [Test]
        public void ColSize_MaxContent() {
            MockApplication mockView = MockApplication.Setup<GridLayout_ColSizeMaxContent>();
            GridLayout_ColSizeMaxContent root = (GridLayout_ColSizeMaxContent) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 600, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(600, 0, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(700, 0, 100, 100), root[2].layoutResult.AllocatedRect);

            Assert.AreEqual(new Rect(0, 100, 600, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(600, 100, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(700, 100, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_ColSizeMinContent.xml")]
        public class GridLayout_ColSizeMinContent : UIElement { }

        [Test]
        public void ColSize_MinContent() {
            MockApplication mockView = MockApplication.Setup<GridLayout_ColSizeMinContent>();
            GridLayout_ColSizeMinContent root = (GridLayout_ColSizeMinContent) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 400, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 0, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(500, 0, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 400, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 100, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(500, 100, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_GrowToMaxSize.xml")]
        public class GridLayout_GrowToMaxSize : UIElement { }

        [Test]
        public void GrowToMaxSize_NoFractions() {
            MockApplication mockView = MockApplication.Setup<GridLayout_GrowToMaxSize>();
            mockView.Update();
            GridLayout_GrowToMaxSize root = (GridLayout_GrowToMaxSize) mockView.RootElement;
            Assert.AreEqual(1000, root.layoutResult.actualSize.width);

            Assert.AreEqual(0, root[0].layoutResult.allocatedPosition.x);
            Assert.AreEqual(300, root[1].layoutResult.allocatedPosition.x);
            Assert.AreEqual(700, root[2].layoutResult.allocatedPosition.x);
        }

        [Template("Data/Layout/Grid/GridLayout_GrowToMaxSize_Fractional.xml")]
        public class GridLayout_GrowToMaxSize_Fractional : UIElement { }

        [Test]
        public void GrowToMaxSize_Fractions() {
            MockApplication mockView = MockApplication.Setup<GridLayout_GrowToMaxSize_Fractional>();
            GridLayout_GrowToMaxSize_Fractional root = (GridLayout_GrowToMaxSize_Fractional) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(1000, root.layoutResult.actualSize.width);

            Assert.AreEqual(0, root[0].layoutResult.allocatedPosition.x);
            Assert.AreEqual(400, root[0].layoutResult.allocatedSize.width);

            Assert.AreEqual(400, root[1].layoutResult.allocatedPosition.x);
            Assert.AreEqual(400, root[1].layoutResult.allocatedSize.width);

            Assert.AreEqual(800, root[2].layoutResult.allocatedPosition.x);
            Assert.AreEqual(200, root[2].layoutResult.allocatedSize.width);
        }

        [Template("Data/Layout/Grid/GridLayout_ResolveMaxContentTrackSize.xml")]
        public class GridLayout_ResolveMaxContentTrackSize : UIElement { }

        [Test]
        public void ResolveMaxContentTrackSize() {
            MockApplication mockView = MockApplication.Setup<GridLayout_ResolveMaxContentTrackSize>();
            mockView.Update();
            GridLayout_ResolveMaxContentTrackSize root = (GridLayout_ResolveMaxContentTrackSize) mockView.RootElement;
            Assert.AreEqual(300, root.layoutResult.actualSize.width);

            Assert.AreEqual(0, root[0].layoutResult.allocatedPosition.x);
            Assert.AreEqual(100, root[1].layoutResult.allocatedPosition.x);
            Assert.AreEqual(0, root[2].layoutResult.allocatedPosition.x); // wraps to next line because prev was 2 wide
        }

        [Test]
        public void ResolveMaxContentTrackSize_WithColGap() {
            MockApplication mockView = MockApplication.Setup<GridLayout_ResolveMaxContentTrackSize>();
            GridLayout_ResolveMaxContentTrackSize root = (GridLayout_ResolveMaxContentTrackSize) mockView.RootElement;
            root.style.SetGridLayoutColGap(10, StyleState.Normal);
            mockView.Update();
            Assert.AreEqual(320, root.layoutResult.actualSize.width);
        }

//        [Template("Data/Layout/Grid/GridLayout_ColCollapseMaxSizeContribution.xml")]
//        public class GridLayout_ColCollapseMaxSizeContribution : UIElement { }
//
//        [Test]
//        public void ColCollapseMaxSizeContribution() {
//            MockApplication mockView = MockApplication.Setup<GridLayout_ColCollapseMaxSizeContribution>();
//            GridLayout_ColCollapseMaxSizeContribution root = (GridLayout_ColCollapseMaxSizeContribution) mockView.RootElement;
//
//            mockView.Update();
//
//            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
//            Assert.AreEqual(new Rect(0, 100, 100, 200), root[1].layoutResult.AllocatedRect);
//            Assert.AreEqual(new Rect(0, 300, 200, 100), root[2].layoutResult.AllocatedRect);
//            Assert.AreEqual(new Rect(100, 0, 100, 100), root[3].layoutResult.AllocatedRect);
//            Assert.AreEqual(new Rect(100, 100, 100, 100), root[4].layoutResult.AllocatedRect);
//            Assert.AreEqual(new Rect(100, 200, 100, 100), root[5].layoutResult.AllocatedRect);
//        }

        [Template("Data/Layout/Grid/GridLayout_ColMaxSizeContribution_NotCollapsed.xml")]
        public class GridLayout_ColMaxSizeContribution_NotCollapsed : UIElement { }

        [Test]
        public void ColMaxSizeContribution_NotCollapsed() {
            MockApplication mockView = MockApplication.Setup<GridLayout_ColMaxSizeContribution_NotCollapsed>();
            GridLayout_ColMaxSizeContribution_NotCollapsed root = (GridLayout_ColMaxSizeContribution_NotCollapsed) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 100, 200), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 300, 200, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 100, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 200, 100, 100), root[5].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 100, 100, 100), root[6].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_ExplicitPlaced_Flex.xml")]
        public class GridLayout_ExplicitPlaced_Flex : UIElement { }

        [Test]
        public void ExplicitPlaced_Flex() {
            MockApplication mockView = MockApplication.Setup<GridLayout_ExplicitPlaced_Flex>();
            GridLayout_ExplicitPlaced_Flex root = mockView.RootElement as GridLayout_ExplicitPlaced_Flex;

            mockView.Update();

            Assert.AreEqual(new Rect(100, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(300, 100, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 400, 100), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_ImplicitRowUnplaced3x2.xml")]
        public class GridLayout_ImplicitRowUnplaced3x2 : UIElement { }

        [Test]
        public void ImplicitRowPlaced3x2() {
            MockApplication mockView = MockApplication.Setup<GridLayout_ImplicitRowUnplaced3x2>();
            GridLayout_ImplicitRowUnplaced3x2 root = mockView.RootElement as GridLayout_ImplicitRowUnplaced3x2;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 100, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 200, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_ImplicitColUnplaced3x2.xml")]
        public class GridLayout_ImplicitColUnplaced3x2 : UIElement { }

        [Test]
        public void ImplicitColUnplaced3x2() {
            MockApplication mockView = MockApplication.Setup<GridLayout_ImplicitColUnplaced3x2>();
            GridLayout_ImplicitColUnplaced3x2 root = mockView.RootElement as GridLayout_ImplicitColUnplaced3x2;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 100, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 100, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_AssignBasicPlacements.xml")]
        public class GridLayout_AssignBasicPlacements : UIElement { }

        [Test]
        public void AssignBasicPlacements() {
            MockApplication mockView = MockApplication.Setup<GridLayout_AssignBasicPlacements>();
            GridLayout_AssignBasicPlacements root = (GridLayout_AssignBasicPlacements) mockView.RootElement;

            mockView.Update();

            AwesomeGridLayoutBox box = (AwesomeGridLayoutBox) root.layoutBox;

            Assert.AreEqual(2, box.RowCount);
            Assert.AreEqual(3, box.ColCount);

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 100, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 100, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_VerticalSparse.xml")]
        public class GridLayout_VerticalSparse : UIElement { }

        [Test]
        public void VerticalSparse() {
            MockApplication mockView = MockApplication.Setup<GridLayout_VerticalSparse>();
            GridLayout_VerticalSparse root = (GridLayout_VerticalSparse) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 300, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 200, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 200, 100, 100), root[4].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_VerticalDense.xml")]
        public class GridLayout_VerticalDense : UIElement { }

        [Test]
        public void VerticalDense() {
            MockApplication mockView = MockApplication.Setup<GridLayout_VerticalDense>();
            GridLayout_VerticalDense root = (GridLayout_VerticalDense) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 300, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 200, 100, 100), root[4].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_HorizontalSparse.xml")]
        public class GridLayout_HorizontalSparse : UIElement { }

        [Test]
        public void HorizontalSparse() {
            MockApplication mockView = MockApplication.Setup<GridLayout_HorizontalSparse>();
            GridLayout_HorizontalSparse root = (GridLayout_HorizontalSparse) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 300), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 100, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 200, 100, 100), root[4].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_HorizontalDense.xml")]
        public class GridLayout_HorizontalDense : UIElement { }

        [Test]
        public void HorizontalDense() {
            MockApplication mockView = MockApplication.Setup<GridLayout_HorizontalDense>();
            GridLayout_HorizontalDense root = (GridLayout_HorizontalDense) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 300), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 100, 100, 100), root[4].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_Margin_OffsetFromLayoutBox.xml")]
        public class GridLayout_Margin_OffsetFromLayoutBox : UIElement { }

        [Test]
        public void Margin_OffsetFromLayoutBox() {
            MockApplication mockView = MockApplication.Setup<GridLayout_Margin_OffsetFromLayoutBox>();
            GridLayout_Margin_OffsetFromLayoutBox root = (GridLayout_Margin_OffsetFromLayoutBox) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 100, 100), root[2].layoutResult.AllocatedRect);

            Assert.AreEqual(new Vector2(30, 0), root[0].layoutResult.alignedPosition);
            Assert.AreEqual(new Vector2(100, 30), root[1].layoutResult.alignedPosition);
            Assert.AreEqual(new Vector2(200, 0), root[2].layoutResult.alignedPosition);
        }

        [Template("Data/Layout/Grid/GridLayout_RowLocked_ColumnFlow.xml")]
        public class GridLayout_RowLocked_ColumnFlow : UIElement { }

        [Test]
        public void RowLocked_ColumnFlow() {
            MockApplication mockView = MockApplication.Setup<GridLayout_RowLocked_ColumnFlow>();
            GridLayout_RowLocked_ColumnFlow root = (GridLayout_RowLocked_ColumnFlow) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 100, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 200, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_GapUsage.xml")]
        public class GridLayout_GapUsage : UIElement { }

        [Test]
        public void GapUsage() {
            MockApplication mockView = MockApplication.Setup<GridLayout_GapUsage>();
            GridLayout_GapUsage root = (GridLayout_GapUsage) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(110, 0, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(220, 0, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 110, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(110, 110, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(220, 110, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_MixImplicitExplicit.xml")]
        public class GridLayout_MixImplicitExplicit : UIElement { }

        [Test]
        public void MixImplicitExplicit() {
            MockApplication mockView = MockApplication.Setup<GridLayout_MixImplicitExplicit>();
            GridLayout_MixImplicitExplicit root = (GridLayout_MixImplicitExplicit) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 200, 200), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 100, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(300, 0, 100, 200), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 100, 100), root[5].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 200, 100, 100), root[6].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_ExplicitPlaced_MinWidth.xml")]
        public class GridLayout_ExplicitPlaced_MinWidth : UIElement { }

        [Test]
        public void ExplicitPlaced_MinWidth() {
            MockApplication mockView = MockApplication.Setup<GridLayout_ExplicitPlaced_MinWidth>();
            GridLayout_ExplicitPlaced_MinWidth root = (GridLayout_ExplicitPlaced_MinWidth) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 50, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 0, 50, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(50, 0, 100, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(150, 0, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 50, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(50, 100, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_HorizontalBlockSize.xml")]
        public class GridLayout_HorizontalBlockSize : UIElement { }

        [Test]
        public void HorizontalBlockSize() {
            MockApplication mockView = MockApplication.Setup<GridLayout_HorizontalBlockSize>();
            GridLayout_HorizontalBlockSize root = (GridLayout_HorizontalBlockSize) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 300, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(300, 100, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 200, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_VerticalBlockSize.xml")]
        public class GridLayout_VerticalBlockSize : UIElement { }

        [Test]
        public void VerticalBlockSize() {
            MockApplication mockView = MockApplication.Setup<GridLayout_VerticalBlockSize>();
            GridLayout_VerticalBlockSize root = (GridLayout_VerticalBlockSize) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 100, 200), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 300), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 300, 100, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 100, 100), root[4].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 100, 100, 100), root[5].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/Grid/GridLayout_Align.xml")]
        public class GridLayout_Align : UIElement { }

        [Test]
        public void Align() {
            MockApplication mockView = MockApplication.Setup<GridLayout_Align>();
            GridLayout_Align root = (GridLayout_Align) mockView.RootElement;

            mockView.Update();

            Assert.AreEqual(new Vector2(0, 0), root[0].layoutResult.alignedPosition);
            Assert.AreEqual(new Vector2(100, 0), root[1].layoutResult.alignedPosition);
            Assert.AreEqual(new Vector2(200, 0), root[2].layoutResult.alignedPosition);

            root.style.SetAlignItemsHorizontal(0.5f, StyleState.Normal);
            root.style.SetAlignItemsVertical(0.5f, StyleState.Normal);

            mockView.Update();

            Assert.AreEqual(new Vector2(25, 25), root[0].layoutResult.alignedPosition);
            Assert.AreEqual(new Vector2(125, 25), root[1].layoutResult.alignedPosition);
            Assert.AreEqual(new Vector2(225, 25), root[2].layoutResult.alignedPosition);

            root.style.SetAlignItemsHorizontal(1f, StyleState.Normal);
            root.style.SetAlignItemsVertical(1f, StyleState.Normal);

            mockView.Update();

            Assert.AreEqual(new Vector2(50, 50), root[0].layoutResult.alignedPosition);
            Assert.AreEqual(new Vector2(150, 50), root[1].layoutResult.alignedPosition);
            Assert.AreEqual(new Vector2(250, 50), root[2].layoutResult.alignedPosition);
        }

    }

}