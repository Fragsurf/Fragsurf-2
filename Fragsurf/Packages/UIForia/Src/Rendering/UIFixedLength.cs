using System;
using System.Diagnostics;
using JetBrains.Annotations;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public struct UIFixedLength {

        public readonly float value;
        public readonly UIFixedUnit unit;

        [DebuggerStepThrough]
        public UIFixedLength(float value, UIFixedUnit unit = UIFixedUnit.Pixel) {
            this.value = value;
            this.unit = unit;
        }

        public static bool operator ==(UIFixedLength self, UIFixedLength other) {
            if (float.IsNaN(self.value) && float.IsNaN(other.value)) {
                return self.unit == other.unit;
            }

            return Mathf.Approximately(self.value, other.value) && self.unit == other.unit;
        }

        public static bool operator !=(UIFixedLength self, UIFixedLength other) {
            return !(self == other);
        }

        public static UIFixedLength Unset => new UIFixedLength(FloatUtil.UnsetValue);

        public bool Equals(UIFixedLength other) {
            return ((float.IsNaN(value) && float.IsNaN(other.value)) || Mathf.Approximately(value, other.value)) && unit == other.unit;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is UIFixedLength length && Equals(length);
        }

        public override int GetHashCode() {
            unchecked {
                return (value.GetHashCode() * 397) ^ (int) unit;
            }
        }

        [Pure]
        [DebuggerStepThrough]
        public bool IsDefined() {
            return FloatUtil.IsDefined(value);
        }
        
        public static implicit operator UIFixedLength(int value) {
            return new UIFixedLength(value, UIFixedUnit.Pixel);
        }

        public static implicit operator UIFixedLength(float value) {
            return new UIFixedLength(value, UIFixedUnit.Pixel);
        }

        public static implicit operator UIFixedLength(double value) {
            return new UIFixedLength((float) value, UIFixedUnit.Pixel);
        }

        public static UIFixedLength Percent(float value) {
            return new UIFixedLength(value, UIFixedUnit.Percent);
        }

        public override string ToString() {
            return $"{value} {unit}";
        }

        public static float Resolve(UIFixedLength length, float relativeBase, float emSize, float viewportWidth, float viewportHeight) {
            switch (length.unit) {
                case UIFixedUnit.Unset:
                case UIFixedUnit.Pixel:
                    return length.value;
                case UIFixedUnit.Percent:
                    return length.value * relativeBase;
                case UIFixedUnit.Em:
                    return length.value * emSize;
                case UIFixedUnit.ViewportWidth:
                    return length.value * viewportWidth;
                case UIFixedUnit.ViewportHeight:
                    return length.value * viewportHeight;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}