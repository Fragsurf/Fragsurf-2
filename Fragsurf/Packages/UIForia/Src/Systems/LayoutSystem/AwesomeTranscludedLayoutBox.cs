using System;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Systems {

    public class AwesomeTranscludedLayoutBox : AwesomeLayoutBox {

        private LightList<AwesomeLayoutBox> childList;
        
        protected override float ComputeContentWidth() {
            throw new NotImplementedException();
        }

        protected override float ComputeContentHeight() {
            throw new NotImplementedException();
        }

        public override void OnChildrenChanged(LightList<AwesomeLayoutBox> childList) {
            this.childList = this.childList ?? new LightList<AwesomeLayoutBox>(childList.size);
            this.childList.AddRange(childList);
        }

        public override void RunLayoutHorizontal(int frameId) {
            throw new NotImplementedException();
        }

        public override void RunLayoutVertical(int frameId) {
            throw new NotImplementedException();
        }

        public override void OnStyleChanged(StructList<StyleProperty> propertyList) {}

        public LightList<AwesomeLayoutBox> GetChildren() {
            return childList;
        }

    }

}