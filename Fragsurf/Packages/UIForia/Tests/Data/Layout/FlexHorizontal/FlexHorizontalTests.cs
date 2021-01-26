using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace Layout {

    public class FlexHorizontalTests {

        [Template("Data/Layout/FlexHorizontal/FlexHorizontal_DistributeSpaceHorizontal.xml")]
        public class FlexHorizontal_DistributeSpaceHorizontal : UIElement { }

        [Test]
        public void DistributeSpaceHorizontal_Default() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_DistributeSpaceHorizontal>();
            FlexHorizontal_DistributeSpaceHorizontal root = (FlexHorizontal_DistributeSpaceHorizontal) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(000, 0, 100, 500), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 500), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 100, 500), root[2].layoutResult.AllocatedRect);
        }

        [Test]
        public void DistributeSpaceHorizontal_AfterContent() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_DistributeSpaceHorizontal>();
            FlexHorizontal_DistributeSpaceHorizontal root = (FlexHorizontal_DistributeSpaceHorizontal) app.RootElement;
            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.AfterContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(000, 0, 100, 500), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 500), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 100, 500), root[2].layoutResult.AllocatedRect);
        }

        [Test]
        public void DistributeSpaceHorizontal_CenterContent() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_DistributeSpaceHorizontal>();
            FlexHorizontal_DistributeSpaceHorizontal root = (FlexHorizontal_DistributeSpaceHorizontal) app.RootElement;

            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.CenterContent, StyleState.Normal);

            app.Update();

            Assert.AreEqual(new Rect(100, 0, 100, 500), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 100, 500), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(300, 0, 100, 500), root[2].layoutResult.AllocatedRect);
        }


        [Test]
        public void DistributeSpaceHorizontal_BeforeContent() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_DistributeSpaceHorizontal>();
            FlexHorizontal_DistributeSpaceHorizontal root = (FlexHorizontal_DistributeSpaceHorizontal) app.RootElement;

            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.BeforeContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(200, 0, 100, 500), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(300, 0, 100, 500), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 0, 100, 500), root[2].layoutResult.AllocatedRect);
        }

        [Test]
        public void DistributeSpaceHorizontal_AroundContent() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_DistributeSpaceHorizontal>();
            FlexHorizontal_DistributeSpaceHorizontal root = (FlexHorizontal_DistributeSpaceHorizontal) app.RootElement;

            // makes math cleaner
            root.style.SetPreferredWidth(600f, StyleState.Normal);
            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.AroundContent, StyleState.Normal);

            app.Update();

            Assert.AreEqual(new Rect(50, 0, 100, 500), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(250, 0, 100, 500), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(450, 0, 100, 500), root[2].layoutResult.AllocatedRect);
        }
        
        [Test]
        public void DistributeSpaceHorizontal_AroundContentIgnoresGap() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_DistributeSpaceHorizontal>();
            FlexHorizontal_DistributeSpaceHorizontal root = (FlexHorizontal_DistributeSpaceHorizontal) app.RootElement;
            
            root.style.SetFlexLayoutGapHorizontal(20, StyleState.Normal);
            root.style.SetFlexLayoutGapVertical(20, StyleState.Normal);

            // makes math cleaner
            root.style.SetPreferredWidth(600f, StyleState.Normal);
            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.AroundContent, StyleState.Normal);

            app.Update();

            Assert.AreEqual(new Rect(50, 0, 100, 500), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(250, 0, 100, 500), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(450, 0, 100, 500), root[2].layoutResult.AllocatedRect);
        }

        [Test]
        public void DistributeSpaceHorizontal_BetweenContent() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_DistributeSpaceHorizontal>();
            FlexHorizontal_DistributeSpaceHorizontal root = (FlexHorizontal_DistributeSpaceHorizontal) app.RootElement;

            // makes math cleaner
            root.style.SetPreferredWidth(600f, StyleState.Normal);
            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.BetweenContent, StyleState.Normal);

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 500), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(250, 0, 100, 500), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(500, 0, 100, 500), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontal/FlexHorizontal_GrowUnconstrained.xml")]
        public class FlexHorizontal_GrowUnconstrained : UIElement { }

        [Test]
        public void GrowUnconstrained() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_GrowUnconstrained>();
            FlexHorizontal_GrowUnconstrained root = (FlexHorizontal_GrowUnconstrained) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 400, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(600, 0, 200, 100), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontal/FlexHorizontal_GrowConstrained.xml")]
        public class FlexHorizontal_GrowConstrained : UIElement { }

        [Test]
        public void GrowConstrained() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_GrowConstrained>();
            FlexHorizontal_GrowConstrained root = (FlexHorizontal_GrowConstrained) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 350, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(350, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(550, 0, 350, 100), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontal/FlexHorizontal_GrowWithExtraSpace.xml")]
        public class FlexHorizontal_GrowWithExtraSpace : UIElement { }

        [Test]
        public void GrowWithExtraSpace() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_GrowWithExtraSpace>();
            FlexHorizontal_GrowWithExtraSpace root = (FlexHorizontal_GrowWithExtraSpace) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(200, 0, 300, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(500, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(700, 0, 300, 100), root[2].layoutResult.AllocatedRect);
        }


        [Template("Data/Layout/FlexHorizontal/FlexHorizontal_RespectMarginHorizontal.xml")]
        public class FlexHorizontal_RespectMarginHorizontal : UIElement { }

        [Test]
        public void RespectMarginHorizontal() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_RespectMarginHorizontal>();
            FlexHorizontal_RespectMarginHorizontal root = (FlexHorizontal_RespectMarginHorizontal) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(10, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(310, 0, 100, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(1220, 0, 100, 100), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontal/FlexHorizontal_ShrinkUnconstrained.xml")]
        public class FlexHorizontal_ShrinkUnconstrained : UIElement { }

        [Test]
        public void ShrinkUnconstrained() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_ShrinkUnconstrained>();
            FlexHorizontal_ShrinkUnconstrained root = (FlexHorizontal_ShrinkUnconstrained) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 0, 200, 100), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontal/FlexHorizontal_ShrinkConstrained.xml")]
        public class FlexHorizontal_ShrinkConstrained : UIElement { }

        [Test]
        public void ShrinkConstrained() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_ShrinkConstrained>();
            FlexHorizontal_ShrinkConstrained root = (FlexHorizontal_ShrinkConstrained) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 175, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(175, 0, 250, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(425, 0, 175, 100), root[2].layoutResult.AllocatedRect);
        }
        
        [Template("Data/Layout/FlexHorizontal/FlexHorizontal_ShrinkWithOverflow.xml")]
        public class FlexHorizontal_ShrinkWithOverflow : UIElement { }

        [Test]
        public void ShrinkWithOverflow() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_ShrinkWithOverflow>();
            FlexHorizontal_ShrinkWithOverflow root = (FlexHorizontal_ShrinkWithOverflow) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 250, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(250, 0, 250, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(500, 0, 250, 100), root[2].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontal/FlexHorizontal_Gap.xml")] 
        public class FlexHorizontal_Gap : UIElement { 
            public UIElement containerGap => children[0];
            public UIElement containerGapHorizontal => children[1];
            public UIElement containerGapVertical => children[2];
        }

        [Test]
        public void Gap() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_Gap>();
            FlexHorizontal_Gap root = (FlexHorizontal_Gap)app.RootElement;

            app.Update();

            UIElement container = root.containerGap;
            Assert.AreEqual(new Rect(0, 0, 100, 500), container[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(110, 0, 100, 500), container[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(220, 0, 100, 500), container[2].layoutResult.AllocatedRect);
            
            container = root.containerGapHorizontal;
            Assert.AreEqual(new Rect(0, 0, 100, 500), container[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(110, 0, 100, 500), container[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(220, 0, 100, 500), container[2].layoutResult.AllocatedRect);
            
            // gap not applied
            container = root.containerGapVertical;
            Assert.AreEqual(new Rect(0, 0, 100, 500), container[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(100, 0, 100, 500), container[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 100, 500), container[2].layoutResult.AllocatedRect);
        }
        
        [Template("Data/Layout/FlexHorizontal/FlexHorizontal_GapContentSize.xml")] 
        public class FlexHorizontal_GapContentSize : UIElement { 
        }
        
        [Test]
        public void GapContentSize() {
            MockApplication app = MockApplication.Setup<FlexHorizontal_GapContentSize>();
            FlexHorizontal_GapContentSize root = (FlexHorizontal_GapContentSize)app.RootElement;

            app.Update();
            
            Assert.AreEqual(new Rect(0, 0, 320, 100), root.layoutResult.AllocatedRect);
        }
    }
    
}