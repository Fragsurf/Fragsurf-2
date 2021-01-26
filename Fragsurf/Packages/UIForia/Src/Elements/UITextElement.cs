using SVGX;
using UIForia.Attributes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Text;

namespace UIForia.Elements {

    [TemplateTagName("Text")]
    public class UITextElement : UIElement, IStyleChangeHandler {

        public string text;
        internal TextInfo textInfo;
        internal TextSpan textSpan;

        private bool shouldUpdateSpanStyle;
        private SVGXTextStyle spanStyle;
        
        internal TextInfo TextInfo => textInfo;

        public override void OnEnable() {
            SetText(text);
            textSpan.SetStyle(style.GetTextStyle());
        }

        public override void OnDisable() {
            textSpan?.parent?.RemoveChild(textSpan);
        }

        public string GetText() {
            return text;
        }

        public void SetText(string newText) {
            if (textSpan == null) {
                if (parent is UITextElement textParent) {
                    textSpan = new TextSpan(text, style.GetTextStyle());
                    textInfo = textParent.textInfo;
                    textParent.textSpan.InsertChild(textSpan, (uint) siblingIndex);
                }
                else {
                    textInfo = new TextInfo(newText, style.GetTextStyle());
                    textSpan = textInfo.rootSpan;
                }
            }

            if (this.text == newText) {
                return;
            }

            this.text = newText;
            textSpan.SetText(text);
        }

        public override string GetDisplayName() {
            return "Text";
        }

        public void OnStylePropertyChanged(in StyleProperty property) {
            switch (property.propertyId) {
                
                case StylePropertyId.TextFontSize:
                    textSpan.SetFontSize(style.GetResolvedFontSize());
                    break;

                case StylePropertyId.TextFontStyle:
                    textSpan.SetFontStyle(property.AsFontStyle);
                    break;

                case StylePropertyId.TextAlignment:
                    textSpan.SetTextAlignment(property.AsTextAlignment);
                    break;

                case StylePropertyId.TextFontAsset:
                    textSpan.SetFont(property.AsFont);
                    break;

                case StylePropertyId.TextTransform:
                    textSpan.SetTextTransform(property.AsTextTransform);
                    break;

                case StylePropertyId.TextWhitespaceMode:
                    textSpan.SetWhitespaceMode(property.AsWhitespaceMode);
                    break;

                case StylePropertyId.TextColor:
                    textSpan.SetTextColor(property.AsColor);
                    break;

                case StylePropertyId.TextGlowColor:
                    textSpan.SetGlowColor(property.AsColor);
                    break;

                case StylePropertyId.TextGlowOffset:
                    textSpan.SetGlowOffset(property.AsFloat);
                    break;

                case StylePropertyId.TextGlowOuter:
                    textSpan.SetGlowOuter(property.AsFloat);
                    break;

                case StylePropertyId.TextUnderlayX:
                    textSpan.SetUnderlayX(property.AsFloat);
                    break;

                case StylePropertyId.TextUnderlayY:
                    break;

                case StylePropertyId.TextUnderlayDilate:
                    textSpan.SetUnderlayDilate(property.AsFloat);
                    break;

                case StylePropertyId.TextUnderlayColor:
                    textSpan.SetUnderlayColor(property.AsColor);
                    break;

                case StylePropertyId.TextUnderlaySoftness:
                    textSpan.SetUnderlaySoftness(property.AsFloat);
                    break;

                case StylePropertyId.TextFaceDilate:
                    textSpan.SetFaceDilate(property.AsFloat);
                    break;

                case StylePropertyId.TextGlowPower:
                case StylePropertyId.TextUnderlayType:
                    break;
            }
        }

    }

}