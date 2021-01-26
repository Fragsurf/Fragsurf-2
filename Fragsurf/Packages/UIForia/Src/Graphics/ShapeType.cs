using System;

namespace Vertigo {

    [Flags]
    public enum ShapeType {

        Unset = 0,
        Rect = 1 << 0,
        RoundedRect = 1 << 1,
        Circle = 1 << 2,
        Ellipse = 1 << 3,
        Rhombus = 1 << 4,
        Triangle = 1 << 5,
        Polygon = 1 << 6,
        Text = 1 << 7,
        Sector = 1 << 8,
        Path = 1 << 9,
        ClosedPath = 1 << 10,
        Sprite = 1 << 11,

    }

}