using System.Xml.Linq;
using UIForia.Templates;
using UIForia.Util;

namespace UIForia.Parsing {

    public enum ParsedTemplateType {

        FromCode,
        Dynamic

    }
    
    public struct RawTemplateContent {

        public string templateId;
        public XElement content;
        public XElement elementDefinition;
        public ParsedTemplateType type;
        public ProcessedType processedType;

    }

    public class TemplateShell {

        public readonly string filePath;
        public StructList<UsingDeclaration> usings;
        public StructList<StyleDefinition> styles;
        public StructList<RawTemplateContent> unprocessedContentNodes;
        public LightList<string> referencedNamespaces;

        public TemplateShell(string filePath) {
            this.filePath = filePath;
            this.usings = new StructList<UsingDeclaration>(2);
            this.styles = new StructList<StyleDefinition>(2);
            this.referencedNamespaces = new LightList<string>(4);
            this.unprocessedContentNodes = new StructList<RawTemplateContent>(2);
        }

        public bool HasContentNode(string templateId) {
            return GetElementTemplateContent(templateId) != null;
        }

        public XElement GetElementTemplateContent(string templateId) {
            for (int i = 0; i < unprocessedContentNodes.size; i++) {
                if (unprocessedContentNodes.array[i].templateId == templateId) {
                    return unprocessedContentNodes.array[i].content;
                }
            }

            return null;
        }

    }

}