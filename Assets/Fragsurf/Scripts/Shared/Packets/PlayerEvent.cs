using Lidgren.Network;

namespace Fragsurf.Shared.Packets
{

    public enum PlayerEventType : byte
    {
        None,
        Backfill,
        Connected,
        Disconnected,
        Introduced,
        ChangedName,
        ChangedTeam,
        LatencyUpdated,
        Spectate
    }

    public class PlayerEvent : IBasePacket
    {
        public PlayerEventType EventType;
        public int ClientIndex;
        public int SpecTarget;
        public byte TeamNumber;
        public int Latency;
        public string DisplayName = string.Empty;
        public ulong SteamID;

        public int ByteSize => 0;
        public SendCategory Sc { get; set; } = SendCategory.UI_Important;
		public bool DisableAutoPool => false;

        public void Reset()
        {
            Sc = SendCategory.UI_Important;
            ClientIndex = 0;
            DisplayName = string.Empty;
            TeamNumber = 0;
            Latency = 0;
            EventType = PlayerEventType.None;
        }

        public void Read(NetBuffer buffer)
        {
            EventType = (PlayerEventType)buffer.ReadByte();
            ClientIndex = buffer.ReadInt32();
            TeamNumber = buffer.ReadByte();
            Latency = buffer.ReadInt32();
            DisplayName = buffer.ReadString();
            SteamID = buffer.ReadUInt64();

            switch(EventType)
            {
                case PlayerEventType.Spectate:
                    SpecTarget = buffer.ReadInt32();
                    break;
            }
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write((byte)EventType);
            buffer.Write(ClientIndex);
            buffer.Write(TeamNumber);
            buffer.Write(Latency);
            buffer.Write(DisplayName);
            buffer.Write(SteamID);

            switch (EventType)
            {
                case PlayerEventType.Spectate:
                    buffer.Write(SpecTarget);
                    break;
            }
        }

    }
}
