namespace UIForia.Elements {

    public static class InputSerializers {

        public static IInputSerializer<int> IntSerializer = new CallbackSerializer<int>((int input) => input.ToString("D"));
        public static IInputSerializer<float> FloatSerializer = new CallbackSerializer<float>((float input) => input.ToString("G"));
        public static IInputSerializer<double> DoubleSerializer = new CallbackSerializer<double>((double input) => input.ToString("G"));
        public static IInputSerializer<string> StringSerializer = new CallbackSerializer<string>((string input) => input);

    }

}