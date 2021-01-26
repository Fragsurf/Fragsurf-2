using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Animation {

    public struct MaterialPropertyId {

        public MaterialId materialId;
        public int propertyId;

        public MaterialPropertyId(MaterialId materialId, int propertyId) {
            this.materialId = materialId;
            this.propertyId = propertyId;
        }

    }

    public struct ProcessedMaterialKeyFrameGroup {

        public readonly MaterialPropertyId materialIdPair;
        public readonly LightList<ProcessedMaterialKeyFrame> frames;

        public ProcessedMaterialKeyFrameGroup(MaterialPropertyId materialIdPair, LightList<ProcessedMaterialKeyFrame> frames) {
            this.materialIdPair = materialIdPair;
            this.frames = frames;
        }

    }

    public struct ProcessedStyleKeyFrameGroup {

        public readonly StylePropertyId propertyId;
        public readonly LightList<ProcessedStyleKeyFrame> frames;

        public ProcessedStyleKeyFrameGroup(StylePropertyId propertyId, LightList<ProcessedStyleKeyFrame> frames) {
            this.propertyId = propertyId;
            this.frames = frames;
        }

    }

}