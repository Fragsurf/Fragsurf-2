using UnityEngine;

namespace DTCommandPalette {
	public interface ICommand {
		string DisplayTitle {
			get;
		}

		string DisplayDetailText {
			get;
		}

		Texture2D DisplayIcon {
			get;
		}

		// NOTE (darren): higher sorting priority => higher in list
		float SortingPriority {
			get;
		}

		bool IsValid();

		void Execute();
	}
}
