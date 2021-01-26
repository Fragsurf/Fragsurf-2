using System.Collections.Generic;
using System.Diagnostics;

namespace UIForia.Extensions {

    public static class DictionaryExtensions {

        [DebuggerStepThrough]
        public static U GetOrDefault<T, U>(this Dictionary<T, U> self, T key, U defaultValue = default(U)) {
            U retn;
            if (self.TryGetValue(key, out retn)) {
                return retn;
            }

            return defaultValue;
        }

    }

}