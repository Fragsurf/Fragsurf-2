namespace UIForia.Rendering {

    public struct CornerDefinition {

        public float topLeftX;
        public float topLeftY;
        public float topRightX;
        public float topRightY;
        public float bottomLeftX;
        public float bottomLeftY;
        public float bottomRightX;
        public float bottomRightY;

        public CornerDefinition(float clip) {
            this.topLeftX = clip;
            this.topLeftY = clip;
            this.topRightX = clip;
            this.topRightY = clip;
            this.bottomLeftX = clip;
            this.bottomLeftY = clip;
            this.bottomRightX = clip;
            this.bottomRightY = clip;
        }

    }

}