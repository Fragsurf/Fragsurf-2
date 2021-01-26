using UIForia.Systems;

namespace UIForia.Layout.LayoutTypes {

    public enum GridTrackSizeType {

        Value,
        Repeat,
        MinMax,
        RepeatFit,
        RepeatFill

    }

    public struct GridItemPlacement {

        public readonly int index;
        public readonly string name;

        public GridItemPlacement(string name) {
            if (name != null) {
                this.index = 0;
                this.name = name;
            }
            else {
                this.name = null;
                this.index = 0;
            }
        }

        public GridItemPlacement(int index) {
            this.name = null;
            this.index = index;
        }

        public static implicit operator GridItemPlacement(int value) {
            return new GridItemPlacement(value);
        }

        public static bool operator ==(in GridItemPlacement a, in GridItemPlacement b) {
            return a.index == b.index && a.name == b.name;
        }

        public static bool operator !=(GridItemPlacement a, GridItemPlacement b) {
            return a.index != b.index || a.name != b.name;
        }

        public bool Equals(in GridItemPlacement other) {
            return index == other.index && string.Equals(name, other.name);
        }

        public override bool Equals(object obj) {
            return obj is GridItemPlacement other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (index * 397) ^ (name != null ? name.GetHashCode() : 0);
            }
        }

    }

    public struct GridTrackSize {

        public GridTrackSizeType type;
        public GridCellDefinition cell;

        public GridTrackSize(in GridCellDefinition cellDefinition) {
            this.type = GridTrackSizeType.Value;
            this.cell = cellDefinition;
        }

        public static GridTrackSize Default => new GridTrackSize(
            new GridCellDefinition() {
                growFactor = 0,
                shrinkFactor = 0,
                shrinkLimit = new GridCellSize(0, GridTemplateUnit.Pixel),
                growLimit = new GridCellSize(0, GridTemplateUnit.Pixel),
                baseSize = new GridCellSize(1, GridTemplateUnit.MaxContent)
            });

    }

}