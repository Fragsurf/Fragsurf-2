using System;
using UIForia.Rendering;

namespace UIForia.Editor {

    public class AnimatedPropertyGenerator<T> : AnimatedPropertyGenerator {

        public AnimatedPropertyGenerator(StylePropertyId propertyId, T defaultValue, InheritanceType inheritanceType = InheritanceType.NotInherited, string defaultValueOverride = null)
            : base(propertyId, typeof(T), defaultValue, inheritanceType, defaultValueOverride) { }

    }

    public abstract class AnimatedPropertyGenerator : PropertyGenerator {

        protected AnimatedPropertyGenerator(StylePropertyId propertyId, Type type, object defaultValue, InheritanceType inheritanceType, string defaultValueOverride)
            : base(propertyId, type, defaultValue, inheritanceType, defaultValueOverride) { }

    }

}