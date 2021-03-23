
using Fragsurf.Shared.Entity;

namespace Fragsurf.Shared.Player
{
    public class BasePlayer
    {
        public int ClientIndex;
        public ulong SteamId;
        public string DisplayName;
        public bool Introduced;
        public byte Team;
        public float ConnectionTime;
        public int LatencyMs;
        public bool IsFake;
        public bool Disconnected;
        public NetEntity Entity;
        public byte[] TicketData;
        public float TimeSinceLastMessage;
        public float TickTimeDiff;
        public int MTU = 1000;
    }
}
