using UIForia.Elements;
using UIForia.Util;

namespace Systems.SelectorSystem {

    public class AttributeValueMatchIndex : SelectorIndex {

        private readonly AttributeIndex parent;
        private readonly string attrValue;
        private readonly string attrName;

        public AttributeValueMatchIndex(AttributeIndex parent, string attrName, string attrValue) {
            this.parent = parent;
            this.attrName = attrName;
            this.attrValue = attrValue;
        }

        public override void Gather(UIElement origin, int templateId, LightList<UIElement> resultSet) {
            parent.Gather(origin, templateId, resultSet);
            for (int i = 0; i < resultSet.size; i++) {
                string attr = resultSet.array[i].GetAttribute(attrName);
                if (attr != attrValue) {
                    resultSet.SwapRemoveAt(i--);
                }
            }
        }

        public override void Filter(UIElement origin, int templateId, LightList<UIElement> resultSet) {
            for (int i = 0; i < resultSet.size; i++) {
                string attr = resultSet.array[i].GetAttribute(attrName);
                if (attr != attrValue) {
                    resultSet.SwapRemoveAt(i--);
                }
            }
        }

    }

}