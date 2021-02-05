using Fragsurf.FSM.Actors;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using UnityEngine;

namespace Fragsurf.Mapping
{
    [Inject(InjectRealm.Shared, typeof(TestGamemode))]
    public class SH_TestGamemode : FSSharedScript
    {

        private bool _showTriggers;

        [ConVar("mapmode.testint", "This is a test variable :()", ConVarFlags.Replicator | ConVarFlags.Gamemode)]
        public int TestOkay { get; set; }
        [ConVar("mapmode.testcolor", "This is a test color ():", ConVarFlags.Gamemode)]
        public Color TestColor { get; set; } = Color.red;
        [ConVar("mapmode.showtriggers", "", ConVarFlags.Gamemode)]
        public bool ShowTriggers
        {
            get => _showTriggers;
            set
            {
                _showTriggers = value;
                foreach(var fsmTrigger in FindObjectsOfType<FSMTrigger>())
                {
                    fsmTrigger.EnableRenderers(value);
                }
            }
        }

        protected override void OnPlayerIntroduced(IPlayer player)
        {
            if (!Game.IsHost)
            {
                return;
            }
            MapLoader.Instance.CurrentMap.GetSpawnPoint(out Vector3 spawnPos, out Vector3 spawnAngles);
            var ent = new Human(Game);
            ent.Origin = spawnPos;
            ent.Angles = spawnAngles;
            ent.OwnerId = player.ClientIndex;
            player.Entity = ent;
            Game.EntityManager.AddEntity(ent);
        }

    }
}

