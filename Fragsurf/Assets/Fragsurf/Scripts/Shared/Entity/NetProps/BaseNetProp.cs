using System;
using System.Reflection;
using System.Collections.Generic;
using Lidgren.Network;

namespace Fragsurf.Shared.Entity
{
    public abstract class BaseNetProp
    {

        public readonly IHasNetProps Entity;
        public readonly NetPropertyAttributeData PropAttr;

        protected bool CanSet => !(PropAttr.NetPropAttribute.SkipIfAuthority && Entity.HasAuthority);

        public BaseNetProp(IHasNetProps instance, NetPropertyAttributeData propAttr)
        {
            Entity = instance;
            PropAttr = propAttr;
        }

        public abstract void Read(NetBuffer buffer);
        public abstract void Write(NetBuffer buffer);
        public abstract bool Differs();
        public abstract void StoreValue();

        private static Dictionary<Type, List<NetPropertyAttributeData>> _attributeCache
            = new Dictionary<Type, List<NetPropertyAttributeData>>();

        public static List<NetPropertyAttributeData> GetAttributeData(Type t)
        {
            if (!_attributeCache.ContainsKey(t))
            {
                _attributeCache[t] = new List<NetPropertyAttributeData>();
                foreach (PropertyInfo p in t.GetProperties())
                {
                    foreach (Attribute a in p.GetCustomAttributes(true))
                    {
                        if (a is NetPropertyAttribute np)
                        {
                            var attrData = new NetPropertyAttributeData(p, np);
                            _attributeCache[t].Add(attrData);
                        }
                    }
                }
            }
            return _attributeCache[t];
        }

        public static BaseNetProp GetNetProp(IHasNetProps instance, NetPropertyAttributeData propAttr)
        {
            if (propAttr.PropertyType.IsUnmanaged())
            {
                var genericType = typeof(UnmanagedNetProp<>).MakeGenericType(propAttr.PropertyType);
                return Activator.CreateInstance(genericType, instance, propAttr) as BaseNetProp;
            }
            else if(propAttr.PropertyType == typeof(string))
            {
                return new StringNetProp(instance, propAttr);
            }
            return null;
        }

        public class NetPropertyAttributeData
        {
            public NetPropertyAttributeData(PropertyInfo pi, NetPropertyAttribute netPropAttr)
            {
                NetPropAttribute = netPropAttr;
                PropertyInfo = pi;
                Getter = pi.GetGetMethod();
                Setter = pi.GetSetMethod();
                PropertyType = pi.PropertyType;
            }
            public readonly NetPropertyAttribute NetPropAttribute;
            public readonly PropertyInfo PropertyInfo;
            public readonly MethodInfo Getter;
            public readonly MethodInfo Setter;
            public readonly Type PropertyType;
        }
    }
}
