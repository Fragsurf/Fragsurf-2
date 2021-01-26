using System.Diagnostics;
using JetBrains.Annotations;
using UIForia.Util;

namespace UIForia {

    public struct OffsetMeasurement {

        public float value;
        public OffsetMeasurementUnit unit;

        public OffsetMeasurement(float value, OffsetMeasurementUnit unit = OffsetMeasurementUnit.Pixel) {
            this.value = value;
            this.unit = unit;
        }

        public static OffsetMeasurement Unset => new OffsetMeasurement(FloatUtil.UnsetValue, 0);

        [Pure]
        [DebuggerStepThrough]
        public bool IsDefined() {
            return FloatUtil.IsDefined(value);
        }
        
        public bool Equals(OffsetMeasurement other) {
            return value.Equals(other.value) && unit == other.unit;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is OffsetMeasurement a && Equals(a);
        }

        public override int GetHashCode() {
            unchecked {
                return (value.GetHashCode() * 397) ^ (int) unit;
            }
        }
        
        public static bool operator ==(OffsetMeasurement self, OffsetMeasurement other) {
            if (float.IsNaN(self.value) && float.IsNaN(other.value)) {
                return self.unit == other.unit;
            }

            return self.value == other.value && self.unit == other.unit;
        }

        public static bool operator !=(OffsetMeasurement self, OffsetMeasurement other) {
            return !(self == other);
        }

        public static implicit operator OffsetMeasurement(int value) {            
            return new OffsetMeasurement(value, OffsetMeasurementUnit.Pixel);
        }

        public static implicit operator OffsetMeasurement(float value) {
            return new OffsetMeasurement(value, OffsetMeasurementUnit.Pixel);
        }

        public static implicit operator OffsetMeasurement(double value) {
            return new OffsetMeasurement((float) value, OffsetMeasurementUnit.Pixel);
        }

        public override string ToString() {
            return $"{value} {unit}";
        }
    }

    public struct OffsetMeasurementPair {

        public OffsetMeasurement x;
        public OffsetMeasurement y;

        public OffsetMeasurementPair(OffsetMeasurement x, OffsetMeasurement y) {
            this.x = x;
            this.y = y;
        }
        
        public bool IsDefined() {
            return x.IsDefined() && y.IsDefined();
        }
        
        public static bool operator ==(OffsetMeasurementPair self, OffsetMeasurementPair other) {
            return self.x == other.x && self.y == other.y;
        }

        public static bool operator !=(OffsetMeasurementPair self, OffsetMeasurementPair other) {
            return !(self == other);
        }

        public bool Equals(OffsetMeasurementPair other) {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is OffsetMeasurementPair a && Equals(a);
        }

        public override int GetHashCode() {
            unchecked {
                return (x.GetHashCode() * 397) ^ y.GetHashCode();
            }
        }

    }

}