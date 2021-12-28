using System;
using System.Runtime.InteropServices;
using Fragsurf.Shared;
using Fragsurf.Shared.Packets;
using Steamworks;
using Steamworks.Data;
using Fragsurf.Network;

namespace Fragsurf.Client
{
    public class SteamConnectionManager : Steamworks.ConnectionManager
    {

        public Action<ConnectionInfo> _OnConnected;
        public Action<ConnectionInfo> _OnDisconnected;
        public Action<ConnectionInfo> _OnConnectionChanged;
        public Action<IntPtr, int, long, long, int> _OnMessage;

        public override void OnConnected(ConnectionInfo data)
        {
            base.OnConnected(data);

            _OnConnected?.Invoke(data);
        }

        public override void OnDisconnected(ConnectionInfo data)
        {
            base.OnDisconnected(data);

            _OnDisconnected?.Invoke(data);
        }

        public override void OnConnectionChanged(ConnectionInfo info)
        {
            base.OnConnectionChanged(info);

            _OnConnectionChanged?.Invoke(info);
        }

        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            base.OnMessage(data, size, messageNum, recvTime, channel);

            _OnMessage?.Invoke(data, size, messageNum, recvTime, channel);
        }

    }

    public class ClientSteamP2PSocket : ClientBaseSocket
    {

        private SteamConnectionManager _connection;
        private NetBuffer _buffer = new NetBuffer();
        private NetBuffer _readBuffer = new NetBuffer();

        public ClientSteamP2PSocket(ClientSocketManager socketMan)
            : base(socketMan)
        {
            _buffer.Data = new byte[10000];
            _readBuffer.Data = new byte[10000];
        }

        public override void Connect(string host, int port)
        {
            if(_connection != null)
            {
                Disconnect("Connecting to new host");
            }

            if (ulong.TryParse(host, out ulong steamid))
            {
                _connection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(steamid);

                _connection._OnConnected = (connection) =>
                {
                    SocketMan.HandleConnected();
                };

                _connection._OnDisconnected = (connection) =>
                {
                    if ((int)connection.EndReason == 1999)
                    {
                        SocketMan.HandleDisconnected(DenyReason.MapChange.ToString());
                    }
                    else
                    {
                        SocketMan.HandleDisconnected(connection.State.ToString());
                    }
                };

                _connection._OnMessage = (data, size, msgNum, recvTime, channel) =>
                {
                    _readBuffer.LengthBytes = size;
                    _readBuffer.Position = 0;
                    Marshal.Copy(data, _readBuffer.Data, 0, size);
                    SocketMan.HandleIncomingData(_readBuffer);
                };

                _connection._OnConnectionChanged = (info) =>
                {
                    if (info.State == ConnectionState.ProblemDetectedLocally)
                    {
                        _connection?.Close();
                        SocketMan.SetSocketStatus(ClientSocketStatus.ProblemDetected);
                    }
                };
            }
        }

        public override void Disconnect(string reason)
        {
            if(_connection == null)
            {
                return;
            }
            _connection.Close();
            _connection._OnConnected = null;
            _connection._OnDisconnected = null;
            _connection._OnMessage = null;
            _connection._OnConnectionChanged = null;
            _connection = null;
            SocketMan.SetSocketStatus(ClientSocketStatus.Disconnected, reason);
        }

        public override float GetRoundTripTime()
        {
            throw new NotImplementedException();
        }

        public override void SendPacket(IBasePacket packet)
        {
            GetP2PPacket(packet, out byte[] data, out int length, out int channel, out SendType type);
            _connection.Connection.SendMessage(data, 0, length, type);
        }

        public override void Tick()
        {
            _connection?.Receive();
        }

        public override void SetFakeLag(int milliseconds)
        {
            SteamNetworkingUtils.FakeRecvPacketLag = milliseconds;
        }

        public override void SetFakeLoss(int percent)
        {
            SteamNetworkingUtils.FakeRecvPacketLoss = percent;
        }

        private void GetP2PPacket(IBasePacket packet, out byte[] data, out int length, out int channel, out SendType send)
        {
            _buffer.Position = 0;
            _buffer.LengthBytes = 0;

            SocketMan.WritePacketHeader(packet, _buffer);
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
            length = _buffer.LengthBytes;
            channel = packet.Sc.SequenceChannel;
        }

    }
}


