using System;
using Lidgren.Network;

namespace Fragsurf.Shared.Entity
{
    public class UnmanagedNetProp<T> : BaseNetProp
        where T : unmanaged, IEquatable<T>
    {

        private T _lastKnownValue;
        private readonly Func<T> _get;
        private readonly Action<T> _set;
        private bool _initialized;
        private static byte[] _buffer = new byte[1024];

        public UnmanagedNetProp(IHasNetProps instance, NetPropertyAttributeData data)
            : base(instance, data)
        {
            _get = data.Getter.CreateDelegate(typeof(Func<T>), instance) as Func<T>;
            _set = data.Setter.CreateDelegate(typeof(Action<T>), instance) as Action<T>;
        }

        public override bool Differs()
        {
            if (!_initialized)
            {
                StoreValue();
                _initialized = true;
                return true;
            }
            return !_get().Equals(_lastKnownValue);
        }

        public override unsafe void Read(NetBuffer buffer)
        {
            var sz = sizeof(T);

            if(_buffer.Length < sz)
            {
                _buffer = new byte[sz];
            }

            Buffer.BlockCopy(buffer.Data, buffer.PositionInBytes, _buffer, 0, sz);
            buffer.Position += sz * 8;

            T readValue = Serializer.Deserialize<T>(_buffer);
            if ((!readValue.Equals(_lastKnownValue) || !_initialized) && CanSet)
            {
                _initialized = true;
                _set?.Invoke(readValue);
                StoreValue();
            }
        }

        public override void Write(NetBuffer buffer)
        {
            buffer.Write(Serializer.Serialize(_get()));
            StoreValue();
        }

        public override void StoreValue()
        {
            _lastKnownValue = _get();
        }
    }
}