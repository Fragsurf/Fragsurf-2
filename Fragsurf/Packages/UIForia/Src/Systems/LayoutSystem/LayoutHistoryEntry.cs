using System.Diagnostics;
using UIForia.Layout;
using UIForia.Util;

namespace UIForia.Systems {

    [DebuggerDisplay("[{frameId} {direction}] {reason} {description}")]
    public struct LayoutHistoryEntry {

        public int frameId;
        public string description;
        public LayoutDirection direction;
        public LayoutReason reason;

    }

    public struct LayoutFrameData {
        
        public StructList<LayoutFrameDataEntry> entries;
        
        public LayoutFrameData(StructList<LayoutFrameDataEntry> entries) {
            this.entries = entries;
        }

    }

    public struct LayoutFrameDataEntry {

        public readonly LayoutReason reason;
        public readonly string description;
        
        public LayoutFrameDataEntry(LayoutReason reason, string description) {
            this.reason = reason;
            this.description = description;
        }

    }

    public enum LayoutReason {

        Initialized,
        HierarchyChanged,
        StyleSizeChanged,
        FinalSizeChanged,
        DescendentStyleSizeChanged,
        BorderPaddingChanged,

        TextContentChanged

    }

}