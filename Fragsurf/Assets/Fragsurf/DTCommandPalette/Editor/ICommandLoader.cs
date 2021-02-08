using System;

namespace DTCommandPalette {
	public interface ICommandLoader {
		ICommand[] Load();
	}
}
