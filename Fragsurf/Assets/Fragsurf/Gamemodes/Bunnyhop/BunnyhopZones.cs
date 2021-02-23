using Fragsurf.Actors;
using Fragsurf.Shared;
using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    [Inject(InjectRealm.Client, typeof(Bunnyhop))]
    public class BunnyhopZones : FSSharedScript
    {

        private Color _startZoneColor = Color.green;
        private Color _endZoneColor = Color.red;
        private Color _cpZoneColor = Color.yellow;

        [ConVar("bunnyhop.startzonecolor", "Color for start zones", ConVarFlags.Gamemode | ConVarFlags.UserSetting)]
        public Color StartZoneColor
        {
            get => _startZoneColor;
            set
            {
                _startZoneColor = value;
                OutlineTracks();
            }
        }
        [ConVar("bunnyhop.endzonecolor", "Color for end zones", ConVarFlags.Gamemode | ConVarFlags.UserSetting)]
        public Color EndZoneColor
        {
            get => _endZoneColor;
            set
            {
                _endZoneColor = value;
                OutlineTracks();
            }
        }
        [ConVar("bunnyhop.cpzonecolor", "Color for checkpoint zones", ConVarFlags.Gamemode | ConVarFlags.UserSetting)]
        public Color CpZoneColor
        {
            get => _cpZoneColor;
            set
            {
                _cpZoneColor = value;
                OutlineTracks();
            }
        }

        protected override void _Initialize()
        {
            OutlineTracks();
        }

        private void OutlineTracks()
        {
            foreach (var track in GameObject.FindObjectsOfType<FSMTrack>())
            {
                OutlineTrack(track);
            }
        }

        private void OutlineTrack(FSMTrack track)
        {
            switch (track.TrackType)
            {
                case FSMTrackType.Linear:
                    OutlineTrigger(track.LinearData.StartTrigger, StartZoneColor);
                    OutlineTrigger(track.LinearData.EndTrigger, EndZoneColor);
                    foreach (var cp in track.LinearData.Checkpoints)
                    {
                        OutlineTrigger(cp, CpZoneColor);
                    }
                    break;
                case FSMTrackType.Staged:
                    foreach (var stage in track.StageData.Stages)
                    {
                        OutlineTrigger(stage.StartTrigger, StartZoneColor);
                        OutlineTrigger(stage.EndTrigger, EndZoneColor);
                    }
                    break;
                case FSMTrackType.Bonus:
                    OutlineTrigger(track.BonusData.StartTrigger, StartZoneColor);
                    OutlineTrigger(track.BonusData.EndTrigger, EndZoneColor);
                    break;
            }
        }

        private void OutlineTrigger(FSMTrigger trigger, Color color)
        {
            foreach (var mf in trigger.GetComponentsInChildren<MeshFilter>())
            {
                LineHelper.GenerateOutline(mf, color, 2f);
            }
        }

    }
}
