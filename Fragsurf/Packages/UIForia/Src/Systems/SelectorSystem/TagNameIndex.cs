using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Util;

namespace Systems.SelectorSystem {

    public class TagNameIndex : SelectorIndex {

        public bool isDirty;
        private readonly StructList<TagNameIndexEntry> entries;

        public TagNameIndex() {
            this.entries = new StructList<TagNameIndexEntry>(4);
        }

        public void Add(UIElement child) {
            if ((child.flags & UIElementFlags.InTagIndex) != 0) {
                return;
            }

            isDirty = true;
            child.flags |= UIElementFlags.InTagIndex;
            entries.Add(new TagNameIndexEntry() {
                element = child,
                depth = child.hierarchyDepth,
                templateId = child.templateMetaData.id
            });
        }

        public void Remove(UIElement child) {
            if ((child.flags & UIElementFlags.InTagIndex) == 0) {
                return;
            }

            isDirty = true;
        }

        private void UpdateIndex() {
            for (int i = 0; i < entries.size; i++) { }

            Array.Sort(entries.array, 0, entries.size, s_DepthComp);
        }

        private static readonly DepthComp s_DepthComp = new DepthComp();

        public class DepthComp : IComparer<TagNameIndexEntry> {

            public int Compare(TagNameIndexEntry x, TagNameIndexEntry y) {
                return x.depth - y.depth;
            }

        }

        public override void Gather(UIElement origin, int templateId, LightList<UIElement> resultSet) {
            if (isDirty) {
                UpdateIndex();
                isDirty = false;
            }

            int depth = origin.hierarchyDepth;
            // todo -- keep sorted by depth
            for (int i = 0; i < entries.size; i++) {
                ref TagNameIndexEntry entry = ref entries.array[i];
                if (entry.depth < depth && entry.templateId == templateId) {
                    resultSet.Add(entry.element);
                }
            }
        }

        public override void Filter(UIElement origin, int templateId, LightList<UIElement> resultSet) {
            for (int i = 0; i < resultSet.size; i++) {
                if (resultSet.array[i].tagNameIndex != this) {
                    resultSet.SwapRemoveAt(i--);
                }
            }
        }

    }

}