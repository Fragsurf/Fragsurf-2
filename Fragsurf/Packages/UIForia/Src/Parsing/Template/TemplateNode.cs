using System;
using UIForia.Parsing.Expressions;
using UIForia.Text;
using UIForia.Util;

namespace UIForia.Parsing {

    public struct TemplateNodeDebugData {

        public string fileName;
        public string tagName;
        public TemplateLineInfo lineInfo;

        public override string ToString() {
            return $"<{tagName}> @({fileName} line {lineInfo})";
        }

    }

    public struct AttributeNodeDebugData {

        public string fileName;
        public string tagName;
        public string content;
        public TemplateLineInfo lineInfo;

        public AttributeNodeDebugData(string fileName, string tagName, TemplateLineInfo lineInfo, string content) {
            this.fileName = fileName;
            this.tagName = tagName;
            this.lineInfo = lineInfo;
            this.content = content;
        }
    }
    
        
    public abstract class TemplateNode {

        public StructList<AttributeDefinition> attributes;
        public LightList<TemplateNode> children;
        public TemplateRootNode root;
        public TemplateNode parent;
        public ProcessedType processedType;
        public string originalString;
        public string tagName;
        public string namespaceName;
        public TemplateLineInfo lineInfo;
        public string genericTypeResolver;
        public string requireType;
        public bool isModified;

        protected TemplateNode(TemplateRootNode root, TemplateNode parent, ProcessedType processedType, StructList<AttributeDefinition> attributes, in TemplateLineInfo templateLineInfo) {
            this.root = root;
            this.parent = parent;
            this.attributes = attributes;
            this.processedType = processedType;
            this.lineInfo = templateLineInfo;
        }

        public virtual void AddChild(TemplateNode child) {
            children = children ?? new LightList<TemplateNode>();
            children.Add(child);
        }

        public bool HasProperty(string attr) {
            if (attributes == null) return false;
            for (int i = 0; i < attributes.size; i++) {
                if (attributes.array[i].type == AttributeType.Property) {
                    if (attributes.array[i].key == attr) {
                        return true;
                    }
                }
            }

            return false;
        }

        public TemplateNode this[int i] => children?.array[i];

        public int ChildCount => children?.size ?? 0;
        public Type ElementType => processedType.rawType;
        
        public TemplateNodeDebugData TemplateNodeDebugData => new TemplateNodeDebugData() {
            lineInfo = lineInfo,
            tagName = tagName,
            fileName = root != null ? root.templateShell.filePath : ((TemplateRootNode)this).templateShell.filePath
        };


    }

}