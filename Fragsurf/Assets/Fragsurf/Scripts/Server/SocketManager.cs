using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using Fragsurf.Shared;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;
using Lidgren.Network;
using System.Net.NetworkInformation;
using UnityEngine;

namespace Fragsurf.Server
{
    public enum PlayerJoiningState
    {
        Approved,
        Denied,
        PendingFurtherNotice
    }

    public class PlayerJoiningEventArgs : EventArgs
    {
        public ServerPlayer Player;
        public PlayerJoiningState JoiningState;
        public string DenyReason;
    }

    public class SocketManager : FSComponent, INetworkInterface
    {

        public event Action<ServerStatus> OnServerStatusChanged;
        public event Action<NetIncomingMessage> OnIncomingUnconnectedData;

        private List<ServerPlayer> _players = new List<ServerPlayer>();
        private List<BaseSocket> _sockets = new List<BaseSocket>();
        private List<ServerPlayer> _playerCache = new List<ServerPlayer>(256);
        private bool _started;
        private bool _socketsAllGood;
        private int _clientIndex;
        private string _serverName = "Unnamed Server";

        public int NextClientIndex => ++_clientIndex;
        public ServerStatus Status { get; private set; } = ServerStatus.None;

        [ConVar("server.name")]
        public string ServerName
        {
            get => _serverName;
            set => _serverName = value.Substring(0, Mathf.Min(value.Length - 1, MaxServerNameLength));
        }
        [ConVar("server.description")]
        public string ServerDescription { get; set; }
        [ConVar("server.password")]
        public string ServerPassword { get; set; }
        [ConVar("server.port")]
        public int GameplayPort { get; set; } = 42020;
        [ConVar("server.maxplayers")]
        public int MaxPlayers { get; set; } = 10;
        [ConVar("server.connectiontimeout")]
        public int ConnectionTimeout { get; set; } = 30;
        //[ConVar("server.broadcastip")]
        //public string BroadcastIp { get; set; }
        //[ConVar("server.localip")]
        //public string LocalIp { get; set; }

        public const int MaxServerNameLength = 96;

        protected override void _Tick()
        {
            var socketsAllGood = false;

            if(_sockets.Count > 0)
            {
                socketsAllGood = true;
            }

            foreach(var socket in _sockets)
            {
                if(socket.SocketStatus == ServerStatus.AcceptingConnections)
                {
                    socket.Tick();
                }
                else
                {
                    socketsAllGood = false;
                }
            }

            if (socketsAllGood && !_socketsAllGood)
            {
                SetServerStatus(ServerStatus.AcceptingConnections);
                _socketsAllGood = true;
            }
        }

        protected override void _Update()
        {
            for(int i = Game.PlayerManager.Players.Count - 1; i >= 0; i--)
            {
                var player = Game.PlayerManager.Players[i] as ServerPlayer;
                if (player.IsFake)
                {
                    continue;
                }

                player.TimeSinceLastMessage += UnityEngine.Time.deltaTime;

                if (player.TimeSinceLastMessage >= 30)
                {
                    DisconnectPlayer(player, "Radio silence");
                }
            }
        }

        protected override void _Destroy()
        {
            Shutdown();
        }

        [ConCommand("server.start")]
        public void StartServer()
        {
            if(_started)
            {
                UnityEngine.Debug.Log("Server is already started.");
                return;
            }

            //if (string.IsNullOrWhiteSpace(LocalIp))
            //{
            //    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            //    socket.Connect("8.8.8.8", 65530);
            //    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            //    LocalIp = endPoint.Address.ToString();
            //}

            _started = true;

            _sockets.Add(new LNLSocket(this));

            if (Steamworks.SteamClient.IsValid)
            {
                _sockets.Add(new SteamP2PSocket(this));
            }

            foreach (var socket in _sockets)
            {
                if (socket.SocketStatus == ServerStatus.None)
                {
                    socket.StartSocket();
                }
            }
        }

        [ConCommand("server.shutdown")]
        public void Shutdown()
        {
            SetServerStatus(ServerStatus.ShuttingDown);
            StopSockets("Shutdown");
            SetServerStatus(ServerStatus.Shutdown);
        }

