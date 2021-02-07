using System.Threading.Tasks;
using Lidgren.Network;
using Fragsurf.Shared;
using Fragsurf.Shared.Packets;
using Steamworks;
using Fragsurf.Server;
using Fragsurf.Utility;
using Fragsurf.UI;

namespace Fragsurf.Client
{
    public enum ClientSocketStatus
    {
        None,
        Connecting,
        Connected,
        Disconnected,
        ProblemDetected
    }

    public delegate void ClientSocketStatusChangedHandler(ClientSocketStatus status, string reason = null);
    public delegate void ClientSocketPacketReceivedHandler(IBasePacket packet);

    public class ClientSocketManager : FSComponent, INetworkInterface
    {

        public event ClientSocketStatusChangedHandler OnStatusChanged;

        public ClientSocketStatus Status { get; private set; } = ClientSocketStatus.Disconnected;
        public string HostAddress { get; private set; }
        public int HostPort { get; private set; }

        public float AverageRoundtripTime => _pingAverage.TrimmedAverage;

        private ClientBaseSocket _socket;
        private MovingAverage _pingAverage = new MovingAverage(20);
        private MovingAverage _timeOffsetAverage = new MovingAverage(20);
        private MovingAverage _tickOffsetAverage = new MovingAverage(20);
        private float _pingTimer;
        private const float _pingInterval = 0.5f;

        public string EnteredPassword = string.Empty;

        protected override void _Initialize()
        {
            DevConsole.RegisterCommand("net.disconnect", "Disconnects from the host", this, Cmd_Disconnect);
            DevConsole.RegisterCommand("disconnect", "Disconnects from the host", this, Cmd_Disconnect);
            DevConsole.RegisterCommand("net.fakelag", "", this, (e) =>
            {
                _socket?.SetFakeLag(int.Parse(e[1]));
            });
            DevConsole.RegisterCommand("net.fakeloss", "", this, (e) =>
            {
                _socket?.SetFakeLoss(int.Parse(e[1]));
            });
        }

        protected override void _Destroy()
        {
            OnStatusChanged = null;
            Shutdown();
        }

        protected override void _Tick()
        {
            _socket?.Tick();
        }

        protected override void _Update()
        {
            if(Game.Live)
            {
                if (_pingTimer > 0)
                {
                    _pingTimer -= UnityEngine.Time.deltaTime;
                    if (_pingTimer <= 0)
                    {
                        SendPing();
                        _pingTimer = _pingInterval;
                    }
                }
            }
        }

        public const int HeaderSize = 1;
        public void WritePacketHeader(IBasePacket packet, NetBuffer buffer)
        {
            buffer.Write(PacketUtility.GetPacketTypeId(packet.GetType()));
        }

        private bool PacketIsWaiting(IBasePacket packet)
        {
            foreach(var asyncWait in _waitingPackets)
            {
                if(asyncWait.Packet == packet)
                {
                    return true;
                }
            }
            return false;
        }

        public void Shutdown()
        {
            Disconnect("Shutdown");
        }

        public void HandleIncomingData(NetBuffer buffer)
        {
            var packetType = buffer.ReadByte();
            var packet = PacketUtility.TakePacket(packetType);

            ProcessPacket(buffer, packetType, packet);

            if(packet != null && !PacketIsWaiting(packet))
            {
                PacketUtility.PutPacket(packet);
            }
        }

        private class AsyncPacketWait
        {
            public AsyncPacketWait(System.Type type)
            {
                Type = type;
            }
            public readonly System.Type Type;
            public IBasePacket Packet;
        }

        private System.Collections.Generic.List<AsyncPacketWait> _waitingPackets = new System.Collections.Generic.List<AsyncPacketWait>(256);

        public async Task<T> WaitForPacketAsync<T>(int timeout) 
            where T : IBasePacket
        {
            var asyncObj = new AsyncPacketWait(typeof(T));
            _waitingPackets.Add(asyncObj);
            while(timeout > 0 && asyncObj.Packet == null)
            {
                timeout -= 100;
                await Task.Delay(100);
            }
            _waitingPackets.Remove(asyncObj);
            if(asyncObj.Packet != null)
            {
                return (T)asyncObj.Packet;
            }
            return default;
        }

        private void ProcessPacket(NetBuffer buffer, byte typeId, IBasePacket packet)
        {
            packet.Read(buffer);

            for (int i = 0; i < _waitingPackets.Count; i++)
            {
                if (packet.GetType() == _waitingPackets[i].Type)
                {
                    _waitingPackets[i].Packet = packet;
                }
            }

            if (typeId == PacketUtility.GetPacketTypeId<Ping>())
            {
                ReceivePing(packet as Ping);
                return;
            }
            else if (packet is PlayerIntroduction intro)
            {
                if (intro.Step == PlayerIntroduction.JoinStep.PendingFileSync)
                {
                    SetSocketStatus(ClientSocketStatus.Connected);
                }
                else
                {
                    SetSocketStatus(ClientSocketStatus.Disconnected);
                }
            }

            Game.PlayerManager.RaisePlayerPacketReceived(Game.PlayerManager.LocalPlayer, packet);
        }

        private AuthTicket _authTicket;

