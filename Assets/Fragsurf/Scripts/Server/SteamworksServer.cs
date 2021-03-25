using Steamworks;
using Fragsurf.Shared;
using Fragsurf.Maps;
using Fragsurf.Shared.Player;
using UnityEngine;

namespace Fragsurf.Server
{
    public class SteamworksServer : FSServerScript
    {

        [ConVar("server.steamqueryport", "")]
        public int SteamQueryPort { get; set; } = 43026;
        [ConVar("server.steamport", "")]
        public int SteamPort { get; set; } = 43025;
        [ConVar("server.requiresteamauth", "")]
        public bool RequireSteamAuth { get; set; } = false;

        private const string DefaultServerName = "New Fragsurf Server";

        protected override void _Start()
        {
            SteamServer.OnValidateAuthTicketResponse += SteamServer_OnValidateAuthTicketResponse;

            var socketMan = Game.Network as SocketManager;
            var init = new SteamServerInit("fragsurf", "Fragsurf");
            init.GamePort = (ushort)socketMan.GameplayPort;
            init.QueryPort = (ushort)SteamQueryPort;
            init.SteamPort = (ushort)SteamPort;
            init.VersionString = Structure.Version;

            SteamServer.Init(Structure.AppId, init);
            SteamServer.ServerName = socketMan.ServerName ?? DefaultServerName;
            SteamServer.DedicatedServer = true;
            SteamServer.Passworded = !string.IsNullOrEmpty(socketMan.ServerPassword);
            SteamServer.MaxPlayers = socketMan.MaxPlayers;
            SteamServer.AutomaticHeartbeats = true;
            SteamServer.LogOnAnonymous();

            DevConsole.WriteLine($"Steamworks Server initialized\n - Name: {SteamServer.ServerName}\n - Password: {socketMan.ServerPassword}\n - Version: {init.VersionString}\n - Game Port: {init.GamePort} (This must be open to allow players to connect)\n - Query Port: {init.QueryPort} (This must be open to show on master server list)\n - Steam Port: {init.SteamPort} (This must be open to show on master server list)");

            var serverConsole = GameObject.FindObjectOfType<ServerConsole>();
            if (serverConsole)
            {
                serverConsole.SetTitle(socketMan.ServerName);
            }
        }

        protected override void _Tick()
        {
            if (SteamServer.IsValid)
            {
                SteamServer.RunCallbacks();
            }
        }

        protected override void _Destroy()
        {
            SteamServer.OnValidateAuthTicketResponse -= SteamServer_OnValidateAuthTicketResponse;
            SteamServer.Shutdown();
        }

        private void SteamServer_OnValidateAuthTicketResponse(SteamId steamid, SteamId owner, AuthResponse response)
        {
            var player = Game.PlayerManager.FindPlayer(steamid);
            if (player == null || !SteamServer.IsValid)
            {
                return;
            }

            if(response == AuthResponse.OK)
            {
                SteamServer.UpdatePlayer(steamid, player.DisplayName, 0);
                return;
            }

            if (RequireSteamAuth)
            {
                ((SocketManager)Game.Network).DisconnectPlayer(player.ClientIndex, DenyReason.SteamAuthFailed.ToString());
            }
        }

        protected override void OnGameLoaded()
        {
            if (!SteamServer.IsValid)
            {
                return;
            }

            SteamServer.GameTags = Game.GamemodeLoader.Gamemode.Data.Name;
            SteamServer.ServerName = ((SocketManager)Game.Network).ServerName ?? DefaultServerName;
            SteamServer.MapName = Map.Current.Name;
        }

        protected override void OnPlayerIntroduced(BasePlayer player)
        {
            if (!SteamServer.IsValid)
            {
                return;
            }

            if (player.IsFake)
            {
                SteamServer.BotCount++;
                return;
            }

            var canKick = RequireSteamAuth && !Game.IsServerHost(player.ClientIndex);

            if (player.TicketData == null 
                || player.TicketData.Length == 0)
            {
                if (canKick)
                {
                    ((GameServer)Game).Socket.DisconnectPlayer(player, DenyReason.SteamAuthFailed.ToString());
                }
                return;
            }

            var session = SteamServer.BeginAuthSession(player.TicketData, player.SteamId);
            if(!session && canKick)
            {
                ((GameServer)Game).Socket.DisconnectPlayer(player, DenyReason.SteamAuthFailed.ToString());
            }
        }

        protected override void OnPlayerDisconnected(BasePlayer player)
        {
            if (!SteamServer.IsValid)
            {
                return;
            }

            if (player.IsFake)
            {
                SteamServer.BotCount--;
            }
            else
            {
                SteamServer.EndSession(player.SteamId);
            }
        }

        //private void OnIncomingRawData(byte[] data, int length, IPEndPoint endpoint)
        //{
        //    if(length >= 4)
        //    {
        //        if (BitConverter.ToUInt32(data, 0) == 0xFFFFFFFF)
        //        {
        //            SteamServer.HandleIncomingPacket(data, length, IpStringToUint(endpoint.Address.ToString()), (ushort)endpoint.Port);
        //            if (SteamServer.GetOutgoingPacket(out Steamworks.Data.OutgoingPacket packet))
        //            {
        //                var ipstr = IpUintToString(packet.Address);
        //                var endpointOut = new IPEndPoint(IPAddress.Parse(ipstr), packet.Port);
        //                GameServer.Instance.Socket.SendUnconnectedData(packet.Data, endpointOut);
        //            }
        //        }
        //    }
        //}

        //public static uint IpStringToUint(string ipString)
        //{
        //    var ipAddress = IPAddress.Parse(ipString);
        //    var ipBytes = ipAddress.GetAddressBytes();
        //    var ip = (uint)ipBytes[0] << 24;
        //    ip += (uint)ipBytes[1] << 16;
        //    ip += (uint)ipBytes[2] << 8;
        //    ip += (uint)ipBytes[3];
        //    return ip;
        //}

        //public static string IpUintToString(uint ipUint)
        //{
        //    var ipBytes = BitConverter.GetBytes(ipUint);
        //    var ipBytesRevert = new byte[4];
        //    ipBytesRevert[0] = ipBytes[3];
        //    ipBytesRevert[1] = ipBytes[2];
        //    ipBytesRevert[2] = ipBytes[1];
        //    ipBytesRevert[3] = ipBytes[0];
        //    return new IPAddress(ipBytesRevert).ToString();
        //}

    }
}
