using Fragsurf.Shared;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;

namespace Fragsurf.Client
{
    public class ClientPlayerManager : FSClientScript
    {

        protected override void OnPlayerPacketReceived(IPlayer player, IBasePacket packet)
        {
            switch (packet)
            {
                case PlayerEvent playerEvent:
                    IncomingPlayerEvent(playerEvent);
                    break;
            }
        }

        private void IncomingPlayerEvent(PlayerEvent playerEvent)
        {
            PeerPlayer peer = (PeerPlayer)Game.PlayerManager.FindPlayer(playerEvent.ClientIndex);

            if(peer == null && 
                (playerEvent.EventType != PlayerEventType.Backfill && playerEvent.EventType != PlayerEventType.Introduced))
            {
                UnityEngine.Debug.Log("Player event [" + playerEvent.EventType + "] but player is missing: " + playerEvent.ClientIndex);
                return;
            }

            switch (playerEvent.EventType)
            {
                case PlayerEventType.Backfill:
                case PlayerEventType.Introduced:
                    AddPeer(playerEvent);
                    break;
                case PlayerEventType.Disconnected:
                    peer.Disconnected = true;
                    Game.PlayerManager.RemovePlayer(peer);
                    break;
                case PlayerEventType.ChangedName:
                    peer.DisplayName = playerEvent.DisplayName;
                    break;
                case PlayerEventType.ChangedTeam:
                    Game.PlayerManager.SetPlayerTeam(peer, playerEvent.TeamNumber);
                    break;
                case PlayerEventType.LatencyUpdated:
                    peer.Latency = playerEvent.Latency;
                    break;
                case PlayerEventType.Spectate:
                    if (peer.ClientIndex != Game.ClientIndex)
                        Game.PlayerManager.SetPlayerSpectateTarget(peer, Game.PlayerManager.FindPlayer(playerEvent.SpecTarget));
                    break;
            }
        }

        private void AddPeer(PlayerEvent e)
        {
            var peer = new PeerPlayer(e.ClientIndex);
            peer.DisplayName = e.DisplayName;
            peer.Team = e.TeamNumber;
            peer.SteamId = e.SteamID;
            Game.PlayerManager.IntroducePlayer(peer);
        }

    }
}

