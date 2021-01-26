using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;
using UIForia.Parsing;
using UIForia.Templates;
using UIForia.Util;

namespace UIForia {

    public struct ConstructedElement {

        public readonly int tagNameId;
        public readonly UIElement element;

        [UsedImplicitly]
        public ConstructedElement(int tagNameId, UIElement element) {
            this.tagNameId = tagNameId;
            this.element = element;
        }

    }

    public struct DynamicTemplate {

        public readonly int typeId;
        public readonly int templateId;
        public readonly Type type;

        public DynamicTemplate(Type type, int typeId, int templateId) {
            this.type = type;
            this.typeId = typeId;
            this.templateId = templateId;
        }

    }

    public class CompiledTemplateData {

        public LightList<CompiledTemplate> compiledTemplates;
        public LightList<CompiledSlot> compiledSlots;
        public LightList<CompiledBinding> compiledBindings;
        public LightList<DynamicTemplate> dynamicTemplates;
        public StyleSheetImporter styleImporter;
        public Func<int, ConstructedElement> constructElement;
        public Dictionary<string, int> tagNameIdMap;

        public TemplateMetaData[] templateMetaData;
        public Func<UIElement, TemplateScope, UIElement>[] templates;
        public Func<UIElement, UIElement, TemplateScope, UIElement>[] slots;
        public Action<UIElement, UIElement>[] bindings;
        private int nextTagNameId;
       // public readonly Dictionary<Type, int> templateTypeMap = new Dictionary<Type, int>();
        public TemplateSettings templateSettings;
        public Dictionary<int, Func<ConstructedElement>> constructorFnMap;
        public MaterialDatabase materialDatabase;

        public CompiledTemplateData(TemplateSettings templateSettings) {
            this.templateSettings = templateSettings;
            this.compiledSlots = new LightList<CompiledSlot>();
            this.compiledTemplates = new LightList<CompiledTemplate>(128);
            this.compiledBindings = new LightList<CompiledBinding>(128);
            this.styleImporter = new StyleSheetImporter(templateSettings, templateSettings.resourceManager);
            this.tagNameIdMap = new Dictionary<string, int>();
            this.nextTagNameId = 1;
        }

        public CompiledTemplate CreateTemplate(string filePath, string templateName) {
            CompiledTemplate compiledTemplate = new CompiledTemplate();
            compiledTemplate.filePath = filePath;
            compiledTemplate.guid = Guid.NewGuid().ToString().Replace('-', '_');
            compiledTemplate.templateId = compiledTemplates.size;
            compiledTemplates.Add(compiledTemplate);
            compiledTemplate.templateMetaData = new TemplateMetaData(compiledTemplate.templateId, filePath, null, null);
            compiledTemplate.templateName = templateName;
            return compiledTemplate;
        }

        public CompiledSlot CreateSlot(string filePath, string templateName, string slotName, SlotType slotType) {
            CompiledSlot compiledSlot = new CompiledSlot();
            compiledSlot.filePath = filePath;
            compiledSlot.templateName = templateName;
            compiledSlot.slotName = slotName;
            compiledSlot.slotType = slotType;
            compiledSlot.guid = Guid.NewGuid().ToString().Replace('-', '_');
            compiledSlot.slotId = compiledSlots.size;
            compiledSlots.Add(compiledSlot);
            return compiledSlot;
        }

        public CompiledBinding AddBinding(TemplateNode templateNode, CompiledBindingType bindingType) {
            CompiledBinding binding = new CompiledBinding();
            TemplateRootNode root = templateNode as TemplateRootNode ?? templateNode.root;
            binding.filePath = root.templateShell.filePath;
            binding.bindingType = bindingType;
            binding.elementTag = templateNode.originalString;
            binding.bindingId = compiledBindings.size;
            binding.guid = Guid.NewGuid().ToString().Replace('-', '_');
            binding.templateName = root.templateName;
            compiledBindings.Add(binding);
            return binding;
        }

        public StyleSheet ImportStyleSheet(in StyleDefinition styleDefinition, MaterialDatabase materialDatabase, string originPath = "") {
            return styleImporter.Import(styleDefinition, materialDatabase, true);
        }

        public bool TryGetTemplate<T>(out DynamicTemplate retn) where T : UIElement {
            retn = default;
            if (dynamicTemplates == null) {
                return false;
            }

            for (int i = 0; i < dynamicTemplates.Count; i++) {
                if (dynamicTemplates[i].type == typeof(T)) {
                    retn = dynamicTemplates[i];
                    return true;
                }
            }

            return false;
        }

        public ConstructedElement ConstructElement(int typeId) {
            return constructElement.Invoke(typeId);
        }

        public int GetTagNameId(string tagName) {
            if (tagNameIdMap.TryGetValue(tagName, out int id)) {
                return id;
            }

            id = nextTagNameId++;
            tagNameIdMap.Add(tagName, id);
            return id;
        }
        
        public void AddDynamicTemplate(Type type, int typeId, int templateId) {
            dynamicTemplates = dynamicTemplates ?? new LightList<DynamicTemplate>();
            dynamicTemplates.Add(new DynamicTemplate(type, typeId, templateId));
        }

        public void Destroy() {
            Array.Clear(compiledTemplates.array, 0, compiledTemplates.size);
            Array.Clear(compiledBindings.array, 0, compiledBindings.size);
            Array.Clear(compiledSlots.array, 0, compiledSlots.size);
            Array.Clear(templateMetaData, 0, templateMetaData.Length);
            constructElement = null;
            constructorFnMap?.Clear();
        }

    }

}