using System;
using System.Collections.Generic;
using UIForia.Compilers;
using UIForia.Compilers.Style;
using UIForia.Elements;

namespace UIForia {

    public interface ITemplateLoader {

        Func<UIElement, TemplateScope, UIElement>[] LoadTemplates();
        
        Func<UIElement, UIElement, TemplateScope, UIElement>[] LoadSlots();
        
        Action<UIElement, UIElement>[] LoadBindings();

        TemplateMetaData[] LoadTemplateMetaData(Dictionary<string, StyleSheet> sheetMap, UIStyleGroupContainer[] styleListArray);

        string[] StyleFilePaths { get; }

        Dictionary<string, int> TagNameIdMap { get; }

        ConstructedElement ConstructElement(int typeId);

        DynamicTemplate[] DynamicTemplates { get; }

        MaterialDatabase GetMaterialDatabase();

    }

}