using System;
using Systems.SelectorSystem;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Systems {

    public interface IStyleSystem : ISystem {

        event Action<UIElement, StructList<StyleProperty>> onStylePropertyChanged;

        void SetStyleProperty(UIElement element, StyleProperty propertyValue);

        void AddSelectors(Selector[] selectors);

    }

}