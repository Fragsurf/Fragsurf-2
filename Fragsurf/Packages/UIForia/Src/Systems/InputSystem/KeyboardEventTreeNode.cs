using UIForia.Elements;
using UIForia.Util;

namespace UIForia.Systems.UIForia.Systems.Input {

    public class KeyboardEventTreeNode : IHierarchical {
        
        private readonly UIElement element;
        
        public KeyboardEventTreeNode(UIElement element) {
            this.element = element;
        }
        
        public int UniqueId => element.id;
        public IHierarchical Element => element;
        public IHierarchical Parent => element.parent;
    }

}