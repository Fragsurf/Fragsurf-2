using UIForia.Rendering;

namespace UIForia.Animation {

    public struct ProcessedMaterialKeyFrame {

        public readonly float time;
        public readonly MaterialProperty materialProperty;
        
        public ProcessedMaterialKeyFrame(float time, MaterialProperty materialProperty) {
            this.time = time;
            this.materialProperty = materialProperty;
        }
        
    }

    public struct ProcessedStyleKeyFrame {

        public readonly float time;
        public readonly StyleKeyFrameValue value;
        
        public ProcessedStyleKeyFrame(float time, StyleKeyFrameValue value) {
            this.time = time;
            this.value = value;
        }

    }

}