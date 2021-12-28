using Fragsurf.Shared.Entity;
using Fragsurf.Network;

namespace Fragsurf.Shared.Packets
{
    public class EntityUpdate : IBasePacket
    {

        public int EntityId;
        public byte EntityTypeId;
        public int ChangedTick;
        public float ChangedTime;
        public int PropDataSize;
        public byte[] PropData;

        public SendCategory Sc { get; set; } = SendCategory.EntityFast;
        public int ByteSize => 0;
        public bool DisableAutoPool => false;

        public void Load(NetEntity entity)
        {
            EntityId = entity.EntityId;
            EntityTypeId = entity.TypeId;
            ChangedTick = entity.ChangedTick;
            ChangedTime = entity.ChangedTime;
            entity.GetPropData(out PropDataSize, out PropData);
        }

        public void Read(NetBuffer buffer)
        {
            EntityId = buffer.ReadInt32();
            EntityTypeId = buffer.ReadByte();
            ChangedTick = buffer.ReadInt32();
            ChangedTime = buffer.ReadSingle();
            PropDataSize = buffer.ReadInt32();
            PropData = buffer.ReadBytes(PropDataSize);
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(EntityId);
            buffer.Write(EntityTypeId);
            buffer.Write(ChangedTick);
            buffer.Write(ChangedTime);
            buffer.Write(PropDataSize);
            buffer.Write(PropData);
        }

        public void Reset()
        {
            EntityId = 0;
            EntityTypeId = 0;
            ChangedTick = 0;
            ChangedTime = 0;
            PropDataSize = 0;
            PropData = null;
            Sc = SendCategory.EntityFast;
        }

    }
}

