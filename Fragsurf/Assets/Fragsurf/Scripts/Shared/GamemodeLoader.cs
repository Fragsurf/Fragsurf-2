using System;
using UnityEngine;

namespace Fragsurf.Shared
{
    public class GamemodeLoader : FSSharedScript
    {

        public void LoadGamemode(GamemodeData data)
        {
            Gamemode = Activator.CreateInstance(data.GamemodeType) as BaseGamemode;
            if(Gamemode == null)
            {
                DevConsole.WriteLine("Failed to instatiate gamemode type: " + data.GamemodeType);
                Gamemode = new DefaultGamemode();
            }
            Gamemode.Load(Game);
            Game.DefaultConfig.ExecutePostLoad();
        }

        public bool LoadGamemode(string gamemodeName = null)
        {
            foreach(var gm in Resources.FindObjectsOfTypeAll<GamemodeData>())
            {
                if(gm.Name.Equals(gamemodeName, StringComparison.OrdinalIgnoreCase))
                {
                    LoadGamemode(gm);
                    return true;
                }
            }
            return false;
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

