using UIForia.Elements;
using UIForia.Util;

namespace Systems.SelectorSystem {

    public class AttributeIndex : SelectorIndex {

        public string attributeName;

        public override void Gather(UIElement origin, int templateId, LightList<UIElement> resultSet) { }

        public override void Filter(UIElement origin, int templateId, LightList<UIElement> resultSet) { }

    }

}