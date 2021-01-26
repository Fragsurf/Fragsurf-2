using NUnit.Framework;
using Tests.Mocks;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UnityEngine;

namespace Layout {

    public class FlexHorizontalWrapTests {

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_NoWrap.xml")]
        public class FlexHorizontalWrap_NoWrap : UIElement { }

        [Test]
        public void NoWrap() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_NoWrap>();
            FlexHorizontalWrap_NoWrap root = (FlexHorizontalWrap_NoWrap) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_WrapWhenTrackFull.xml")]
        public class FlexHorizontalWrap_WrapWhenTrackFull : UIElement { }

        [Test]
        public void WrapWhenTrackFull() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_WrapWhenTrackFull>();
            FlexHorizontalWrap_WrapWhenTrackFull root = (FlexHorizontalWrap_WrapWhenTrackFull) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 0, 200, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 200, 100), root[3].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_WrapWhenItemTooBig.xml")]
        public class FlexHorizontalWrap_WrapWhenItemTooBig : UIElement { }

        [Test]
        public void WrapWhenItemTooBig() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_WrapWhenItemTooBig>();
            FlexHorizontalWrap_WrapWhenItemTooBig root = (FlexHorizontalWrap_WrapWhenItemTooBig) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 800, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 200, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 200, 200, 100), root[3].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_WrapWhenItemOverflows.xml")]
        public class FlexHorizontalWrap_WrapWhenItemOverflows : UIElement { }

        [Test]
        public void WrapWhenItemOverflows() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_WrapWhenItemOverflows>();
            FlexHorizontalWrap_WrapWhenItemOverflows root = (FlexHorizontalWrap_WrapWhenItemOverflows) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 300, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(300, 100, 200, 100), root[3].layoutResult.AllocatedRect);
        }
        
        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_WrapWhenItemOverflowsWithGap.xml")]
        public class FlexHorizontalWrap_WrapWhenItemOverflowsWithGap : UIElement { }

        [Test]
        public void WrapWhenItemOverflowsWithGap() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_WrapWhenItemOverflowsWithGap>();
            FlexHorizontalWrap_WrapWhenItemOverflowsWithGap root = (FlexHorizontalWrap_WrapWhenItemOverflowsWithGap) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(210, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 120, 300, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(310, 120, 200, 100), root[3].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_GrowInTrack.xml")]
        public class FlexHorizontalWrap_GrowInTrack : UIElement { }

        [Test]
        public void GrowInTrack() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_GrowInTrack>();
            FlexHorizontalWrap_GrowInTrack root = (FlexHorizontalWrap_GrowInTrack) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 200, 100), root[4].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_ShrinkInTrack.xml")]
        public class FlexHorizontalWrap_ShrinkInTrack : UIElement { }

        [Test]
        public void ShrinkInTrack() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_ShrinkInTrack>();
            FlexHorizontalWrap_ShrinkInTrack root = (FlexHorizontalWrap_ShrinkInTrack) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 300, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(300, 0, 300, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 600, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 300, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(300, 200, 300, 100), root[4].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_DistributeSpaceInTrack.xml")]
        public class FlexHorizontalWrap_DistributeSpaceInTrack : UIElement { }

        [Test]
        public void DistributeSpaceInTrack() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_DistributeSpaceInTrack>();
            FlexHorizontalWrap_DistributeSpaceInTrack root = (FlexHorizontalWrap_DistributeSpaceInTrack) app.RootElement;

            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.AfterContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 200, 100), root[4].layoutResult.AllocatedRect);

            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.BeforeContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(200, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 200, 200, 100), root[4].layoutResult.AllocatedRect);

            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.BetweenContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 200, 200, 100), root[4].layoutResult.AllocatedRect);

            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.AroundContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(50, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(350, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 200, 200, 100), root[4].layoutResult.AllocatedRect);

            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.CenterContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(100, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(300, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 200, 200, 100), root[4].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_DistributeSpaceBetweenTracks.xml")]
        public class FlexHorizontalWrap_DistributeSpaceBetweenTracks : UIElement { }

        [Test]
        public void DistributeSpaceBetweenTracks() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_DistributeSpaceBetweenTracks>();
            FlexHorizontalWrap_DistributeSpaceBetweenTracks root = (FlexHorizontalWrap_DistributeSpaceBetweenTracks) app.RootElement;

            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.AfterContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 100, 400, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 100, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 200, 200, 100), root[4].layoutResult.AllocatedRect);

            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.BeforeContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 300, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 300, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 400, 400, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 400, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 500, 200, 100), root[4].layoutResult.AllocatedRect);

            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.BetweenContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 250, 400, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 250, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 500, 200, 100), root[4].layoutResult.AllocatedRect);

            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.AroundContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 50, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 50, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 250, 400, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 250, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 450, 200, 100), root[4].layoutResult.AllocatedRect);

            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.CenterContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 150, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 150, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 250, 400, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 250, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 350, 200, 100), root[4].layoutResult.AllocatedRect);
        }

        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_WrapWhenTrackFullWithGap.xml")]
        public class FlexHorizontalWrap_WrapWhenTrackFullWithGap : UIElement { }
        
        [Test]
        public void WrapWhenTrackFullWithGap() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_WrapWhenTrackFullWithGap>();
            FlexHorizontalWrap_WrapWhenTrackFullWithGap root = (FlexHorizontalWrap_WrapWhenTrackFullWithGap) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 110, 100, 100), root[1].layoutResult.AllocatedRect);
        }
        
        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_NoWrapWithGap.xml")]
        public class FlexHorizontalWrap_NoWrapWithGap : UIElement { }
        
        [Test]
        public void NoWrapWithGap() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_NoWrapWithGap>();
            FlexHorizontalWrap_NoWrapWithGap root = (FlexHorizontalWrap_NoWrapWithGap) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
        }
        
        [Template("Data/Layout/FlexHorizontalWrap/FlexHorizontalWrap_NoWrapTwoItemsWithGap.xml")]
        public class FlexHorizontalWrap_NoWrapTwoItemsWithGap : UIElement { }
        
        [Test]
        public void NoWrapTwoItemsWithGap() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_NoWrapTwoItemsWithGap>();
            FlexHorizontalWrap_NoWrapTwoItemsWithGap root = (FlexHorizontalWrap_NoWrapTwoItemsWithGap) app.RootElement;

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 100, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(110, 0, 100, 100), root[1].layoutResult.AllocatedRect);
        }
        
        [Test]
        public void WrapWhenItemTooBigWithGap() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_WrapWhenItemTooBig>();
            FlexHorizontalWrap_WrapWhenItemTooBig root = (FlexHorizontalWrap_WrapWhenItemTooBig) app.RootElement;
            
            root.style.SetFlexLayoutGapHorizontal(10, StyleState.Normal);
            root.style.SetFlexLayoutGapVertical(10, StyleState.Normal);

            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 110, 800, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 220, 200, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(210, 220, 200, 100), root[3].layoutResult.AllocatedRect);
        }

        
        [Test]
        public void DistributeSpaceBetweenTracksWithGap() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_DistributeSpaceBetweenTracks>();
            FlexHorizontalWrap_DistributeSpaceBetweenTracks root = (FlexHorizontalWrap_DistributeSpaceBetweenTracks) app.RootElement;
            
            root.style.SetFlexLayoutGapHorizontal(10, StyleState.Normal);
            root.style.SetFlexLayoutGapVertical(10, StyleState.Normal);

            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.AfterContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(210, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 110, 390, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 110, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 220, 200, 100), root[4].layoutResult.AllocatedRect);
            
            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.BeforeContent, StyleState.Normal);
            app.Update();
            
            Assert.AreEqual(new Rect(0, 280, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(210, 280, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 390, 390, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 390, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 500, 200, 100), root[4].layoutResult.AllocatedRect);
            
            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.BetweenContent, StyleState.Normal);
            app.Update();
            
            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(210, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 250, 390, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 250, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 500, 200, 100), root[4].layoutResult.AllocatedRect);
            
            // gap is not applied when AroundContent distribution is used.
            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.AroundContent, StyleState.Normal);
            app.Update();
            
            Assert.AreEqual(new Rect(0, 50, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(210, 50, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 250, 390, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 250, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 450, 200, 100), root[4].layoutResult.AllocatedRect);
            
            root.style.SetDistributeExtraSpaceVertical(SpaceDistribution.CenterContent, StyleState.Normal);
            app.Update();
            
            Assert.AreEqual(new Rect(0, 140, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(210, 140, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 250, 390, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 250, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 360, 200, 100), root[4].layoutResult.AllocatedRect);
        }
        
        [Test]
        public void DistributeSpaceInTrackWithGap() {
            MockApplication app = MockApplication.Setup<FlexHorizontalWrap_DistributeSpaceInTrack>();
            FlexHorizontalWrap_DistributeSpaceInTrack root = (FlexHorizontalWrap_DistributeSpaceInTrack) app.RootElement;
            
            root.style.SetFlexLayoutGapHorizontal(10, StyleState.Normal);
            root.style.SetFlexLayoutGapVertical(10, StyleState.Normal);

            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.AfterContent, StyleState.Normal);
            app.Update();

            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(210, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 110, 390, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 110, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 220, 200, 100), root[4].layoutResult.AllocatedRect);

            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.BeforeContent, StyleState.Normal);
            app.Update();
            
            Assert.AreEqual(new Rect(190, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 110, 390, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 110, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 220, 200, 100), root[4].layoutResult.AllocatedRect);
            
            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.BetweenContent, StyleState.Normal);
            app.Update();
            
            Assert.AreEqual(new Rect(0, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 110, 390, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 110, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 220, 200, 100), root[4].layoutResult.AllocatedRect);
            
            // gap is not applied when AroundContent distribution is used.
            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.AroundContent, StyleState.Normal);
            app.Update();
            
            Assert.AreEqual(new Rect(50, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(350, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 110, 400, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 110, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 220, 200, 100), root[4].layoutResult.AllocatedRect);
            
            root.style.SetDistributeExtraSpaceHorizontal(SpaceDistribution.CenterContent, StyleState.Normal);
            app.Update();
            
            Assert.AreEqual(new Rect(95, 0, 200, 100), root[0].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(305, 0, 200, 100), root[1].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(0, 110, 390, 100), root[2].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(400, 110, 200, 100), root[3].layoutResult.AllocatedRect);
            Assert.AreEqual(new Rect(200, 220, 200, 100), root[4].layoutResult.AllocatedRect);
        }
        
    }

}