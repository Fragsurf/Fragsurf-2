using System;

namespace Fragsurf
{
	[Flags]
	public enum ConVarFlags
	{
		None			= 0,
		Replicator		= 1,
		Cheat			= 2,
		UserSetting		= 4,
		Silent			= 8,
		Gamemode		= 16
	}
}