using System;
using System.Globalization;
using System.Text;

namespace UIForia.Elements {

    public class FloatFormatter : IInputFormatter {

        private static StringBuilder builder = new StringBuilder(32);

        private static char k_Decimal = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

        public string Format(string input) {
            builder.Clear();
            bool foundDecimal = false;
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
                else if (c == k_Decimal && !foundDecimal) {
                    builder.Append(k_Decimal);
                    foundDecimal = true;
                }
            }

            return builder.ToString();
        }

    }

}