using Lidgren.Network;
using Fragsurf.Shared.Packets;
using Steamworks;

namespace Fragsurf.Client
{
    public class ClientLidgrenSocket : ClientBaseSocket
    {

        public ClientLidgrenSocket(ClientSocketManager socketMan)
            : base(socketMan)
        { }

        private NetClient _client;
        
        public override void Connect(string host, int port)
        {
            if(_client != null)
            {
                Disconnect();
                // wait a frame for lidgren to fully shutdown?
                // or subscribe to an event
            }

            var config = new NetPeerConfiguration("fragsurf");
            config.MaximumHandshakeAttempts = 5;
            config.ResendHandshakeInterval = 3f;
            _client = new NetClient(config);
            _client.Start();

            var hail = _client.CreateMessage();
            hail.Write(SteamClient.IsValid ? SteamClient.SteamId : 0);
            _client.Connect(host, port, hail);
        }

        public override float GetRoundTripTime()
        {
            if (_client == null)
            {
                return 0f;
            }
            return _client.Connections[0].AverageRoundtripTime;
        }

        public override void Disconnect(string reason = "Client disconnected")
        {
            _client?.Disconnect(reason);
            _client = null;
        }

        public override void SendPacket(IBasePacket packet)
        {
            if(_client == null)
            {
                return;
            }

            var om = _client.CreateMessage();
            SocketMan.WritePacketHeader(packet, om);
            packet.Write(om);
            _client.SendMessage(om, packet.Sc.DeliveryMethod, packet.Sc.SequenceChannel);
        }

        public override void Tick()
        {
            if (_client == null)
            {
                return;
            }

            NetIncomingMessage im;
            while ((im = _client.ReadMessage()) != null)
            {
                if (_client.Status == NetPeerStatus.Running)
                {
                    switch (im.MessageType)
                    {
                        case NetIncomingMessageType.Data:
                            SocketMan.HandleIncomingData(im);
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            var status = (NetConnectionStatus)im.ReadByte();
                            string reason = null;
                            switch(status)
                            {
                                case NetConnectionStatus.Connected:
                                    SocketMan.HandleConnected();
                                    break;
                                case NetConnectionStatus.Disconnected:
                                    reason = im.ReadString();
                                    SocketMan.HandleDisconnected(reason);
                                    _client?.Shutdown(reason);
                                    _client = null;
                                    break;
                            }
                            break;
                        case NetIncomingMessageType.ErrorMessage:
                            UnityEngine.Debug.LogError(im.ReadString());
                            break;
                    }
                }

                if (_client == null || _client.Status == NetPeerStatus.ShutdownRequested)
                {
                    break;
                }

                _client.Recycle(im);
            }
        }

    }
}

