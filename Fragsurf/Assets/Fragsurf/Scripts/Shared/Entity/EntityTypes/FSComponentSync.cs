using System.Linq;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public class FSComponentSync : NetEntity
    {
        public FSComponentSync(FSGameLoop game)
            : base(game)
        {

        }

        public FSComponentSync(FSGameLoop game, int targetId)
            : base(game)
        {
            ComponentUniqueID = targetId;
        }

        private IHasNetProps _targetObject;
        private int _targetIdentifier;

        [NetProperty]
        public int ComponentUniqueID
        {
            get { return _targetIdentifier; }
            set
            {
                _targetIdentifier = value;

                Debug.Log(Game.IsHost + ":" + value);

                foreach (var component in Game.FSComponents)
                {
                    if (!(component is IHasNetProps props)
                        || props.UniqueId != _targetIdentifier)
                    {
                        continue;
                    }
                    _targetObject = props;
                    BuildNetProps(props);
                    Debug.Log(Game.IsHost + ":" + _targetObject);
                    break;
                }
            }
        }

        protected override void _Tick()
        {
            if (Game.IsHost && _targetObject == null)
            {
                Delete();
            }
        }
    }
}

