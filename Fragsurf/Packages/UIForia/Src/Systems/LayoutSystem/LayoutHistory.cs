using System.Diagnostics;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Util;

namespace UIForia.Systems {

    public class LayoutHistory {

        public UIElement element;
        public StructList<LayoutHistoryEntry> log;

        public LayoutHistory(UIElement element) {
            this.element = element;
            this.log = new StructList<LayoutHistoryEntry>();
        }

        public void AddLogEntry(LayoutDirection direction, int frameId, LayoutReason reason, string description = null) {
            log.Add(new LayoutHistoryEntry() {
                direction = direction,
                frameId = frameId,
                reason = reason,
                description = description
            });
        }

        public void AddHierarchyChange(int frameId, int oldChildCount, int childCount) {
            log.Add(new LayoutHistoryEntry() {
                frameId = frameId,
                description = "Hierarchy changed (old child count: " + oldChildCount + ", new child count: " + childCount + ")"
            });
        }

        public void AddLayoutHorizontalCall(int frameId) {
            log.Add(new LayoutHistoryEntry() {
                frameId = frameId,
                direction = LayoutDirection.Horizontal
            });
        }

        public void AddLayoutVerticalCall(int frameId) {
            log.Add(new LayoutHistoryEntry() {
                frameId = frameId,
                direction = LayoutDirection.Vertical
            });
        }

        public void SizeChanged(float finalWidth, float newWidth) { }

        public LayoutFrameData GetResults(LayoutDirection direction, int frameId) {
            StructList<LayoutFrameDataEntry> historyEntries = StructList<LayoutFrameDataEntry>.Get();

            for (int i = 0; i < log.size; i++) {
                if (log.array[i].frameId != frameId) {
                    continue;
                }

                if (log.array[i].direction != direction) {
                    continue;
                }

                LayoutFrameDataEntry entry = new LayoutFrameDataEntry(log.array[i].reason, log.array[i].description);
                historyEntries.Add(entry);
            }

            return new LayoutFrameData(historyEntries);
        }

        public bool RanLayoutInFrame(LayoutDirection direction, int frameId) {
            for (int i = 0; i < log.size; i++) {
                if (log.array[i].frameId == frameId && log.array[i].direction == direction) {
                    return true;
                }
            }

            return false;
        }

    }

}