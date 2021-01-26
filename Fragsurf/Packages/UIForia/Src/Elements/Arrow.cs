using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Elements {

    public enum ArrowRotation {

        Left,
        Right,
        Up,
        Down

    }

    /// <summary>
    /// Renders an arrowhead that can be used as an arrow-like border or a chevron. Uses BackgroundColor as
    /// arrow color and respects margins.
    /// </summary>
    public class Arrow : UIContainerElement, IElementBackgroundPainter {

        /// <summary>
        /// Sets the direction in which the arrow points. Defaults to left.
        /// </summary>
        public ArrowRotation Rotation;

        public override void OnCreate() {
            style.SetPainter("self", StyleState.Normal);
        }

        private Path2D path = new Path2D();

        public void PaintBackground(RenderContext ctx) {
            path.Clear();
            Matrix4x4 m = layoutResult.matrix.ToMatrix4x4();
            //  m *= Matrix4x4.Scale(new Vector3(0.1f, 0.1f, 0.1f));
            path.SetTransform(m); //layoutResult.matrix.ToMatrix4x4());

            path.BeginPath();

            float xMax = (layoutResult.actualSize.width - layoutResult.margin.left - layoutResult.margin.right);
            float yMax = (layoutResult.actualSize.height - layoutResult.margin.top - layoutResult.margin.bottom);
            switch (Rotation) {
                case ArrowRotation.Down:
                    path.MoveTo(0, 0);
                    path.LineTo(xMax / 2, yMax);
                    path.LineTo(layoutResult.ActualWidth, 0);
                    break;
                case ArrowRotation.Up:
                    path.MoveTo(0, yMax);
                    path.LineTo(xMax / 2, 0);
                    path.LineTo(layoutResult.ActualWidth, yMax);
                    break;
                case ArrowRotation.Right:
                    path.MoveTo(0, 2);
                    path.LineTo(xMax, yMax / 2);
                    path.LineTo(0, layoutResult.ActualHeight);
                    break;
                case ArrowRotation.Left:
                    path.MoveTo(xMax, 2);
                    path.LineTo(0, yMax / 2);
                    path.LineTo(xMax, layoutResult.ActualHeight);
                    break;
            }
            
            path.EndPath();
            path.SetStrokeWidth(3f);
            path.SetStroke(style.BackgroundColor.r == -1 ? Color.grey : style.BackgroundColor);
            path.Stroke();
            ctx.DrawPath(path);
        }

    }

}