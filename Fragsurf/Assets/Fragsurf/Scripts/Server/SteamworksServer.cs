using Steamworks;
using Fragsurf.Shared;
using Fragsurf.Shared.Maps;
using Fragsurf.Shared.Player;
using UnityEngine;
using System;
using Fragsurf.Utility;

namespace Fragsurf.Server
{
    public class SteamworksServer : FSServerScript
    {
        public int SteamQueryPort { get; set; } = 43026;
        public int SteamPort { get; set; } = 43025;
        private bool _minFps = true;

        private const string DefaultServerName = "New Fragsurf Server";

        protected override void _Initialize()
        {
            DevConsole.RegisterVariable("server.minfps", "", () => _minFps, v => _minFps = v, this);
            DevConsole.RegisterVariable("server.steamport", "", () => SteamPort, v => SteamPort = v, this);
            DevConsole.RegisterVariable("server.steamqueryport", "", () => SteamQueryPort, v => SteamQueryPort = v, this);
        }

        protected override void _Start()
        {
            SteamServer.OnValidateAuthTicketResponse += SteamServer_OnValidateAuthTicketResponse;

            var init = new SteamServerInit("fragsurf", "Fragsurf");
            init.GamePort = (ushort)GameServer.Instance.Socket.GameplayPort;
            init.QueryPort = (ushort)SteamQueryPort;
            init.SteamPort = (ushort)SteamPort;
            init.VersionString = Structure.Version;

            try
            {
                SteamServer.Init(Structure.AppId, init);
                SteamServer.ServerName = GameServer.Instance.Socket.ServerName ?? DefaultServerName;
                SteamServer.DedicatedServer = true;
                SteamServer.Passworded = !string.IsNullOrEmpty(GameServer.Instance.Socket.ServerPassword);
                SteamServer.MaxPlayers = GameServer.Instance.Socket.MaxPlayers;
                SteamServer.AutomaticHeartbeats = true;
                SteamServer.LogOnAnonymous();

                DevConsole.WriteLine($"Steamworks Server initialized\n - Name: {SteamServer.ServerName}\n - Password: {GameServer.Instance.Socket.ServerPassword}\n - Version: {init.VersionString}\n - Game Port: {init.GamePort} (This must be open to allow players to connect)\n - Query Port: {init.QueryPort} (This must be open to show on master server list)\n - Steam Port: {init.SteamPort} (This must be open to show on master server list)");

                GameObject.FindObjectOfType<ServerConsole>()?.SetTitle(GameServer.Instance.Socket.ServerName);
            }
            catch(Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        protected override void _Update()
        {
            if(SteamServer.IsValid)
            {
                SteamServer.RunCallbacks();
            }

            Application.targetFrameRate = Game.PlayerManager.PlayerCount > 0
                ? _minFps ? (int)(1 / Time.fixedDeltaTime) + 5 : TimeStep.Instance.TargetFPS
                : 10;
        }

        protected override void _Destroy()
        {
            try
            {
                SteamServer.Shutdown();
            }
            catch
            {
                // whatever
            }

            SteamServer.OnValidateAuthTicketResponse -= SteamServer_OnValidateAuthTicketResponse;
        }

        private void SteamServer_OnValidateAuthTicketResponse(SteamId steamid, SteamId owner, AuthResponse response)
        {
            var player = Game.PlayerManager.FindPlayer(steamid);
            if(player == null)
            {
                return;
            }

            if (response != AuthResponse.OK)
            {
                GameServer.Instance.Socket.DisconnectPlayer(player.ClientIndex, DenyReason.SteamAuthFailed.ToString());
            }
            else
            {
                SteamServer.UpdatePlayer(steamid, player.DisplayName, 0);
            }
        }

        protected override void OnMapEvent(IFragsurfMap map, MapEventType eventType, bool hasNextMap)
        {
            if(eventType == MapEventType.Loaded)
            {
                SteamServer.ServerName = GameServer.Instance.Socket.ServerName ?? DefaultServerName;
                SteamServer.MapName = MapLoader.Instance.CurrentMap.Name;
            }
        }

        protected override void OnGameLoaded()
        {
            SteamServer.GameTags = Game.GamemodeLoader.Gamemode.Name;
        }

        protected override void OnPlayerIntroduced(IPlayer player)
        {
            if(player.IsFake)
            {
                SteamServer.BotCount++;
            }
            else
            {
                var sp = player as ServerPlayer;
                if(sp.TicketData != null 
                    && sp.TicketData.Length > 0
                    && !Game.IsLocalServer
                    && !SteamServer.BeginAuthSession(sp.TicketData, sp.SteamId))
                {
                    ((GameServer)Game).Socket.DisconnectPlayer(sp, DenyReason.SteamAuthFailed.ToString());
                }
            }
        }

        protected override void OnPlayerDisconnected(IPlayer player)
        {
            if(player.IsFake)
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
