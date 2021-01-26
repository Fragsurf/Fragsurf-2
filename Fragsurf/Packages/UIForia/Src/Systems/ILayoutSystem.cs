using System.Collections.Generic;
using UIForia.Elements;
using UnityEngine;

namespace UIForia.Systems {

    public interface ILayoutSystem : ISystem {

        IList<UIElement> QueryPoint(Vector2 point, IList<UIElement> retn);

        AwesomeLayoutRunner GetLayoutRunner(UIElement viewRoot);

    }

}