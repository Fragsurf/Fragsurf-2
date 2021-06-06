using UnityEngine;
using Fragsurf.Shared;
using Fragsurf.Shared.Player;
using Fragsurf.Movement;
using Fragsurf.Utility;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Gamemodes.Tricksurf
{
    [Inject(InjectRealm.Shared, typeof(Tricksurf))]
    public class SH_WorldHop : FSSharedScript
    {

        public Vector3 WhopOrigin = new Vector3(78f, 0, -20.8f);
        public Vector3 WhopVelocity = new Vector3(78f, 0, -20.8f);

        protected override void _Start()
        {
            base._Start();

            var ts = Game.GetFSComponent<SH_Tricksurf>();
            if (ts.TrickData != null)
            {
                WhopVelocity = UnityExtensions.StringToVector3(ts.TrickData.whop_velocity);
                WhopOrigin = UnityExtensions.StringToVector3(ts.TrickData.whop_origin);
            }
        }

        [ChatCommand("Spectate a player", "spec", "spectate")]
        public void SpecCmd(BasePlayer player, string target) { }
        [ChatCommand("Spawn with worldhop position & velocity", "whop", "worldhop", "wh")]
        public void WorldHopCmd(BasePlayer player) { }
        [ChatCommand("Enter Noclip mode", "noclip", "nc")]
        public void NoclipCmd(BasePlayer player) { }

        protected override void OnPlayerChatCommand(BasePlayer player, string[] args)
        {
            if (args != null && args.Length > 0)
            {
                switch (args[0])
                {
                    case "spectate":
                    case "spec":
                        if (player.Team != 0)
                        {
                            Game.PlayerManager.SetPlayerTeam(player, 0);
                        }

                        if (!Game.IsHost)
                        {
                            if (args.Length > 1)
                            {
                                var name = string.Join(" ", args, 1, args.Length - 1);
                                var playerToSpec = Game.PlayerManager.FindPlayer(name);
                                if (playerToSpec != null && playerToSpec.Entity is Human hu2spec)
                                {
                                    Game.Get<SpectateController>().TargetHuman = hu2spec;
                                }
                            }
                        }
                        break;
                    case "whop":
                        Game.GetFSComponent<SH_Tricksurf>().InvalidateTrack(player);

                        if (Game.IsHost)
                        {
                            SpawnWhop(player, WhopVelocity, WhopOrigin);
                        }
                        break;
                    case "noclip":
                        if(player.Entity is Human hu
                            && hu.MovementController is CSMovementController csm)
                        {
                            if (csm.MoveType != MoveType.Noclip)
                            {
                                csm.MoveType = MoveType.Noclip;
                                Game.GetFSComponent<SH_Tricksurf>().InvalidateTrack(player);
                                Game.GetFSComponent<SH_Tricksurf>().EnableDetection(player, false);
                            }
                            else
                            {
                                csm.MoveType = MoveType.Walk;
                                Game.GetFSComponent<SH_Tricksurf>().InvalidateTrack(player);
                                Game.GetFSComponent<SH_Tricksurf>().EnableDetection(player, true);
                            }
                        }
                        break;
                }
            }
        }

        protected override void OnPlayerIntroduced(BasePlayer player)
        {
            if (Game.IsHost)
            {
                Game.PlayerManager.SetPlayerTeam(player, 1);
                SpawnPlayer(player);
            }
        }

        private void SpawnPlayer(BasePlayer player)
        {
            Game.PlayerManager.SetPlayerTeam(player, 1);

            if (!(player.Entity is Human hu))
            {
                if (player.Entity != null)
                {
                    player.Entity.Delete();
                }
                hu = new Human(Game);
                Game.EntityManager.AddEntity(hu);
                hu.OwnerId = player.ClientIndex;
            }

            hu.Spawn();
        }

        private void SpawnWhop(BasePlayer player, Vector3 velocity, Vector3 origin)
        {
            SpawnPlayer(player);

            var hu = player.Entity as Human;
            if(hu == null)
            {
                return;
            }

            hu.Origin = origin + Vector3.up;
            hu.Velocity = velocity;
            hu.BaseVelocity = Vector3.zero;
            hu.SetAngles(Quaternion.LookRotation(WhopVelocity, Vector3.up).eulerAngles);
        }

    }
}