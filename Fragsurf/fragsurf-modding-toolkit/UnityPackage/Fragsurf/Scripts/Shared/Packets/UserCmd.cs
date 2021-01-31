using UnityEngine;
using Lidgren.Network;
using Fragsurf.Movement;

namespace Fragsurf.Shared.Packets
{
    public class UserCmd : IBasePacket
    {

        public struct CmdFields
        {
            public long Timestamp;
            public float HostTime;
            public int PacketId;
            public int ClientIndex;
            public InputActions Buttons;
            public Vector3 Origin;
            public Vector3 Angles;
            public Vector3 Velocity;
            public Vector3 BaseVelocity;
            public Vector3 MousePosition;
            public int HoveredEntity;
        }

        public CmdFields Fields = new CmdFields();

        public SendCategory Sc { get; set; } = SendCategory.Input;
		public bool DisableAutoPool => false;

        public int ByteSize => 80;

        public float HostTime { get { return Fields.HostTime; } set { Fields.HostTime = value; } }
        public int PacketId { get { return Fields.PacketId; } set { Fields.PacketId = value; } }
        public int ClientIndex { get { return Fields.ClientIndex; } set { Fields.ClientIndex = value; } }
        public InputActions Buttons { get { return Fields.Buttons; } set { Fields.Buttons = value; } }
        public Vector3 Origin { get { return Fields.Origin; } set { Fields.Origin = value; } }
        public Vector3 Angles { get { return Fields.Angles; } set { Fields.Angles = value; } }
        public Vector3 Velocity { get { return Fields.Velocity; } set { Fields.Velocity = value; } }
        public Vector3 BaseVelocity { get { return Fields.BaseVelocity; } set { Fields.BaseVelocity = value; } }
        public Vector3 MousePosition { get { return Fields.MousePosition; } set { Fields.MousePosition = value; } }
        public int HoveredEntity { get { return Fields.HoveredEntity; } set { Fields.HoveredEntity = value; } }

        public void Reset()
        {
            Fields = new CmdFields();
            Sc = SendCategory.Input;
            HoveredEntity = -1;
        }

        public void Copy(UserCmd copy)
        {
            Fields = copy.Fields;
        }

        public void Read(NetBuffer buffer)
        {
            Fields.HostTime = buffer.ReadSingle();
            Fields.PacketId = buffer.ReadInt32();
            Fields.ClientIndex = buffer.ReadInt32();
            Fields.Buttons = (InputActions)buffer.ReadInt32();
            Fields.Origin = buffer.ReadVector3();
            Fields.Angles = buffer.ReadVector3();
            Fields.Velocity = buffer.ReadVector3();
            Fields.BaseVelocity = buffer.ReadVector3();
            Fields.MousePosition = buffer.ReadVector3();
            Fields.HoveredEntity = buffer.ReadInt32();
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(Fields.HostTime);
            buffer.Write(Fields.PacketId);
            buffer.Write(Fields.ClientIndex);
            buffer.Write((int)Fields.Buttons);
            buffer.Write(Fields.Origin);
            buffer.Write(Fields.Angles);
            buffer.Write(Fields.Velocity);
            buffer.Write(Fields.BaseVelocity);
            buffer.Write(Fields.MousePosition);
            buffer.Write(Fields.HoveredEntity);
        }

    }
}

