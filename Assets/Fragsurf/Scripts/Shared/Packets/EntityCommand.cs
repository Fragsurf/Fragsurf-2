using Fragsurf.Network;

namespace Fragsurf.Shared.Packets
{
    public class EntityCommand : IBasePacket
    {

        public int EntityId;
        public string CommandName = string.Empty;
        public string ArgsJson = string.Empty;

        public SendCategory Sc => SendCategory.UI_Important;
        public int ByteSize => 0;
        public bool DisableAutoPool => false;

        public void Read(NetBuffer buffer)
        {
            EntityId = buffer.ReadInt32();
            CommandName = buffer.ReadString();
            ArgsJson = buffer.ReadString();
        }

        public void Reset()
        {
            EntityId = 0;
            CommandName = string.Empty;
            ArgsJson = string.Empty;
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(EntityId);
            buffer.Write(CommandName);
            buffer.Write(ArgsJson);
        }

    }
}

