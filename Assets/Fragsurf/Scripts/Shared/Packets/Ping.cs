using Fragsurf.Network;

namespace Fragsurf.Shared.Packets
{
    public class Ping : IBasePacket
    {
        public SendCategory Sc => SendCategory.Unreliable;
		public bool DisableAutoPool => false;
        public int ByteSize => 16;

        public float SendTime;
        public int CurrentRTT;
        public float RemoteTime;
        public int RemoteTick;

        public void Reset()
        {
            SendTime = 0;
            CurrentRTT = 0;
            RemoteTime = 0;
            RemoteTick = 0;
        }

        public void Read(NetBuffer buffer)
        {
            SendTime = buffer.ReadSingle();
            CurrentRTT = buffer.ReadInt32();
            RemoteTime = buffer.ReadSingle();
            RemoteTick = buffer.ReadInt32();
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(SendTime);
            buffer.Write(CurrentRTT);
            buffer.Write(RemoteTime);
            buffer.Write(RemoteTick);
        }
    }
}
