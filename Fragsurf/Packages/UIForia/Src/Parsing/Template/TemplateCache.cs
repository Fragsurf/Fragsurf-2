using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UIForia.Attributes;
using UIForia.Exceptions;
using UIForia.Util;

namespace UIForia.Parsing {

    public class TemplateCache {

        private readonly TemplateSettings settings;
        private readonly XMLTemplateParser xmlTemplateParser;
        private readonly Dictionary<string, LightList<TemplateRootNode>> templateMap;

        public TemplateCache(TemplateSettings settings) {
            this.settings = settings;
            this.xmlTemplateParser = new XMLTemplateParser(settings);
            this.templateMap = new Dictionary<string, LightList<TemplateRootNode>>(37);
        }

        private string ResolveTemplateFilePath(ProcessedType processedType) {
            TemplateAttribute templateAttr = processedType.templateAttr;
            if (templateAttr.templateType == TemplateType.Internal) {
                return templateAttr.filePath;
            }

            string path = settings.filePathResolver?.Invoke(processedType.rawType, templateAttr);

            if (path != null) {
                return path;
            }

            string namespacePath = processedType.rawType.Namespace;
 
            if (namespacePath != null && namespacePath.Contains(".")) {
                namespacePath = namespacePath.Replace(".", Path.DirectorySeparatorChar.ToString());
            }

            string xmlPath;
            
            // Special behavior for template attributes with no file path parameter. We figure out the whole path
            // based on a convention that looks for a given template like:
            //     namespace My.Name.Space { [Template] public class MyElement : UIElement ... }
            // right here:
            // basepath + My/Name/Space/ClassName.xml
            if (templateAttr.templateType == TemplateType.DefaultFile) {
                
                string basePath = namespacePath == null 
                    ? processedType.rawType.Name
                    : Path.Combine(namespacePath, processedType.rawType.Name);

                string relativePath = basePath + settings.templateFileExtension;
                xmlPath = Path.GetFullPath(Path.Combine(settings.templateResolutionBasePath, relativePath));
                if (!File.Exists(xmlPath)) {
                    throw new TemplateNotFoundException(processedType, xmlPath);
                }

                return relativePath;
            }

            // first we try to find the template based on the resolution base path + a guessed namespace path 
            if (namespacePath != null) {
                
                // namespace My.Name.Space.MyElement { [Template("MyElement.xml#id")] }
                // basepath + My/Name/Space/MyElement/MyElement.xml
                // templateRootNamespace = My/Name/Space
                xmlPath = Path.GetFullPath(Path.Combine(settings.templateResolutionBasePath, namespacePath, templateAttr.filePath));
                if (File.Exists(xmlPath)) {
                    return Path.Combine(namespacePath, templateAttr.filePath);
                }
            }

            // namespace My.Name.Space.MyElement { [Template("My/Name/Space/MyElement/MyElement.xml#id")] }
            // basepath + My/Name/Space/MyElement/MyElement.xml
            // templateRootNamespace = My/Name/Space
            // ------
            // If the previous method didn't find a template we probably have a full path in the template attribute.
            // This should be the mode that is compatible with non-convention paths and namespaces
            xmlPath = Path.GetFullPath(Path.Combine(settings.templateResolutionBasePath, templateAttr.filePath));
            if (File.Exists(xmlPath)) {
                return templateAttr.filePath;
            }
            
            throw new TemplateNotFoundException(processedType, xmlPath);
        }

        public TemplateRootNode GetParsedTemplate(ProcessedType processedType) {
            TemplateAttribute templateAttr = processedType.templateAttr;

            templateAttr.filePath = ResolveTemplateFilePath(processedType);
            if (templateAttr.fullPathId == null) {
                templateAttr.fullPathId = templateAttr.templateId == null
                    ? templateAttr.filePath
                    : templateAttr.filePath + "#" + templateAttr.templateId;
            }

            Debug.Assert(templateAttr.fullPathId != null, "templateAttr.fullPathId != null");
            
            if (templateMap.TryGetValue(templateAttr.fullPathId, out LightList<TemplateRootNode> list)) {

                for (int i = 0; i < list.size; i++) {
                    
                    if (list.array[i].processedType.rawType == processedType.rawType) {
                        return list.array[i];
                    }     
                    
                }

                TemplateRootNode retn = list[0].Clone(processedType);
                list.Add(retn);
                return retn;

            }

            list = new LightList<TemplateRootNode>(2);
            
            templateMap[templateAttr.fullPathId] = list;

            TemplateDefinition templateDefinition = GetTemplateDefinition(processedType);

            templateAttr.source = templateDefinition.contents;
            
            TemplateShell shell = xmlTemplateParser.GetOuterTemplateShell(templateAttr);

            TemplateRootNode templateRootNode = new TemplateRootNode(templateAttr.templateId, shell, processedType, null, default) {
                tagName = processedType.tagName
            };

            list.Add(templateRootNode);

            xmlTemplateParser.Parse(templateRootNode, processedType);

            return templateRootNode;
        }

        private string ResolveTemplateFilePath(TemplateType templateType, string filepath) {
            switch (templateType) {
                case TemplateType.DefaultFile: {
                    return settings.GetTemplatePath(filepath);
                }
                case TemplateType.Internal: {
                    return settings.GetInternalTemplatePath(filepath);
                }

                case TemplateType.File: {
                    return settings.GetTemplatePath(filepath);
                }

                default:
                    return "NONE";
            }
        }

        private TemplateDefinition GetTemplateDefinition(ProcessedType processedType) {
            TemplateAttribute templateAttr = processedType.templateAttr;

            string templatePath = ResolveTemplateFilePath(templateAttr.templateType, templateAttr.filePath);

            switch (templateAttr.templateType) {
                case TemplateType.Internal: {
                    string file = settings.TryReadFile(templatePath);

                    if (file == null) {
                        throw new TemplateParseException(settings.templateResolutionBasePath, $"Cannot find template in (internal) path {templatePath}.");
                    }

                    return new TemplateDefinition() {
                        contents = file,
                        filePath = templateAttr.templateType == TemplateType.File ? processedType.rawType.AssemblyQualifiedName : templateAttr.filePath,
                        language = TemplateLanguage.XML
                    };
                }

                case TemplateType.DefaultFile: 
                case TemplateType.File: {
                    string file = settings.TryReadFile(templatePath);
                    if (file == null) {
                        throw new TemplateParseException(settings.templateResolutionBasePath, $"Cannot find template in path {templatePath}.");
                    }

                    return new TemplateDefinition() {
                        contents = file,
                        filePath = templateAttr.filePath,
                        language = TemplateLanguage.XML
                    };
                }

                default:
                    return new TemplateDefinition() {
                        contents = templateAttr.source,
                        filePath = templatePath,
                        language = TemplateLanguage.XML
                    };
            }
        }

    }

}