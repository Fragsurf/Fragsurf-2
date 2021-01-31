using Lidgren.Network;
using Fragsurf.Shared;
using Fragsurf.Shared.Packets;

namespace Fragsurf.Server
{
    public class ServerUserPlugins : SharedUserPlugins
    {
        public void OnReceivePlayerPacket(IBasePacket packet, ServerPlayer player)
        {
            if (!(packet is CustomPacket))
            {
                InvokeEventSubscriptions("OnReceivePlayerPacket", packet, player);
            }
            else
            {
                InvokeEventSubscriptions("OnReceiveCustomPacket", packet, player);
            }
        }
        public void OnServerStatusChanged(ServerStatus newStatus)
        {
            InvokeEventSubscriptions("OnServerStatusChanged", newStatus);
        }
    }
}

