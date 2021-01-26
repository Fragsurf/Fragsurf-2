using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UIForia.Attributes;
using UIForia.Compilers;
using UIForia.Systems;
using UIForia.Util;

namespace UIForia.Elements {

    public struct RepeatItemKey {

        public readonly string keyString;
        public readonly long keyLong;

        [UsedImplicitly]
        public RepeatItemKey(long keyLong) {
            this.keyString = null;
            this.keyLong = keyLong;
        }

        [UsedImplicitly]
        public RepeatItemKey(int keyInt) {
            this.keyString = null;
            this.keyLong = keyInt;
        }

        [UsedImplicitly]
        public RepeatItemKey(string keyString) {
            this.keyLong = 0;
            this.keyString = keyString;
        }

        public static bool operator ==(RepeatItemKey a, RepeatItemKey b) {
            return a.keyLong == b.keyLong && string.Equals(a.keyString, b.keyString, StringComparison.Ordinal);
        }

        public static bool operator !=(RepeatItemKey a, RepeatItemKey b) {
            return a.keyLong != b.keyLong || !string.Equals(a.keyString, b.keyString, StringComparison.Ordinal);
        }

        public bool Equals(RepeatItemKey other) {
            return keyString == other.keyString && keyLong == other.keyLong;
        }

        public override bool Equals(object obj) {
            return obj is RepeatItemKey other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return ((keyString != null ? keyString.GetHashCode() : 0) * 397) ^ keyLong.GetHashCode();
            }
        }

    }

    public abstract class UIRepeatElement : UIElement {

        public int templateSpawnId;
        public int indexVarId;
        public UIElement templateContextRoot;
        public TemplateScope scope;
        public int itemVarId;

    }

    public sealed class UIRepeatCountElement : UIRepeatElement {

        public int count;

        [OnPropertyChanged(nameof(count))]
        public void OnCountChanged(int prevCount) {
            if (count > prevCount) {
                // first create and add children
                int diff = count - prevCount;
                for (int i = 0; i < diff; i++) {
                    UIElement child = application.CreateTemplate(templateSpawnId, templateContextRoot, this, scope);
                    
                    ContextVariable<int> indexVariable = new ContextVariable<int>(indexVarId, "index", prevCount + i);

                    child.bindingNode.CreateLocalContextVariable(indexVariable);
                }
            }
            else {
                int diff = prevCount - count;
                for (int i = 0; i < diff; i++) {
                    children.array[children.size - 1].Destroy();
                }
            }
        }

    }

    internal struct RepeatIndex {

        public UIElement element;
        public RepeatItemKey key;

    }

    public sealed class UIRepeatElement<T> : UIRepeatElement {

        public IList<T> list;
        private IList<T> previousList;
        private int previousSize;
        public Func<T, RepeatItemKey> keyFn;
        private bool skipUpdate;

        private int prevRangeStart;
        private int prevRangeEnd;

        [UsedImplicitly] public int start;
        [UsedImplicitly] public int end = int.MaxValue;
        private RepeatIndex[] keys;

        public override void OnUpdate() {
            int rangeStart = start;
            int rangeEnd = end;
            int listCount = list?.Count ?? 0;

            if (rangeStart < 0) rangeStart = 0;
            if (rangeEnd < rangeStart) rangeEnd = rangeStart;

            if (rangeStart >= listCount) rangeStart = listCount;

            if (rangeEnd < rangeStart) rangeEnd = rangeStart;
            if (rangeEnd > listCount) rangeEnd = listCount;

            if (keyFn == null) {
                UpdateWithoutKeyFunc(rangeStart, rangeEnd);
            }
            else {
                UpdateWithKeyFunc(rangeStart, rangeEnd);
            }

            prevRangeStart = rangeStart;
            prevRangeEnd = rangeEnd;
        }

        private StructList<RepeatIndex> availableChildren;
        private StructList<RepeatIndex> lastFrameChildren;
        private LightList<UIElement> childrenSwapList;

        private void UpdateWithKeyFunc(int rangeStart, int rangeEnd) {
            // build list of children
            // element registers for a key
            // for each key in range
            // find element
            // if element doesnt exist
            // create it
            // if old element no longer referenced, delete it

            lastFrameChildren = lastFrameChildren ?? new StructList<RepeatIndex>(rangeEnd - rangeStart);
            availableChildren = availableChildren ?? new StructList<RepeatIndex>();

            availableChildren.AddRange(lastFrameChildren);
            lastFrameChildren.Clear();

            for (int i = rangeStart; i < rangeEnd; i++) {
                RepeatItemKey key = keyFn(list[i]);

                // find old child who has key == key last frame
                RepeatIndex keypair = default;
                keypair.key = key;

                for (int j = 0; j < availableChildren.size; j++) {
                    ref RepeatItemKey childKey = ref availableChildren.array[j].key;

                    if (childKey.keyLong == key.keyLong && string.Equals(childKey.keyString, key.keyString, StringComparison.Ordinal)) {
                        keypair.element = availableChildren.array[j].element;
                        availableChildren.SwapRemoveAt(j);
                        break;
                    }
                }

                if (keypair.element == null) {
                    UIElement child = application.CreateTemplate(templateSpawnId, templateContextRoot, this, scope);
                    ContextVariable<int> indexVariable = new ContextVariable<int>(indexVarId, "index", default);
                    ContextVariable<T> itemVariable = new ContextVariable<T>(itemVarId, "item", default);
                    child.bindingNode.CreateLocalContextVariable(itemVariable);
                    child.bindingNode.CreateLocalContextVariable(indexVariable);
                    keypair.element = child;
                }

                lastFrameChildren.Add(keypair);
            }

            while (availableChildren.size > 0) {
                // set parent to null avoids child reshuffle
                availableChildren.array[availableChildren.size - 1].element.parent = null;
                availableChildren.array[availableChildren.size - 1].element.Destroy();
                availableChildren.size--;
            }

            children.Clear();

            children.EnsureCapacity(lastFrameChildren.size);
            children.size = lastFrameChildren.size;

            for (int i = 0; i < lastFrameChildren.size; i++) {
                UIElement child = lastFrameChildren.array[i].element;
                child.siblingIndex = i;
                children.array[i] = child;
                ((ContextVariable<T>) child.bindingNode.GetContextVariable(itemVarId)).value = list[rangeStart + i];
                ((ContextVariable<int>) child.bindingNode.GetContextVariable(indexVarId)).value = rangeStart + i;
            }
        }

        private void UpdateWithoutKeyFunc(int rangeStart, int rangeEnd) {
            if (prevRangeStart != rangeStart || prevRangeEnd != rangeEnd) {
                int prevCount = prevRangeEnd - prevRangeStart;
                int currCount = rangeEnd - rangeStart;

                // todo -- add option to 'orphan' child on remove

                if (currCount > prevCount) {
                    // first create and add children
                    int diff = currCount - prevCount;
                    for (int i = 0; i < diff; i++) {
                        UIElement child = application.CreateTemplate(templateSpawnId, templateContextRoot, this, scope);

                        
                        ContextVariable<int> indexVariable = new ContextVariable<int>(indexVarId, "index", default);
                        ContextVariable<T> itemVariable = new ContextVariable<T>(itemVarId, "item", default);

                        child.bindingNode.CreateLocalContextVariable(itemVariable);
                        child.bindingNode.CreateLocalContextVariable(indexVariable);
                    }
                }
                else {
                    int diff = prevCount - currCount;
                    for (int i = 0; i < diff; i++) {
                        children.array[children.size - 1].Destroy();
                    }
                }
            }

            for (int i = 0; i < children.size; i++) {
                UIElement child = children.array[i];
                child.siblingIndex = i;

                ContextVariable ptr = child.bindingNode.localVariable;
                
                while (ptr != null) {
                    
                    if (ptr.id == itemVarId) {
                        ((ContextVariable<T>) ptr).value = list[rangeStart + i];
                    }

                    if (ptr.id == indexVarId) {
                        ((ContextVariable<int>) ptr).value = rangeStart + i;
                    }

                    ptr = ptr.next;
                }
                
            }
        }

    }

}