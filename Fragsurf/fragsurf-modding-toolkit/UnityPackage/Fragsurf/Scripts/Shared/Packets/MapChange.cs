using Lidgren.Network;

namespace Fragsurf.Shared.Packets
{
    public class MapChange : IBasePacket
    {
        public string MapName;
        public string Gamemode;
        public int ClientIndex;

        public int ByteSize => 0;
        public SendCategory Sc => SendCategory.UI_Important;
		public bool DisableAutoPool => false;

        public void Reset()
        {
            MapName = string.Empty;
            Gamemode = string.Empty;
            ClientIndex = 0;
        }

        public void Read(NetBuffer buffer)
        {
            MapName = buffer.ReadString();
            Gamemode = buffer.ReadString();
            ClientIndex = buffer.ReadInt32();
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(MapName);
            buffer.Write(Gamemode);
            buffer.Write(ClientIndex);
        }

    }
}
