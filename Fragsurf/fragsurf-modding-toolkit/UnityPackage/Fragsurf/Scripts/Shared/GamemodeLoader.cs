using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared
{
    public class GamemodeLoader : FSSharedScript
    {
        private string _defaultGamemode = "CustomGamemode";
        private List<BaseGamemode> _gamemodes = new List<BaseGamemode>();

        public bool Ranked { get; private set; }

        public bool LoadGamemode(string gamemodeName = null)
        {
            var result = true;

            var gamemodeInstance = BaseGamemode.GetInstance<BaseGamemode>(gamemodeName ?? DevConsole.GetVariable<string>("game.mode"));

            if (gamemodeInstance == null)
            {
                DevConsole.WriteLine($"Couldn't load gamemode: {gamemodeName}");
                gamemodeInstance = BaseGamemode.GetDefaultGamemode();
            }

            Gamemode = gamemodeInstance;

            InitGamemodes(gamemodeInstance);
            LoadGamemodes();

            Game.DefaultConfig.ExecutePostLoad();

            return result;
        }

        protected override void _Initialize()
        {
            DevConsole.RegisterVariable("game.mode", "Default gamemode", () => _defaultGamemode, v => _defaultGamemode = v, this);
            DevConsole.RegisterVariable("game.ranked", "This match is competitive ranked.  Only useful for internal services.", () => Ranked, v => Ranked = v, this);
            DevConsole.RegisterCommand("game.refreshini", "Refresh gamemode configs", this, RefreshConfigs);
        }

        protected override void OnGameUnloaded()
        {
            UnloadGamemodes();
        }

        public T GetGamemode<T>()
            where T : BaseGamemode
        {
            foreach(var gm in _gamemodes)
            {
                if(gm is T)
                {
                    return (T)gm;
                }
            }
            return default;
        }

        private void InitGamemodes(BaseGamemode gamemode)
        {
            if (gamemode.Dependencies != null)
            {
                foreach (var d in gamemode.Dependencies)
                {
                    if (d == gamemode.Name) continue;
                    var requiredGamemode = BaseGamemode.GetInstance<BaseGamemode>(d);
                    if (requiredGamemode == null) continue;
                    InitGamemodes(requiredGamemode);
                }
            }

            _gamemodes.Add(gamemode);
            gamemode.LoadConfig();
        }

        private void UnloadGamemodes()
        {
            for (int i = _gamemodes.Count - 1; i >= 0; i--)
            {
                UnloadGamemode(_gamemodes[i]);
            }
            _gamemodes.Clear();
        }

        private void LoadGamemodes()
        {
            foreach(var gamemode in _gamemodes)
            {
                gamemode.Load(Game);
            }
        }

        private void UnloadGamemode(BaseGamemode gamemode)
        {
            if (!_gamemodes.Contains(gamemode))
            {
                return;
            }
            gamemode.Unload(Game);
            _gamemodes.Remove(gamemode);
        }

        private void RefreshConfigs(string[] args)
        {
            foreach(var gm in _gamemodes)
            {
                gm.LoadConfig();
            }
        }

    }
}

