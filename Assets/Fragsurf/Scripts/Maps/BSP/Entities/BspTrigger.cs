using Fragsurf.Actors;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using SourceUtils.ValveBsp.Entities;
using UnityEngine;

namespace Fragsurf.BSP
{
    public class BspTrigger<T> : GenericBspEntityMonoBehaviour<T>
        where T : Entity
    {
        public string TargetName => Entity != null ? Entity.TargetName : string.Empty;

        private FSMTrigger _trigger;

        private bool PassesFilter(NetEntity ent)
        {
            if(Entity == null)
            {
                return true;
            }

            var fn = Entity.GetRawPropertyValue("filtername");
            if (!string.IsNullOrEmpty(fn))
            {
                foreach (var f in FindBspEntities(fn))
                {
                    if (!(f is BspBaseFilter tf))
                    {
                        continue;
                    }
                    return tf.Passes(ent);
                }
            }
            return true;
        }

        private void TryStartTouch(NetEntity ent)
        {
            Fire("OnStartTouch", ent);
            if (!PassesFilter(ent)) 
            { 
                return;
            }
            Fire("OnTrigger", ent);
            OnStartTouch(ent);
        }

        private void TryTouch(NetEntity ent)
        {
            Fire("OnTouching", ent);
            if (!PassesFilter(ent))
            {
                return;
            }
            Fire("OnTrigger", ent);
            OnTouch(ent);
        }

        protected virtual void Awake()
        {
            _trigger = gameObject.AddComponent<FSMTrigger>();
            _trigger.OnTriggerEnter.AddListener((ent) =>
            {
                if (!EntityEnabled)
                {
                    return;
                }
                TryStartTouch(ent);
            });

            _trigger.OnTriggerExit.AddListener((ent) =>
            {
                if (!EntityEnabled)
                {
                    return;
                }
                Fire("OnEndTouch", ent);
                OnEndTouch(ent);
            });

            _trigger.OnTriggerStay.AddListener((ent) =>
            {
                if (!EntityEnabled)
                {
                    return;
                }
                TryTouch(ent);
            });
        }

        protected virtual void OnTouch(NetEntity entity) { }
        protected virtual void OnStartTouch(NetEntity entity) { }
        protected virtual void OnEndTouch(NetEntity entity) { }

    }

    [EntityComponent("trigger_*", "func_bomb_target")]
    public class BspTriggerPartial : BspTrigger<TriggerMultiple>
    {

    }
}

