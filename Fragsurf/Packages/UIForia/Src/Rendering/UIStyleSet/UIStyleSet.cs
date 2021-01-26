using System;
using System.Collections.Generic;
using System.Diagnostics;
using Systems.SelectorSystem;
using SVGX;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Parsing.Style;
using UIForia.Systems;
using UIForia.Text;
using UIForia.Util;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace UIForia.Rendering {

    [DebuggerDisplay("id = {element.id} state = {currentState}")]
    public partial class UIStyleSet {

        public readonly UIElement element;

        internal StyleState currentState;
        internal StyleState containedStates;
        internal bool hasAttributeStyles;

        private UIStyleGroup instanceStyle;
        internal readonly StructList<StyleEntry> availableStyles;
        internal readonly LightList<UIStyleGroupContainer> styleGroupContainers; // probably only need to store the names
        internal readonly IntMap<StyleProperty> propertyMap;

        // idea -- for styles are inactive, sort them to the back of the available styles list,
        // then we have to look though less of an array (also track a count for how many styles are active)

        // reduce memory per-style
        // improve lookup time
        // support selectors

        public UIStyleSet(UIElement element) {
            this.element = element;
            this.currentState = StyleState.Normal;
            this.containedStates = StyleState.Normal;
            this.availableStyles = new StructList<StyleEntry>(4);
            this.styleGroupContainers = new LightList<UIStyleGroupContainer>(3);
            this.propertyMap = new IntMap<StyleProperty>();
            this.hasAttributeStyles = false;
        }

        public StyleState CurrentState => currentState;

        public UIStyleSetStateProxy Normal => new UIStyleSetStateProxy(this, StyleState.Normal);
        public UIStyleSetStateProxy Hover => new UIStyleSetStateProxy(this, StyleState.Hover);
        public UIStyleSetStateProxy Focus => new UIStyleSetStateProxy(this, StyleState.Focused);
        public UIStyleSetStateProxy Active => new UIStyleSetStateProxy(this, StyleState.Active);

        internal List<UIStyleGroupContainer> GetBaseStyles() {
            List<UIStyleGroupContainer> retn = ListPool<UIStyleGroupContainer>.Get();
            for (int i = 0; i < styleGroupContainers.Count; i++) {
                retn.Add(styleGroupContainers[i]);
            }

            return retn;
        }

        // manual contains check to avoid boxing in the list implementation
        private static bool ListContainsStyleProperty(LightList<StylePropertyId> list, StylePropertyId target) {
            int arraySize = list.Count;
            StylePropertyId[] array = list.Array;
            for (int j = 0; j < arraySize; j++) {
                if (array[j] == target) {
                    return true;
                }
            }

            return false;
        }

        private void CreateStyleEntry(LightList<StylePropertyId> toUpdate, UIStyleGroup group, UIStyleRunCommand styleRunCommand, StyleType styleType, StyleState styleState, int ruleCount) {
            if (styleRunCommand.style == null) return;

            containedStates |= styleState;

            if ((currentState & styleState) != 0) {
                AddMissingProperties(toUpdate, styleRunCommand.style);
                RunCommands(styleRunCommand.runCommands);
            }

            availableStyles.Add(new StyleEntry(group, styleRunCommand, styleType, styleState, availableStyles.Count, ruleCount));
        }

        internal void ClearPropertyMap() {
            // when clearing the property map we want to retain the styles that we have inherited from elsewhere
            // to do this, we need to read in the inherited values, store them, clear the map, then write the values back
            LightList<StyleProperty> inherited = LightList<StyleProperty>.Get();
            inherited.EnsureCapacity(StyleUtil.InheritedProperties.Count);
            StyleProperty[] inheritedArray = inherited.Array;
            for (int i = 0; i < StyleUtil.InheritedProperties.Count; i++) {
                int key = BitUtil.SetHighLowBits(1, (int) StyleUtil.InheritedProperties[i]);
                if (propertyMap.TryGetValue(key, out StyleProperty inheritedValue)) {
                    inherited.AddUnchecked(inheritedValue);
                }
            }

            propertyMap.Clear();
            // re-apply values
            for (int i = 0; i < inherited.Count; i++) {
                int key = BitUtil.SetHighLowBits(1, (int) inheritedArray[i].propertyId);
                propertyMap.Add(key, inheritedArray[i]);
            }

            LightList<StyleProperty>.Release(ref inherited);
        }

        public void internal_AddBaseStyle(UIStyleGroupContainer style) {
            styleGroupContainers.Add(style);
        }

        public void internal_Initialize() {
            containedStates = 0;
            hasAttributeStyles = false;

            LightList<StylePropertyId> toUpdate = LightList<StylePropertyId>.Get();

            if (instanceStyle != null) {
                CreateStyleEntry(toUpdate, instanceStyle, instanceStyle.normal, StyleType.Instance, StyleState.Normal, 0);
                CreateStyleEntry(toUpdate, instanceStyle, instanceStyle.hover, StyleType.Instance, StyleState.Hover, 0);
                CreateStyleEntry(toUpdate, instanceStyle, instanceStyle.focused, StyleType.Instance, StyleState.Focused, 0);
                CreateStyleEntry(toUpdate, instanceStyle, instanceStyle.active, StyleType.Instance, StyleState.Active, 0);
            }

            for (int i = 0; i < styleGroupContainers.size; i++) {
                CreateStyleGroups(styleGroupContainers.array[i], toUpdate);
            }

            SortStyles();

            UpdatePropertyMap(toUpdate);

            LightList<StylePropertyId>.Release(ref toUpdate);
        }

        private void AppendSharedStyles(LightList<UIStyleGroupContainer> updatedStyles, int index) {
            int count = updatedStyles.Count;
            UIStyleGroupContainer[] updatedStyleArray = updatedStyles.Array;

            LightList<StylePropertyId> toUpdate = LightList<StylePropertyId>.Get();
            styleGroupContainers.EnsureAdditionalCapacity(updatedStyles.Count - index);

            for (int i = index; i < count; i++) {
                CreateStyleGroups(updatedStyleArray[i], toUpdate);
                styleGroupContainers.array[i] = updatedStyleArray[i];
            }

            styleGroupContainers.size = count;

            SortStyles();

            UpdatePropertyMap(toUpdate);

            LightList<StylePropertyId>.Release(ref toUpdate);
        }

        private void ResetSharedStyles(LightList<UIStyleGroupContainer> updatedStyles) {
            int count = updatedStyles.Count;
            UIStyleGroupContainer[] updatedStyleArray = updatedStyles.array;

            for (int i = 0; i < styleGroupContainers.size; i++) {

                if (!updatedStyles.Contains(styleGroupContainers.array[i])) {
                    for (int j = 0; j < styleGroupContainers.array[i].groups.Length; j++) {
                        RunCommands(styleGroupContainers.array[i].groups[j].normal.runCommands, false);
                    }
                }

            }

            availableStyles.Clear();
            styleGroupContainers.Clear();
            ClearPropertyMap();

            styleGroupContainers.EnsureCapacity(updatedStyles.size);

            containedStates = 0;
            hasAttributeStyles = false;

            LightList<StylePropertyId> toUpdate = LightList<StylePropertyId>.Get();

            if (instanceStyle != null) {
                CreateStyleEntry(toUpdate, instanceStyle, instanceStyle.normal, StyleType.Instance, StyleState.Normal, 0);
                CreateStyleEntry(toUpdate, instanceStyle, instanceStyle.hover, StyleType.Instance, StyleState.Hover, 0);
                CreateStyleEntry(toUpdate, instanceStyle, instanceStyle.focused, StyleType.Instance, StyleState.Focused, 0);
                CreateStyleEntry(toUpdate, instanceStyle, instanceStyle.active, StyleType.Instance, StyleState.Active, 0);
            }

            for (int i = 0; i < count; i++) {
                CreateStyleGroups(updatedStyleArray[i], toUpdate);
                styleGroupContainers.array[i] = updatedStyleArray[i];
            }

            styleGroupContainers.size = count;

            SortStyles();

            UpdatePropertyMap(toUpdate);

            LightList<StylePropertyId>.Release(ref toUpdate);
        }

        internal void UpdateSharedStyles(LightList<UIStyleGroupContainer> updatedStyles) {
            int count = styleGroupContainers.Count;
            UIStyleGroupContainer[] currentContainers = styleGroupContainers.Array;
            UIStyleGroupContainer[] updatedContainers = updatedStyles.Array;

            if (updatedStyles.Count > styleGroupContainers.Count) {
                // if we have more styles in the incoming list
                // check that all existing styles match
                // if they do, make sure all updated styles are not present in the template
                for (int i = 0; i < count; i++) {
                    if (currentContainers[i] != updatedContainers[i]) {
                        ResetSharedStyles(updatedStyles);
                        return;
                    }
                }

                AppendSharedStyles(updatedStyles, count);
            }
            else if (updatedStyles.Count < styleGroupContainers.Count) {
                // todo -- optimize
                ResetSharedStyles(updatedStyles);
            }
            else {
                // todo -- optimize
                for (int i = 0; i < count; i++) {
                    if (currentContainers[i] != updatedContainers[i]) {
                        ResetSharedStyles(updatedStyles);
                        return;
                    }
                }
            }
        }

        private void CreateStyleGroups(UIStyleGroupContainer groupContainer, LightList<StylePropertyId> toUpdate) {
            for (int i = 0; i < groupContainer.groups.Length; i++) {
                UIStyleGroup group = groupContainer.groups[i];

                if (group.HasAttributeRule) {
                    hasAttributeStyles = true;
                }

                if (group.rule == null || group.rule != null && group.rule.IsApplicableTo(element)) {
                    int ruleCount = group.CountRules();
                    CreateStyleEntry(toUpdate, group, group.normal, groupContainer.styleType, StyleState.Normal, ruleCount);
                    CreateStyleEntry(toUpdate, group, group.hover, groupContainer.styleType, StyleState.Hover, ruleCount);
                    CreateStyleEntry(toUpdate, group, group.focused, groupContainer.styleType, StyleState.Focused, ruleCount);
                    CreateStyleEntry(toUpdate, group, group.active, groupContainer.styleType, StyleState.Active, ruleCount);
                }
            }
        }

        private void AddStyleGroups(LightList<StylePropertyId> toUpdate, UIStyleGroupContainer container) {
            styleGroupContainers.Add(container);

            if (container.hasAttributeStyles) {
                hasAttributeStyles = true;
            }

            for (int j = 0; j < container.groups.Length; j++) {
                UIStyleGroup group = container.groups[j];

                if (group.rule == null || group.rule != null && group.rule.IsApplicableTo(element)) {
                    int ruleCount = group.CountRules();
                    CreateStyleEntry(toUpdate, group, group.normal, container.styleType, StyleState.Normal, ruleCount);
                    CreateStyleEntry(toUpdate, group, group.hover, container.styleType, StyleState.Hover, ruleCount);
                    CreateStyleEntry(toUpdate, group, group.focused, container.styleType, StyleState.Focused, ruleCount);
                    CreateStyleEntry(toUpdate, group, group.active, container.styleType, StyleState.Active, ruleCount);
                }
            }
        }

        private static void AddMissingProperties(LightList<StylePropertyId> toUpdate, UIStyle style) {
            int count = style.PropertyCount;
            StyleProperty[] properties = style.array;

            for (int i = 0; i < count; i++) {
                StylePropertyId propertyId = properties[i].propertyId;
                if (!ListContainsStyleProperty(toUpdate, propertyId)) {
                    toUpdate.Add(propertyId);
                }
            }
        }

        internal void EnterState(StyleState state) {
            if (state == StyleState.Normal || (currentState & state) != 0) {
                return;
            }

            StyleState oldState = currentState;
            currentState |= state;

            if ((containedStates & state) == 0) {
                return;
            }

            LightList<StylePropertyId> toUpdate = LightList<StylePropertyId>.Get();
            IStyleSystem styleSystem = element.application.styleSystem;

            StyleEntry[] styleEntries = availableStyles.Array;
            for (int i = 0; i < availableStyles.Count; i++) {
                StyleEntry entry = styleEntries[i];

                // if this is a state we had not been in before, mark it's properties for update
                if ((entry.state & oldState) == 0 && (entry.state & state) != 0) {
                    AddMissingProperties(toUpdate, entry.styleRunCommand.style);
                    RunCommands(entry.styleRunCommand.runCommands);
                }
            }

            UpdatePropertyMap(toUpdate);

            LightList<StylePropertyId>.Release(ref toUpdate);
        }

        /// <summary>
        /// Runs all runCommands that are applicable for the current state.
        /// </summary>
        internal void RunCommands() {
            StyleEntry[] styleEntries = availableStyles.Array;
            for (int i = 0; i < availableStyles.Count; i++) {
                StyleEntry entry = styleEntries[i];
                if ((entry.state & currentState) != 0) {
                    RunCommands(entry.styleRunCommand.runCommands);
                }
            }
        }

        private void RunCommands(LightList<IRunCommand> runCommands, bool enter = true) {
            if (runCommands == null) {
                return;
            }

            for (int index = 0; index < runCommands.Count; index++) {
                if (enter && (runCommands[index].RunCommandType & RunCommandType.Enter) != 0) {
                    runCommands[index].Run(element, RunCommandType.Enter);
                }
                else if (!enter &&  (runCommands[index].RunCommandType & RunCommandType.Exit) != 0) {
                    runCommands[index].Run(element, RunCommandType.Exit);
                }
            }
        }

        internal void ExitState(StyleState state) {
            if (state == StyleState.Normal || (currentState & state) == 0) {
                return;
            }

            StyleState oldState = currentState;
            currentState &= ~(state);
            currentState |= StyleState.Normal;

            if ((containedStates & state) == 0) {
                return;
            }

            LightList<StylePropertyId> toUpdate = LightList<StylePropertyId>.Get();

            StyleEntry[] styleEntries = availableStyles.Array;
            for (int i = 0; i < availableStyles.Count; i++) {
                StyleEntry entry = styleEntries[i];

                // if this a state we were in that is now invalid, mark it's properties for update
                if ((entry.state & oldState) != 0 && (entry.state & state) != 0) {
                    AddMissingProperties(toUpdate, entry.styleRunCommand.style);
                    RunCommands(entry.styleRunCommand.runCommands, false);
                }
            }

            UpdatePropertyMap(toUpdate);

            LightList<StylePropertyId>.Release(ref toUpdate);
        }

        internal void UpdateInheritedStyles() {
            if (element.parent == null) {
                return;
            }

            int count = StyleUtil.InheritedProperties.Count;

            UIStyleSet parentStyle = element.parent.style;

            if (parentStyle == null) return;

            for (int i = 0; i < count; i++) {
                int propertyId = (int) StyleUtil.InheritedProperties[i];
                int key = BitUtil.SetHighLowBits(1, propertyId);
                propertyMap[key] = parentStyle.GetComputedStyleProperty(StyleUtil.InheritedProperties[i]);
            }
        }

        internal bool SetInheritedStyle(StyleProperty property) {
            if (propertyMap.ContainsKey((int) property.propertyId)) {
                return false;
            }

            int key = BitUtil.SetHighLowBits(1, (int) property.propertyId);
            StyleProperty current;
            if (propertyMap.TryGetValue(key, out current)) {
                if (current != property) {
                    propertyMap[key] = property;
                    return true;
                }

                return false;
            }
            else {
                propertyMap[key] = property;
                return true;
            }
        }

        public bool IsInState(StyleState state) {
            return (currentState & state) != 0;
        }

        public bool IsHovered {
            get => IsInState(StyleState.Hover);
        }

        public bool IsFocused {
            get => IsInState(StyleState.Focused);
        }

        public bool IsActive {
            get => IsInState(StyleState.Active);
        }

        public bool HasBaseStyles => styleGroupContainers.Count > 0;

        public float LineHeightSize => 16f; // todo -- wrong

        // todo -- handle inherited?
        public bool IsDefined(StylePropertyId propertyId) {
            return propertyMap.ContainsKey((int) propertyId);
        }

        internal void AddStyleGroupContainer(UIStyleGroupContainer container) {
            if (styleGroupContainers.Contains(container)) {
                return;
            }

            LightList<StylePropertyId> toUpdate = LightList<StylePropertyId>.Get();

            AddStyleGroups(toUpdate, container);

            SortStyles();

            UpdatePropertyMap(toUpdate);

            LightList<StylePropertyId>.Release(ref toUpdate);
        }

        public void RemoveStyleGroupContainer(UIStyleGroupContainer container) {
            if (!styleGroupContainers.Contains(container)) {
                return;
            }

            styleGroupContainers.Remove(container);

            LightList<StylePropertyId> toUpdate = LightList<StylePropertyId>.Get();

            for (int i = 0; i < container.groups.Length; i++) {
                UIStyleGroup group = container.groups[i];

                for (int j = 0; j < availableStyles.Count; j++) {
                    if (availableStyles[j].sourceGroup == group) {
                        if ((availableStyles[j].state & currentState) != 0) {
                            AddMissingProperties(toUpdate, availableStyles[j].styleRunCommand.style);
                            RunCommands(availableStyles[j].styleRunCommand.runCommands);
                        }

                        availableStyles.RemoveAt(j--);
                    }
                }
            }

            for (int i = 0; i < styleGroupContainers.Count; i++) {
                hasAttributeStyles = hasAttributeStyles || styleGroupContainers[i].hasAttributeStyles;
            }

            for (int i = 0; i < availableStyles.Count; i++) {
                containedStates |= availableStyles[i].state;
            }

            UpdatePropertyMap(toUpdate);

            // todo -- handle inheritance, probably done in the style system and not here

            LightList<StylePropertyId>.Release(ref toUpdate);
        }

        private void SortStyles() {
            availableStyles.Sort((a, b) => a.priority < b.priority ? 1 : -1);
        }

        public StyleProperty GetPropertyValue(StylePropertyId propertyId) {
            // can't use ComputedStyle here because this is used to compute that value
            return GetPropertyValueInState(propertyId, currentState);
        }

        // I think this won't return normal or inherited styles right now, should it?
        public StyleProperty GetPropertyValueInState(StylePropertyId propertyId, StyleState state) {
            for (int i = 0; i < availableStyles.Count; i++) {
                if ((availableStyles[i].state & state) == 0) {
                    continue;
                }

                StyleProperty property;
                if (availableStyles[i].styleRunCommand.style.TryGetProperty(propertyId, out property)) {
                    return property;
                }
            }

            return DefaultStyleValues_Generated.GetPropertyValue(propertyId);
//            return new StyleProperty(propertyId, IntUtil.UnsetValue, IntUtil.UnsetValue, FloatUtil.UnsetValue, null);
        }

        // I think this won't return normal or inherited styles right now, should it?
        public bool TryGetPropertyValueInState(StylePropertyId propertyId, StyleState state, out StyleProperty property) {
            for (int i = 0; i < availableStyles.Count; i++) {
                if ((availableStyles[i].state & state) == 0) {
                    continue;
                }

                if (availableStyles[i].styleRunCommand.style.TryGetProperty(propertyId, out property)) {
                    return true;
                }
            }

            property = default;
            return false;
        }

        private UIStyle GetOrCreateInstanceStyle(StyleState state) {
            if (instanceStyle == null) {
                instanceStyle = new UIStyleGroup() {
                    name = "Instance",
                    styleType = StyleType.Instance
                };
            }

            switch (state) {
                case StyleState.Normal:
                    if (instanceStyle.normal.style == null) {
                        instanceStyle.normal = UIStyleRunCommand.CreateInstance();
                        availableStyles.Add(new StyleEntry(instanceStyle, instanceStyle.normal, StyleType.Instance, StyleState.Normal, byte.MaxValue, byte.MaxValue));
                        containedStates |= StyleState.Normal;
                        SortStyles();
                    }

                    return instanceStyle.normal.style;

                case StyleState.Hover:
                    if (instanceStyle.hover.style == null) {
                        instanceStyle.hover = UIStyleRunCommand.CreateInstance();
                        availableStyles.Add(new StyleEntry(instanceStyle, instanceStyle.hover, StyleType.Instance, StyleState.Hover, byte.MaxValue, byte.MaxValue));
                        containedStates |= StyleState.Hover;
                        SortStyles();
                    }

                    return instanceStyle.hover.style;

                case StyleState.Active:
                    if (instanceStyle.active.style == null) {
                        instanceStyle.active = UIStyleRunCommand.CreateInstance();
                        availableStyles.Add(new StyleEntry(instanceStyle, instanceStyle.active, StyleType.Instance, StyleState.Active, byte.MaxValue, byte.MaxValue));
                        containedStates |= StyleState.Active;
                        SortStyles();
                    }

                    return instanceStyle.active.style;

                case StyleState.Focused:
                    if (instanceStyle.focused.style == null) {
                        instanceStyle.focused = UIStyleRunCommand.CreateInstance();
                        availableStyles.Add(new StyleEntry(instanceStyle, instanceStyle.focused, StyleType.Instance, StyleState.Focused, byte.MaxValue, byte.MaxValue));
                        containedStates |= StyleState.Focused;
                        SortStyles();
                    }

                    return instanceStyle.focused.style;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private UIStyleGroupContainer FindContainerForGroup(UIStyleGroup group) {
            for (int i = 0; i < styleGroupContainers.size; i++) {
                int groupsCount = styleGroupContainers[i].groups.Length;
                for (int j = 0; j < groupsCount; j++) {
                    if (styleGroupContainers[i].groups[j] == group) {
                        return styleGroupContainers[i];
                    }
                }
            }

            return null;
        }

        public string GetPropertySource(StylePropertyId propertyId) {
            if (!IsDefined(propertyId)) {
                return "Default";
            }

            for (int i = 0; i < availableStyles.Count; i++) {
                if ((currentState & availableStyles[i].state) == 0) {
                    continue;
                }

                if (availableStyles[i].styleRunCommand.style.DefinesProperty(propertyId)) {
                    if (availableStyles[i].type == StyleType.Instance) {
                        switch (availableStyles[i].state) {
                            case StyleState.Normal:
                                return "Instance [Normal]";

                            case StyleState.Hover:
                                return "Instance [Hover]";

                            case StyleState.Active:
                                return "Instance [Active]";

                            case StyleState.Focused:
                                return "Instance [Focused]";

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    if (availableStyles[i].type == StyleType.Shared) {
                        UIStyleGroupContainer container = FindContainerForGroup(availableStyles[i].sourceGroup);
                        string containerName = container?.name ?? "Unknown";
                        switch (availableStyles[i].state) {
                            case StyleState.Normal:
                                return $"{containerName} [Normal]";

                            case StyleState.Active:
                                return $"{containerName} [Active]";

                            case StyleState.Hover:
                                return $"{containerName} [Hover]";

                            case StyleState.Focused:
                                return $"{containerName} [Focused]";

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    switch (availableStyles[i].state) {
                        case StyleState.Normal:
                            return $"<{element.GetDisplayName()}> [Normal]";

                        case StyleState.Active:
                            return $"<{element.GetDisplayName()}> [Active]";

                        case StyleState.Hover:
                            return $"<{element.GetDisplayName()}> [Hover]";

                        case StyleState.Focused:
                            return $"<{element.GetDisplayName()}> [Focused]";

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return "Unknown";
        }

        public void SetProperty(in StyleProperty property, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            if ((state & currentState) == 0) {
                style.SetProperty(property);
                return;
            }

            StyleProperty oldValue = GetPropertyValue(property.propertyId);

            style.SetProperty(property);

            IStyleSystem styleSystem = element.application.styleSystem;

            StyleProperty currentValue;
            if (TryGetPropertyValueInState(property.propertyId, currentState, out currentValue)) {

                if (oldValue != currentValue) {
                    propertyMap[(int) property.propertyId] = currentValue;
                    styleSystem?.SetStyleProperty(element, currentValue);
                }
            }
            else {
                propertyMap.Remove((int) property.propertyId);
                styleSystem?.SetStyleProperty(element, property);
            }
        }

        public void SetAnimatedProperty(StyleProperty property) {
            if (StyleUtil.CanAnimate(property.propertyId)) {
                //animatedProperties[property.propertyId] = animatedProperty;
                SetProperty(property, StyleState.Normal); // todo -- need another priority group for this
            }
        }

        private int FindNextStyleEntryForGroup(UIStyleGroup group, int startIdx) {
            StyleEntry[] entries = availableStyles.Array;
            for (int i = startIdx; i < availableStyles.Count; i++) {
                if (entries[i].sourceGroup == group) return i;
            }

            return -1;
        }

        public void UpdateApplicableAttributeRules() {
            if (!hasAttributeStyles) return;

            int containerCount = styleGroupContainers.Count;
            UIStyleGroupContainer[] containers = styleGroupContainers.Array;

            LightList<StylePropertyId> toUpdate = LightList<StylePropertyId>.Get();

            for (int i = 0; i < containerCount; i++) {
                IReadOnlyList<UIStyleGroup> groups = containers[i].groups;
                for (int j = 0; j < groups.Count; j++) {
                    UIStyleGroup group = groups[j];

                    if (!group.HasAttributeRule) {
                        continue;
                    }

                    bool isApplicable = group.rule.IsApplicableTo(element);
                    // if the rule no longer applies, remove the entries from this group from the list
                    // if we had properties from this group that were active, add them to our update list to be recomputed

                    if (!isApplicable) {
                        int nextIdx = 0;
                        while (true) {
                            nextIdx = FindNextStyleEntryForGroup(group, nextIdx);

                            if (nextIdx >= 0) {
                                bool isActive = (availableStyles[nextIdx].state & currentState) != 0;

                                if (isActive) {
                                    AddMissingProperties(toUpdate, availableStyles[nextIdx].styleRunCommand.style);
                                    RunCommands(availableStyles[nextIdx].styleRunCommand.runCommands, false);
                                }

                                availableStyles.RemoveAt(nextIdx);
                                continue;
                            }

                            break;
                        }
                    }
                    else {
                        // if currently active, check if it we have any styles from this group 
                        // if we do then we don't need to do anything
                        // if we don't then we need to create style entries 
                        int nextIdx = FindNextStyleEntryForGroup(group, 0);
                        if (nextIdx == -1) {
                            int ruleCount = group.CountRules();
                            CreateStyleEntry(toUpdate, group, group.normal, group.styleType, StyleState.Normal, ruleCount);
                            CreateStyleEntry(toUpdate, group, group.hover, group.styleType, StyleState.Hover, ruleCount);
                            CreateStyleEntry(toUpdate, group, group.focused, group.styleType, StyleState.Focused, ruleCount);
                            CreateStyleEntry(toUpdate, group, group.active, group.styleType, StyleState.Active, ruleCount);
                        }
                    }
                }
            }

            SortStyles();

            UpdatePropertyMap(toUpdate);

            LightList<StylePropertyId>.Release(ref toUpdate);
        }

        private void UpdatePropertyMap(LightList<StylePropertyId> toUpdate) {
            StylePropertyId[] propertyIdArray = toUpdate.Array;
            IStyleSystem styleSystem = element.application.styleSystem;

            for (int i = 0; i < toUpdate.Count; i++) {
                StylePropertyId propertyId = propertyIdArray[i];

                StyleProperty oldValue = propertyMap[(int) propertyId];
                if (TryGetPropertyValueInState(propertyId, currentState, out StyleProperty property)) {
                    if (oldValue != property) {
                        propertyMap[(int) property.propertyId] = property;
                        styleSystem?.SetStyleProperty(element, property);
                    }
                }
                else {
                    propertyMap.Remove((int) propertyId);
                    styleSystem?.SetStyleProperty(element, GetPropertyValue(propertyId));
                }
            }

            //styleSystem.SetStyleProperties(element, toUpdate);
        }

        public void SetGridLayoutColAutoSize(in GridTrackSize trackSize, StyleState state) {
            SetGridLayoutColAutoSize(new[] {trackSize}, state);
        }

        public void SetGridLayoutRowAutoSize(in GridTrackSize trackSize, StyleState state) {
            SetGridLayoutRowAutoSize(new[] {trackSize}, state);
        }

        public void SetGridItemPlacement(int x, int y, int width, int height, StyleState state) {
            SetGridItemX(new GridItemPlacement(x), state);
            SetGridItemY(new GridItemPlacement(y), state);
            SetGridItemWidth(new GridItemPlacement(width < 1 ? 1 : width), state);
            SetGridItemHeight(new GridItemPlacement(height < 1 ? 1 : height), state);
        }

        public void SetGridItemPlacement(int x, int y, StyleState state) {
            SetGridItemX(new GridItemPlacement(x), state);
            SetGridItemY(new GridItemPlacement(y), state);
            SetGridItemWidth(new GridItemPlacement(1), state);
            SetGridItemHeight(new GridItemPlacement(1), state);
        }

        public BorderRadius GetBorderRadius(StyleState state) {
            return new BorderRadius(
                GetBorderRadiusTopLeft(state),
                GetBorderRadiusTopRight(state),
                GetBorderRadiusBottomRight(state),
                GetBorderRadiusBottomLeft(state)
            );
        }

        public void SetBorder(FixedLengthRect value, StyleState state) {
            SetBorderTop(value.top, state);
            SetBorderRight(value.right, state);
            SetBorderBottom(value.bottom, state);
            SetBorderLeft(value.left, state);
        }

        public FixedLengthRect GetBorder(StyleState state) {
            return new FixedLengthRect(
                GetBorderTop(state),
                GetBorderRight(state),
                GetBorderBottom(state),
                GetBorderLeft(state)
            );
        }

        public void SetMargin(FixedLengthRect value, StyleState state) {
            SetMarginTop(value.top, state);
            SetMarginRight(value.right, state);
            SetMarginBottom(value.bottom, state);
            SetMarginLeft(value.bottom, state);
        }

        public FixedLengthRect GetMargin(StyleState state) {
            return new FixedLengthRect(
                GetMarginTop(state),
                GetMarginRight(state),
                GetMarginBottom(state),
                GetMarginLeft(state)
            );
        }

        public void SetPadding(FixedLengthRect value, StyleState state) {
            SetPaddingTop(value.top, state);
            SetPaddingRight(value.right, state);
            SetPaddingBottom(value.bottom, state);
            SetPaddingLeft(value.left, state);
        }

        public FixedLengthRect GetPadding(StyleState state) {
            return new FixedLengthRect(
                GetPaddingTop(state),
                GetPaddingRight(state),
                GetPaddingBottom(state),
                GetPaddingLeft(state)
            );
        }

        public void SetBorderRadius(BorderRadius newBorderRadius, StyleState state) {
            SetBorderRadiusBottomLeft(newBorderRadius.bottomLeft, state);
            SetBorderRadiusBottomRight(newBorderRadius.bottomRight, state);
            SetBorderRadiusTopRight(newBorderRadius.topRight, state);
            SetBorderRadiusTopLeft(newBorderRadius.topLeft, state);
        }

        public void SetTransformPosition(Vector2 position, StyleState state) {
            SetTransformPositionX(position.x, state);
            SetTransformPositionY(position.y, state);
        }

#if UNITY_EDITOR
        /// <summary>
        ///  Keeping this for the debugger display
        /// </summary>
        /// <param name="retn"></param>
        /// <returns></returns>
        public IList<string> GetStyleNameList(IList<string> retn = null) {
            retn = retn ?? new List<string>(styleGroupContainers.Count);
            for (int i = 0; i < styleGroupContainers.Count; i++) {
                if (styleGroupContainers[i].styleType == StyleType.Shared) {
                    retn.Add(styleGroupContainers[i].name);
                }
            }

            return retn;
        }

        /// <summary>
        /// For Inspector / debugging only!
        /// </summary>
        /// <returns></returns>
        internal UIStyleGroup GetInstanceStyle() {
            return instanceStyle;
        }
#endif

        // base -> selectors -> instance
        public void SetBaseStyles(LightList<UIStyleGroupContainer> styles) {
            // todo -- this could be a lot faster, this is happening every frame in dynamic bindings :(

            for (int i = 0; i < styles.size; i++) {
                if (styles[i] == null) {
                    styles.RemoveAt(i--);
                }
            }

            UpdateSharedStyles(styles);
        }

        public string GetStyleNames() {
            TextUtil.StringBuilder.Clear();

            for (int i = 0; i < styleGroupContainers.Count; i++) {
                if (styleGroupContainers[i].styleType == StyleType.Shared) {
                    if (TextUtil.StringBuilder.Length > 0) {
                        TextUtil.StringBuilder.Append(" ");
                    }

                    TextUtil.StringBuilder.Append(styleGroupContainers[i].name);
                }
            }

            return TextUtil.StringBuilder.ToString();
        }

        // todo -- explore caching this value
        public float GetResolvedFontSize() {
            UIFixedLength fontSize = TextFontSize;
            switch (fontSize.unit) {
                case UIFixedUnit.Unset:
                    return 0;

                case UIFixedUnit.Pixel:
                    return fontSize.value;

                case UIFixedUnit.Em:
                case UIFixedUnit.Percent:

                    if (element.parent != null) {
                        return element.parent.style.GetResolvedFontSize() * fontSize.value;
                    }

                    return DefaultStyleValues_Generated.TextFontSize.value * fontSize.value;

                case UIFixedUnit.ViewportWidth:
                    return element.View.Viewport.width * fontSize.value;

                case UIFixedUnit.ViewportHeight:
                    return element.View.Viewport.height * fontSize.value;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public SVGXTextStyle GetTextStyle() {
            return new SVGXTextStyle() {
                fontSize = GetResolvedFontSize(),
                alignment = TextAlignment,
                fontAsset = TextFontAsset,
                fontStyle = TextFontStyle,
                glowOffset = TextGlowOffset,
                glowOuter = TextGlowOuter,
                glowColor = TextGlowColor,
                outlineColor = TextOutlineColor,
                outlineSoftness = TextOutlineSoftness,
                outlineWidth = TextOutlineWidth,
                textTransform = TextTransform,
                whitespaceMode = TextWhitespaceMode,
                textColor = TextColor,
                faceDilate = TextFaceDilate,
                underlayColor = TextUnderlayColor,
                underlaySoftness = TextUnderlaySoftness,
                underlayX = TextUnderlayX,
                underlayY = TextUnderlayY,
                underlayDilate = TextUnderlayDilate,
            };
        }

        public void AddSelectorStyleGroup(Selector selector) {
            throw new NotImplementedException();
        }

        public void RemoveSelectorStyleGroup(Selector selector) {
            throw new NotImplementedException();
        }

        internal StructList<AnimatedProperty> animatedProperties;

        internal AnimationFlags animationFlags;

        internal bool TryGetAnimatedProperty(StylePropertyId propertyId, out AnimatedProperty property) {
            if (animatedProperties == null) {
                property = default;
                return false;
            }

            for (int i = 0; i < animatedProperties.size; i++) {
                ref AnimatedProperty animatedProperty = ref animatedProperties.array[i];
                if (animatedProperty.propertyId == propertyId) {
                    property = animatedProperty;
                    return true;
                }
            }

            property = default;
            return false;
        }

        internal void SetAnimatedMeasurementProperty(StylePropertyId propertyId, in StyleProperty v0, in StyleProperty v1, float time) {
            animatedProperties = animatedProperties ?? StructList<AnimatedProperty>.Get();
            switch (propertyId) {
                case StylePropertyId.PreferredWidth:
                    animationFlags |= AnimationFlags.PreferredWidth;
                    break;

                case StylePropertyId.MinWidth:
                    animationFlags |= AnimationFlags.MinWidth;
                    break;

                case StylePropertyId.MaxWidth:
                    animationFlags |= AnimationFlags.MaxWidth;
                    break;

                case StylePropertyId.PreferredHeight:
                    animationFlags |= AnimationFlags.PreferredHeight;
                    break;

                case StylePropertyId.MinHeight:
                    animationFlags |= AnimationFlags.MinHeight;
                    break;

                case StylePropertyId.MaxHeight:
                    animationFlags |= AnimationFlags.MaxHeight;
                    break;
            }

            for (int i = 0; i < animatedProperties.size; i++) {
                ref AnimatedProperty animatedProperty = ref animatedProperties.array[i];
                if (animatedProperty.propertyId == propertyId) {
                    animatedProperty = new AnimatedProperty(propertyId, v0, v1, time);
                    return;
                }
            }

            animatedProperties.Add(new AnimatedProperty(propertyId, v0, v1, time));
            // styleSystem.SetStyleProperty(element, propertyId);
        }
        
        public void SetAlignmentPercentageX(float percentage, StyleState state = StyleState.Normal) {
            SetAlignmentOriginX(new OffsetMeasurement(percentage, OffsetMeasurementUnit.Percent), state);
            SetAlignmentOffsetX(new OffsetMeasurement(-percentage, OffsetMeasurementUnit.Percent), state);
        }
        
        public void SetAlignmentPercentageY(float percentage, StyleState state = StyleState.Normal) {
            SetAlignmentOriginY(new OffsetMeasurement(percentage, OffsetMeasurementUnit.Percent), state);
            SetAlignmentOffsetY(new OffsetMeasurement(-percentage, OffsetMeasurementUnit.Percent), state);
        }
    }

    [Flags]
    public enum AnimationFlags {

        PreferredWidth = 1 << 0,
        MinWidth = 1 << 1,
        MaxWidth = 1 << 2,
        PreferredHeight = 1 << 3,
        MinHeight = 1 << 4,
        MaxHeight = 1 << 5

    }

}