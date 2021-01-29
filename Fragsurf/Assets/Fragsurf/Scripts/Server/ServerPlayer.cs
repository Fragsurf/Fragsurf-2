using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;

namespace Fragsurf.Server
{
    public class ServerPlayer : IPlayer
    {

        public ServerPlayer(ulong steamId, int index, float connectionTime, bool isFake = false)
        {
            AccountId = steamId;
            ConnectionTime = connectionTime;
            ClientIndex = index;
            IsFake = isFake;
        }

        public readonly float ConnectionTime;
        public int MTU = 1000;
        public bool IsFake { get; }
        public int ClientIndex { get; private set; }
        public ulong AccountId { get; set; } = 0;
        public NetEntity Entity { get; set; }
        public string DisplayName { get; set; }
        public bool Introduced { get; set; }
        public byte Team { get; set; }
        public bool Disconnected { get; set; }
        public int Latency { get; set; }
        public float TimeSinceLastMessage;
        public byte[] TicketData;
        public float TickTimeDiff;

    }
}
