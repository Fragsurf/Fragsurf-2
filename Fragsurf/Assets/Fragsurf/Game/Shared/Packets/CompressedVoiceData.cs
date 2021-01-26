using System;
using Lidgren.Network;

namespace Fragsurf.Shared.Packets
{
    public class CompressedVoiceData : IBasePacket
    {

        public SendCategory Sc => SendCategory.Voice;
		public bool DisableAutoPool => false;

        public int ByteSize => 0;
        public int ClientIndex;
        public int DataLength { get; private set; }
        public byte[] Data { get; private set; } = new byte[1024 * 64];

        public void Reset()
        {
            ClientIndex = 0;
            DataLength = 0;
        }

        public void Read(NetBuffer buffer)
        {
            ClientIndex = buffer.ReadInt32();
            DataLength = buffer.ReadInt32();
            buffer.ReadBytes(Data, 0, DataLength);
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(ClientIndex);
            buffer.Write(DataLength);
            buffer.Write(Data, 0, DataLength);
        }

        public void SetData(byte[] buffer, int length)
        {
            Buffer.BlockCopy(buffer, 0, Data, 0, length);
            DataLength = length;
        }
    }

}
