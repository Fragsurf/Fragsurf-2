using Fragsurf.Shared.Player;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Client
{
    public class PeerPlayer : IPlayer
    {
        public PeerPlayer(int clientIndex)
        {
            ClientIndex = clientIndex;
        }

        public int ClientIndex { get; }
        public ulong SteamId { get; set; }
        public string DisplayName { get; set; }
        public bool Introduced { get; set; }
        public NetEntity Entity { get; set; }
        public byte Team { get; set; }
        public int Latency { get; set; }
        public bool Disconnected { get; set; }
        public bool IsFake => SteamId < 100000;
    }
}
