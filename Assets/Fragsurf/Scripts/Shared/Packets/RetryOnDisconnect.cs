using Lidgren.Network;

namespace Fragsurf.Shared.Packets
{
    public class RetryOnDisconnect : IBasePacket
    {

        public SendCategory Sc => SendCategory.UI_Important;
        public bool DisableAutoPool => false;
        public int ByteSize => 0;

        public void Reset()
        {
        }

        public void Read(NetBuffer buffer)
        {
        }

        public void Write(NetBuffer buffer)
        {
        }

    }
}
