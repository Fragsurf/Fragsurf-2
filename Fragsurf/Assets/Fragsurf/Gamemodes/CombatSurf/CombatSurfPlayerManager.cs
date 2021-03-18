using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    [Inject(InjectRealm.Shared, typeof(CombatSurf))]
    public class CombatSurfPlayerManager : FSSharedScript
    {

        protected override void OnHumanSpawned(Human hu)
        {
            if (!Game.IsHost)
            {
                SetTeamColor(hu);
            }
        }

        protected override void OnPlayerIntroduced(BasePlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }

            CreateHuman(player);

            var t1players = 0;
            var t2players = 0;

            foreach(var pl in Game.PlayerManager.Players)
            {
                if(pl.Team == 1)
                {
                    t1players++;
                }
                else if(pl.Team == 2)
                {
                    t2players++;
                }
            }

            if(t1players == 0 || t1players <= t2players)
            {
                Game.PlayerManager.SetPlayerTeam(player, 1);
            }
            else
            {
                Game.PlayerManager.SetPlayerTeam(player, 2);
            }
        }

        protected override void OnPlayerChangedTeam(BasePlayer player)
        {
            if (!Game.IsHost)
            {
                SetTeamColor(player.Entity as Human);
                return;
            }

            var teamColor = PlayerManager.GetTeamColor(player.Team);
            var teamName = player.Team == 0
                ? $"<color={teamColor.HashRGBA()}>Spectators</color>"
                : $"<color={teamColor.HashRGBA()}>Team " + player.Team + "</color>";

            Game.TextChat.MessageAll($"<color=yellow>{player.DisplayName}</color> has joined {teamName}");

            if(!(player.Entity is Human hu))
            {
                CreateHuman(player);
                hu = player.Entity as Human;
            }

            if(player.Team <= 0)
            {
                hu.Dead = true;
                hu.Enabled = false;
                return;
            }

            hu.Enabled = true;

            var rm = Game.Get<RoundManager>();
            if(rm == null
                || rm.MatchState != MatchStates.Live)
            {
                hu.Spawn(player.Team);
                return;
            }

            if(rm.RoundState == RoundStates.Freeze)
            {
                hu.Spawn(player.Team);
            }
            else
            {
                hu.Dead = true;
            }
        }

        private void CreateHuman(BasePlayer player)
        {
            if (!(player.Entity is Human hu))
            {
                if (player.Entity != null)
                {
                    player.Entity.Delete();
                }
                hu = new Human(Game);
                player.Entity = hu;
                Game.EntityManager.AddEntity(hu);
                hu.OwnerId = player.ClientIndex;
            }

            if (player.IsFake)
            {
                hu.BotController = new BotController(hu);
            }
        }

        [ChatCommand("Give an item [AK47/Knife/AWP/Axe/Bat/etc]", "give")]
        public void Give(BasePlayer player, string item)
        {
            if (!Game.IsHost)
            {
                return;
            }

            if (!(player.Entity is Human hu))
            {
                return;
            }
            hu.Give(item);
        }

        [ChatCommand("Spawns a bot", "bot")]
        public void SpawnBot(BasePlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }

            Game.PlayerManager.CreateFakePlayer("Fake Player");
        }

        private void SetTeamColor(Human hu)
        {
            if(hu == null)
            {
                return;
            }
            var owner = Game.PlayerManager.FindPlayer(hu.OwnerId);
            if (owner == null || hu.HumanGameObject == null)
            {
                return;
            }
            var color = PlayerManager.GetTeamColor(owner.Team);
            hu.HumanGameObject.SetColor(color);
        }

    }

}

