using System;

namespace UIForia.Text {

    public struct SelectionRange {

        public readonly int cursorIndex;
        public readonly int selectIndex;

        public SelectionRange(int cursorIndex, int selectIndex = -1) {
            this.cursorIndex = Math.Max(0, cursorIndex);
            this.selectIndex = selectIndex;
        }

        public bool HasSelection => selectIndex > -1 && selectIndex != cursorIndex;

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SelectionRange a && Equals(a);
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = cursorIndex;
                hashCode = (hashCode * 397) ^ selectIndex;
                return hashCode;
            }
        }

        public bool Equals(SelectionRange previousSelectionRange) {
            return cursorIndex != previousSelectionRange.cursorIndex
                   || selectIndex != previousSelectionRange.selectIndex;
        }

        public static bool operator ==(SelectionRange a, SelectionRange b) {
            return a.cursorIndex == b.cursorIndex
                   && a.selectIndex == b.selectIndex;
        }

        public static bool operator !=(SelectionRange a, SelectionRange b) {
            return !(a == b);
        }
    }
}
