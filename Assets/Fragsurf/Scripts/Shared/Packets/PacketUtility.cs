using Fragsurf.Utility;
using System;
using System.Collections.Generic;

namespace Fragsurf.Shared.Packets
{
    public static class PacketUtility
    {
        private static Dictionary<Type, byte> _packetTypes;
        private static Dictionary<byte, Type> _invPacketTypes;
        private static Dictionary<Type, Queue<IBasePacket>> _pool = new Dictionary<Type, Queue<IBasePacket>>();
        private const int maxPoolSize = 100;

        static PacketUtility()
        {
            BuildPacketTypes();
        }

        private static void BuildPacketTypes()
        {
            _packetTypes = new Dictionary<Type, byte>();
            _invPacketTypes = new Dictionary<byte, Type>();

            byte idx = 0;
            foreach(var type in ReflectionExtensions.GetTypesImplementing<IBasePacket>())
            {
                _packetTypes.Add(type, idx);
                _invPacketTypes.Add(idx, type);
                idx++;
            }
        }

        public static byte GetPacketTypeId<T>()
            where T : IBasePacket
        {
            return _packetTypes[typeof(T)];
        }

        public static byte GetPacketTypeId(Type t)
        {
            try
            {
                return _packetTypes[t];
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError("Missing : " + t.Name);
            }
            throw new Exception();
        }

        public static Type GetPacketType(byte id)
        {
            return _invPacketTypes[id];
        }

        public static T TakePacket<T>() where T : IBasePacket
        {
            var t = typeof(T);
            if (_pool.ContainsKey(t) && _pool[t].Count > 0)
            {
                return (T)_pool[t].Dequeue();
            }
            return (T)Activator.CreateInstance(t);
        }

        public static IBasePacket TakePacket(byte typeId)
        {
            var type = GetPacketType(typeId);
            if (_pool.ContainsKey(type) && _pool[type].Count > 0)
            {
                return _pool[type].Dequeue();
            }
            return Activator.CreateInstance(type) as IBasePacket;
        }

        public static void PutPacket(IBasePacket packet)
        {
            if(packet.DisableAutoPool)
            {
                return;
            }

            packet.Reset();

            var t = packet.GetType();
            if(!_pool.ContainsKey(t))
            {
                _pool[t] = new Queue<IBasePacket>(maxPoolSize);
            }
            if(_pool[t].Count < maxPoolSize)
            {
                if (_pool[t].Contains(packet))
                {
                    UnityEngine.Debug.LogException(new System.Exception("Pooling a packet that is already pooled: " + t));
                }
                else
                {
                    _pool[t].Enqueue(packet);
                }
            }
        }

        //public static IBasePacket TakePacket(PacketName name)
        //{
        //    // todo : I shouldn't have to update this for every new packet type
        //    switch (name)
        //    {
        //        case PacketName.PlayerIntroduction:
        //            return TakePacket<PlayerIntroduction>();
        //        case PacketName.UserCmd:
        //            return TakePacket<UserCmd>();
        //        case PacketName.ChooseTeam:
        //            return TakePacket<ChooseTeam>();
        //        case PacketName.ChatMessage:
        //            return TakePacket<ChatMessage>();
        //        case PacketName.PlayerEvent:
        //            return TakePacket<PlayerEvent>();
        //        case PacketName.MapChange:
        //            return TakePacket<MapChange>();
        //        case PacketName.CustomPacket:
        //            return TakePacket<CustomPacket>();
        //        case PacketName.FileSync:
        //            return TakePacket<FileSync>();
        //        case PacketName.FileChunk:
        //            return TakePacket<FileChunk>();
        //        case PacketName.PlaySound:
        //            return TakePacket<PlaySound>();
        //        case PacketName.DeleteEntity:
        //            return TakePacket<DeleteEntity>();
        //        case PacketName.SkinManifest:
        //            return TakePacket<SkinManifest>();
        //        case PacketName.PacketPack:
        //            return TakePacket<PacketPack>();
        //        case PacketName.ConnectionApproval:
        //            return TakePacket<ConnectionApproval>();
        //        case PacketName.Ping:
        //            return TakePacket<Ping>();
        //        case PacketName.CompressedVoiceData:
        //            return TakePacket<CompressedVoiceData>();
        //        case PacketName.DamageEvent:
        //            return TakePacket<DamageEvent>();
        //        case PacketName.EntityCommand:
        //            return TakePacket<EntityCommand>();
        //        default:
        //            return null;
        //    }
        //}
    }
}

