using System;

namespace UIForia.Exceptions {
    public class InvalidArgumentException : Exception {

        public InvalidArgumentException(string message = null) : base(message) {
        }
    }
}