using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Shared
{
    public class GamemodeLoader : FSSharedScript
    {

        public bool LoadGamemode(GamemodeData data)
        {
            try
            {
                Gamemode = Activator.CreateInstance(data.GamemodeType) as BaseGamemode;
                if (Gamemode == null)
                {
                    Debug.LogError("Failed to instatiate gamemode type for: " + data.Name);
                    Gamemode = new DefaultGamemode();
                }
                Gamemode.Load(data, Game);
                Game.DefaultConfig.ExecutePostLoad();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load {data.Name}: {e.Message}");
            }
            return false;
        }

        public bool LoadGamemode(string gamemodeName = null)
        {
            var resource = GameData.Instance.DefaultGamemodes
                .FirstOrDefault(x => x.Name.Equals(gamemodeName, StringComparison.OrdinalIgnoreCase));

            if (!resource)
            {
                resource = Resources.Load<GamemodeData>(gamemodeName);
                if (!resource)
                {
                    Debug.LogError("Missing gamemode data: " + gamemodeName);
                    return false;
                }
            }

            return LoadGamemode(resource);
        }

        [ConCommand("gamemode.refreshconfigs")]
        private void RefreshConfigs(string[] args)
        {
            Gamemode?.LoadConfig();
        }

        public static async Task<List<GamemodeData>> QueryAll()
        {
            var result = new List<GamemodeData>();

            await Task.Delay(100);

            result.AddRange(GameData.Instance.DefaultGamemodes);

            return result;
        }

    }
}

