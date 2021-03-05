using System;
using System.Collections.Generic;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Packets;

namespace Fragsurf.Shared.Player
{

    public delegate void PlayerEventHandler(IPlayer player);
    public delegate void PlayerSpectateHandler(IPlayer spectator, IPlayer target);
    public delegate void PlayerCommandHandler(IPlayer player, string[] args);

    public class PlayerManager : FSComponent
    {
        public event PlayerEventHandler OnPlayerConnected;
        public event PlayerEventHandler OnPlayerDisconnected;
        public event PlayerEventHandler OnPlayerApprovedToJoin;
        public event PlayerEventHandler OnPlayerIntroduced;
        public event PlayerEventHandler OnPlayerChangedTeam;
        public event PlayerEventHandler OnPlayerChangedName;
        public event PlayerEventHandler OnPlayerLatencyUpdated;
        public event PlayerEventHandler OnPlayerRunCommand;
        public event PlayerSpectateHandler OnPlayerSpectate;
        public event PlayerCommandHandler OnChatCommand;
        public event Action<IPlayer, IBasePacket> OnPlayerPacketReceived;

        public IPlayer LocalPlayer => FindPlayer(Game.ClientIndex);
        public List<IPlayer> Players { get; } = new List<IPlayer>();
        public Dictionary<IPlayer, List<ulong>> SpecList { get; } = new Dictionary<IPlayer, List<ulong>>();

        public int PlayerCount => Players.Count;

        protected override void _Destroy()
        {
            RemoveAllPlayers();
        }

        public void IntroducePlayer(IPlayer player)
        {
            if (Players.Contains(player))
            {
                UnityEngine.Debug.LogError("Trying to introduce a player that has already been introduced..");
                return;
            }

            player.Introduced = true;

            Players.Add(player);
            SpecList.Add(player, new List<ulong>());

            OnPlayerIntroduced?.Invoke(player);
        }

        public void RemovePlayer(IPlayer player)
        {
            if (!Players.Contains(player))
            {
                return;
            }

            Players.Remove(player);
            SpecList.Remove(player);

            if(player.Entity != null
                && Game.IsHost)
            {
                player.Entity.Delete();
            }

            SetPlayerSpectateTarget(player, null);

            OnPlayerDisconnected?.Invoke(player);
        }

        public void RemoveAllPlayers()
        {
            for(int i = Players.Count - 1; i >= 0; i--)
            {
                RemovePlayer(Players[i]);
            }
        }

        public IPlayer FindPlayer(string name)
        {
            foreach(var player in Players)
            {
                if(player.DisplayName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return player;
                }
            }
            return null;
        }

        public IPlayer FindPlayer(int clientIndex)
        {
            for(int i = 0; i < Players.Count; i++)
            {
                if (Players[i].ClientIndex == clientIndex)
                {
                    return Players[i];
                }
            }
            return null;
        }

        public IPlayer FindPlayer(ulong steamid)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].SteamId == steamid)
                    return Players[i];
            }
            return null;
        }

        public IPlayer FindPlayer(NetEntity human)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Entity == human)
                {
                    return Players[i];
                }
            }
            return null;
        }

        public void SetPlayerTeam(IPlayer player, byte teamNumber)
        {
            player.Team = teamNumber;
            if(teamNumber > 0)
            {
                SetPlayerSpectateTarget(player, null);
            }
            OnPlayerChangedTeam?.Invoke(player);
        }

        public void RaisePlayerPacketReceived(IPlayer player, IBasePacket packet)
        {
            OnPlayerPacketReceived?.Invoke(player, packet);
        }

        public void RaisePlayerConnected(IPlayer player)
        {
            OnPlayerConnected?.Invoke(player);
        }

        public void RaisePlayerApprovedToJoin(IPlayer player)
        {
            OnPlayerApprovedToJoin?.Invoke(player);
        }

        public void RaiseRunCommand(IPlayer player)
        {
            OnPlayerRunCommand?.Invoke(player);
        }

        public void SetPlayerName(IPlayer player, string name)
        {
            player.DisplayName = name;
            OnPlayerChangedName?.Invoke(player);
        }

        public void SetPlayerLatency(IPlayer player, int latency)
        {
            if (player == null)
                return;

            player.LatencyMs = latency;
            OnPlayerLatencyUpdated?.Invoke(player);
        }

        public void SetPlayerSpectateTarget(IPlayer spectator, int targetClientIndex)
        {
            var targetRef = targetClientIndex == -1 
                ? null 
                : FindPlayer(targetClientIndex);
            SetPlayerSpectateTarget(spectator, targetRef);
        }

        public void SetPlayerSpectateTarget(IPlayer spectator, IPlayer target)
        {
            foreach(var kvp in SpecList)
            {
                kvp.Value.Remove(spectator.SteamId);
            }
            if(target != null)
            {
                SpecList[target].Add(spectator.SteamId);
            }
            OnPlayerSpectate?.Invoke(spectator, target);
        }

        public void RaiseChatCommand(IPlayer player, string[] args)
        {
            OnChatCommand?.Invoke(player, args);
        }

    }
}

