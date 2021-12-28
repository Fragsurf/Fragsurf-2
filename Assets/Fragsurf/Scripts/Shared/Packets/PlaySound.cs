using UnityEngine;
using Fragsurf.Network;

namespace Fragsurf.Shared.Packets
{
    public class PlaySound : IBasePacket
    {
        public string Event;
        public Vector3 Position;
        public bool Twod;

        public SendCategory Sc => SendCategory.UI_Important;
		public bool DisableAutoPool => false;
        public int ByteSize => 0;

        public void Reset()
        {
            Event = string.Empty;
            Position = Vector3.zero;
            Twod = false;
        }

        public void Read(NetBuffer buffer)
        {
            Event = buffer.ReadString();
            Position = buffer.ReadVector3();
            Twod = buffer.ReadBoolean();
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(Event);
            buffer.Write(Position);
            buffer.Write(Twod);
        }

    }
}
