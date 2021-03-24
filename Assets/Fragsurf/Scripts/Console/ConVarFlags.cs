using System;

namespace Fragsurf
{
	[Flags]
	public enum ConVarFlags
	{
		None				= 0,
		Replicator			= 1,
		Cheat				= 2,
		UserSetting			= 4,
		Silent				= 8,
		Gamemode			= 16,
		UserSettingHidden	= 32,
		Poll				= 64
	}
}