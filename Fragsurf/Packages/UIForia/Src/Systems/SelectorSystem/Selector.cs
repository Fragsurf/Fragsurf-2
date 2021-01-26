using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;

namespace Systems.SelectorSystem {

    public class Selector {

        public SelectorQuery rootQuery;
        public UIStyle matchStyle;

        public void Run(UIElement origin) {
            LightList<UIElement> result = LightList<UIElement>.Get();

            rootQuery.Gather(origin, result);

            if (result.size == 0) {
                result.Release();
                return;
            }

            if (rootQuery.next == null) {
                // match!    
                for (int i = 0; i < result.size; i++) {
                    // result.array[i].style.SetSelectorStyle(matchStyle);
                }

                result.Release();
                return;
            }

            for (int i = 0; i < result.size; i++) {
                if (rootQuery.next.Run(origin, result.array[i])) {
                    // result.array[i].style.SetSelectorStyle(matchStyle);
                }
            }

            result.Release();
        }

    }

}