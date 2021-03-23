using Fragsurf.Shared.Packets;

namespace Fragsurf.Client
{
    public abstract class ClientBaseSocket
    {

        public ClientBaseSocket(ClientSocketManager socketMan)
        {
            SocketMan = socketMan;
        }

        protected readonly ClientSocketManager SocketMan;

        public abstract float GetRoundTripTime();
        public abstract void Connect(string host, int port);
        public abstract void Disconnect(string reason);
        public abstract void Tick();
        public abstract void SendPacket(IBasePacket packet);
        public virtual void Update() { }

        public virtual void SetFakeLag(int milliseconds) { }
        public virtual void SetFakeLoss(int percent) { }
    }
}

