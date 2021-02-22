using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared 
{
    [Inject(InjectRealm.Shared)]
    public class Timelines : FSSharedScript
    {

        private List<TimelineReplay> _replays = new List<TimelineReplay>();
        private List<Timeline> _timelines = new List<Timeline>();

        protected override void OnPlayerRunCommand(IPlayer player)
        {
            foreach(var timeline in _timelines)
            {
                if(timeline.Human == player.Entity)
                {
                    timeline.RunCommand();
                }
            }
        }

        public Timeline CreateTimeline(Human hu)
        {
            var result = new Timeline(hu);
            _timelines.Add(result);
            return result;
        }

        public void RemoveTimeline(Timeline timeline)
        {
            _timelines.Remove(timeline);
        }

        public void Replay(Timeline timeline)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _replays.Add(new TimelineReplay(obj, timeline));
        }

        protected override void _Tick()
        {
            foreach(var replay in _replays)
            {
                replay.Tick();
            }
        }

    }

}


