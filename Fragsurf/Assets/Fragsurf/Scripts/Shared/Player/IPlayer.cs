
using Fragsurf.Shared.Entity;

namespace Fragsurf.Shared.Player
{
    public interface IPlayer
    {
        int ClientIndex { get; }
        ulong SteamId { get; }
        string DisplayName { get; set; }
        bool Introduced { get; set; }
        byte Team { get; set; }
        int LatencyMs { get; set; }
        bool IsFake { get; }
        bool Disconnected { get; set; }
        NetEntity Entity { get; set; }
    }
}
