using System.Runtime.InteropServices;
using UnityEngine.Rendering;

namespace Src.Systems {

    [StructLayout(LayoutKind.Explicit)]
    public struct BlendState {

        public bool Equals(BlendState other) {
            return m_WriteMask == other.m_WriteMask && m_SourceColorBlendMode == other.m_SourceColorBlendMode && m_DestinationColorBlendMode == other.m_DestinationColorBlendMode && m_SourceAlphaBlendMode == other.m_SourceAlphaBlendMode && m_DestinationAlphaBlendMode == other.m_DestinationAlphaBlendMode && m_ColorBlendOperation == other.m_ColorBlendOperation && m_AlphaBlendOperation == other.m_AlphaBlendOperation && m_Padding == other.m_Padding && check0 == other.check0 && check1 == other.check1;
        }

        public override bool Equals(object obj) {
            return obj is BlendState other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = m_WriteMask.GetHashCode();
                hashCode = (hashCode * 397) ^ m_SourceColorBlendMode.GetHashCode();
                hashCode = (hashCode * 397) ^ m_DestinationColorBlendMode.GetHashCode();
                hashCode = (hashCode * 397) ^ m_SourceAlphaBlendMode.GetHashCode();
                hashCode = (hashCode * 397) ^ m_DestinationAlphaBlendMode.GetHashCode();
                hashCode = (hashCode * 397) ^ m_ColorBlendOperation.GetHashCode();
                hashCode = (hashCode * 397) ^ m_AlphaBlendOperation.GetHashCode();
                hashCode = (hashCode * 397) ^ m_Padding.GetHashCode();
                hashCode = (hashCode * 397) ^ check0;
                hashCode = (hashCode * 397) ^ check1;
                return hashCode;
            }
        }

        [FieldOffset(0)] public byte m_WriteMask;
        [FieldOffset(1)] public byte m_SourceColorBlendMode;
        [FieldOffset(2)] public byte m_DestinationColorBlendMode;
        [FieldOffset(3)] public byte m_SourceAlphaBlendMode;
        [FieldOffset(4)] public byte m_DestinationAlphaBlendMode;
        [FieldOffset(5)] public byte m_ColorBlendOperation;
        [FieldOffset(6)] public byte m_AlphaBlendOperation;
        [FieldOffset(7)] public byte m_Padding;

        [FieldOffset(0)] public int check0;
        [FieldOffset(4)] public int check1;

        public BlendState(
            ColorWriteMask writeMask = ColorWriteMask.All,
            BlendMode sourceColorBlendMode = BlendMode.One,
            BlendMode destinationColorBlendMode = BlendMode.Zero,
            BlendMode sourceAlphaBlendMode = BlendMode.One,
            BlendMode destinationAlphaBlendMode = BlendMode.Zero,
            BlendOp colorBlendOperation = BlendOp.Add,
            BlendOp alphaBlendOperation = BlendOp.Add) {
            this.check0 = 0;
            this.check1 = 0;
            this.m_WriteMask = (byte) writeMask;
            this.m_SourceColorBlendMode = (byte) sourceColorBlendMode;
            this.m_DestinationColorBlendMode = (byte) destinationColorBlendMode;
            this.m_SourceAlphaBlendMode = (byte) sourceAlphaBlendMode;
            this.m_DestinationAlphaBlendMode = (byte) destinationAlphaBlendMode;
            this.m_ColorBlendOperation = (byte) colorBlendOperation;
            this.m_AlphaBlendOperation = (byte) alphaBlendOperation;
            this.m_Padding = 0;
        }

        public BlendMode sourceBlendMode {
            get { return (BlendMode) m_SourceColorBlendMode; }
            set { m_SourceColorBlendMode = (byte) value; }
        }

        public BlendMode destBlendMode {
            get { return (BlendMode) m_DestinationColorBlendMode; }
            set { m_DestinationColorBlendMode = (byte) value; }
        }

        public BlendOp blendOp {
            get { return (BlendOp) m_ColorBlendOperation; }
            set { m_ColorBlendOperation = (byte) value; }
        }

        public static bool operator ==(BlendState a, BlendState b) {
            return a.check0 == b.check0 && a.check1 == b.check1;
        }

        public static bool operator !=(BlendState a, BlendState b) {
            return a.check0 != b.check0 || a.check1 != b.check1;
        }

        public static BlendState Default => new BlendState(
            ColorWriteMask.All,
            BlendMode.One,
            BlendMode.OneMinusSrcAlpha,
            BlendMode.One,
            BlendMode.OneMinusSrcAlpha
        );

    }

}