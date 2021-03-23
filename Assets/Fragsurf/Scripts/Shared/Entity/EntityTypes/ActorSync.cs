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

        public ActorSync(FSGameLoop game, int targetId)
            : base(game)
        {
            TargetIdentifier = targetId;
        }

        private IHasNetProps _targetObject;
        private int _targetIdentifier;

        [NetProperty]
        public int TargetIdentifier
        {
            get { return _targetIdentifier; }
            set
            {
                _targetIdentifier = value;
                _targetObject = GameObject.FindObjectsOfType<MonoBehaviour>(true)
                    .OfType<IHasNetProps>()
                    .FirstOrDefault(x => x.UniqueId == value);

                if (_targetObject is IHasNetProps dp)
                {
                    BuildNetProps(dp);
                }
            }
        }

        protected override void OnTick()
        {
            if (Game.IsHost && _targetObject == null)
            {
                Delete();
            }
        }
    }
}