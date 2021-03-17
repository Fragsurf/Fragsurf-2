using Fragsurf.Shared.Packets;
using Fragsurf.Shared;
using Fragsurf.Maps;
using Fragsurf.Shared.Player;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Server
{
    public class ServerPlayerManager : FSServerScript
    {
        protected override void OnPlayerIntroduced(BasePlayer player)
        {
            BroadcastPlayerIntroduced(player);
            Game.PlayerManager.SetPlayerTeam(player, 0);
        }

        private void ProcessPlayerIntroduction(BasePlayer player, PlayerIntroduction intro)
        {
            switch (intro.Step)
            {
                case PlayerIntroduction.JoinStep.Introduce:
                    Game.PlayerManager.IntroducePlayer(player);
                    SendPlayerBackfill(player);
                    break;
            }
        }

        protected override void OnPlayerPacketReceived(BasePlayer player, IBasePacket packet)
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
                            SendFileSync(player);
                            break;
                    }
                    break;
                case MapChange mapChange:
                    SendMapChange(player);
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

        protected override void OnPlayerSpectate(BasePlayer spectator, BasePlayer target)
        {
            var ev = PacketUtility.TakePacket<PlayerEvent>();
            ev.EventType = PlayerEventType.Spectate;
            ev.ClientIndex = spectator.ClientIndex;
            ev.SpecTarget = target == null ? -1 : target.ClientIndex;
            Game.GetFSComponent<SocketManager>().BroadcastPacket(ev);
        }

        protected override void OnPlayerDisconnected(BasePlayer player)
        {
            var ev = PacketUtility.TakePacket<PlayerEvent>();
            ev.EventType = PlayerEventType.Disconnected;
            ev.ClientIndex = player.ClientIndex;
            Game.GetFSComponent<SocketManager>().BroadcastPacket(ev);
        }

        protected override void OnPlayerChangedTeam(BasePlayer player)
        {
            var ev = PacketUtility.TakePacket<PlayerEvent>();
            ev.EventType = PlayerEventType.ChangedTeam;
            ev.ClientIndex = player.ClientIndex;
            ev.TeamNumber = player.Team;
            Game.GetFSComponent<SocketManager>().BroadcastPacket(ev);
        }

        protected override void OnPlayerChangedName(BasePlayer player)
        {
            var ev = PacketUtility.TakePacket<PlayerEvent>();
            ev.EventType = PlayerEventType.ChangedName;
            ev.ClientIndex = player.ClientIndex;
            ev.DisplayName = player.DisplayName;
            Game.GetFSComponent<SocketManager>().BroadcastPacket(ev);
        }

        protected override void OnPlayerLatencyUpdated(BasePlayer player)
        {
            var ev = PacketUtility.TakePacket<PlayerEvent>();
            ev.Sc = SendCategory.Unreliable;
            ev.EventType = PlayerEventType.LatencyUpdated;
            ev.ClientIndex = player.ClientIndex;
            ev.Latency = player.LatencyMs;
            Game.GetFSComponent<SocketManager>().BroadcastPacket(ev);
        }

        private void SendFileSync(BasePlayer player)
        {
            var fileSync = PacketUtility.TakePacket<FileSync>();
            fileSync.SyncType = FileSync.FileSyncType.Manifest;
            fileSync.DownloadUrl = DevConsole.GetVariable<string>("net.downloadurl") ?? string.Empty;
            fileSync.Files = new System.Collections.Generic.List<FSFileInfo>(FileSystem.DownloadList);
            Game.GetFSComponent<SocketManager>().SendPacketBrute(player, fileSync);
        }

        private void SendMapChange(BasePlayer player)
        {
            var mapChange = PacketUtility.TakePacket<MapChange>();
            mapChange.MapName = Map.Current.Name;
            mapChange.Gamemode = Game.GamemodeLoader.Gamemode.Data.Name;
            mapChange.ClientIndex = player.ClientIndex;
            Game.GetFSComponent<SocketManager>().SendPacketBrute(player, mapChange);
        }

        public void SendPlayerBackfill(BasePlayer player)
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
                    ev.Latency = playerToSend.LatencyMs;
                    ev.SteamID = playerToSend.SteamId;

                    Game.GetFSComponent<SocketManager>().SendPacketBrute(player.ClientIndex, ev);
                }
            }

            foreach(var ent in Game.EntityManager.Entities)
            {
                if (ent.IsLive)
                {
                    var updatePacket = PacketUtility.TakePacket<EntityUpdate>();
                    updatePacket.Sc = SendCategory.UI_Important;
                    updatePacket.Load(ent);

                    Game.GetFSComponent<SocketManager>().SendPacketBrute(player.ClientIndex, updatePacket);
                }
            }
        }

        private void BroadcastPlayerIntroduced(BasePlayer player)
        {
            var ev = PacketUtility.TakePacket<PlayerEvent>();
            ev.EventType = PlayerEventType.Introduced;
            ev.ClientIndex = player.ClientIndex;
            ev.DisplayName = player.DisplayName;
            ev.TeamNumber = player.Team;
            ev.Latency = player.LatencyMs;
            ev.SteamID = player.SteamId;
            Game.GetFSComponent<SocketManager>().BroadcastPacket(ev);
        }

    }
}

