using Fragsurf.Shared;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;

namespace Fragsurf.Client
{
    public class ClientPlayerManager : FSClientScript
    {

        protected override void OnPlayerPacketReceived(BasePlayer player, IBasePacket packet)
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
            var player = Game.PlayerManager.FindPlayer(playerEvent.ClientIndex);

            if(player == null && 
                (playerEvent.EventType != PlayerEventType.Backfill && playerEvent.EventType != PlayerEventType.Introduced))
            {
                UnityEngine.Debug.Log("Player event [" + playerEvent.EventType + "] but player is missing: " + playerEvent.ClientIndex);
                return;
            }

            switch (playerEvent.EventType)
            {
                case PlayerEventType.Backfill:
                case PlayerEventType.Introduced:
                    CreatePlayerFromEvent(playerEvent);
                    break;
                case PlayerEventType.Disconnected:
                    player.Disconnected = true;
                    Game.PlayerManager.RemovePlayer(player);
                    break;
                case PlayerEventType.ChangedName:
                    player.DisplayName = playerEvent.DisplayName;
                    break;
                case PlayerEventType.ChangedTeam:
                    Game.PlayerManager.SetPlayerTeam(player, playerEvent.TeamNumber);
                    break;
                case PlayerEventType.LatencyUpdated:
                    player.LatencyMs = playerEvent.Latency;
                    break;
                case PlayerEventType.Spectate:
                    if (player.ClientIndex != Game.ClientIndex)
                        Game.PlayerManager.SetPlayerSpectateTarget(player, Game.PlayerManager.FindPlayer(playerEvent.SpecTarget));
                    break;
            }
        }

        private void CreatePlayerFromEvent(PlayerEvent e)
        {
            var peer = new BasePlayer()
            {
                DisplayName = e.DisplayName,
                ClientIndex = e.ClientIndex,
                Team = e.TeamNumber,
                SteamId = e.SteamID
            };
            Game.PlayerManager.IntroducePlayer(peer);
        }

    }
}

