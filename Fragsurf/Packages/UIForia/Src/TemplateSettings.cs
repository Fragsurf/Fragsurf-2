using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UIForia.Attributes;
using UnityEngine;

namespace UIForia {

    public interface HttpStreamingAssetsAdapter {
        string GetResource(string path);
    }

    public class TemplateSettings {

        public string assemblyName;
        public string outputPath;
        public string codeFileExtension;
        /// <summary>
        /// Set this property to your application's script directory that all your [Template("path")] paths are relative to.
        /// If you follow our naming convention all your UIForia template classes will have a namespace that reflects your
        /// actual directory structure (relative to this base path).
        /// </summary>
        public string templateResolutionBasePath;
        
        /// <summary>
        /// Optional style base path relative to your #templateResolutionBasePath.
        /// All style paths in your templates will be relative to templateResolutionBasePath + styleBasePath.
        /// </summary>
        public string styleBasePath;
        public string applicationName;
        public Type rootType;
        public ResourceManager resourceManager;
        public Func<Type, TemplateAttribute, string> filePathResolver;
        public List<Type> dynamicallyCreatedTypes;
        // todo - support more file formats
        public readonly string templateFileExtension = ".xml";

        public MaterialReference[] materialAssets;
        public HttpStreamingAssetsAdapter httpStreamingAssetsAdapter;

        public TemplateSettings() {
            this.applicationName = "DefaultApplication";
            this.assemblyName = "UIForia.Application";
            this.outputPath = Path.Combine(UnityEngine.Application.dataPath, "__UIForiaGenerated__");
            this.codeFileExtension = "cs";
            this.templateResolutionBasePath = Path.Combine(UnityEngine.Application.dataPath);
            this.styleBasePath = string.Empty;
        }

        public string StrippedApplicationName => Regex.Replace(applicationName, @"\s", "" );

        public virtual string TryReadFile(string templatePath) {
            try {
                return File.ReadAllText(templatePath);
            }
            catch (FileNotFoundException e) {
                Debug.LogWarning(e.Message);
                throw;
            }
            catch (Exception) {
                return null;
            }
        }

        public virtual string GetInternalTemplatePath(string fileName) {
            return Path.GetFullPath(Path.Combine(GetCallPath(), fileName));
        }

        private string GetCallPath([CallerFilePath] string callerFilePath = "") {
            return Path.GetDirectoryName(callerFilePath);
        }

        public virtual string GetTemplatePath(string templateAttrTemplate) {
            return Path.GetFullPath(Path.Combine(templateResolutionBasePath, templateAttrTemplate)); 
        }

        public virtual string GetRelativeStylePath(string stylePath) {
            return Path.Combine(templateResolutionBasePath, styleBasePath, stylePath); 
        }
    }

}