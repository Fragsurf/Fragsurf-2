using UIForia;
using UIForia.Rendering;

namespace Systems.SelectorSystem {

    public class SelectorSystem {

        public Application application;

        public StyleStateIndex hoverIndex;
        public StyleStateIndex activeIndex;
        public StyleStateIndex focusIndex;

        public SelectorSystem(Application application) {
            
            hoverIndex = new StyleStateIndex(StyleState.Hover);
            activeIndex = new StyleStateIndex(StyleState.Active);    
            focusIndex = new StyleStateIndex(StyleState.Focused);
            
        }

        public void OnUpdate() {
            
        }

    }

}