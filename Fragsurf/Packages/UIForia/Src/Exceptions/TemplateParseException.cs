using System;
using System.Collections.Generic;
using System.Xml;
using UIForia.Parsing.Expressions.AstNodes;
using UIForia.Parsing.Expressions.Tokenizer;
using UIForia.Text;
using UIForia.Util;

namespace UIForia.Exceptions {

    public class TemplateParseException : Exception {

        private int column;

        private int line;

        private string fileName;

        private string message;

        private ExpressionTokenType tokenType;

        private ASTNode node;

        public TemplateParseException(string message, ASTNode node) : base(message) {
            this.message = message;
            this.node = node;
        }

        public TemplateParseException(string fileName, string message, Exception rootCause) : base(message, rootCause) {
            this.message = message;
            this.fileName = fileName;
        }

        public TemplateParseException(string fileName, string message, Exception rootCause, ASTNode node) : base(message, rootCause) {
            this.message = message;
            this.fileName = fileName;
            if (node != null) {
                this.line = node.line;
                this.column = node.column;
                this.node = node;
            }
        }

        public TemplateParseException(string fileName, string message) : this(fileName, null, message) { }

        public TemplateParseException(IXmlLineInfo element, string message) : this(string.Empty, element, message) { }

        public TemplateParseException(string fileName, IXmlLineInfo element, string message) {
            if (element != null) {
                line = element.LineNumber;
                column = element.LinePosition;
            }

            this.fileName = fileName;
            this.message = message;
        }

        public void SetFileName(string name) {
            this.fileName = $"Error in file {name}: ";
        }

        public override string Message => fileName + $"\nYour template contains an error in line {line} column {column}."
                                                   + $"\n\tMessage:\n\t\t{message}"
                                                   + (node != default ? $"\n\tNode:\n\t\t{node}" : string.Empty);

        public static TemplateParseException UnmatchedSlotName(string fileName, Type type, string slotName, IReadOnlyList<string> availableSlotNames) {
            string listString = StringUtil.ListToString(availableSlotNames);
            return new TemplateParseException(fileName, $"Unmatched slot name '{slotName}' for template {type}. Possible slot names are: {listString}");
        }

        public static TemplateParseException DuplicateSlotName(string fileName, string target) {
            return new TemplateParseException(fileName, $"Invalid slot input, you provided the slot name {target} multiple times");
        }

        public static TemplateParseException InvalidSlotHierarchy(string fileName, Type type, string childTagName, string parentTagName) {
            return new TemplateParseException(fileName, $"Invalid slot hierarchy, the template {type} defines {childTagName} to be a child of {parentTagName}. You can only provide one of these.");
        }

        public static TemplateParseException OrphanedSlot(string fileName, Type type, string slotName) {
            return new TemplateParseException(fileName, $"Invalid slot usage at root of template {type}, the template cannot define a slot usage '{slotName}'");
        }

    }

}