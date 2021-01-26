using UIForia.Rendering;

namespace UIForia.Util {

    public struct MeasurementPair {

        public UIMeasurement x;
        public UIMeasurement y;

        public MeasurementPair(UIMeasurement x, UIMeasurement y) {
            this.x = x;
            this.y = y;
        }

        public bool IsDefined() {
            return x.IsDefined() && y.IsDefined();
        }

        public static bool operator ==(MeasurementPair self, MeasurementPair other) {
            return self.x == other.x && self.y == other.y;
        }

        public static bool operator !=(MeasurementPair self, MeasurementPair other) {
            return !(self == other);
        }

        public bool Equals(MeasurementPair other) {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MeasurementPair a && Equals(a);
        }

        public override int GetHashCode() {
            unchecked {
                return (x.GetHashCode() * 397) ^ y.GetHashCode();
            }
        }

    }
}