using Lidgren.Network;
using System;

namespace Fragsurf.Shared.Entity
{
    public class StringNetProp : BaseNetProp
    {

        private string _lastKnownValue;
        private readonly Func<string> _get;
        private readonly Action<string> _set;
        private bool _initialized;

        public StringNetProp(IHasNetProps instance, NetPropertyAttributeData data)
            : base(instance, data)
        {
            _get = data.Getter.CreateDelegate(typeof(Func<string>), instance) as Func<string>;
            _set = data.Setter.CreateDelegate(typeof(Action<string>), instance) as Action<string>;
        }

        public override bool Differs()
        {
            if (!_initialized)
            {
                StoreValue();
                _initialized = true;
                return true;
            }
            return _get() != _lastKnownValue;
        }

        public override void Read(NetBuffer buffer)
        {
            _set?.Invoke(buffer.ReadString());
            StoreValue();
        }

        public override void StoreValue()
        {
            _lastKnownValue = _get();
        }

        public override void Write(NetBuffer buffer)
        {
            buffer.Write(_get() ?? string.Empty);
            StoreValue();
        }
    }
}