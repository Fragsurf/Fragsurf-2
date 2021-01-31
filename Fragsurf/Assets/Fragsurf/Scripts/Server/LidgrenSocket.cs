using System.Net;
using System.Collections.Generic;
using Fragsurf.Shared.Packets;
using Lidgren.Network;

namespace Fragsurf.Server
{
    public class LidgrenSocket : BaseSocket
    {

        public LidgrenSocket(SocketManager socketMan)
            : base(socketMan)
        {
        }

        private NetServer _server;
        private Dictionary<NetConnection, ulong> _connections = new Dictionary<NetConnection, ulong>();

        public override void Tick()
        {
            if (_server == null)
            {
                return;
            }
            NetIncomingMessage im;
            while ((im = _server.ReadMessage()) != null)
            {
                ulong steamid = 0;

                if(im.SenderConnection != null)
                {
                    if (_connections.ContainsKey(im.SenderConnection))
                    {
                        steamid = _connections[im.SenderConnection];
                    }
                    else if (im.MessageType == NetIncomingMessageType.ConnectionApproval)
                    {
                        steamid = im.SenderConnection.RemoteHailMessage.ReadUInt64();
                        _connections.Add(im.SenderConnection, steamid);
                        im.SenderConnection.Approve();
                        return;
                    }
                }

                switch (im.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        var status = (NetConnectionStatus)im.ReadByte();
                        if(status == NetConnectionStatus.Disconnected)
                        {
                            _connections.Remove(im.SenderConnection);
                            SocketMan.HandlePlayerDisconnected(steamid);
                        }
                        else if(status == NetConnectionStatus.Connected)
                        {
                            if(!SocketMan.InitiatePlayer(steamid))
                            {
                                im.SenderConnection.Disconnect("idk");
                            }
                        }
                        break;
                    case NetIncomingMessageType.Data:
                        SocketMan.HandleIncomingData2(steamid, im);
                        break;
                    case NetIncomingMessageType.UnconnectedData:
                        SocketMan.HandleIncomingUnconnectedData(im);
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                    //case NetIncomingMessageType.DebugMessage:
                    //case NetIncomingMessageType.VerboseDebugMessage:
                    //case NetIncomingMessageType.WarningMessage:
                    //case NetIncomingMessageType.Error:
                        UnityEngine.Debug.LogError(im.ReadString());
                        break;

                }
                _server.Recycle(im);
            }
        }

        public override void StartSocket()
        {
            if (_server != null)
            {
                UnityEngine.Debug.LogError("Don't start server that's already been started.");
                return;
            }

            var peerConfig = BuildPeerConfig();
            _server = new NetServer(peerConfig);
            _server.Start();

            DevConsole.WriteLine($"Accepting connections.\nLocal address: {peerConfig.LocalAddress}\nBroadcast address: {peerConfig.BroadcastAddress}\nPort: {peerConfig.Port}\n");

            SetSocketStatus(ServerStatus.AcceptingConnections);
        }

        public override void SendUnconnectedData(byte[] data, IPEndPoint endpoint)
        {
            //var om = _server.CreateMessage(data.Length);
            //om.Data = data;
            //om.LengthBytes = data.Length;
            //om.Position = 0;

            _server.RawSend(data, 0, data.Length, endpoint);
        }

        public override void StopSocket(string reason = "Server stopped")
        {
            if(_server != null)
            {
                _server.Shutdown(reason);
            }
        }

        private NetConnection FindConnection(ulong steamid)
        {
            foreach(var kvp in _connections)
            {
                if(kvp.Value == steamid)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        public override void DisconnectPlayer(ServerPlayer player, string reason = "Bye")
        {
            var netCon = FindConnection(player.AccountId);
            if(netCon != null)
            {
                netCon.Disconnect(reason);
            }
        }

        private List<NetConnection> _sendCache = new List<NetConnection>(256);
        public override void SendPacket(List<ServerPlayer> players, IBasePacket packet)
        {
            _sendCache.Clear();

            foreach (var player in players)
            {
                var netCon = FindConnection(player.AccountId);
                if (netCon != null)
                {
                    _sendCache.Add(netCon);
                }
            }

            if (_sendCache.Count > 0)
            {
                var om = _server.CreateMessage(packet.ByteSize + SocketManager.HeaderLength);
                SocketMan.WritePacketHeader(om, packet);
                packet.Write(om);

                _server.SendMessage(om, _sendCache, packet.Sc.DeliveryMethod, packet.Sc.SequenceChannel);
            }
        }

        public override void SendPacket(ServerPlayer player, IBasePacket packet)
        {
            var netCon = FindConnection(player.AccountId);
            if (netCon != null)
            {
                var om = _server.CreateMessage(packet.ByteSize + SocketManager.HeaderLength);
                SocketMan.WritePacketHeader(om, packet);
                packet.Write(om);

                _server.SendMessage(om, netCon, packet.Sc.DeliveryMethod, packet.Sc.SequenceChannel);
            }
        }

        private NetPeerConfiguration BuildPeerConfig()
        {
            var result = new NetPeerConfiguration("fragsurf")
            {
                AcceptIncomingConnections = true,
                Port = SocketMan.GameplayPort,
                MaximumConnections = 100,
                EnableUPnP = true,
                ConnectionTimeout = 30f
            };

            // todo: some users are failing to connect to localhost
            // I don't know why, but it could be ipv4/ipv6 problem.
            // for now connect to self using steamid, and debug with ipv6 later
            if(IPAddress.TryParse(SocketMan.BroadcastIp, out IPAddress broadcast))
            {
                result.BroadcastAddress = broadcast;
            }

            if(IPAddress.TryParse(SocketMan.LocalIp, out IPAddress local))
            {
                result.LocalAddress = local;
            }
            else
            {
                result.LocalAddress = IPAddress.Any;
            }

            result.SendBufferSize *= 2;
            result.EnableMessageType(NetIncomingMessageType.ConnectionApproval
                | NetIncomingMessageType.Data
                | NetIncomingMessageType.UnconnectedData
                | NetIncomingMessageType.ConnectionLatencyUpdated);

#if UNITY_EDITOR
            result.SimulatedMinimumLatency = .1f;
            result.SimulatedRandomLatency = .05f;
            result.SimulatedLoss = .03f;
            result.SimulatedDuplicatesChance = .03f;
#endif

            return result;
        }

    }
}

