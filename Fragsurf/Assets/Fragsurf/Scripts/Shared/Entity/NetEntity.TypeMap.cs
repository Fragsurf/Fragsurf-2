using Fragsurf.Utility;
using System;
using System.Collections.Generic;

namespace Fragsurf.Shared.Entity
{
    public partial class NetEntity
    {

        private static Dictionary<byte, Type> _entityTypeMap;
        public static byte GetEntityTypeId(Type type)
        {
            if (_entityTypeMap == null)
            {
                RebuildEntityTypeMap();
            }
            foreach (var kvp in _entityTypeMap)
            {
                if (type == kvp.Value)
                {
                    return kvp.Key;
                }
            }
            return 0;
        }

        public static NetEntity CreateInstanceOfEntity(FSGameLoop game, byte typeId)
        {
            if (_entityTypeMap == null)
            {
                RebuildEntityTypeMap();
            }
            if (!_entityTypeMap.ContainsKey(typeId))
            {
                return null;
            }
            return Activator.CreateInstance(_entityTypeMap[typeId], args: game) as NetEntity;
        }

        public static void RebuildEntityTypeMap()
        {
            _entityTypeMap = new Dictionary<byte, Type>();
            byte index = 0;
            foreach (var t in ReflectionExtensions.GetTypesOf<NetEntity>())
            {
                _entityTypeMap.Add(index, t);
                index++;
            }
        }

    }
}

