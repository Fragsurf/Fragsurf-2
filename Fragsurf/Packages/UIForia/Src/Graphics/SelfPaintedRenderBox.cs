namespace UIForia.Rendering {

    public interface IElementBackgroundPainter {

        void PaintBackground(RenderContext ctx);

    }

    public interface IElementForegroundPainter {

        void PaintForeground(RenderContext ctx);

    }

    public class SelfPaintedRenderBox : RenderBox {

        public override void PaintBackground(RenderContext ctx) {
            if (element is IElementBackgroundPainter bgPainter) {
                bgPainter.PaintBackground(ctx);
            }
        }

        public override void PaintForeground(RenderContext ctx) {
            if (element is IElementForegroundPainter fgPainter) {
                fgPainter.PaintForeground(ctx);
            }
        }

        public override void Enable() {
            
        }

    }

}