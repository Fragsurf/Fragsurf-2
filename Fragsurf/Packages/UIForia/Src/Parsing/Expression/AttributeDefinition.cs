using System;
using System.Diagnostics;
using UIForia.Compilers;
using UIForia.Util;

namespace UIForia.Parsing.Expressions {

    [Flags]
    public enum AttributeType {

        Context = 1,
        Alias = 1 << 2,
        Property = 1 << 3,
        Style = 1 << 4,
        Attribute = 1 << 5,
        Event = 1 << 6,
        Conditional = 1 << 7,
        Mouse = 1 << 8,
        Key = 1 << 9,
        Controller = 1 << 10,
        Touch = 1 << 11,
        Slot = 1 << 12,
        InstanceStyle = 1 << 13,
        Expose = 1 << 14,
        ImplicitVariable = 1 << 15,
        ChangeHandler = 1 << 16,
        Drag = 1 << 17

    }

    [Flags]
    public enum AttributeFlags : ushort {

        Const = 1 << 1,
        EnableOnly = 1 << 2,
        InnerContext = 1 << 3,
        Sync = 1 << 4,
        OnChange = 1 << 5,

        StyleStateHover = 1 << 6,
        StyleStateFocus = 1 << 7,
        StyleStateActive = 1 << 8

    }


    public class SlotAttributeData {

        public int slotDepth;
        public ProcessedType slotContextType;
        public ContextVariableDefinition[] contextStack;
        public LightList<string> namespaces;
        public TemplateMetaData templateMetaData;

    }

    [DebuggerDisplay("type={type} {key}={value}")]
    public struct AttributeDefinition {

        public readonly string key;
        public readonly string value;
        public readonly string rawValue;
        public int line;
        public int column;
        public AttributeType type;
        public AttributeFlags flags;
        public SlotAttributeData slotAttributeData;
        public TemplateShell templateShell;
        
        public AttributeDefinition(string rawValue, AttributeType type, AttributeFlags flags,  string key, string value, TemplateShell templateShell, int line = -1, int column = -1) {
            this.rawValue = rawValue;
            this.type = type;
            this.flags = flags;
            this.key = key;
            this.value = value;
            this.templateShell = templateShell;
            this.line = line;
            this.column = column;
            this.slotAttributeData = null;
        }

        public AttributeNodeDebugData DebugData => new AttributeNodeDebugData() {
            content = rawValue,
            fileName = templateShell.filePath,
            lineInfo = new TemplateLineInfo(line, column),
            tagName = ""
        };
        
        public string StrippedValue {
            get {
                if (value[0] == '{' && value[value.Length -1] == '}') {
                    return value.Substring(1, value.Length - 2);
                }

                return value;
            }
        }

    }

}