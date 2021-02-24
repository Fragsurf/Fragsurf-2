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

        public ReplayHuman(FSGameLoop game)
            : base(game)
        {
            InterpolationMode = InterpolationMode.Frame;
        }

        protected override void _Start()
        {
            base._Start();
            
            InterpolationMode = InterpolationMode.Frame;
        }

        protected override void _Delete()
        {
            base._Delete();

            if (_lineObject)
            {
                GameObject.Destroy(_lineObject);
            }
        }

    }
}

