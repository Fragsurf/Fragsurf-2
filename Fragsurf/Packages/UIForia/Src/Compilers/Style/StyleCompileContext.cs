using System;
using System.Collections.Generic;
using UIForia.Exceptions;
using UIForia.Parsing.Style.AstNodes;
using UIForia.Util;

namespace UIForia.Compilers.Style {

    public class StyleCompileContext {

        private static readonly Func<StyleConstant, string, bool> s_FindStyleConstant = (element, name) => element.name == name;

        public string fileName;

        public Dictionary<string, LightList<StyleConstant>> importedStyleConstants;
        public Dictionary<string, StyleConstant> constantsWithReferences;
        public LightList<StyleConstant> constants;
        public ResourceManager resourceManager;
        public MaterialDatabase materialDatabase;

        public StyleCompileContext(MaterialDatabase materialDatabase) {
            this.materialDatabase = materialDatabase;
            this.importedStyleConstants = new Dictionary<string, LightList<StyleConstant>>();
            this.constantsWithReferences = new Dictionary<string, StyleConstant>();
            this.constants = new LightList<StyleConstant>();
        }

        public void Release() {
            importedStyleConstants.Clear();
            constantsWithReferences.Clear();
            constants.Clear();
        }

        /// <summary>
        /// Returns a referenced StyleASTNode if the passed in node is a ReferenceNode.
        /// Only called once all references have been resolved in the constants list.
        /// </summary>
        /// <param name="node">A node that can be a ReferenceNode or something else.</param>
        /// <returns>The referenced node or the node itself if it's a regular one.</returns>
        /// <exception cref="CompileException">thrown in case a reference cannot be resolved.</exception>
        public StyleASTNode GetValueForReference(StyleASTNode node) {
            if (node is ConstReferenceNode referenceNode) {
                for (int index = 0; index < constants.Count; index++) {
                    StyleConstant c = constants[index];
                    if (c.name == referenceNode.identifier) {
                        return c.value;
                    }
                }

                if (referenceNode.children.Count > 0) {
                    if (importedStyleConstants.ContainsKey(referenceNode.identifier)) {
                        DotAccessNode importedConstant = (DotAccessNode) referenceNode.children[0];

                        StyleConstant importedStyleConstant = importedStyleConstants[referenceNode.identifier]
                            .Find(importedConstant.propertyName, s_FindStyleConstant);

                        if (importedStyleConstant.name == null) {
                            throw new CompileException(importedConstant, $"Could not find referenced property '{importedConstant.propertyName}' in imported scope '{referenceNode.identifier}'.");
                        }

                        return importedStyleConstant.value;
                    }

                    throw new CompileException(referenceNode, "Constants cannot reference members of other constants.");
                }


                throw new CompileException(referenceNode, $"Couldn't resolve reference {referenceNode}. Known references are: {PrintConstants()}");
            }

            return node;
        }

        internal string PrintConstants() {
            if (constants.Count == 0) return string.Empty;

            string result = "\n\t" + constants[0].name;
            for (int index = 1; index < constants.Count; index++) {
                result += "\n\t" + constants[index].name;
            }

            return result;
        }

        public static StyleCompileContext Create(StyleSheetImporter styleSheetImporter) {
            throw new NotImplementedException();
        }

    }

}