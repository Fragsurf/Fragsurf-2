using System;
using System.Reflection;

namespace DTCommandPalette {
	public interface ICommandWithArguments : ICommand {
		ArgumentInfo[] Arguments {
			get;
		}

		void Execute(object[] args);
	}
}