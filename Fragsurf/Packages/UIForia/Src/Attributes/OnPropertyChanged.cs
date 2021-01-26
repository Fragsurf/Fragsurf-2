using System;

namespace UIForia.Attributes {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class OnPropertyChanged : Attribute {

        public readonly string propertyName;

        public OnPropertyChanged(string propertyName) {
            this.propertyName = propertyName;
        }

    }

}