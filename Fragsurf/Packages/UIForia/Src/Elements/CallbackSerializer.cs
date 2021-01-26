using System;

namespace UIForia.Elements {

    public class CallbackSerializer<T> : IInputSerializer<T> {

        public Func<T, string> serialize;

        public CallbackSerializer(Func<T, string> serialize) {
            this.serialize = serialize;
        }

        public string Serialize(T input) {
            return serialize.Invoke(input);
        }

    }

}