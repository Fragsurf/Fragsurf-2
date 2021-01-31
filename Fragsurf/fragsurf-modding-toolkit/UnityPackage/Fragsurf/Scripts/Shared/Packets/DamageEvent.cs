using Fragsurf.Shared.Player;
using Fragsurf.Shared.Entity;
using Lidgren.Network;

namespace Fragsurf.Shared.Packets
{
    public class DamageEvent : IBasePacket
    {
        public SendCategory Sc => SendCategory.EntityImportant;
        public int ByteSize => 0;
        public bool DisableAutoPool => false;

        public DamageInfo DamageInfo = new DamageInfo();

        public void Read(NetBuffer buffer)
        {
            DamageInfo.WeaponId = buffer.ReadInt32();
            DamageInfo.AttackerEntityId = buffer.ReadInt32();
            DamageInfo.VictimEntityId = buffer.ReadInt32();
            DamageInfo.Amount = buffer.ReadInt32();
            DamageInfo.ResultedInDeath = buffer.ReadBoolean();
            DamageInfo.HitNormal = buffer.ReadVector3();
            DamageInfo.HitPoint = buffer.ReadVector3();
            DamageInfo.Viewpunch = buffer.ReadSingle();
            DamageInfo.HitArea = (HitboxArea)buffer.ReadByte();
            DamageInfo.DamageType = (DamageType)buffer.ReadByte();
        }

        public void Reset()
        {
            DamageInfo = new DamageInfo();
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(DamageInfo.WeaponId);
            buffer.Write(DamageInfo.AttackerEntityId);
            buffer.Write(DamageInfo.VictimEntityId);
            buffer.Write(DamageInfo.Amount);
            buffer.Write(DamageInfo.ResultedInDeath);
            buffer.Write(DamageInfo.HitNormal);
            buffer.Write(DamageInfo.HitPoint);
            buffer.Write(DamageInfo.Viewpunch);
            buffer.Write((byte)DamageInfo.HitArea);
            buffer.Write((byte)DamageInfo.DamageType);
        }
    }
}

