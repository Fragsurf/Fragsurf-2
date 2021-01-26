namespace UIForia.Rendering {

    public struct AnimatedProperty {

        public float time;
        public StyleProperty value0;
        public StyleProperty value1;
        public StylePropertyId propertyId;

        public AnimatedProperty(StylePropertyId propertyId, in StyleProperty v0, in StyleProperty v1, float time) {
            this.propertyId = propertyId;
            this.value0 = v0;
            this.value1 = v1;
            this.time = time;
        }

    }

}