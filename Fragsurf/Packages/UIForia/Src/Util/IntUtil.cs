using System.Diagnostics;

namespace UIForia.Util {

    public static class IntUtil {

        public const int UnsetValue = int.MinValue;
        
        [DebuggerStepThrough]
        public static bool IsDefined(int value) {
            return value != int.MinValue;
        }

    }

}