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
            //MapLoader.Instance.OnMapEvent += OnMapEvent;
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
            game.GameLoader.PreGameUnloaded += OnPreGameUnloaded;
            game.GameLoader.GameUnloaded += OnGameUnloaded;

            _game = game;
        }

        private void OnDestroy()
        {
            if (_game)
            {
                //if (MapLoader.Instance != null)
                //{
                //    MapLoader.Instance.OnMapEvent -= OnMapEvent;
                //}
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
                _game.GameLoader.PreGameUnloaded -= OnPreGameUnloaded;
                _game.GameLoader.GameUnloaded -= OnGameUnloaded;
            }   
        }

        //protected virtual void OnMapEvent(IFragsurfMap map, MapEventType eventType, bool hasNextMap) { }
        protected virtual void OnGameLoaded() { }
        protected virtual void OnPreGameLoaded() { }
        protected virtual void OnGameUnloaded() { }
        protected virtual void OnPreGameUnloaded() { }
        protected virtual void OnEntityAdded(NetEntity entity) { }
        protected virtual void OnEntityDestroyed(NetEntity entity) { }
        protected virtual void OnEntityUpdated(NetEntity entity, int remoteTick, double remoteTime) { }
        protected virtual void OnHumanTrigger(NetEntity entity, Actors.FSMTrigger trigger, TriggerEventType type, float offset = 0f) { }
        protected virtual void OnPlayerPacketReceived(IPlayer player, IBasePacket packet) { }
        protected virtual void OnPlayerConnected(IPlayer player) { }
        protected virtual void OnPlayerApprovedToJoin(IPlayer player) { }
        protected virtual void OnPlayerIntroduced(IPlayer player) { }
        protected virtual void OnPlayerDisconnected(IPlayer player) { }
        protected virtual void OnPlayerChangedTeam(IPlayer player) { }
        protected virtual void OnPlayerChangedName(IPlayer player) { }
        protected virtual void OnPlayerLatencyUpdated(IPlayer player) { }
        protected virtual void OnPlayerRunCommand(IPlayer player) { }
        protected virtual void OnPlayerSpectate(IPlayer spectator, IPlayer target) { }
        protected virtual void OnPlayerChatCommand(IPlayer player, string[] args) { }

    }
}