        private void StopSockets(string reason = "Shutdown")
        {
            foreach(var socket in _sockets)
            {
                if(socket.SocketStatus == ServerStatus.AcceptingConnections)
                {
                    socket.StopSocket(reason);
                }
            }
        }

        public void RaisePlayerPacketReceivedEvent(ServerPlayer player, IBasePacket packet)
        {
            Game.PlayerManager.RaisePlayerPacketReceived(player, packet);
        }

        public void DisconnectAllPlayers(string reason = "The Culling")
        {
            for(int i = Game.PlayerManager.Players.Count - 1; i >= 0; i--)
            {
                DisconnectPlayer(Game.PlayerManager.Players[i] as ServerPlayer, reason);
            }
        }

        public void DisconnectPlayer(int clientIndex, string reason = "Disconnected")
        {
            var player = FindPlayer(clientIndex);
            if(player != null)
            {
                DisconnectPlayer(player, reason);
            }
        }

        public void DisconnectPlayer(ServerPlayer player, string reason = "Disconnected")
        {
            foreach(var socket in _sockets)
            {
                socket.DisconnectPlayer(player, reason);
            }

            HandlePlayerDisconnected(player.ClientIndex);
        }

        public void BroadcastPacket(IBasePacket packet)
        {
            SendPacket(Game.PlayerManager.Players, packet);
        }

        public void SendPacket(int clientIndex, IBasePacket packet)
        {
            var player = FindPlayer(clientIndex);
            if(player != null)
            {
                SendPacket(player, packet);
            }
            else
            {
                PacketUtility.PutPacket(packet);
            }
        }

        public void SendPacketBrute(int clientIndex, IBasePacket packet)
        {
            var player = FindPlayer(clientIndex);
            if(player != null)
            {
                SendPacketBrute(player, packet);
            }
            else
            {
                PacketUtility.PutPacket(packet);
            }
        }

        public void SendPacketBrute(ServerPlayer player, IBasePacket packet)
        {
            if(player.IsFake)
            {
                PacketUtility.PutPacket(packet);
                return;
            }

            foreach (var socket in _sockets)
            {
                if (socket.SocketStatus == ServerStatus.AcceptingConnections)
                {
                    socket.SendPacket(player, packet);
                }
            }

            PacketUtility.PutPacket(packet);
        }

        public void SendPacket(ServerPlayer player, IBasePacket packet)
        {
            if(!player.Introduced || player.IsFake)
            {
                PacketUtility.PutPacket(packet);
                return;
            }

            foreach (var socket in _sockets)
            {
                if (socket.SocketStatus == ServerStatus.AcceptingConnections)
                {
                    socket.SendPacket(player, packet);
                }
            }

            PacketUtility.PutPacket(packet);
        }

        public void SendPacket(List<IPlayer> players, IBasePacket packet)
        {
            _playerCache.Clear();

            foreach(var player in players)
            {
                var svp = (ServerPlayer)player;
                if(player.Introduced && !player.IsFake)
                {
                    _playerCache.Add(svp);
                }
            }

            foreach (var socket in _sockets)
            {
                if (socket.SocketStatus == ServerStatus.AcceptingConnections)
                {
                    socket.SendPacket(_playerCache, packet);
                }
            }

            PacketUtility.PutPacket(packet);
        }

        public const int HeaderLength = 1;
        public void WritePacketHeader(NetBuffer buffer, IBasePacket packet)
        {
            buffer.Write(PacketUtility.GetPacketTypeId(packet.GetType()));
        }

        public ServerPlayer CreatePlayer()
        {
            var player = new ServerPlayer(0, NextClientIndex, Game.ElapsedTime);
            _players.Add(player);
            Game.PlayerManager.RaisePlayerConnected(player);
            return player;
        }

        public bool InitiatePlayer(ulong steamid)
        {
            throw new NotImplementedException();
            //var player = FindPlayer(steamid);
            //if (player != null)
            //{
            //    DisconnectPlayer(player, DenyReason.MapChange.ToString());
            //    HandlePlayerDisconnected(steamid);
            //    return false;
            //}
            //player = new ServerPlayer(steamid, NextClientIndex, Game.ElapsedTime);
            //_players.Add(player);
            //Game.PlayerManager.RaisePlayerConnected(player);
            //return true;
        }

