using Lidgren.Network;

namespace Fragsurf.Shared.Packets
{
    public class ChooseTeam : IBasePacket
    {
        public byte TeamNumber; // 0 = spectate, 1+ = team

        public SendCategory Sc => SendCategory.UI_Important;
		public bool DisableAutoPool => false;
        public int ByteSize => 0;

        public void Reset()
        {
            TeamNumber = 0;
        }

        public void Read(NetBuffer buffer)
        {
            TeamNumber = buffer.ReadByte();
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(TeamNumber);
        }

    }
}
