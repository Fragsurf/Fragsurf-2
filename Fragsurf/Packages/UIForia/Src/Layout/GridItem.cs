using UIForia.Util;

namespace UIForia.Layout {

    public struct GridItem {

        public int trackStart;
        public int trackSpan;
        public int resolvedTrackStart;

        public GridItem(int trackStart, int trackSpan, int resolvedTrackStart = -1) {
            this.trackStart = trackStart;
            this.trackSpan = trackSpan;
            this.resolvedTrackStart = resolvedTrackStart;
        }

        public bool IsAutoPlaced => resolvedTrackStart == -1;
        public bool IsAxisLocked => IntUtil.IsDefined(trackStart);

    }

}

