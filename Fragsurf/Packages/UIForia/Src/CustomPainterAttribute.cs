using System;

namespace UIForia {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EffectParameter : System.Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CustomPainterAttribute : Attribute {

        public readonly string name;

        public CustomPainterAttribute(string name) {
            this.name = name;
        }

    }

}