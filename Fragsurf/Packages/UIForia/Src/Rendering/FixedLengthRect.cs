using System.Diagnostics;
using UIForia.Util;

namespace UIForia.Layout {

    [DebuggerDisplay("{top}, {right}, {bottom}, {left}")]
    public struct FixedLengthRect {

        public override int GetHashCode() {
            unchecked {
                var hashCode = top.GetHashCode();
                hashCode = (hashCode * 397) ^ right.GetHashCode();
                hashCode = (hashCode * 397) ^ bottom.GetHashCode();
                hashCode = (hashCode * 397) ^ left.GetHashCode();
                return hashCode;
            }
        }

        public UIFixedLength top;
        public UIFixedLength right;
        public UIFixedLength bottom;
        public UIFixedLength left;

        public static readonly ContentBoxRect Unset = new ContentBoxRect(FloatUtil.UnsetValue);

        public FixedLengthRect(float value) {
            this.top = value;
            this.right = value;
            this.bottom = value;
            this.left = value;
        }

        public FixedLengthRect(float top, float right, float bottom, float left) {
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.left = left;
        }

        public FixedLengthRect(UIFixedLength top, UIFixedLength right, UIFixedLength bottom, UIFixedLength left) {
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.left = left;
        }

        public bool Equals(FixedLengthRect other) {
            return top == other.top
                   && right == other.right
                   && bottom == other.bottom
                   && left == other.left;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FixedLengthRect rect && Equals(rect);
        }

        [DebuggerStepThrough]
        public static bool operator ==(FixedLengthRect self, FixedLengthRect other) {
            return self.top == other.top
                   && self.left == other.left
                   && self.right == other.right
                   && self.bottom == other.bottom;
        }

        [DebuggerStepThrough]
        public static bool operator !=(FixedLengthRect self, FixedLengthRect other) {
            return !(self == other);
        }

    }

}