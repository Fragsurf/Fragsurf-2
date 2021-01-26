using UIForia.Elements;
using UIForia.Parsing.Style;

namespace UIForia.Rendering {

    public sealed class UIStyleRule {

        /// <summary>
        /// true if this rule was preceded by a `not` keyword
        /// </summary>
        public readonly bool invert;

        /// <summary>
        /// the attribute name
        /// </summary>
        public readonly string attributeName;

        /// <summary>
        /// null == any value is ok
        /// </summary>
        public readonly string attributeValue;

        public readonly StyleAttributeExpression expression;

        public UIStyleRule next;

        public UIStyleRule(bool invert, StyleAttributeExpression expression, UIStyleRule next = null) {
            this.invert = invert;
            this.expression = expression;
            this.next = next;
        }

        public UIStyleRule(bool invert, string attributeName, string attributeValue, UIStyleRule next = null) {
            this.invert = invert;
            this.attributeName = attributeName;
            this.attributeValue = attributeValue;
            this.next = next;
        }

        public bool IsApplicableTo(UIElement element) {
            if (next != null) {
                return IsApplicableToNext(element, next.IsApplicableTo(element));
            }

            return IsApplicableToNext(element, true);
        }

        private bool IsApplicableToNext(UIElement element, bool nextApplies) {
            if (!nextApplies) return false;

            if (expression != null) {
                return invert ^ expression.Execute() == 0;
            }

            if (attributeValue == null) {
                return invert ^ element.HasAttribute(attributeName);
            }

            return invert ^ element.GetAttribute(attributeName) == attributeValue;
        }
    }

}