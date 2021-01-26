using Fragsurf.Shared.Packets;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;

namespace Fragsurf.Server
{
    public class ServerPlayerManager : FSServerScript
    {
        protected override void OnPlayerIntroduced(IPlayer player)
        {
            BroadcastPlayerIntroduced(player);
            Game.PlayerManager.SetPlayerTeam(player, 0);
        }

        private void ProcessPlayerIntroduction(IPlayer player, PlayerIntroduction intro)
        {
            switch (intro.Step)
            {
                case PlayerIntroduction.JoinStep.Introduce:
                    Game.PlayerManager.IntroducePlayer(player);
                    SendPlayerBackfill(player);
                    break;
            }
        }

        protected override void OnPlayerPacketReceived(IPlayer player, IBasePacket packet)
        {
            switch (packet)
            {
                case PlayerIntroduction intro:
                    ProcessPlayerIntroduction(player, intro);
                    break;
                case FileSync fileSync:
                    switch (fileSync.SyncType)
                    {
                        case FileSync.FileSyncType.Initiate:
                            SendFileSync((ServerPlayer)player);
                            break;
                    }
                    break;
                case MapChange mapChange:
                    SendMapChange((ServerPlayer)player);
                    break;
                case ChooseTeam chooseTeam:
                    Game.PlayerManager.SetPlayerTeam(player, chooseTeam.TeamNumber);
                    break;
                case PlayerEvent pe:
                    if(pe.EventType == PlayerEventType.Spectate)
                    {
                        var spectator = Game.PlayerManager.FindPlayer(pe.ClientIndex);
                        Game.PlayerManager.SetPlayerSpectateTarget(spectator, Game.PlayerManager.FindPlayer(pe.SpecTarget));
                    }
                    break;
            }
        }

        protected override void OnPlayerSpectate(IPlayer spectator, IPlayer target)
        {
            var ev = PacketUtility.TakePacket<PlayerEvent>();
            ev.EventType = PlayerEventType.Spectate;
            ev.ClientIndex = spectator.ClientIndex;
            ev.SpecTarget = target == null ? -1 : target.ClientIndex;
            Game.GetFSComponent<SocketManager>().BroadcastPacket(ev);
        }

        protected override void OnPlayerDisconnected(IPlayer player)
        {
            var ev = PacketUtility.TakePacket<PlayerEvent>();
            ev.EventType = PlayerEventType.Disconnected;
            ev.ClientIndex = player.ClientIndex;
            Game.GetFSComponent<SocketManager>().BroadcastPacket(ev);
        }

        protected override void OnPlayerChangedTeam(IPlayer player)
        {
            var ev = PacketUtility.TakePacket<PlayerEvent>();
            ev.EventType = PlayerEventType.ChangedTeam;
            ev.ClientIndex = player.ClientIndex;
            ev.TeamNumber = player.Team;
            Game.GetFSComponent<SocketManager>().BroadcastPacket(ev);
        }

        protected override void OnPlayerChangedName(IPlayer player)
        {
            var ev = PacketUtility.TakePacket<PlayerEvent>();
            ev.EventType = PlayerEventType.ChangedName;
            ev.ClientIndex = player.ClientIndex;
            ev.DisplayName = player.DisplayName;
            Game.GetFSComponent<SocketManager>().BroadcastPacket(ev);
        }

        protected override void OnPlayerLatencyUpdated(IPlayer player)
        {
            var ev = PacketUtility.TakePacket<PlayerEvent>();
            ev.Sc = SendCategory.Unreliable;
            ev.EventType = PlayerEventType.LatencyUpdated;
            ev.ClientIndex = player.ClientIndex;
            ev.Latency = player.Latency;
            Game.GetFSComponent<SocketManager>().BroadcastPacket(ev);
        }

        private void SendFileSync(ServerPlayer player)
        {
            var fileSync = PacketUtility.TakePacket<FileSync>();
            fileSync.SyncType = FileSync.FileSyncType.Manifest;
            fileSync.DownloadUrl = DevConsole.GetVariable<string>("net.downloadurl") ?? string.Empty;
            fileSync.Files = new System.Collections.Generic.List<FSFileInfo>(FileSystem.DownloadList);
            Game.GetFSComponent<SocketManager>().SendPacketBrute(player, fileSync);
        }

        private void SendMapChange(ServerPlayer player)
        {
            var mapChange = PacketUtility.TakePacket<MapChange>();
            mapChange.MapName = MapLoader.Instance.CurrentMap.Name;
            mapChange.Gamemode = Game.GamemodeLoader.Gamemode.Name;
            mapChange.ClientIndex = player.ClientIndex;
            Game.GetFSComponent<SocketManager>().SendPacketBrute(player, mapChange);
        }

        public void SendPlayerBackfill(IPlayer player)
        {
            foreach(var playerToSend in Game.PlayerManager.Players)
            {
                if(playerToSend.ClientIndex != player.ClientIndex)
                {
                    var ev = PacketUtility.TakePacket<PlayerEvent>();
                    ev.Sc = SendCategory.UI_Important;
                    ev.EventType = PlayerEventType.Backfill;
                    ev.ClientIndex = playerToSend.ClientIndex;
                    ev.DisplayName = playerToSend.DisplayName;
                    ev.TeamNumber = playerToSend.Team;
                    ev.Latency = playerToSend.Latency;
                    ev.SteamID = playerToSend.AccountId;

                    Game.GetFSComponent<SocketManager>().SendPacketBrute(player.AccountId, ev);
                }
            }

            foreach(var ent in Game.EntityManager.Entities)
            {
                if (ent.IsValid())
                {
                    var updatePacket = PacketUtility.TakePacket<EntityUpdate>();
                    updatePacket.Sc = SendCategory.UI_Important;
                    updatePacket.Load(ent);

                    Game.GetFSComponent<SocketManager>().SendPacketBrute(player.AccountId, updatePacket);
                }
            }
        }

        private void BroadcastPlayerIntroduced(IPlayer player)
        {
            var ev = PacketUtility.TakePacket<PlayerEvent>();
            ev.EventType = PlayerEventType.Introduced;
            ev.ClientIndex = player.ClientIndex;
            ev.DisplayName = player.DisplayName;
            ev.TeamNumber = player.Team;
            ev.Latency = player.Latency;
            ev.SteamID = player.AccountId;
            Game.GetFSComponent<SocketManager>().BroadcastPacket(ev);
        }

    }
}

