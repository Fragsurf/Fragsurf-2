using System;

namespace Vertigo {

    [Flags]
    public enum PaintMode {

        None = 0,
        Color = 1 << 0,
        Texture = 1 << 1,
        TextureTint = 1 << 2,
        LetterBoxTexture = 1 << 3,
        Shadow = 1 << 4,
        ShadowTint = 1 << 5,
        Gradient = 1 << 6

    }

}