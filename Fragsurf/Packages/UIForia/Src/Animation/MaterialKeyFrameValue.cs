using UIForia.Rendering;

namespace UIForia.Animation {

    public struct MaterialKeyFrameValue {

        public readonly MaterialId materialId;
        public readonly int propertyId;
        public readonly MaterialPropertyValue2 propertyValue;

        public MaterialKeyFrameValue(MaterialId materialId, int propertyId, in MaterialPropertyValue2 propertyValue) {
            this.materialId = materialId;
            this.propertyId = propertyId;
            this.propertyValue = propertyValue;
        }

    }

}