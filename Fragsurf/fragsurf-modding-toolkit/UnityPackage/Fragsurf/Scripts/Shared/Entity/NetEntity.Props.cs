using System.Collections.Generic;
using Lidgren.Network;

namespace Fragsurf.Shared.Entity
{
    public partial class NetEntity : IHasNetProps
    {

        private Dictionary<byte, BaseNetProp> _props;

        public void GetPropData(out int sz, out byte[] buffer)
        {
            var tempBuffer = new NetBuffer();
            tempBuffer.Position = 0;

            foreach (var kvp in _props)
            {
                tempBuffer.Write(kvp.Key);
                kvp.Value.Write(tempBuffer);
            }

            sz = tempBuffer.LengthBytes;
            buffer = tempBuffer.Data;
        }

        public void LoadPropData(byte[] propData)
        {
            var tempBuffer = new NetBuffer();
            tempBuffer.Data = propData;
            tempBuffer.LengthBytes = propData.Length;
            tempBuffer.Position = 0;

            for(int i = 0; i < _props.Count; i++)
            {
                var propIdx = tempBuffer.ReadByte();
                _props[propIdx].Read(tempBuffer);
            }
        }

        private byte _netPropIndex;
        public void BuildNetProps(IHasNetProps objectInstance)
        {
            if (_props == null)
            {
                _props = new Dictionary<byte, BaseNetProp>();
            }

            var t = objectInstance.GetType();
            var propAttrData = BaseNetProp.GetAttributeData(t);

            foreach (var propAttr in propAttrData)
            {
                var netProp = BaseNetProp.GetNetProp(objectInstance, propAttr);
                if (netProp != null)
                {
                    _props.Add(_netPropIndex++, netProp);
                }
            }
        }

        public bool PropertyHasChanged()
        {
            foreach (var prop in _props)
            {
                if (prop.Value.Differs())
                {
                    return true;
                }
            }
            return false;
        }

    }
}

