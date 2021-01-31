using Lidgren.Network;

namespace Fragsurf.Shared.Packets
{
    public class DeleteEntity : IBasePacket
    {
        public int EntityId;

        public SendCategory Sc => SendCategory.UI_Important;
		public bool DisableAutoPool => false;
        public int ByteSize => 4;

        public void Reset()
        {
            EntityId = 0;
        }

        public void Read(NetBuffer buffer)
        {
            EntityId = buffer.ReadInt32();
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(EntityId);
        }
    }
}
