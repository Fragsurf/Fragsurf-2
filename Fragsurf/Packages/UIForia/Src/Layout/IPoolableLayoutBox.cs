using JetBrains.Annotations;
using UIForia.Elements;

namespace UIForia.Layout {

    public interface IPoolableLayoutBox {

        bool IsInPool { get; set; }

        void OnSpawn([NotNull] UIElement element);

        void OnRelease();

    }

}