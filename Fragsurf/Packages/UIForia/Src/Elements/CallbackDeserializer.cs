using System;

namespace UIForia.Elements {

    public class CallbackDeserializer<T> : IInputDeserializer<T> {

        public Func<string, T> deserialize;

        public CallbackDeserializer(Func<string, T> deserialize) {
            this.deserialize = deserialize;
        }

        public T Deserialize(string input) {
            return deserialize.Invoke(input);
        }

    }

}