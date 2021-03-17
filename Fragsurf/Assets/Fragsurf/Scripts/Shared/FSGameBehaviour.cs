using UnityEngine;
using Fragsurf.Maps;
using Fragsurf.Shared.Player;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Packets;

namespace Fragsurf.Shared
{
    public class FSGameBehaviour : MonoBehaviour
    {

        private FSGameLoop _game;

        protected void Init(FSGameLoop game)
        {
            game.PlayerManager.OnPlayerPacketReceived += OnPlayerPacketReceived;
            game.PlayerManager.OnPlayerConnected += OnPlayerConnected;
            game.PlayerManager.OnPlayerApprovedToJoin += OnPlayerApprovedToJoin;
            game.PlayerManager.OnPlayerDisconnected += OnPlayerDisconnected;
            game.PlayerManager.OnPlayerIntroduced += OnPlayerIntroduced;
            game.PlayerManager.OnPlayerChangedTeam += OnPlayerChangedTeam;
            game.PlayerManager.OnPlayerChangedName += OnPlayerChangedName;
            game.PlayerManager.OnPlayerLatencyUpdated += OnPlayerLatencyUpdated;
            game.PlayerManager.OnPlayerRunCommand += OnPlayerRunCommand;
            game.PlayerManager.OnPlayerSpectate += OnPlayerSpectate;
            game.PlayerManager.OnChatCommand += OnPlayerChatCommand;
            game.EntityManager.OnEntityAdded += OnEntityAdded;
            game.EntityManager.OnEntityDestroyed += OnEntityDestroyed;
            game.EntityManager.OnEntityUpdated += OnEntityUpdated;
            game.EntityManager.OnHumanTrigger += OnHumanTrigger;
            game.GameLoader.PreGameLoaded += OnPreGameLoaded;
            game.GameLoader.GameLoaded += OnGameLoaded;

            _game = game;
        }

        private void OnDestroy()
        {
            if (_game)
            {
                _game.PlayerManager.OnPlayerPacketReceived -= OnPlayerPacketReceived;
                _game.PlayerManager.OnPlayerConnected -= OnPlayerConnected;
                _game.PlayerManager.OnPlayerApprovedToJoin -= OnPlayerApprovedToJoin;
                _game.PlayerManager.OnPlayerDisconnected -= OnPlayerDisconnected;
                _game.PlayerManager.OnPlayerIntroduced -= OnPlayerIntroduced;
                _game.PlayerManager.OnPlayerChangedTeam -= OnPlayerChangedTeam;
                _game.PlayerManager.OnPlayerChangedName -= OnPlayerChangedName;
                _game.PlayerManager.OnPlayerLatencyUpdated -= OnPlayerLatencyUpdated;
                _game.PlayerManager.OnPlayerRunCommand -= OnPlayerRunCommand;
                _game.PlayerManager.OnPlayerSpectate -= OnPlayerSpectate;
                _game.PlayerManager.OnChatCommand -= OnPlayerChatCommand;
                _game.EntityManager.OnEntityAdded -= OnEntityAdded;
                _game.EntityManager.OnEntityDestroyed -= OnEntityDestroyed;
                _game.EntityManager.OnEntityUpdated -= OnEntityUpdated;
                _game.EntityManager.OnHumanTrigger -= OnHumanTrigger;
                _game.GameLoader.PreGameLoaded -= OnPreGameLoaded;
                _game.GameLoader.GameLoaded -= OnGameLoaded;
            }   
        }

        protected virtual void OnGameLoaded() { }
        protected virtual void OnPreGameLoaded() { }
        protected virtual void OnEntityAdded(NetEntity entity) { }
        protected virtual void OnEntityDestroyed(NetEntity entity) { }
        protected virtual void OnEntityUpdated(NetEntity entity, int remoteTick, double remoteTime) { }
        protected virtual void OnHumanTrigger(NetEntity entity, Actors.FSMTrigger trigger, TriggerEventType type, float offset = 0f) { }
        protected virtual void OnPlayerPacketReceived(BasePlayer player, IBasePacket packet) { }
        protected virtual void OnPlayerConnected(BasePlayer player) { }
        protected virtual void OnPlayerApprovedToJoin(BasePlayer player) { }
        protected virtual void OnPlayerIntroduced(BasePlayer player) { }
        protected virtual void OnPlayerDisconnected(BasePlayer player) { }
        protected virtual void OnPlayerChangedTeam(BasePlayer player) { }
        protected virtual void OnPlayerChangedName(BasePlayer player) { }
        protected virtual void OnPlayerLatencyUpdated(BasePlayer player) { }
        protected virtual void OnPlayerRunCommand(BasePlayer player) { }
        protected virtual void OnPlayerSpectate(BasePlayer spectator, BasePlayer target) { }
        protected virtual void OnPlayerChatCommand(BasePlayer player, string[] args) { }

    }
}

