using Fragsurf.Shared.Packets;

namespace Fragsurf.Shared
{
    public interface INetworkInterface
    {
        void BroadcastPacket(IBasePacket packet);
        void SendPacket(int clientIndex, IBasePacket packet);
        void Shutdown();
    }
}

