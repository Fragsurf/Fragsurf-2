using Lidgren.Network;

namespace Fragsurf.Shared.Packets
{
    public class FileChunk : IBasePacket
    {
        public string File;
        public long TotalLength;
        public int ChunkLength;
        public byte[] Data;
        public bool Temporary;

        public int ByteSize => 0;
        public SendCategory Sc => SendCategory.FileTransfer;
		public bool DisableAutoPool => false;

        public void Reset()
        {
            File = string.Empty;
            TotalLength = 0;
            ChunkLength = 0;
            Data = null;
            Temporary = false;
        }

        public void Read(NetBuffer buffer)
        {
            File = buffer.ReadString().ToLower();
            TotalLength = buffer.ReadInt64();
            ChunkLength = buffer.ReadInt32();
            Data = buffer.ReadBytes(ChunkLength);
            Temporary = buffer.ReadBoolean();
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(File);
            buffer.Write(TotalLength);
            buffer.Write(Data.Length);
            buffer.Write(Data);
            buffer.Write(Temporary);
        }
    }
}
