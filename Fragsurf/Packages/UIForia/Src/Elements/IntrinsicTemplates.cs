using UIForia.Attributes;
using UIForia.UIInput;

namespace UIForia.Elements {

    [TemplateTagName("Group")]
    public class UIGroupElement : UIContainerElement {

        public override string GetDisplayName() {
            return "Group";
        }

    }

    [TemplateTagName("Panel")]
    public class UIPanelElement : UIContainerElement {

        public override string GetDisplayName() {
            return "Panel";
        }

    }

    [TemplateTagName("Section")]
    public class UISectionElement : UIContainerElement {

        public override string GetDisplayName() {
            return "Section";
        }

    }

    [TemplateTagName("Div")]
    public class UIDivElement : UIContainerElement {

        public override string GetDisplayName() {
            return "Div";
        }

    }

    [TemplateTagName("Header")]
    public class UIHeaderElement : UIContainerElement {

        public override string GetDisplayName() {
            return "Header";
        }

    }

    [TemplateTagName("Footer")]
    public class UIFooterElement : UIContainerElement {

        public override string GetDisplayName() {
            return "Footer";
        }

    }

    [TemplateTagName("Label")]
    public class UILabelElement : UITextElement {

        public string forElement;

        [OnMouseClick]
        public void OnClick() {
            UIElement forEl = parent.FindById(forElement);
            if (forEl is IFocusable focusable) {
                application.InputSystem.RequestFocus(focusable);
            }
        }

        public override string GetDisplayName() {
            return "Label";
        }

    }

    [TemplateTagName("Paragraph")]
    public class UIParagraphElement : UITextElement {

        public override string GetDisplayName() {
            return "Paragraph";
        }

    }

    [TemplateTagName("Heading1")]
    public class UIHeading1Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading1";
        }

    }

    [TemplateTagName("Heading2")]
    public class UIHeading2Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading2";
        }

    }

    [TemplateTagName("Heading3")]
    public class UIHeading3Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading3";
        }

    }

    [TemplateTagName("Heading4")]
    public class UIHeading4Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading4";
        }

    }

    [TemplateTagName("Heading5")]
    public class UIHeading5Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading5";
        }

    }

    [TemplateTagName("Heading6")]
    public class UIHeading6Element : UITextElement {

        public override string GetDisplayName() {
            return "Heading6";
        }

    }

}