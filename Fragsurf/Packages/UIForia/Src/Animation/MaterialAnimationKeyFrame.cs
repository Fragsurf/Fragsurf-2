namespace UIForia.Animation {

    public struct MaterialAnimationKeyFrame {

        public readonly float key;
        public readonly MaterialKeyFrameValue[] properties;

        public MaterialAnimationKeyFrame(float key, MaterialKeyFrameValue[] properties) {
            this.key = key;
            this.properties = properties;
        }

    }

}