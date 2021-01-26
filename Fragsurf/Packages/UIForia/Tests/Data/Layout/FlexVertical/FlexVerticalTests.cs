using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace Layout {

    public class FlexVerticalTests {

        [Template("Data/Layout/FlexVertical/FlexVertical_DistributeSpaceVertical.xml")]
        public class FlexVertical_DistributeSpaceVertical : UIElement { }

        [Test]
        public void DistributeSpaceVertical_Default() {
            MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>();
            FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 500, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 500, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 500, 100), root[2].layoutResult.AllocatedRect);
        }

        [Test]
        public void DistributeSpaceVertical_AfterContent() {
            MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>();
            FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;
            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.AfterContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 0, 500, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 500, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 500, 100), root[2].layoutResult.AllocatedRect);
        }

        [Test]
        public void DistributeSpaceVertical_CenterContent() {
            MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>();
            FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;

            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.CenterContent, StyleState.Normal);

            app.Update();

            Assert.AreEqual(new Rect(0, 100, 500, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 500, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 300, 500, 100), root[2].layoutResult.AllocatedRect);
        }


        [Test]
        public void DistributeSpaceVertical_BeforeContent() {
            MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>();
            FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;

            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.BeforeContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 200, 500, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 300, 500, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 400, 500, 100), root[2].layoutResult.AllocatedRect);
        }

        [Test]
        public void DistributeSpaceVertical_AroundContent() {
            MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>();
            FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;

            // makes math cleaner
            root.style.SetPreferredHeight(600f, StyleState.Normal);
            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.AroundContent, StyleState.Normal);

            app.Update();

            Assert.AreEqual(new Rect(0, 50, 500, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 250, 500, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 450, 500, 100), root[2].layoutResult.AllocatedRect);
        }
        
        [Test]
        public void DistributeSpaceVertical_AroundContentIgnoresGap() {
            MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>();
            FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;
            
            root.style.SetFlexLayoutGapHorizontal(20, StyleState.Normal);
            root.style.SetFlexLayoutGapVertical(20, StyleState.Normal);

            // makes math cleaner
            root.style.SetPreferredHeight(600f, StyleState.Normal);
            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.AroundContent, StyleState.Normal);

            app.Update();

            Assert.AreEqual(new Rect(0, 50, 500, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 250, 500, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 450, 500, 100), root[2].layoutResult.AllocatedRect);
        }

        [Test]
        public void DistributeSpaceVertical_BetweenContent() {
            MockApplication app = MockApplication.Setup<FlexVertical_DistributeSpaceVertical>();
            FlexVertical_DistributeSpaceVertical root = (FlexVertical_DistributeSpaceVertical) app.RootElement;

            root.style.SetPreferredHeight(600f, StyleState.Normal);
            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.BetweenContent, StyleState.Normal);

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 500, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 250, 500, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 500, 500, 100), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexVertical/FlexVertical_GrowUnconstrained.xml")]
        public class FlexVertical_GrowUnconstrained : UIElement { }

        [Test]
        public void GrowUnconstrained() {
            MockApplication app = MockApplication.Setup<FlexVertical_GrowUnconstrained>();
            FlexVertical_GrowUnconstrained root = (FlexVertical_GrowUnconstrained) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 200), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 100, 400), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 600, 100, 200), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexVertical/FlexVertical_GrowConstrained.xml")]
        public class FlexVertical_GrowConstrained : UIElement { }

        [Test]
        public void GrowConstrained() {
            MockApplication app = MockApplication.Setup<FlexVertical_GrowConstrained>();
            FlexVertical_GrowConstrained root = (FlexVertical_GrowConstrained) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 350), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 350, 100, 200), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 550, 100, 350), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexVertical/FlexVertical_GrowWithExtraSpace.xml")]
        public class FlexVertical_GrowWithExtraSpace : UIElement { }

        [Test]
        public void GrowWithExtraSpace() {
            MockApplication app = MockApplication.Setup<FlexVertical_GrowWithExtraSpace>();
            FlexVertical_GrowWithExtraSpace root = (FlexVertical_GrowWithExtraSpace) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 200, 100, 300), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 500, 100, 200), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 700, 100, 300), root[2].layoutResult.AllocatedRect);
        }


        [Template("Data/Layout/FlexVertical/FlexVertical_RespectMarginVertical.xml")]
        public class FlexVertical_RespectMarginVertical : UIElement { }

        [Test]
        public void RespectMarginVertical() {
            MockApplication app = MockApplication.Setup<FlexVertical_RespectMarginVertical>();
            FlexVertical_RespectMarginVertical root = (FlexVertical_RespectMarginVertical) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 10, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 310, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 1220, 100, 100), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexVertical/FlexVertical_ShrinkUnconstrained.xml")]
        public class FlexVertical_ShrinkUnconstrained : UIElement { }

        [Test]
        public void ShrinkUnconstrained() {
            MockApplication app = MockApplication.Setup<FlexVertical_ShrinkUnconstrained>();
            FlexVertical_ShrinkUnconstrained root = (FlexVertical_ShrinkUnconstrained) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 200), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 100, 200), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 400, 100, 200), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexVertical/FlexVertical_ShrinkConstrained.xml")]
        public class FlexVertical_ShrinkConstrained : UIElement { }

        [Test]
        public void ShrinkConstrained() {
            MockApplication app = MockApplication.Setup<FlexVertical_ShrinkConstrained>();
            FlexVertical_ShrinkConstrained root = (FlexVertical_ShrinkConstrained) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0,   100, 175), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 175, 100, 250), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 425, 100, 175), root[2].layoutResult.AllocatedRect);
        }
        
        [Template("Data/Layout/FlexVertical/FlexVertical_ShrinkWithOverflow.xml")]
        public class FlexVertical_ShrinkWithOverflow : UIElement { }

        [Test]
        public void ShrinkWithOverflow() {
            MockApplication app = MockApplication.Setup<FlexVertical_ShrinkWithOverflow>();
            FlexVertical_ShrinkWithOverflow root = (FlexVertical_ShrinkWithOverflow) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0,   100, 250), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 250, 100, 250), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 500, 100, 250), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexVertical/FlexVertical_Gap.xml")]
        public class FlexVertical_Gap : UIElement {
            public UIElement containerGap => children[0];
            public UIElement containerGapHorizontal => children[1];
            public UIElement containerGapVertical => children[2];
        }

        [Test]
        public void Gap() {
            MockApplication app = MockApplication.Setup<FlexVertical_Gap>();
            FlexVertical_Gap root = (FlexVertical_Gap)app.RootElement;

            app.Update();

            UIElement container = root.containerGap;
            Assert.AreEqual(new Rect(0, 0, 100, 100), container[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 110, 100, 100), container[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 220, 100, 100), container[2].layoutResult.AllocatedRect);
            
            // gap not applied
            container = root.containerGapHorizontal;
            Assert.AreEqual(new Rect(0, 0, 100, 100), container[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 100, 100), container[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 100, 100), container[2].layoutResult.AllocatedRect);
            
            container = root.containerGapVertical;
            Assert.AreEqual(new Rect(0, 0, 100, 100), container[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 110, 100, 100), container[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 220, 100, 100), container[2].layoutResult.AllocatedRect);
        }
        
        [Template("Data/Layout/FlexVertical/FlexVertical_GapContentSize.xml")]
        public class FlexVertical_GapContentSize : UIElement {
        }

        [Test]
        public void GapContentSize() {
            MockApplication app = MockApplication.Setup<FlexVertical_GapContentSize>();
            FlexVertical_GapContentSize root = (FlexVertical_GapContentSize)app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 320), root.layoutResult.AllocatedRect);
        }
    }
}