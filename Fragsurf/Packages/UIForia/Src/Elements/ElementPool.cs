using System;

namespace UIForia.Elements {


    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PoolableElementAttribute : Attribute { }

    public class ElementPool { }

}