        public void HandlePlayerDisconnected(int clientIndex)
        {
            var player = FindPlayer(clientIndex);
            if(player == null)
            {
                return;
            }
            if(player.Introduced)
            {
                Game.PlayerManager.RemovePlayer(player);
            }
            player.Disconnected = true;
            _players.Remove(player);
        }

        public void HandleIncomingUnconnectedData(NetIncomingMessage im)
        {
            OnIncomingUnconnectedData?.Invoke(im);
        }

        public void SendUnconnectedData(byte[] data, IPEndPoint endpoint)
        {
            foreach(var socket in _sockets)
            {
                socket.SendUnconnectedData(data, endpoint);
            }
        }

        public void HandleIncomingData2(int clientIndex, NetBuffer data)
        {
            var player = FindPlayer(clientIndex);

            if(player == null)
            {
                DisconnectPlayer(clientIndex);
                return;
            }

            player.TimeSinceLastMessage = 0f;

            var packetType = data.ReadByte();
            var packet = PacketUtility.TakePacket(packetType);

            ProcessPacket(player, packet, data);

            if (packet != null)
            {
                PacketUtility.PutPacket(packet);
            }
        }

        private void ProcessPacket(ServerPlayer player, IBasePacket packet, NetBuffer data)
        {
            packet?.Read(data);

            if (packet is ConnectionApproval ca)
            {
                var deny = CanPlayerJoin(ca);
                if (deny == DenyReason.None)
                {
                    player.SteamId = ca.SteamID;
                    player.DisplayName = ca.DisplayName;
                    player.TicketData = ca.TicketData;
                    Game.PlayerManager.RaisePlayerApprovedToJoin(player);

                    var returnApproval = PacketUtility.TakePacket<PlayerIntroduction>();
                    returnApproval.Step = PlayerIntroduction.JoinStep.PendingFileSync;
                    SendPacketBrute(player, returnApproval);
                }
                else
                {
                    DisconnectPlayer(player, deny.ToString());
                }
                return;
            }

            if (packet is UserCmd userCmd)
            {
                if (player.Entity != null)
                {
                    player.TickTimeDiff = Game.ElapsedTime - userCmd.HostTime;
                }
                RaisePlayerPacketReceivedEvent(player, userCmd);
                return;
            }
            else if (packet is Shared.Packets.Ping ping)
            {
                Game.PlayerManager.SetPlayerLatency(player, ping.CurrentRTT);
                var newPing = PacketUtility.TakePacket<Shared.Packets.Ping>();
                newPing.RemoteTick = Game.CurrentTick;
                newPing.RemoteTime = Game.ElapsedTime;
                newPing.SendTime = ping.SendTime;
                newPing.CurrentRTT = ping.CurrentRTT;
                SendPacket(player, newPing);
                return;
            }

            // packet is invalid?
            if (packet == null)
            {
                return;
            }

            // raise event
            RaisePlayerPacketReceivedEvent(player, packet);
        }

        public ServerPlayer FindPlayer(int clientIndex)
        {
            foreach (var player in _players)
            {
                if (player.ClientIndex == clientIndex)
                {
                    return player;
                }
            }
            return null;
        }

        private void SetServerStatus(ServerStatus status)
        {
            if (status == Status)
            {
                return;
            }

            Status = status;
            OnServerStatusChanged?.Invoke(status);
        }

        private DenyReason CanPlayerJoin(ConnectionApproval approval)
        {
            if(Game.PlayerManager.PlayerCount >= MaxPlayers)
            {
                return DenyReason.Full;
            }

            if (!string.IsNullOrEmpty(ServerPassword) && !string.Equals(approval.Password, ServerPassword))
            {
                return DenyReason.WrongPassword;
            }

            if (approval.GameVersion != Structure.Version)
            {
                return DenyReason.WrongGameVersion;
            }

            if (Game.GameLoader.State == GameLoaderState.ChangingMap)
            {
                return DenyReason.MapChange;
            }

            if (!Game.Live)
            {
                return DenyReason.NotLive;
            }

            return DenyReason.None;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static int GetAvailablePort(int startingPort)
        {
            IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            //getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            //getting active tcp listners - WCF service listening in tcp
            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            //getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (int i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
        }

    }
}
