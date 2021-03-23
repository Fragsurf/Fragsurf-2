using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.Utility;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    [Inject(InjectRealm.Shared, typeof(CombatSurf))]
    public class CombatSurfPlayerManager : FSSharedScript
    {

        protected override void _Start()
        {
            if (!Game.IsHost)
            {
                return;
            }
            Game.Get<RoundManager>().OnMatchStart += CombatSurfPlayerManager_OnMatchStart;
            Game.Get<RoundManager>().OnRoundFreeze += CombatSurfPlayerManager_OnRoundFreeze;
        }

        private void CombatSurfPlayerManager_OnRoundFreeze(int roundNumber)
        {
            foreach(var player in Game.PlayerManager.Players)
            {
                if(player.Team > 0 && player.Entity is Human hu)
                {
                    EquipHuman(hu);
                }
            }
        }

        private void CombatSurfPlayerManager_OnMatchStart()
        {
            foreach (var ent in Game.EntityManager.Entities)
            {
                if (!(ent is Human hu))
                {
                    continue;
                }
                for(int i = hu.Equippables.Items.Count - 1; i >= 0; i--)
                {
                    hu.Equippables.Items[i].Delete();
                }
            }
        }

        protected override void OnHumanSpawned(Human hu)
        {
            if (!Game.IsHost)
            {
                SetTeamColor(hu);
                return;
            }
            EquipHuman(hu);
        }

        protected override void OnHumanKilled(Human hu)
        {
            var rm = Game.Get<RoundManager>();
            if(!rm || rm.MatchState != MatchStates.Live)
            {
                StartCoroutine(RespawnIn(hu, 5f));
            }
        }

        private IEnumerator RespawnIn(Human hu, float delay)
        {
            yield return new WaitForSeconds(delay);

            var player = Game.PlayerManager.FindPlayer(hu.OwnerId);
            var rm = Game.Get<RoundManager>();

            if (!hu.Dead
                || !hu.Enabled
                || player == null
                || player.Team == 0
                || (rm && rm.MatchState == MatchStates.Live))
            {
                yield break;
            }

            hu.Spawn(player.Team);
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

            var rm = Game.Get<RoundManager>();
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

            hu.Enabled = player.Team > 0;

            if(player.Team > 0
                && (rm.MatchState != MatchStates.Live
                || rm.RoundState == RoundStates.Freeze))
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

        [ChatCommand("Kill yourself", "suicide", "kill")]
        public void Suicide(BasePlayer player)
        {
            if (Game.IsHost && player.Entity is Human hu)
            {
                hu.Dead = true;
            }
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

        private void EquipHuman(Human hu)
        {
            if (!hu.Equippables.HasItemInSlot(ItemSlot.Light))
            {
                hu.Give("M1911");
            }

            if (!hu.Equippables.HasItemInSlot(ItemSlot.Melee))
            {
                hu.Give("Knife");
            }

            foreach (var item in hu.Equippables.Items)
            {
                if (!(item.EquippableGameObject is GunEquippable gun))
                {
                    continue;
                }
                gun.RoundsInClip = gun.GunData.RoundsPerClip;
                gun.ExtraRounds = gun.GunData.RoundsPerClip * gun.GunData.MaxClips;
            }
        }

    }

}

