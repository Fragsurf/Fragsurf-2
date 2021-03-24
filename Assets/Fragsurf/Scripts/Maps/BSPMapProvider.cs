using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Maps
{
    public class BSPMapProvider : IMapProvider
    {

        public async Task<List<BaseMap>> GetMapsAsync()
        {
            var result = new List<BaseMap>();

            if (Directory.Exists(Structure.MapsFolder))
            {
                foreach (var file in Directory.GetFiles(Structure.MapsFolder, "*.bsp", SearchOption.AllDirectories))
                {
                    result.Add(new BSPMap()
                    {
                        Author = "Unknown",
                        FilePath = file,
                        Name = Path.GetFileNameWithoutExtension(file)
                    });
                }
            }

            foreach(var file in GetMaps("240"))
            {
                result.Add(new BSPMap()
                {
                    Author = "Unknown",
                    FilePath = file,
                    Name = Path.GetFileNameWithoutExtension(file),
                    AppId = 240,
                    MountedGame = "CSS"
                });
            }

            foreach (var file in GetMaps("730"))
            {
                result.Add(new BSPMap()
                {
                    Author = "Unknown",
                    FilePath = file,
                    Name = Path.GetFileNameWithoutExtension(file),
                    AppId = 730,
                    MountedGame = "CSGO"
                });
            }

            await Task.Delay(100);

            return result;
        }

        private List<string> GetMaps(string appid)
        {
            var result = new List<string>();
            try
            {
                var lib = GamePipeLib.Model.Steam.SteamRoot.Instance.GetGame(appid);
                if (lib == null)
                {
                    return result;
                }
                if (!Directory.Exists(lib.GameDir))
                {
                    return result;
                }
                return Directory.GetFiles(lib.GameDir, "*.bsp", SearchOption.AllDirectories).ToList();
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
            return result;
        }

    }
}
