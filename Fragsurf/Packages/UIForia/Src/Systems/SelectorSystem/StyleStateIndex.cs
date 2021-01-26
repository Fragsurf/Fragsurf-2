using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;

namespace Systems.SelectorSystem {

    public class StyleStateIndex : SelectorIndex {

        private readonly StyleState styleState;
        private readonly LightList<UIElement> elements;

        public StyleStateIndex(StyleState styleState) {
            this.styleState = styleState;
            this.elements = new LightList<UIElement>();
        }

        public void SetElements(LightList<UIElement> elements) {
            this.elements.AddRange(elements);
            size = elements.size;
        }

        public override void Gather(UIElement origin, int templateId, LightList<UIElement> resultSet) {
            int depth = origin.hierarchyDepth;
            for (int i = 0; i < elements.size; i++) {
                UIElement element = elements.array[i];
                if (element.hierarchyDepth < depth && element.templateMetaData.id == templateId) {
                    resultSet.Add(element);
                }
            }
        }

        public override void Filter(UIElement origin, int templateId, LightList<UIElement> resultSet) {
            for (int i = 0; i < resultSet.size; i++) {
                if ((resultSet.array[i].style.currentState & styleState) == 0) {
                    resultSet.SwapRemoveAt(i--);
                }
            }
        }

    }

}