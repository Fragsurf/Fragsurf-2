using System.Diagnostics;

namespace UIForia.Layout {

    [DebuggerDisplay("(id = {id} (colItem={colItem.trackStart}, {colItem.trackSpan}), (rowItem={rowItem.trackStart}, {rowItem.trackSpan})")]
    public struct GridPlacement {

        public readonly int id;
        public readonly int index;
        public readonly GridItem colItem;
        public readonly GridItem rowItem;

        public GridPlacement(int id, int index, GridItem colItem, GridItem rowItem) {
            this.id = id;
            this.index = index;
            this.colItem = colItem;
            this.rowItem = rowItem;
        }

    }

}