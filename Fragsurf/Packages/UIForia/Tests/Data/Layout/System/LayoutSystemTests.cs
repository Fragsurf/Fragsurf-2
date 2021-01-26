using System.Collections.Generic;
using NUnit.Framework;
using Tests.Data.Layout;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace LayoutSystem {

    public class LayoutSystemTests {

        [Template("Data/Layout/System/LayoutSystemTest_General.xml")]
        public class LayoutTestThing : UIElement {

            public UIGroupElement child0;
            public UIGroupElement child1;
            public UIGroupElement child2;
            public List<int> list;

            public override void OnCreate() {
                child0 = FindById<UIGroupElement>("child0");
                child1 = FindById<UIGroupElement>("child1");
                child2 = FindById<UIGroupElement>("child2");
            }

        }

        [Test]
        public void Works() {
            MockApplication app = MockApplication.Setup<LayoutTestThing>();
            app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
            LayoutTestThing root = (LayoutTestThing) app.RootElement;
            app.Update();
            Assert.AreEqual(300, root.child0.layoutResult.actualSize.width);
            Assert.AreEqual(100, root.child0.layoutResult.actualSize.height);
            Assert.AreEqual(new Vector2(0, 100), root.child1.layoutResult.localPosition);
            Assert.AreEqual(100, root.child1.layoutResult.actualSize.width);
            Assert.AreEqual(100, root.child1.layoutResult.actualSize.height);

            Assert.AreEqual(new Vector2(0, 200), root.child2.layoutResult.localPosition);
            Assert.AreEqual(100, root.child2.layoutResult.actualSize.width);
            Assert.AreEqual(100, root.child2.layoutResult.actualSize.height);
        }

        [Test]
        public void Updates() {
            MockApplication app = MockApplication.Setup<LayoutTestThing>();
            app.SetViewportRect(new Rect(0, 0, 1000f, 1000f));
            LayoutTestThing root = (LayoutTestThing) app.RootElement;
            app.Update();
            Assert.AreEqual(Vector2.zero, root.child0.layoutResult.localPosition);
            Assert.AreEqual(new Vector2(0, 100), root.child1.layoutResult.localPosition);
            Assert.AreEqual(100, root.child1.layoutResult.actualSize.width);
            Assert.AreEqual(100, root.child1.layoutResult.actualSize.height);

            Assert.AreEqual(new Vector2(0, 200), root.child2.layoutResult.localPosition);
            Assert.AreEqual(100, root.child2.layoutResult.actualSize.width);
            Assert.AreEqual(100, root.child2.layoutResult.actualSize.height);

            root.child2.style.SetPreferredWidth(200, StyleState.Normal);
            app.Update();
            Assert.AreEqual(Vector2.zero, root.child0.layoutResult.localPosition);
            Assert.AreEqual(new Vector2(0, 100), root.child1.layoutResult.localPosition);
            Assert.AreEqual(100, root.child1.layoutResult.actualSize.width);
            Assert.AreEqual(100, root.child1.layoutResult.actualSize.height);

            Assert.AreEqual(new Vector2(0, 200), root.child2.layoutResult.localPosition);
            Assert.AreEqual(200, root.child2.layoutResult.actualSize.width);
            Assert.AreEqual(100, root.child2.layoutResult.actualSize.height);
        }

        [Template("Data/Layout/System/LayoutSystemTest_General.xml#content_sized")]
        public class LayoutSystemTest_Content : UIElement { }

        [Test]
        public void ContentSized() {
            MockApplication app = MockApplication.Setup<LayoutSystemTest_Content>();
            LayoutSystemTest_Content root = (LayoutSystemTest_Content) app.RootElement;
            app.Update();
            Assert.AreEqual(new Rect(0, 100, 300, 50), root[1].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/System/LayoutSystemTest_General.xml#max_size_changes")]
        public class LayoutSystemTest_MaxSizeChanges : UIElement { }

        [Test]
        public void MaxSizeChanges() {
            MockApplication app = MockApplication.Setup<LayoutSystemTest_MaxSizeChanges>();
            LayoutSystemTest_MaxSizeChanges root = (LayoutSystemTest_MaxSizeChanges) app.RootElement;
            app.Update();
            Assert.AreEqual(new Rect(0, 100, 300, 50), root[1].layoutResult.AllocatedRect);
            root[1].style.SetMaxWidth(150f, StyleState.Normal);
            app.Update();
            Assert.AreEqual(new Rect(0, 100, 150, 50), root[1].layoutResult.AllocatedRect);
        }


        [Template("Data/Layout/System/LayoutSystemTest_General.xml#width_changes_to_content")]
        public class LayoutSystemTest_WidthChangesToContent : UIElement { }

        [Test]
        public void WidthSizeConstraintChangesToContent() {
            MockApplication app = MockApplication.Setup<LayoutSystemTest_WidthChangesToContent>();
            LayoutSystemTest_WidthChangesToContent root = (LayoutSystemTest_WidthChangesToContent) app.RootElement;
            app.Update();
            Assert.AreEqual(new Rect(0, 100, 400, 50), root[1].layoutResult.AllocatedRect);
            root[1].style.SetMaxWidth(UIMeasurement.Content100, StyleState.Normal);
            app.Update();
            Assert.AreEqual(new Rect(0, 100, 300, 50), root[1].layoutResult.AllocatedRect);
            root[1][0].style.SetPreferredWidth(150f, StyleState.Normal);
            app.Update();
            Assert.AreEqual(new Rect(0, 100, 150, 50), root[1].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/System/LayoutSystemTest_General.xml#height_changes_to_maxcontent")]
        public class LayoutSystemTest_HeightChangesToMaxContent : UIElement { }

        [Test]
        public void HeightSizeConstraintChangesToMaxContent() {
            MockApplication app = MockApplication.Setup<LayoutSystemTest_HeightChangesToMaxContent>();
            LayoutSystemTest_HeightChangesToMaxContent root = (LayoutSystemTest_HeightChangesToMaxContent) app.RootElement;

            app.Update();
            Assert.AreEqual(new Rect(0, 100, 400, 300), root[1].layoutResult.AllocatedRect);

            root[1].style.SetMaxHeight(UIMeasurement.Content100, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 100, 400, 50), root[1].layoutResult.AllocatedRect);
        }


        [Template("Data/Layout/System/LayoutSystemTest_General.xml#height_changes_to_mincontent")]
        public class LayoutSystemTest_HeightChangesToMinContent : UIElement { }

        [Test]
        public void HeightSizeConstraintChangesToMinContent() {
            MockApplication app = MockApplication.Setup<LayoutSystemTest_HeightChangesToMinContent>();
            LayoutSystemTest_HeightChangesToMinContent root = (LayoutSystemTest_HeightChangesToMinContent) app.RootElement;

            app.Update();
            Assert.AreEqual(new Rect(0, 100, 400, 300), root[1].layoutResult.AllocatedRect);

            root[1].style.SetMinHeight(UIMeasurement.Content100, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 100, 400, 500), root[1].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/System/LayoutSystemTest_General.xml#child_enabled")]
        public class LayoutSystemTest_ChildEnabled : UIElement { }

        [Test]
        public void ChildEnabled() {
            MockApplication app = MockApplication.Setup<LayoutSystemTest_ChildEnabled>();
            app.Update();
            LayoutSystemTest_ChildEnabled root = (LayoutSystemTest_ChildEnabled) app.RootElement;
            Assert.IsFalse(root[1][0].isEnabled);
            Assert.AreEqual(new Rect(0, 100, 200, 50), root[1].layoutResult.AllocatedRect);
            root[1][0].SetEnabled(true);
            Assert.IsTrue(root[1][0].isEnabled);
            app.Update();
            Assert.IsTrue(root[1][0].isEnabled);
            Assert.AreEqual(new Rect(0, 100, 300, 100), root[1].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/System/LayoutSystemTest_General.xml#child_disabled")]
        public class LayoutSystemTest_ChildDisabled : UIElement { }

        [Test]
        public void ChildDisabled() {
            MockApplication app = MockApplication.Setup<LayoutSystemTest_ChildDisabled>();
            app.Update();
            LayoutSystemTest_ChildDisabled root = (LayoutSystemTest_ChildDisabled) app.RootElement;

            Assert.IsTrue(root[1][0].isEnabled);
            Assert.AreEqual(new Rect(0, 100, 300, 100), root[1].layoutResult.AllocatedRect);

            root[1][0].SetEnabled(false);

            app.Update();
            Assert.IsFalse(root[1][0].isEnabled);
            Assert.AreEqual(new Rect(0, 100, 200, 50), root[1].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/System/LayoutSystemTest_General.xml#screen_pos_updated")]
        public class LayoutSystemTest_ScreenPositionUpdated : UIElement { }

        [Test]
        public void ScreenPositionsGetUpdated() {
            MockApplication app = MockApplication.Setup<LayoutSystemTest_ScreenPositionUpdated>();

            LayoutSystemTest_ScreenPositionUpdated root = (LayoutSystemTest_ScreenPositionUpdated) app.RootElement;
            UIElement nestedChild1 = root[1][0];
            UIElement nestedChild2 = root[1][1];
            app.Update();

            Assert.AreEqual(new Rect(0, 000, 100, 100), root[0].layoutResult.ScreenRect);
            Assert.AreEqual(new Rect(0, 100, 100, 200), root[1].layoutResult.ScreenRect);
            Assert.AreEqual(new Rect(0, 100, 100, 100), nestedChild1.layoutResult.ScreenRect);
            Assert.AreEqual(new Rect(0, 200, 100, 100), nestedChild2.layoutResult.ScreenRect);
            Assert.AreEqual(new Rect(0, 300, 100, 100), root[2].layoutResult.ScreenRect);

            nestedChild1.style.SetPreferredHeight(500f, StyleState.Normal);
            app.Update();
            Assert.AreEqual(new Rect(0, 000, 100, 100), root[0].layoutResult.ScreenRect);
            Assert.AreEqual(new Rect(0, 100, 100, 600), root[1].layoutResult.ScreenRect);
            Assert.AreEqual(new Rect(0, 100, 100, 500), nestedChild1.layoutResult.ScreenRect);
            Assert.AreEqual(new Rect(0, 600, 100, 100), nestedChild2.layoutResult.ScreenRect);
            Assert.AreEqual(new Rect(0, 700, 100, 100), root[2].layoutResult.ScreenRect);
        }

        [Test]
        public void RunWidthLayout() {
            MockApplication app = MockApplication.Setup<BasicLayoutTest_GathersDirtyData>();

            BasicLayoutTest_GathersDirtyData root = app.RootElement as BasicLayoutTest_GathersDirtyData;

            app.Update();

            UIElement one = root[0];
            UIElement two = root[1];
            UIElement three = root[2];

            Assert.AreEqual(3, root.layoutBox.childCount);
            Assert.AreEqual(100, one.layoutBox.finalWidth);
            Assert.AreEqual(100, two.layoutBox.finalWidth);
            Assert.AreEqual(100, three.layoutBox.finalWidth);

            Assert.AreEqual(0, one.layoutResult.localPosition.x);
            Assert.AreEqual(100, two.layoutResult.localPosition.x);
            Assert.AreEqual(200, three.layoutResult.localPosition.x);

            root.FindById("one").SetEnabled(false);

            app.Update();

            Assert.AreEqual(2, root.layoutBox.childCount);
            Assert.AreEqual(100, two.layoutBox.finalWidth);
            Assert.AreEqual(100, three.layoutBox.finalWidth);

            Assert.AreEqual(0, two.layoutResult.localPosition.x);
            Assert.AreEqual(100, three.layoutResult.localPosition.x);
        }

        [Test]
        public void HandleEnableDisableElementWithContentAncestor() {
            MockApplication app = MockApplication.Setup<BasicLayoutTest_ContentAncestor>();
            BasicLayoutTest_ContentAncestor root = app.RootElement as BasicLayoutTest_ContentAncestor;

            app.Update();

            UIElement toggle = root["disable-me"];
            UIElement wrapper = root[0];

            Assert.AreEqual(200, root.layoutBox.finalWidth);
            Assert.AreEqual(200, wrapper.layoutBox.finalWidth);
            Assert.AreEqual(100, toggle.layoutBox.finalWidth);

            toggle.SetEnabled(false);

            app.Update();

            Assert.AreEqual(100, root.layoutBox.finalWidth);
            Assert.AreEqual(100, wrapper.layoutBox.finalWidth);

            toggle.SetEnabled(true);

            app.Update();

            Assert.AreEqual(200, root.layoutBox.finalWidth);
            Assert.AreEqual(200, wrapper.layoutBox.finalWidth);
            Assert.AreEqual(100, toggle.layoutBox.finalWidth);
        }

        [Test]
        public void RespondToBlockSizeChange() {
            MockApplication app = MockApplication.Setup<BasicLayoutTest_BlockSizeChanges>();

            UIElement viewRoot = app.RootElement;

            BasicLayoutTest_BlockSizeChanges root = viewRoot as BasicLayoutTest_BlockSizeChanges;

            UIElement blockProvider = root[0];
            UIElement one = blockProvider[0];
            UIElement two = blockProvider[1];
            UIElement contentSize = blockProvider[2];
            UIElement blockUser = contentSize[0];

            app.Update();

            Assert.AreEqual(3, blockProvider.layoutBox.childCount);
            Assert.AreEqual(300, blockUser.layoutBox.finalWidth);

            blockProvider.style.SetPreferredWidth(200f, StyleState.Normal);
            app.Update();

//        Assert.IsTrue(root.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 1));
//        Assert.IsTrue(blockProvider.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 1));
//        Assert.IsTrue(contentSize.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 1));
//        Assert.IsTrue(blockUser.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 1));
//        Assert.IsFalse(one.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 1));
//        Assert.IsFalse(two.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 1));
//        
            Assert.AreEqual(200, blockProvider.layoutBox.finalWidth);
            Assert.AreEqual(200, blockUser.layoutBox.finalWidth);
//        
//        app.Update();
//        
//        Assert.IsFalse(root.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 2));
//        Assert.IsFalse(blockProvider.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 2));
//        Assert.IsFalse(contentSize.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 2));
//        Assert.IsFalse(blockUser.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 2));
//        Assert.IsFalse(one.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 2));
//        Assert.IsFalse(two.layoutHistory.RanLayoutInFrame(LayoutDirection.Horizontal, 2));
        }

        [Test]
        public void UseViewBlockSize() {
            MockApplication app = MockApplication.Setup<BasicLayoutTest_BlockSizeChanges>();

            UIElement viewRoot = app.RootElement;

            BasicLayoutTest_BlockSizeChanges root = viewRoot as BasicLayoutTest_BlockSizeChanges;

            UIElement blockProvider = root[0];
            UIElement one = blockProvider[0];
            UIElement two = blockProvider[1];
            UIElement contentSize = blockProvider[2];
            UIElement blockUser = contentSize[0];

            app.GetView(0).SetSize(1920, 1080);

            blockProvider.style.SetPreferredWidth(new UIMeasurement(1f, UIMeasurementUnit.Content), StyleState.Normal);

            app.Update();

            Assert.AreEqual(3, blockProvider.layoutBox.childCount);
            Assert.AreEqual(1920, blockUser.layoutBox.finalWidth);
        }

        //     [Template("Data/Layout/System/LayoutSystemTest_Sorting.xml#sort_layers_ascending")]
        //     public class LayoutSystemTest_SortLayersAscending : UIElement { }
        //
        //     [Test]
        //     public void SortByLayersInAscendingOrder() {
        //
        //         MockApplication app = MockApplication.Setup<LayoutSystemTest_SortLayersAscending>();
        //         LayoutSystemTest_SortLayersAscending root = (LayoutSystemTest_SortLayersAscending) app.RootElement;
        //         app.SetViewportRect(new Rect(0, 0, 400, 400));
        //
        //         app.Update();
        //
        //         LightList<UIElement> elements = app.LayoutSystem.GetVisibleElements();
        //         
        //         Assert.AreEqual(root[1], elements[0]);
        //         Assert.AreEqual(root[3], elements[1]);
        //         Assert.AreEqual(root[2], elements[2]);
        //         Assert.AreEqual(root[0], elements[3]);
        //         Assert.AreEqual(root, elements[4]);
        //     }
        //
        //     [Test]
        //     public void SortByZIndexInAscendingOrder() {
        //         string template = @"
        //     <UITemplate>
        //         <Contents style.preferredWidth='100f' style.preferredHeight='200f'>
        //             <Group x-id='child0' style.preferredWidth='100f' style.preferredHeight='100f'/>
        //             <Group x-id='child1' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
        //             <Group x-id='child2' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
        //             <Group x-id='child3' style.preferredWidth='100f' style.preferredHeight='100f'/>
        //         </Contents>
        //     </UITemplate>
        //     ";
        //         MockApplication app = MockApplication.Setup<LayoutTestThing>();
        //         LayoutTestThing root = (LayoutTestThing) app.RootElement;
        //         app.SetViewportRect(new Rect(0, 0, 400, 400));
        //
        //         app.Update();
        //
        //         LightList<UIElement> elements = app.LayoutSystem.GetVisibleElements();
        //         Assert.AreEqual(root.FindById("child1"), elements[0]);
        //         Assert.AreEqual(root.FindById("child2"), elements[1]);
        //         Assert.AreEqual(root.FindById("child3"), elements[2]);
        //         Assert.AreEqual(root.FindById("child0"), elements[3]);
        //         Assert.AreEqual(root, elements[4]);
        //     }
        //
        //     [Test]
        //     public void SortByLayerAndZIndexInAscendingOrder() {
        //         string template = @"
        //     <UITemplate>
        //         <Contents style.preferredWidth='100f' style.preferredHeight='200f'>
        //             <Group x-id='e0' style.preferredWidth='100f' style.preferredHeight='100f'>
        //                 <Group x-id='e1' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
        //                 <Group x-id='e2' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
        //                 <Group x-id='e3' style.preferredWidth='100f' style.preferredHeight='100f'/>
        //                 <Group x-id='e4'>
        //                     <Group x-id='e5' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
        //                     <Group x-id='e6' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
        //                     <Group x-id='e7' style.preferredWidth='100f' style.preferredHeight='100f'/>
        //                 </Group>
        //             </Group>
        //             <Group x-id='e8' style.preferredWidth='100f' style.preferredHeight='100f'>
        //                 <Group x-id='e9' style.preferredWidth='100f' style.preferredHeight='100f'/>
        //                 <Group x-id='e10' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
        //                 <Group x-id='e11' style.preferredWidth='100f' style.preferredHeight='100f'/>
        //                 <Group x-id='e12'>
        //                     <Group x-id='e13' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
        //                     <Group x-id='e14' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
        //                     <Group x-id='e15' style.preferredWidth='100f' style.preferredHeight='100f'/>
        //                 </Group>
        //             </Group>
        //             <Group x-id='e16' style.preferredWidth='100f' style.preferredHeight='100f'>
        //                 <Group x-id='e17' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
        //                 <Group x-id='e18' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
        //                 <Group x-id='e19' style.preferredWidth='100f' style.preferredHeight='100f'/>
        //                 <Group x-id='e20'>
        //                     <Group x-id='e21' style.preferredWidth='100f' style.layer='1' style.preferredHeight='100f'/>
        //                     <Group x-id='e22' style.preferredWidth='100f' style.zIndex='1' style.preferredHeight='100f'/>
        //                     <Group x-id='e23' style.preferredWidth='100f' style.preferredHeight='100f'/>
        //                 </Group>
        //             </Group>
        //         </Contents>
        //     </UITemplate>
        //     ";
        //         MockApplication app = MockApplication.Setup<LayoutTestThing>();
        //         LayoutTestThing root = (LayoutTestThing) app.RootElement;
        //         app.SetViewportRect(new Rect(0, 0, 400, 400));
        //
        //         app.Update();
        //
        //         LightList<UIElement> elements = app.LayoutSystem.GetVisibleElements();
        //
        //         Assert.AreEqual(root.FindById("e21"), elements[0]);
        //         Assert.AreEqual(root.FindById("e17"), elements[1]);
        //         Assert.AreEqual(root.FindById("e13"), elements[2]);
        //         Assert.AreEqual(root.FindById("e5"), elements[3]);
        //         Assert.AreEqual(root.FindById("e1"), elements[4]);
        //         Assert.AreEqual(root.FindById("e22"), elements[5]);
        //         Assert.AreEqual(root.FindById("e18"), elements[6]);
        //         Assert.AreEqual(root.FindById("e14"), elements[7]);
        //         Assert.AreEqual(root.FindById("e10"), elements[8]);
        //         Assert.AreEqual(root.FindById("e6"), elements[9]);
        //         Assert.AreEqual(root.FindById("e2"), elements[10]);
        //         Assert.AreEqual(root.FindById("e23"), elements[11]);
        //         Assert.AreEqual(root.FindById("e20"), elements[12]);
        //         Assert.AreEqual(root.FindById("e19"), elements[13]);
        //         Assert.AreEqual(root.FindById("e16"), elements[14]);
        //         Assert.AreEqual(root.FindById("e15"), elements[15]);
        //         Assert.AreEqual(root.FindById("e12"), elements[16]);
        //         Assert.AreEqual(root.FindById("e11"), elements[17]);
        //         Assert.AreEqual(root.FindById("e9"), elements[18]);
        //         Assert.AreEqual(root.FindById("e8"), elements[19]);
        //         Assert.AreEqual(root.FindById("e7"), elements[20]);
        //         Assert.AreEqual(root.FindById("e4"), elements[21]);
        //         Assert.AreEqual(root.FindById("e3"), elements[22]);
        //         Assert.AreEqual(root.FindById("e0"), elements[23]);
        //         Assert.AreEqual(root, elements[24]);
        //     }
        //

    }

}