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

        protected virtual void Awake()
        {
            _trigger = gameObject.AddComponent<FSMTrigger>();
            _trigger.OnTriggerEnter.AddListener((ent) =>
            {
                if (!EntityEnabled)
                {
                    return;
                }
                Fire("OnStartTouch", ent);
                OnStartTouch(ent);
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
                OnTouch(ent);
            });
        }

        protected virtual void OnTouch(NetEntity entity) { }
        protected virtual void OnStartTouch(NetEntity entity) { }
        protected virtual void OnEndTouch(NetEntity entity) { }

        protected override void OnOutputFired(BspEntityOutput output)
        {
            base.OnOutputFired(output);

            if(string.IsNullOrWhiteSpace(output.Parameter))
            {
                return;
            }

            if(output.Parameter.StartsWith("targetname", System.StringComparison.OrdinalIgnoreCase)
                || output.Parameter.StartsWith("classname", System.StringComparison.OrdinalIgnoreCase))
            {
                if(output.Activator == null)
                {
                    return;
                }

                var s = output.Parameter.Split(' ');
                if(s.Length != 2)
                {
                    return;
                }

                output.Activator.EntityName = s[1];
            }

            if (output.Parameter.StartsWith("gravity", System.StringComparison.OrdinalIgnoreCase))
            {
                if (!(output.Activator is Human hu)
                    || !(hu.MovementController is CSMovementController csm))
                {
                    return;
                }

                var s = output.Parameter.Split(' ');
                if (s.Length != 2
                    || !float.TryParse(s[1], out float grav))
                {
                    return;
                }

                csm.MoveData.GravityFactor = grav;
            }

            if (output.Parameter.StartsWith("basevelocity", System.StringComparison.OrdinalIgnoreCase))
            {
                if (!(output.Activator is Human hu))
                {
                    return;
                }

                var s = output.Parameter.Split(' ');
                if (s.Length != 4
                    || !float.TryParse(s[1], out float x)
                    || !float.TryParse(s[2], out float y)
                    || !float.TryParse(s[3], out float z))
                {
                    return;
                }

                var vec = new Vector3(x, z, y) * .0254f;
                hu.BaseVelocity += vec;
            }


        }

    }

    [EntityComponent("trigger_*", "func_bomb_target")]
    public class BspTriggerPartial : BspTrigger<FuncBrush>
    {

    }
}

