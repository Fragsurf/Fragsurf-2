using System;

namespace UIForia.Compilers {

    public class InvalidLeftHandStatementException : Exception {

        public InvalidLeftHandStatementException(string message, string input) : base($" {message} is an invalid LHS expression statement, parsed from {input}") { }

    }

}