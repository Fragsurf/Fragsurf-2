using UIForia.Rendering;

namespace UIForia.Animation {

    public struct StyleKeyFrameValue {

        public readonly StyleProperty styleProperty;

        public StyleKeyFrameValue(StyleProperty property) {
            this.styleProperty = property;
        }

        public StylePropertyId propertyId => styleProperty.propertyId;

        public static implicit operator StyleKeyFrameValue(StyleProperty property) {
            return new StyleKeyFrameValue(property);
        }

    }

}