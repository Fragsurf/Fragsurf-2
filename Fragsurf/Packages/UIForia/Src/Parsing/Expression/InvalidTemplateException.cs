using System;

namespace UIForia.Parsing.Expressions {

    public class InvalidTemplateException : Exception {

        public string templatePath;

        private readonly string template;

        public InvalidTemplateException(string message) : base(message) {
            this.templatePath = string.Empty;
        }
        
        public InvalidTemplateException(string templatePath, Exception cause) : base(cause.Message, cause) {
            this.templatePath = templatePath;
        }
        
        public InvalidTemplateException(string templatePath, string message) : base(message) {
            this.templatePath = templatePath;
        }

        public InvalidTemplateException(string templatePath, string message, string template) : base(message) {
            this.templatePath = templatePath;
            this.template = template;
        }

        public override string Message =>
            (templatePath ?? "Path: " + templatePath + "\n")
            + (template != null ? "Template: \n" + template + "\n" : string.Empty)
            + base.Message;
    }

}