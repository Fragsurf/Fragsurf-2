using UIForia.Elements;
using UIForia.Util;

namespace Systems.SelectorSystem {

    public abstract class SelectorIndex {

        public int size;

        public abstract void Gather(UIElement origin, int templateId, LightList<UIElement> resultSet);

        public abstract void Filter(UIElement origin, int templateId, LightList<UIElement> resultSet);

    }

}