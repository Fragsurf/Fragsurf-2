using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Fragsurf.Shared;
using Fragsurf.Shared.Packets;
using Steamworks;
using Steamworks.Data;
using Lidgren.Network;
using System.Net;
using Fragsurf.Shared.Player;

namespace Fragsurf.Server
{
    public class SteamSocketInterface : Steamworks.SocketManager
    {

        public Action<Connection, ConnectionInfo> _OnConnectionChanged;
        public Action<Connection, ConnectionInfo> _OnConnected;
        public Action<Connection, ConnectionInfo> _OnConnecting;
        public Action<Connection, ConnectionInfo> _OnDisconnected;
        public Action<Connection, NetIdentity, IntPtr, int, long, long, int> _OnMessage;

        public override void OnConnectionChanged(Connection connection, ConnectionInfo data)
        {
            base.OnConnectionChanged(connection, data);

            _OnConnectionChanged?.Invoke(connection, data);
        }

        public override void OnConnected(Connection connection, ConnectionInfo data)
        {
            base.OnConnected(connection, data);

            _OnConnected?.Invoke(connection, data);
        }

        public override void OnConnecting(Connection connection, ConnectionInfo data)
        {
            base.OnConnecting(connection, data);

            connection.UserData = (long)(ulong)data.Identity.SteamId;

            _OnConnecting?.Invoke(connection, data);
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo data)
        {
            base.OnDisconnected(connection, data);

            _OnDisconnected?.Invoke(connection, data);
        }

        public override unsafe void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);

            _OnMessage?.Invoke(connection, identity, data, size, messageNum, recvTime, channel);
        }

        public void SendMessage(ulong steamId, byte[] data, int length, SendType type = SendType.Reliable)
        {
            Connection? connection = FindConnection(steamId);
            if (connection.HasValue)
            {
                connection.Value.SendMessage(data, 0, length, type);
            }
        }

        public void SendMessage(IEnumerable<ulong> steamIds, byte[] data, int length, SendType type = SendType.Reliable)
        {
            foreach(var id in steamIds)
            {
                SendMessage(id, data, length, type);
            }
        }

        public void Disconnect(ulong steamId, int reason)
        {
            Connection? connection = FindConnection(steamId);
            if(connection.HasValue)
            {
                UnityEngine.Debug.Log("Disconnect: " + reason);
                connection.Value.Close(false, reason);
            }
        }

        private Connection? FindConnection(ulong steamid)
        {
            foreach(var con in Connected)
            {
                if(con.UserData == (long)steamid)
                {
                    return con;
                }
            }
            return null;
        }

    }

    public class SteamP2PSocket : BaseSocket
    {

        private SteamSocketInterface _socket;
        private Dictionary<ulong, int> _steamidToClientIndex = new Dictionary<ulong, int>();

        public SteamP2PSocket(SocketManager socketMan) 
            : base(socketMan)
        {
            _buffer.Data = new byte[10000];
        }

        private NetBuffer _buffer = new NetBuffer();

        public override void DisconnectPlayer(BasePlayer player, string reason)
        {
            Enum.TryParse(reason, out DenyReason dr);
            _socket.Disconnect(player.SteamId, (int)dr);
        }

        public override void SendPacket(BasePlayer player, IBasePacket packet)
        {
            SendPacket(player.SteamId, packet);
        }

        public override void SendPacket(List<BasePlayer> players, IBasePacket packet)
        {
            GetP2PPacket(packet, out byte[] data, out int dataLength, out int channel, out SendType type);

            foreach (var player in SocketMan.Game.PlayerManager.Players)
            {
                if (player.Introduced)
                {
                    SendPacket(player.SteamId, data, dataLength, type);
                }
            }
        }

        private NetBuffer _recvBuffer = new NetBuffer() { Data = new byte[1024 * 64] };
        public override void StartSocket()
        {
            if (_socket != null)
            {
                StopSocket("Starting new socket");
            }

            _socket = SteamNetworkingSockets.CreateRelaySocket<SteamSocketInterface>();

            _socket._OnConnected = (connection, connectionInfo) =>
            {
                var pl = SocketMan.CreatePlayer();
                _steamidToClientIndex[connectionInfo.Identity.SteamId] = pl.ClientIndex;
            };

            _socket._OnConnecting = (connection, connectionInfo) =>
            {
                connection.Accept();
            };

            _socket._OnDisconnected = (connection, connectionInfo) =>
            {
                SocketMan.HandlePlayerDisconnected(GetClientIndex(connectionInfo.Identity.SteamId));
            };

            _socket._OnMessage = (connection, id, data, size, msgNum, recvTime, channel) =>
             {
                 if(_recvBuffer.Data.Length < size)
                 {
                     _recvBuffer.Data = new byte[size];
                 }
                 _recvBuffer.Position = 0;
                 _recvBuffer.LengthBytes = size;
                 Marshal.Copy(data, _recvBuffer.Data, 0, size);
                 SocketMan.HandleIncomingData2(GetClientIndex((SteamId)id), _recvBuffer);
             };

            SetSocketStatus(ServerStatus.AcceptingConnections);
        }

        private int GetClientIndex(ulong steamid)
        {
            if (_steamidToClientIndex.ContainsKey(steamid))
            {
                return _steamidToClientIndex[steamid];
            }
            return 0;
        }

        public override void StopSocket(string reason)
        {
            if(_socket == null)
            {
                return;
            }
            _socket._OnConnected = null;
            _socket._OnConnecting = null;
            _socket._OnDisconnected = null;
            _socket._OnMessage = null;
            try { _socket.Close(); } catch { }
            _socket = null;
            SetSocketStatus(ServerStatus.Shutdown);
        }

        public override void Tick()
        {
            if(SteamServer.IsValid
                && _socket != null)
            {
                _socket.Receive();
            }
        }

        private void DisconnectPlayer(ulong steamid, string reason)
        {
            Enum.TryParse(reason, out DenyReason dr);
            _socket.Disconnect(steamid, (int)dr);
            _steamidToClientIndex.Remove(steamid);
        }

        private void SendPacket(ulong steamid, IBasePacket packet)
        {
            GetP2PPacket(packet, out byte[] data, out int dataLength, out int channel, out SendType type);
            SendPacket(steamid, data, dataLength, type);
        }

        private void SendPacket(ulong steamid, byte[] data, int dataLength, SendType type)
        {
            _socket.SendMessage(steamid, data, dataLength, type);
        }

        public override void SendUnconnectedData(byte[] data, IPEndPoint endpoint)
        {
            // do nothing
        }

        private void GetP2PPacket(IBasePacket packet, out byte[] data, out int dataLength, out int channel, out SendType send)
        {
            _buffer.Position = 0;
            _buffer.LengthBytes = 0;

            SocketMan.WritePacketHeader(_buffer, packet);
            packet.Write(_buffer);

            send = SendType.Unreliable;
            switch (packet.Sc.DeliveryMethod)
            {
                case NetDeliveryMethod.ReliableSequenced:
                case NetDeliveryMethod.ReliableOrdered:
                case NetDeliveryMethod.ReliableUnordered:
                    send = SendType.Reliable;
                    break;
                case NetDeliveryMethod.Unreliable:
                case NetDeliveryMethod.UnreliableSequenced:
                    send = SendType.Unreliable;
                    break;
            }

            data = _buffer.Data;
            dataLength = _buffer.LengthBytes;
            //channel = packet.Sc.SequenceChannel;
            channel = 1;

            if (packet.Sc == SendCategory.FileTransfer)
            {
                send = SendType.Reliable;
                channel = 2;
            }
        }

    }
}

