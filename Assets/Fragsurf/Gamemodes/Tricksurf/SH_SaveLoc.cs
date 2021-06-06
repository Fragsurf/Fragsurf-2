using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Shared;
using Fragsurf.Shared.Player;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;

namespace Fragsurf.Gamemodes.Tricksurf
{
    [Inject(InjectRealm.Shared, typeof(Tricksurf))]
    public class SH_SaveLoc : FSSharedScript
    {

        private struct SaveLoc
        {
            public Vector3 Position;
            public Vector3 Angles;
            public Vector3 Velocity;
        }

        private CircularBuffer<SaveLoc> _saveLocs = new CircularBuffer<SaveLoc>(5000);
        private Dictionary<int, int> _lastUsed = new Dictionary<int, int>();

        [ChatCommand("Saves a location to /tele to", "saveloc", "save")]
        public void SaveLocCmd(BasePlayer player)
        {
            if (!Game.IsHost || !(player.Entity is Human hu))
            {
                return;
            }

            if (_saveLocs.IsFull)
            {
                _saveLocs.Clear();
                _lastUsed.Clear();
            }

            _saveLocs.PushBack(new SaveLoc()
            {
                Position = hu.Origin,
                Angles = hu.Angles,
                Velocity = hu.Velocity
            });
            var idx = _saveLocs.Size - 1;
            _lastUsed[player.ClientIndex] = idx;
            Game.Get<TextChat>().MessagePlayer(player, $"saveloc #{idx}.  Teleport to it with /tele or /tele #{idx}");
        }

        [ChatCommand("Teleports to a saveloc (/tele or /tele #1)", "tele", "t")]
        public void TeleCmd(BasePlayer player)
        {

        }

        protected override void OnPlayerChatCommand(BasePlayer player, string[] args)
        {
            if (args == null || args.Length == 0 || !(player.Entity is Human hu))
            {
                return;
            }

            if (args[0] == "tele")
            {
                Game.GetFSComponent<SH_Tricksurf>().InvalidateTrack(player);

                if (Game.IsHost)
                {
                    int teleNum = _lastUsed.ContainsKey(player.ClientIndex) ? _lastUsed[player.ClientIndex] : 0;
                    if (args.Length > 1)
                    {
                        int.TryParse(args[1].Replace("#", string.Empty), out teleNum);
                        _lastUsed[player.ClientIndex] = teleNum;
                    }
                    Teleport(hu, teleNum);
                }
            }
        }

        private void Teleport(Human hu, int num)
        {
            if (num >= _saveLocs.Size)
            {
                return;
            }

            var saveloc = _saveLocs[num];
            hu.Origin = saveloc.Position;
            hu.SetAngles(saveloc.Angles);
            hu.Velocity = saveloc.Velocity;
        }

    }
}
