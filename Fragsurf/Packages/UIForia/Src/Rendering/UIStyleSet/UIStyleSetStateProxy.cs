
namespace UIForia.Rendering {

    public partial struct UIStyleSetStateProxy {

        private readonly UIStyleSet m_StyleSet;
        public readonly StyleState state;

        internal UIStyleSetStateProxy(UIStyleSet styleSet, StyleState state) {
            this.m_StyleSet = styleSet;
            this.state = state;
        }

    }

}