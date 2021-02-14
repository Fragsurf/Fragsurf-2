using System;
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
            catch(Exception e)
            {
                Debug.LogError($"Failed to load {data.Name}: {e.Message}");
            }
            return false;
        }

        public bool LoadGamemode(string gamemodeName = null)
        {
            var resource = Resources.Load<GamemodeData>(gamemodeName);
            if (!resource)
            {
                Debug.LogError("Missing gamemode data: " + gamemodeName);
                return false;
            }
            return LoadGamemode(resource);
        }

        protected override void OnGameUnloaded()
        {
            Gamemode?.Unload(Game);
        }

        [ConCommand("gamemode.refreshconfigs")]
        private void RefreshConfigs(string[] args)
        {
            Gamemode?.LoadConfig();
        }

    }
}

