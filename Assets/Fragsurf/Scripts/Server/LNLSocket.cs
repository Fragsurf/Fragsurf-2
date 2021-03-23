using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using NetBuffer = Lidgren.Network.NetBuffer;

namespace Fragsurf.Server
{
    public class LNLSocket : BaseSocket
    {

        private NetManager _server;
        private NetBuffer _writeBuffer = new NetBuffer();
        private NetBuffer _readBuffer = new NetBuffer();

        public LNLSocket(SocketManager socketMan)
            : base(socketMan)
        {
            _writeBuffer.LengthBytes = 10000;
            _readBuffer.LengthBytes = 10000;
        }

        public override void Tick()
        {
            _server?.PollEvents();
        }

        private bool FindPeer(BasePlayer player, out NetPeer result)
        {
            result = null;
            foreach(var peer in _server.ConnectedPeerList)
            {
                if(peer.Tag == player)
                {
                    result = peer;
                    return true;
                }
            }
            return false;
        }

        public override void DisconnectPlayer(BasePlayer player, string reason)
        {
            if(FindPeer(player, out NetPeer peer))
            {
                peer.Disconnect();
            }
        }

        public override void SendPacket(BasePlayer player, IBasePacket packet)
        {
            if(FindPeer(player, out NetPeer peer))
            {
                _SendPacket(peer, packet);
            }
        }

        public override void SendPacket(List<BasePlayer> players, IBasePacket packet)
        {
            foreach(var player in players)
            {
                SendPacket(player, packet);
            }
        }

        public override void StartSocket()
        {
            StopSocket("Shutdown");
            var listener = new EventBasedNetListener();
            _server = new NetManager(listener);
            _server.ChannelsCount = 32;
            _server.Start(SocketMan.GameplayPort);
            listener.ConnectionRequestEvent += Listener_ConnectionRequestEvent;
            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
            listener.NetworkErrorEvent += Listener_NetworkErrorEvent;
            SetSocketStatus(ServerStatus.AcceptingConnections);
        }


        private void Listener_NetworkErrorEvent(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError)
        {
            Debug.LogError(socketError.ToString());
        }

        public override void StopSocket(string reason)
        {
            SetSocketStatus(ServerStatus.Shutdown);
            _server?.Stop();
            _server = null;
        }

        public override void SendUnconnectedData(byte[] data, IPEndPoint endpoint)
        {
            throw new NotImplementedException();
        }

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var pl = peer.Tag as BasePlayer;
            if(pl == null)
            {
                throw new Exception("BasePlayer tag undefined on disconnect");
            }
            SocketMan.HandlePlayerDisconnected(pl.ClientIndex);
        }

        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            peer.Tag = SocketMan.CreatePlayer();
        }

        private void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            if(_server.ConnectedPeersCount < SocketMan.MaxPlayers)
            {
                request.AcceptIfKey("Fragsurf");
            }
            else
            {
                request.Reject();
            }
        }

        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var pl = peer.Tag as BasePlayer;
            if(pl == null)
            {
                Debug.LogError("BasePlayer tag is undefined on receive event, disconnecting peer.");
                peer.Disconnect();
                return;
            }
            _readBuffer.LengthBytes = reader.UserDataSize;
            _readBuffer.Position = 0;
            reader.GetBytes(_readBuffer.Data, reader.UserDataSize);
            SocketMan.HandleIncomingData2(pl.ClientIndex, _readBuffer);
            reader.Recycle();
        }

        private void _SendPacket(NetPeer peer, IBasePacket packet)
        {
            if (_server == null 
                || !_server.IsRunning 
                || peer == null 
                || peer.ConnectionState != ConnectionState.Connected)
            {
                return;
            }

            _writeBuffer.Position = 0;
            _writeBuffer.LengthBytes = 0;
            SocketMan.WritePacketHeader(_writeBuffer, packet);
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

            peer.Send(_writeBuffer.Data, 0, _writeBuffer.LengthBytes, (byte)packet.Sc.SequenceChannel, dm);
        }

    }
}

