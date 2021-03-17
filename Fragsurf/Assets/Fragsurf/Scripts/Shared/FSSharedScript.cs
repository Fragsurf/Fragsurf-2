using Fragsurf.Maps;
using Fragsurf.Shared.Player;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Packets;

namespace Fragsurf.Shared
{
    public class FSSharedScript : FSComponent
    {
        protected override void _Hook()
        {
            Game.PlayerManager.OnPlayerPacketReceived += OnPlayerPacketReceived;
            Game.PlayerManager.OnPlayerConnected += OnPlayerConnected;
            Game.PlayerManager.OnPlayerApprovedToJoin += OnPlayerApprovedToJoin;
            Game.PlayerManager.OnPlayerDisconnected += OnPlayerDisconnected;
            Game.PlayerManager.OnPlayerIntroduced += OnPlayerIntroduced;
            Game.PlayerManager.OnPlayerChangedTeam += OnPlayerChangedTeam;
            Game.PlayerManager.OnPlayerChangedName += OnPlayerChangedName;
            Game.PlayerManager.OnPlayerLatencyUpdated += OnPlayerLatencyUpdated;
            Game.PlayerManager.OnPlayerRunCommand += OnPlayerRunCommand;
            Game.PlayerManager.OnPlayerSpectate += OnPlayerSpectate;
            Game.PlayerManager.OnChatCommand += OnPlayerChatCommand;
            Game.EntityManager.OnEntityAdded += OnEntityAdded;
            Game.EntityManager.OnEntityDestroyed += OnEntityDestroyed;
            Game.EntityManager.OnEntityUpdated += OnEntityUpdated;
            Game.EntityManager.OnHumanTrigger += OnHumanTrigger;
            Game.EntityManager.OnHumanKilled += OnHumanKilled;
            Game.EntityManager.OnHumanSpawned += OnHumanSpawned;
            Game.EntityManager.OnHumanDamaged += OnHumanDamaged;
            Game.GameLoader.PreGameLoaded += OnPreGameLoaded;
            Game.GameLoader.GameLoaded += OnGameLoaded;

            var chatCmds = Game.Get<ChatCommands>();
            if (chatCmds)
            {
                chatCmds.Register(this);
            }
        }

        protected override void _Unhook()
        {
            Game.PlayerManager.OnPlayerPacketReceived -= OnPlayerPacketReceived;
            Game.PlayerManager.OnPlayerConnected -= OnPlayerConnected;
            Game.PlayerManager.OnPlayerApprovedToJoin -= OnPlayerApprovedToJoin;
            Game.PlayerManager.OnPlayerDisconnected -= OnPlayerDisconnected;
            Game.PlayerManager.OnPlayerIntroduced -= OnPlayerIntroduced;
            Game.PlayerManager.OnPlayerChangedTeam -= OnPlayerChangedTeam;
            Game.PlayerManager.OnPlayerChangedName -= OnPlayerChangedName;
            Game.PlayerManager.OnPlayerLatencyUpdated -= OnPlayerLatencyUpdated;
            Game.PlayerManager.OnPlayerRunCommand -= OnPlayerRunCommand;
            Game.PlayerManager.OnPlayerSpectate -= OnPlayerSpectate;
            Game.PlayerManager.OnChatCommand -= OnPlayerChatCommand;
            Game.EntityManager.OnEntityAdded -= OnEntityAdded;
            Game.EntityManager.OnEntityDestroyed -= OnEntityDestroyed;
            Game.EntityManager.OnEntityUpdated -= OnEntityUpdated;
            Game.EntityManager.OnHumanTrigger -= OnHumanTrigger;
            Game.EntityManager.OnHumanKilled -= OnHumanKilled;
            Game.EntityManager.OnHumanSpawned -= OnHumanSpawned;
            Game.EntityManager.OnHumanDamaged -= OnHumanDamaged;
            Game.GameLoader.PreGameLoaded -= OnPreGameLoaded;
            Game.GameLoader.GameLoaded -= OnGameLoaded;

            var chatCmds = Game.Get<ChatCommands>();
            if (chatCmds)
            {
                chatCmds.UnRegister(this);
            }
        }

        protected virtual void OnGameLoaded() { }
        protected virtual void OnPreGameLoaded() { }
        protected virtual void OnEntityAdded(NetEntity entity) { }
        protected virtual void OnEntityDestroyed(NetEntity entity) { }
        protected virtual void OnEntityUpdated(NetEntity entity, int remoteTick, double remoteTime) { }
        protected virtual void OnHumanTrigger(NetEntity entity, Actors.FSMTrigger trigger, TriggerEventType type, float offset = 0f) { }
        protected virtual void OnHumanKilled(Human hu) { }
        protected virtual void OnHumanDamaged(Human hu, DamageInfo dmgInfo) { }
        protected virtual void OnHumanSpawned(Human hu) { }
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

