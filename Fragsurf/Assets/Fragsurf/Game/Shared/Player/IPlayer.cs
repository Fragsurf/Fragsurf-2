
using Fragsurf.Shared.Entity;

namespace Fragsurf.Shared.Player
{
    public interface IPlayer
    {
        int ClientIndex { get; }
        ulong AccountId { get; }
        string DisplayName { get; set; }
        bool Introduced { get; set; }
        byte Team { get; set; }
        int Latency { get; set; }
        bool IsFake { get; }
        bool Disconnected { get; set; }
        NetEntity Entity { get; set; }
    }
}
