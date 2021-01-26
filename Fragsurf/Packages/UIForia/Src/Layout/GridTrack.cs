using UIForia.Layout.LayoutTypes;
using UIForia.Util;

namespace UIForia.Layout {

    public struct GridTrack {

        public float position;
        public float outputSize;
        public GridTrackSize size;
        public StructList<int> spanningItems;
        public int autoPlacementCursor;

        public GridTrack(GridTrackSize size) {
            this.size = size;
            this.position = 0;
            this.outputSize = 0;
            this.autoPlacementCursor = 0;
            this.spanningItems = StructList<int>.Get();
        }

        public float End => position + outputSize;

    }

}