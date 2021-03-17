using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class ReplayHuman : Human
    {

        private GameObject _lineObject;
        public string DisplayName;

        public bool DrawPath
        {
            get => _lineObject != null;
            set
            {
                if (_lineObject)
                {
                    GameObject.Destroy(_lineObject);
                }
                if (value && Timeline is BunnyhopTimeline tl)
                {
                    var points = new List<Vector3>();
                    foreach(var pos in tl.Frames)
                    {
                        points.Add(pos.Position);
                    }
                    _lineObject = LineHelper.GeneratePath(points.ToArray(), Color.green);
                }
            }
        }

        public float PlaybackSpeed = 1f;

        public ReplayHuman(FSGameLoop game)
            : base(game)
        {
            _autoReplayTimeline = false;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            InterpolationMode = InterpolationMode.Frame;
        }

        private float _accumulator;
        private float _elapsedTime;
        protected override void OnUpdate()
        {
            base.OnUpdate();

            var playbackSpeed = Mathf.Clamp(PlaybackSpeed, .25f, 5);
            var fixedDeltaTime = Game.FixedDeltaTime / playbackSpeed;
            var newTime = Time.realtimeSinceStartup;
            var frameTime = Mathf.Min(newTime - _elapsedTime, fixedDeltaTime);

            _elapsedTime = newTime;
            _accumulator += frameTime;

            while (_accumulator >= fixedDeltaTime)
            {
                _accumulator -= fixedDeltaTime;
                Timeline?.ReplayTick();
            }
        }

        protected override void OnDelete()
        {
            base.OnDelete();

            if (_lineObject)
            {
                GameObject.Destroy(_lineObject);
            }
        }

    }
}

