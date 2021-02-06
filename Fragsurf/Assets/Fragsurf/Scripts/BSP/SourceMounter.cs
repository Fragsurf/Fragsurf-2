using System.IO;
using System.Collections.Generic;
using SourceUtils;
using GamePipeLib.Model.Steam;

namespace Fragsurf.BSP
{
	public static class SourceMounter
	{
		public static Dictionary<string, List<ValvePackage>> MountedContent = new Dictionary<string, List<ValvePackage>>();

		public static bool Mount(string appid)
		{
			if (MountedContent.ContainsKey(appid))
			{
				return true;
			}

			var game = SteamRoot.Instance.GetGame(appid);

			if (game != null && Directory.Exists(game.GameDir))
			{
				MountedContent.Add(appid, new List<ValvePackage>());
				var di = new DirectoryInfo(game.GameDir);
				foreach (var parent in di.GetDirectories())
				{
					var dirVpks = parent.GetFiles("*dir.vpk");
					if (dirVpks.Length > 0)
					{
						foreach (var dvpk in dirVpks)
						{
							var pkg = new ValvePackage(dvpk.FullName);
							MountedContent[appid].Add(pkg);
						}
					}
				}
				return true;
			}

			return false;
		}
	}
}