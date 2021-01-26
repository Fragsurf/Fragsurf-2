using System.Text;

namespace UIForia.Elements {

    public class IntFormatter : IInputFormatter {

        private static StringBuilder builder = new StringBuilder(32);

        public string Format(string input) {
            builder.Clear();
            bool foundDigit = false;
            bool foundSign = false;

            for (int i = 0; i < input.Length; i++) {
                char c = input[i];

                if (!foundDigit && !foundSign && c == '-') {
                    builder.Append(c);
                    foundSign = true;
                }
                else if (char.IsDigit(c)) {
                    builder.Append(c);
                    foundDigit = true;
                }
            }

            return builder.ToString();
        }

    }

}