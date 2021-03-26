using Lidgren.Network;

namespace Fragsurf.Shared.Packets
{

    public struct SendCategory
    {
        public static SendCategory Unreliable = new SendCategory()
        {
            DeliveryMethod = NetDeliveryMethod.Unreliable,
            SequenceChannel = 0
        };

        public static SendCategory UI_Important = new SendCategory()
        {
            DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
            SequenceChannel = 1
        };

        public static SendCategory Gameplay = new SendCategory()
        {
            DeliveryMethod = NetDeliveryMethod.UnreliableSequenced,
            SequenceChannel = 2
        };

        public static SendCategory Gameplay_Important = new SendCategory()
        {
            DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
            SequenceChannel = 3
        };

        public static SendCategory EntityImportant = new SendCategory()
        {
            DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
            SequenceChannel = 4
        };

        public static SendCategory EntityFast = new SendCategory()
        {
            DeliveryMethod = NetDeliveryMethod.UnreliableSequenced,
            SequenceChannel = 5
        };

        public static SendCategory FileTransfer = new SendCategory()
        {
            DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
            SequenceChannel = 6
        };

        public static SendCategory Input = new SendCategory()
        {
            DeliveryMethod = NetDeliveryMethod.ReliableSequenced,
            SequenceChannel = 7
        };

        public static SendCategory Voice = new SendCategory()
        {
            DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
            SequenceChannel = 8
        };

        public NetDeliveryMethod DeliveryMethod;
        public int SequenceChannel;

        public static bool operator ==(SendCategory a, SendCategory b)
        {
            return a.DeliveryMethod == b.DeliveryMethod && a.SequenceChannel == b.SequenceChannel;
        }

        public static bool operator !=(SendCategory a, SendCategory b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return $"{DeliveryMethod} - {SequenceChannel}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is SendCategory))
                return false;
            return this == (SendCategory)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }

    public interface IBasePacket
    {
        SendCategory Sc { get; }
        int ByteSize { get; }
        void Read(NetBuffer buffer);
        void Write(NetBuffer buffer);
        void Reset();
        bool DisableAutoPool { get; }
    }
}

