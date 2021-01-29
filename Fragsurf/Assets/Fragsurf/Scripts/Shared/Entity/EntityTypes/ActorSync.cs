using System.Linq;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public class ActorSync : NetEntity
    {
        public ActorSync(FSGameLoop game)
            : base(game)
        {

        }

        public ActorSync(int actorId, FSGameLoop game)
            : base(game)
        {
            UniqueActorId = actorId;
        }

        private IHasNetProps _actor;
        private int _uniqueActorId;

        [NetProperty]
        public int UniqueActorId 
        {
            get { return _uniqueActorId; }
            set {
                _uniqueActorId = value;
                _actor = GameObject.FindObjectsOfType<MonoBehaviour>()
                    .OfType<IHasNetProps>()
                    .FirstOrDefault(x => x.UniqueId == value);

                if(_actor != null && _actor is IHasNetProps dp)
                {
                    BuildNetProps(dp);
                }
            }
        }

        protected override void _Tick()
        {
            if (Game.IsHost && _actor == null)
            {
                Delete();
            }
        }
    }
}

