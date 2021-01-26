using System;
using System.IO;

namespace UIForia.Attributes {

    public enum TemplateType {

        Internal,
        File,
        DefaultFile

    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TemplateAttribute : Attribute {

        public string source;
        public string filePath;
        public string templateId;
        public readonly TemplateType templateType;
        public string fullPathId;
        public string relativePath;

        public TemplateAttribute() {
            this.templateType = TemplateType.DefaultFile;
            this.templateId = null;
            this.source = string.Empty;
            this.fullPathId = null;
            this.relativePath = null;
        }

        public TemplateAttribute(TemplateType templateType, string sourceOrPath) {
            this.templateType = templateType;
            this.templateId = null;
            this.source = string.Empty; // set later 
            this.fullPathId = null;

            switch (templateType) {
                case TemplateType.DefaultFile:
                    break;
                case TemplateType.File:
                case TemplateType.Internal:
                    this.fullPathId = sourceOrPath;
                    int idx = sourceOrPath.IndexOf('#');
                    if (idx < 0) {
                        this.filePath = sourceOrPath;
                    }
                    else {
                        this.templateId = sourceOrPath.Substring(idx + 1);
                        this.filePath = sourceOrPath.Substring(0, idx);
                    }

                    int lastSlash = filePath.IndexOf(Path.DirectorySeparatorChar);
                    if (lastSlash > 0) {
                        relativePath = filePath.Substring(lastSlash);
                    }
                    else {
                        relativePath = filePath;
                    }

                    break;
            }
        }

        public TemplateAttribute(string source) : this(TemplateType.File, source) { }

    }

}