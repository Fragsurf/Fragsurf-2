using System;
using System.Collections.Generic;
using System.Net;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;

namespace Fragsurf.Server
{
    public abstract class BaseSocket
    {

        public BaseSocket(SocketManager socketMan)
        {
            SocketMan = socketMan;
        }

        protected readonly SocketManager SocketMan;
        public ServerStatus SocketStatus { get; private set; }

        public Action<ServerStatus> OnSocketStatusChanged;

        //public virtual void Update() { }
        public abstract void Tick();
        public abstract void DisconnectPlayer(BasePlayer player, string reason);
        public abstract void SendPacket(BasePlayer player, IBasePacket packet);
        public abstract void SendPacket(List<BasePlayer> players, IBasePacket packet);
        public abstract void StartSocket();
        public abstract void StopSocket(string reason);
        public abstract void SendUnconnectedData(byte[] data, IPEndPoint endpoint);

        public void SetSocketStatus(ServerStatus status)
        {
            if(status == SocketStatus)
            {
                return;
            }

            SocketStatus = status;
            OnSocketStatusChanged?.Invoke(status);
        }

        private void RaiseSocketStatusChangedEvent(ServerStatus newStatus)
        {
            OnSocketStatusChanged?.Invoke(newStatus);
        }

    }
}
