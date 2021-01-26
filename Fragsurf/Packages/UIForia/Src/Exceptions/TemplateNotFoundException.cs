using System;
using UIForia.Parsing;

namespace UIForia.Exceptions {

    public class TemplateNotFoundException : Exception {

        public TemplateNotFoundException(ProcessedType processedType, string xmlPath) : base($"Unable to find default template for type {processedType.rawType}. Searched using default resolver at paths: \n[{xmlPath}]") { }
        
        public TemplateNotFoundException(string xmlPath, string templateId) : base($"Unable to template at path {xmlPath} with id `{templateId}`") { }

        public TemplateNotFoundException(string message) : base(message) { }

    }

}