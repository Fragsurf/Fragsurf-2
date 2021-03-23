using Fragsurf.Actors;
using Fragsurf.Shared.Entity;
using SourceUtils.ValveBsp.Entities;

namespace Fragsurf.BSP
{
    public class BspTrigger<T> : GenericBspEntityMonoBehaviour<T>
        where T : Entity
    {
        public string TargetName => Entity != null ? Entity.TargetName : string.Empty;

        private FSMTrigger _trigger;

        protected virtual void Awake()
        {
            _trigger = gameObject.AddComponent<FSMTrigger>();
            _trigger.OnTriggerEnter.AddListener((ent) =>
            {
                if (!EntityEnabled)
                {
                    return;
                }
                Fire("OnStartTouch");
                OnStartTouch(ent);
            });
            _trigger.OnTriggerExit.AddListener((ent) =>
            {
                if (!EntityEnabled)
                {
                    return;
                }
                Fire("OnEndTouch");
                OnEndTouch(ent);
            });
            _trigger.OnTriggerStay.AddListener((ent) =>
            {
                if (!EntityEnabled)
                {
                    return;
                }
                OnTouch(ent);
            });
        }

        protected virtual void OnTouch(NetEntity entity) { }
        protected virtual void OnStartTouch(NetEntity entity) { }
        protected virtual void OnEndTouch(NetEntity entity) { }

    }

    [EntityComponent("trigger_*", "func_bomb_target")]
    public class BspTriggerPartial : BspTrigger<FuncBrush>
    {

    }
}