        public void HandleConnected()
        {
            _authTicket = SteamClient.IsValid ? SteamUser.GetAuthSessionTicket() : null;
            _pingAverage.Clear();
            _timeOffsetAverage.Clear();
            _tickOffsetAverage.Clear();
            _pingTimer = 0f;

            var approval = PacketUtility.TakePacket<ConnectionApproval>();
            approval.GameVersion = Structure.Version;
            approval.DisplayName = SteamClient.IsValid ? SteamClient.Name : "Unknown";
            approval.SteamID = SteamClient.IsValid ? SteamClient.SteamId : 0;
            approval.TicketData = _authTicket != null ? _authTicket.Data : new byte[0];
            approval.Password = EnteredPassword;

            BroadcastPacket(approval);
            StartPinging();

            SetSocketStatus(ClientSocketStatus.Connecting);
        }

        public void HandleDisconnected(string reason)
        {
            if (SteamClient.IsValid)
            {
                SteamUser.EndAuthSession(SteamClient.SteamId);
                _authTicket?.Dispose();
                _authTicket = null;
            }

            SetSocketStatus(ClientSocketStatus.Disconnected, reason);
        }

        public void SetSocketStatus(ClientSocketStatus status, string reason = null)
        {
            if(Status == status)
            {
                return;
            }

            if(int.TryParse(reason, out int reasonCode))
            {
                reason = ((DenyReason)reasonCode).ToString();
            }

            UnityEngine.Debug.Log("Client socket status: " + status + ", " + reason);

            if (status == ClientSocketStatus.Disconnected)
            {
                _socket = null;

                if(GameServer.Instance == null)
                {
                    UGuiManager.Instance.Popup("Disconnected from host: " + reason);
                }
            }
            else if(status == ClientSocketStatus.ProblemDetected)
            {
                UGuiManager.Instance.Popup("Problem detected while connecting to the host.  Try again.");
            }

            Status = status;

            OnStatusChanged?.Invoke(status, reason);
        }

        public void Connect(string address, int port, string password = null)
        {
            if(string.IsNullOrEmpty(address))
            {
                UnityEngine.Debug.LogError("Don't do that");
                return;
            }

            HostAddress = address;
            HostPort = port;
            EnteredPassword = password ?? string.Empty;

            if (_socket != null)
            {
                Disconnect();
            }

            if(address.Length == 17)
            {
                _socket = new ClientSteamP2PSocket(this);
            }
            else
            {
                _socket = new ClientLNLSocket(this);
            }

            SetSocketStatus(ClientSocketStatus.Connecting);
            _socket.Connect(address, port);
        }

        private bool _asyncConnectCancelled;
        public void CancelAsyncConnect()
        {
            _asyncConnectCancelled = true;
        }

        public async Task<ClientSocketStatus> ConnectAsync(string address, int port, string password = null)
        {
            _asyncConnectCancelled = false;
            Connect(address, port, password);
            float timeout = 10000f; // wait 10 seconds
            while(Status != ClientSocketStatus.Connected && timeout > 0 && !_asyncConnectCancelled)
            {
                if(Status == ClientSocketStatus.Disconnected)
                {
                    Connect(address, port, password);
                }
                timeout -= 500;
                await Task.Delay(500);
            }
            return Status;
        }

        public void Disconnect(string reason = "Disconnected")
        {
            CancelAsyncConnect();
            _socket?.Disconnect(reason);
            _socket = null;

            HandleDisconnected(reason);
        }

        public void BroadcastPacket(IBasePacket packet)
        {
            _socket?.SendPacket(packet);
            PacketUtility.PutPacket(packet);
        }

        public void BroadcastPacketEnqueue(IBasePacket packet)
        {
            // todo: 
            BroadcastPacket(packet);
        }

        public void SendPacket(int clientIndex, IBasePacket packet)
        {
            BroadcastPacket(packet);
        }

        private void StartPinging()
        {
            _pingTimer = _pingInterval;
        }

        private void Cmd_Disconnect(string[] args)
        {
            Disconnect("bye");
        }

        private void SendPing()
        {
            var ping = PacketUtility.TakePacket<Ping>();
            ping.SendTime = Game.ElapsedTime;
            ping.CurrentRTT = (int)(_pingAverage.TrimmedAverage * 1000f);
            BroadcastPacket(ping);
        }

        private void ReceivePing(Ping ping)
        {
            var rtt = Game.ElapsedTime - ping.SendTime;
            _pingAverage.ComputeAverage(rtt, true);
            var rttAvg = _pingAverage.TrimmedAverage;

            var timeO = ping.RemoteTime - Game.ElapsedTime;
            var tickO = ping.RemoteTick - Game.CurrentTick;
            tickO += UnityEngine.Mathf.RoundToInt(rttAvg / UnityEngine.Time.fixedDeltaTime);
            timeO += rttAvg;

            _timeOffsetAverage.ComputeAverage(timeO, true);
            _tickOffsetAverage.ComputeAverage(tickO, true);
        }

        public int GetRemoteTick(int localTick)
        {
            return localTick + (int)_tickOffsetAverage.TrimmedAverage;
        }

        public int GetLocalTick(int remoteTick)
        {
            return remoteTick - (int)_tickOffsetAverage.TrimmedAverage;
        }

        public float GetRemoteTime(float localTime)
        {
            return localTime + _timeOffsetAverage.TrimmedAverage;
        }

        public float GetLocalTime(float remoteTime)
        {
            return remoteTime - _timeOffsetAverage.TrimmedAverage;
        }

    }
}

