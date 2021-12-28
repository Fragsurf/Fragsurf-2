using Fragsurf.Network;

namespace Fragsurf.Shared.Packets
{

    public enum ChatScope : byte
    {
        Global,
        Team
    }

    public class ChatMessage : IBasePacket
    {

        public ChatScope Scope;
        public int ClientIndex;
        public string Message = string.Empty;
        public string Name = string.Empty;
        public int SupporterLevel = 0;
        public bool FromGame = false;

        public SendCategory Sc => SendCategory.UI_Important;
		public bool DisableAutoPool => false;

        public int ByteSize => (Message.Length * 2) + (Name.Length * 2) + 1;

        public void Reset()
        {
            Scope = ChatScope.Global;
            ClientIndex = 0;
            Message = string.Empty;
            Name = string.Empty;
            FromGame = false;
        }

        public void Read(NetBuffer buffer)
        {
            Scope = (ChatScope)buffer.ReadByte();
            Message = buffer.ReadString();
            Name = buffer.ReadString();
            ClientIndex = buffer.ReadInt32();
            SupporterLevel = buffer.ReadInt32();
            FromGame = buffer.ReadBoolean();
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write((byte)Scope);
            buffer.Write(Message);
            buffer.Write(Name);
            buffer.Write(ClientIndex);
            buffer.Write(SupporterLevel);
            buffer.Write(FromGame);
        }

    }
}
