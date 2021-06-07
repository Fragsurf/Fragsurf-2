using System;
using System.Collections.Generic;
using System.Linq;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Packets;
using UnityEngine;

namespace Fragsurf.Shared.Player
{

    public class PlayerManager : FSComponent
    {

        public event Action<BasePlayer> OnPlayerConnected;
        public event Action<BasePlayer> OnPlayerDisconnected;
        public event Action<BasePlayer> OnPlayerApprovedToJoin;
        public event Action<BasePlayer> OnPlayerIntroduced;
        public event Action<BasePlayer> OnPlayerChangedTeam;
        public event Action<BasePlayer> OnPlayerChangedName;
        public event Action<BasePlayer> OnPlayerLatencyUpdated;
        public event Action<BasePlayer> OnPlayerRunCommand;
        public event Action<BasePlayer, BasePlayer> OnPlayerSpectate;
        public event Action<BasePlayer, string[]> OnChatCommand;
        public event Action<BasePlayer, IBasePacket> OnPlayerPacketReceived;

        public BasePlayer LocalPlayer => FindPlayer(Game.ClientIndex);
        public List<BasePlayer> Players { get; } = new List<BasePlayer>();
        public Dictionary<BasePlayer, List<ulong>> SpecList { get; } = new Dictionary<BasePlayer, List<ulong>>();
        public override bool ExecuteWhenIdling => true;

        public int PlayerCount => Players.Count;

        public static int _fakePlayerIndex = int.MaxValue;

        protected override void _Destroy()
        {
            RemoveAllPlayers();
        }

        public BasePlayer CreateFakePlayer(string displayName)
        {
            var player = new BasePlayer()
            {
                DisplayName = displayName,
                IsFake = true,
                ClientIndex = --_fakePlayerIndex,
                ConnectionTime = Game.ElapsedTime
            };
            IntroducePlayer(player);
            return player;
        }

        public void RemoveFakePlayers()
        {
            foreach(var pl in Players.FindAll(x => x.IsFake))
            {
                RemovePlayer(pl);
            }
        }

        public void IntroducePlayer(BasePlayer player)
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

        public void RemovePlayer(BasePlayer player)
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

        public BasePlayer FindPlayer(string name)
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

        public BasePlayer FindPlayer(int clientIndex)
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

        public BasePlayer FindPlayer(ulong steamid)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].SteamId == steamid)
                    return Players[i];
            }
            return null;
        }

        public BasePlayer FindPlayer(NetEntity human)
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

        public void SetPlayerTeam(BasePlayer player, byte teamNumber)
        {
            if(player.Team == teamNumber)
            {
                return;
            }

            player.Team = teamNumber;
            if(teamNumber > 0)
            {
                SetPlayerSpectateTarget(player, null);
            }

            OnPlayerChangedTeam?.Invoke(player);
        }

        public void RaisePlayerPacketReceived(BasePlayer player, IBasePacket packet)
        {
            OnPlayerPacketReceived?.Invoke(player, packet);
        }

        public void RaisePlayerConnected(BasePlayer player)
        {
            OnPlayerConnected?.Invoke(player);
        }

        public void RaisePlayerApprovedToJoin(BasePlayer player)
        {
            OnPlayerApprovedToJoin?.Invoke(player);
        }

        public void RaiseRunCommand(BasePlayer player)
        {
            OnPlayerRunCommand?.Invoke(player);
        }

        public void SetPlayerName(BasePlayer player, string name)
        {
            player.DisplayName = name;
            OnPlayerChangedName?.Invoke(player);
        }

        public void SetPlayerLatency(BasePlayer player, int latency)
        {
            if (player == null)
                return;

            player.LatencyMs = latency;
            OnPlayerLatencyUpdated?.Invoke(player);
        }

        public void SetPlayerSpectateTarget(BasePlayer spectator, int targetClientIndex)
        {
            var targetRef = targetClientIndex == -1 
                ? null 
                : FindPlayer(targetClientIndex);
            SetPlayerSpectateTarget(spectator, targetRef);
        }

        public void SetPlayerSpectateTarget(BasePlayer spectator, BasePlayer target)
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

        public void RaiseChatCommand(BasePlayer player, string[] args)
        {
            OnChatCommand?.Invoke(player, args);
        }

        public static Dictionary<int, Color> TeamColors = new Dictionary<int, Color>()
        {
            { 0, new Color32(192, 194, 192, 255) },
            { 1, new Color32(245, 122, 15, 255) },
            { 2, new Color32(15, 195, 245, 255) }
        };

        public static Color GetTeamColor(int team)
        {
            if (TeamColors.ContainsKey(team))
            {
                return TeamColors[team];
            }
            return Color.white;
        }

    }
}

