using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Systems {

    public class AwesomeImageLayoutBox : AwesomeLayoutBox {

        protected override float ComputeContentWidth() {
            UIImageElement imageElement = (UIImageElement) element;
            if (imageElement.texture != null) {
                return imageElement.texture.width;
            }

            return imageElement.Width;
        }

        protected override float ComputeContentHeight() {
            UIImageElement imageElement = (UIImageElement) element;
            if (imageElement.texture != null) {
                return imageElement.texture.height;
            }

            return imageElement.Height;
        }

        public override void OnChildrenChanged(LightList<AwesomeLayoutBox> childList) { }

        public override void RunLayoutHorizontal(int frameId) { }

        public override void RunLayoutVertical(int frameId) { }

    }

}