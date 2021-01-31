using Fragsurf.Shared.Packets;

namespace Fragsurf.Shared
{
    public interface INetworkInterface
    {
        void BroadcastPacket(IBasePacket packet);
        void SendPacket(ulong steamid, IBasePacket packet);
        void Shutdown();
    }
}

