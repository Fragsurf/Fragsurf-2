using System.Collections.Generic;
using UIForia.Elements;
using UIForia.UIInput;

namespace UIForia.Systems {

    public interface IInputSystem : ISystem, IInputProvider {

        void RegisterKeyboardHandler(UIElement element);
        
        IReadOnlyList<UIElement> ElementsThisFrame { get; }
        
#if UNITY_EDITOR
        List<UIElement> DebugElementsThisFrame { get; }
        bool DebugMouseUpThisFrame { get; }
#endif
    }

}