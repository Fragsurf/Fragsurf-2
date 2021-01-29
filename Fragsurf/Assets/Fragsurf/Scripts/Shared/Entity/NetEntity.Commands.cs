using System.Reflection;
using System.Collections.Generic;
using Fragsurf.Shared.Packets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Fragsurf.Shared.Entity
{
    public partial class NetEntity
    {

        private void SendCommand(string name, params object[] args)
        {
            var packet = PacketUtility.TakePacket<EntityCommand>();

            packet.CommandName = name;
            packet.EntityId = EntityId;
            packet.ArgsJson = JsonConvert.SerializeObject(args, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            Game.Network.BroadcastPacket(packet);
        }

        public void ReceiveCommand(EntityCommand command)
        {
            var args = new List<object>();
            var jarr = JArray.Parse(command.ArgsJson);
            var method = GetType().GetMethod(command.CommandName, 
                BindingFlags.Default 
                | BindingFlags.Public 
                | BindingFlags.NonPublic 
                | BindingFlags.Instance);

            if(method == null)
            {
                return;
            }

            var p = method.GetParameters();
            for(int i = 0; i < p.Length; i++)
            {
                var obj = JsonConvert.DeserializeObject(jarr[i].ToString(), p[i].ParameterType);
                args.Add(obj);
            }

            method.Invoke(this, args.ToArray());
        }

        protected void NetCommand(Action action)
        {
            SendCommand(action.Method.Name);
        }

        protected void NetCommand<T>(Action<T> action, T param)
        {
            SendCommand(action.Method.Name, param);
        }

        protected void NetCommand<T, T2>(Action<T, T2> action, T param, T2 param2)
        {
            SendCommand(action.Method.Name, param, param2);
        }

        protected void NetCommand<T, T2, T3>(Action<T, T2, T3> action, T param, T2 param2, T3 param3)
        {
            SendCommand(action.Method.Name, param, param2, param3);
        }

    }
}

