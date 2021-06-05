using Fragsurf.Shared.Packets;
using LiteNetLib;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using NetBuffer = Lidgren.Network.NetBuffer; 

namespace Fragsurf.Client
{
    public class ClientLNLSocket : ClientBaseSocket
    {

        private NetManager _client;
        private float _latency;
        private NetBuffer _writeBuffer = new NetBuffer();
        private NetBuffer _readBuffer = new NetBuffer();

        public ClientLNLSocket(ClientSocketManager socketMan)
            : base(socketMan)
        {
            _writeBuffer.LengthBytes = 10000;
            _readBuffer.LengthBytes = 10000;
        }

        public override void Connect(string host, int port)
        {
            if(_client != null)
            {
                Disconnect(string.Empty);
            }
            var listener = new EventBasedNetListener();
            _client = new NetManager(listener);
            _client.ChannelsCount = 32;
            _client.Start();
            _client.Connect(host, port, "Fragsurf");
            _client.DisconnectTimeout = 30000;
            listener.NetworkLatencyUpdateEvent += Listener_NetworkLatencyUpdateEvent;
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            listener.NetworkErrorEvent += Listener_NetworkErrorEvent;
            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;
        }

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            SocketMan.HandleDisconnected(disconnectInfo.Reason.ToString());
        }

        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            SocketMan.HandleConnected();
        }

        private void Listener_NetworkErrorEvent(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError)
        {
            Debug.LogError(socketError.ToString());
        }

        public override void Disconnect(string reason)
        {
            _client?.Stop();
            _client = null;
        }

        public override float GetRoundTripTime()
        {
            return _latency;
        }

        public override void SendPacket(IBasePacket packet)
        {
            if(_client == null || !_client.IsRunning)
            {
                return;
            }

            _writeBuffer.Position = 0;
            _writeBuffer.LengthBytes = 0;
            SocketMan.WritePacketHeader(packet, _writeBuffer);
            packet.Write(_writeBuffer);

            var dm = DeliveryMethod.ReliableOrdered;
            switch (packet.Sc.DeliveryMethod)
            {
                case Lidgren.Network.NetDeliveryMethod.ReliableOrdered:
                    dm = DeliveryMethod.ReliableOrdered;
                    break;
                case Lidgren.Network.NetDeliveryMethod.ReliableSequenced:
                    dm = DeliveryMethod.ReliableSequenced;
                    break;
                case Lidgren.Network.NetDeliveryMethod.ReliableUnordered:
                    dm = DeliveryMethod.ReliableUnordered;
                    break;
                case Lidgren.Network.NetDeliveryMethod.Unreliable:
                case Lidgren.Network.NetDeliveryMethod.UnreliableSequenced:
                    dm = DeliveryMethod.Unreliable;
                    break;
                case Lidgren.Network.NetDeliveryMethod.Unknown:
                    dm = DeliveryMethod.Unreliable;
                    break;
            }

            _client.SendToAll(_writeBuffer.Data, 0, _writeBuffer.LengthBytes, (byte)packet.Sc.SequenceChannel, dm);
        }

        public override void Tick()
        {
            _client?.PollEvents();
        }

        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            _readBuffer.LengthBytes = reader.UserDataSize;
            _readBuffer.Position = 0;
            reader.GetBytes(_readBuffer.Data, reader.UserDataSize);
            SocketMan.HandleIncomingData(_readBuffer);
            reader.Recycle();
        }

        private void Listener_NetworkLatencyUpdateEvent(NetPeer peer, int latency)
        {
            _latency = latency / 1000f;
        }

    }
}

