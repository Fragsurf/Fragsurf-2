namespace Src.Systems {

    internal struct FixedRenderState {

        public DepthState depthState;
        public BlendState blendState;

        public FixedRenderState(in BlendState blendState, in DepthState depthState) {
            this.depthState = depthState;
            this.blendState = blendState;
        }

        public static FixedRenderState Default => new FixedRenderState(BlendState.Default, DepthState.Default);

    }

}