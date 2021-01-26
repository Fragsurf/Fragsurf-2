using System;

namespace UIForia.Elements {

    public static class InputDeserializers {

        public static IInputDeserializer<int> IntDeserializer = new CallbackDeserializer<int>((string input) => {
            try {
                return int.Parse(input);
            }
            catch (Exception) {
                return 0;
            }
        });

        public static IInputDeserializer<float> FloatDeserializer = new CallbackDeserializer<float>((string input) => {
            try {
                return float.Parse(input);
            }
            catch (Exception) {
                return 0f;
            }
        });

        public static IInputDeserializer<double> DoubleDeserializer = new CallbackDeserializer<double>((string input) => {
            try {
                return double.Parse(input);
            }
            catch (Exception) {
                return 0f;
            }
        });

        public static IInputDeserializer<string> StringDeserializer = new CallbackDeserializer<string>((string input) => input);

    }

}