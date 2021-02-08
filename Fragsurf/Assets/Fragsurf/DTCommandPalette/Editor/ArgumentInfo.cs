using System;
using System.Reflection;

namespace DTCommandPalette {
	public class ArgumentInfo {
		public string ArgumentName {
			get; private set;
		}

		public Type ArgumentType {
			get; private set;
		}

		public ArgumentInfo(string argumentName, Type argumentType) {
			ArgumentName = argumentName;
			ArgumentType = argumentType;
		}
	}
}