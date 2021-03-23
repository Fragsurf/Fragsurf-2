using System.Collections.Generic;
using Fragsurf.Shared.Packets;
using Fragsurf.Movement;
using Lidgren.Network;
using MessagePack;

namespace Fragsurf.Gamemodes.Tricksurf
{
    [MessagePackObject]
    public struct TrickCompletion : IBasePacket
    {
        [IgnoreMember] public int CompletionId;
        [IgnoreMember] public int ClientIndex;
        [Key(0)] public int TrickId;
        [Key(1)] public string TrickName;
        [IgnoreMember] public int Points;
        [Key(2)] public int AverageVelocity;
        [Key(3)] public float CompletionTime;
        [Key(4)] public MoveStyle Style;
        [Key(5)] public List<TouchInfo> Touches;
        [IgnoreMember] public int ComboCount;
        [IgnoreMember] public int StartTick;
        [IgnoreMember] public int EndTick;

        [IgnoreMember] public SendCategory Sc => SendCategory.UI_Important;
        [IgnoreMember] public bool DisableAutoPool => true;
        [IgnoreMember] public int ByteSize => 0;

        public void Reset()
        {
            CompletionId = default;
            ClientIndex = default;
            TrickId = default;
            TrickName = default;
            Points = default;
            AverageVelocity = default;
            CompletionTime = default;
            Style = default;
            Touches = default;
            StartTick = default;
            EndTick = default;
            ComboCount = default;
        }

        public void Read(NetBuffer buffer)
        {
            CompletionId = buffer.ReadInt32();
            ClientIndex = buffer.ReadInt32();
            TrickId = buffer.ReadInt32();
            TrickName = buffer.ReadString();
            Points = buffer.ReadInt32();
            AverageVelocity = buffer.ReadInt32();
            CompletionTime = buffer.ReadSingle();
            Style = (MoveStyle)buffer.ReadByte();
            StartTick = buffer.ReadInt32();
            EndTick = buffer.ReadInt32();
            ComboCount = buffer.ReadInt32();
            var touchCount = buffer.ReadInt32();
            if (touchCount > 0)
            {
                Touches = new List<TouchInfo>(touchCount);
                for(int i = 0; i < touchCount; i++)
                {
                    var info = new TouchInfo()
                    {
                        Style = (MoveStyle)buffer.ReadByte(),
                        TriggerId = buffer.ReadInt32(),
                        Time = buffer.ReadSingle(),
                        Velocity = buffer.ReadVector3()
                    };
                    Touches.Add(info);
                }
            }
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(CompletionId);
            buffer.Write(ClientIndex);
            buffer.Write(TrickId);
            buffer.Write(TrickName);
            buffer.Write(Points);
            buffer.Write(AverageVelocity);
            buffer.Write(CompletionTime);
            buffer.Write((byte)Style);
            buffer.Write(StartTick);
            buffer.Write(EndTick);
            buffer.Write(ComboCount);
            buffer.Write(Touches != null ? Touches.Count : 0);
            if(Touches != null)
            {
                foreach(var touch in Touches)
                {
                    buffer.Write((byte)touch.Style);
                    buffer.Write(touch.TriggerId);
                    buffer.Write(touch.Time);
                    buffer.Write(touch.Velocity);
                }
            }
        }
    }
}
