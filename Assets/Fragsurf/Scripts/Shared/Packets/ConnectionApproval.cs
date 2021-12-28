using Fragsurf.Network;

namespace Fragsurf.Shared.Packets
{
    public class ConnectionApproval : IBasePacket
    {
        public string GameVersion = string.Empty;
        public string DisplayName = string.Empty;
        public string Password = string.Empty;
        public ulong SteamID;
        public byte[] TicketData;

        public SendCategory Sc => SendCategory.UI_Important;
		public bool DisableAutoPool => false;

        public int ByteSize => (GameVersion.Length * 2) + (DisplayName.Length * 2) + 12 + TicketData.Length;

        public void Reset()
        {
            GameVersion = string.Empty;
            DisplayName = string.Empty;
            Password = string.Empty;
            SteamID = 0;
            TicketData = null;
        }

        public void Read(NetBuffer buffer)
        {
            GameVersion = buffer.ReadString();
            DisplayName = buffer.ReadString();
            Password = buffer.ReadString();
            SteamID = buffer.ReadUInt64();

            var len = buffer.ReadInt32();
            if(len > 0)
            {
                TicketData = buffer.ReadBytes(len);
            }
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(GameVersion);
            buffer.Write(DisplayName);
            buffer.Write(Password);
            buffer.Write(SteamID);

            if(TicketData == null)
            {
                buffer.Write(0);
            }
            else
            {
                buffer.Write(TicketData.Length);
                buffer.Write(TicketData);
            }
        }

    }
}
