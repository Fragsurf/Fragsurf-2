using System;
using SourceUtils.ValveBsp.Entities;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Shared.Entity;
using Fragsurf.Server;
using System.Linq;
using Fragsurf.Shared.Player;
using Fragsurf.Shared;

namespace Fragsurf.BSP
{
	public class BspEntityMonoBehaviour : MonoBehaviour, IHasNetProps, IDamageable
	{
		public Entity Entity;
		public BspToUnity BspToUnity;
		public bool EntityEnabled { get; private set; } = true;
		public int UniqueId { get; set; }
		public bool HasAuthority => GameServer.Instance != null;

        public bool Dead => false;

        private List<BspEntityOutput> _pendingOutputs = new List<BspEntityOutput>();

		public IEnumerable<BspEntityMonoBehaviour> FindBspEntities(string targetName)
        {
			foreach(var entity in BspToUnity.Entities)
            {
				if(string.Equals(entity.Key.TargetName, targetName, StringComparison.OrdinalIgnoreCase))
                {
					yield return entity.Value;
                }
            }
        }

        private void Start()
        {
			EntityEnabled = Entity.StartDisabled == 0;

            if (!string.IsNullOrWhiteSpace(Entity.ParentName))
            {
				var parent = FindBspEntities(Entity.ParentName).FirstOrDefault();
				if(parent != null)
                {
					transform.SetParent(parent.transform);
                    transform.localRotation = Quaternion.identity;
                }
            }
			
			OnStart();
        }

		protected virtual void OnStart() { }
		protected virtual void OnUpdate() { }

		public void Input(BspEntityOutput output)
        {
            switch (output.TargetInput.ToLower())
            {
				case "enable":
					EntityEnabled = true;
					break;
				case "disable":
					EntityEnabled = false;
					break;
            }

			_Input(output);
        }

		private void Update()
		{
			for (int i = _pendingOutputs.Count - 1; i >= 0; i--)
			{
				_pendingOutputs[i].Delay -= Time.deltaTime;
				if (_pendingOutputs[i].Delay <= 0)
				{
					_Fire(_pendingOutputs[i]);
					_pendingOutputs.RemoveAt(i);
				}
			}
			OnUpdate();
		}

		protected void Fire(string outputName, NetEntity activator = null)
		{
			foreach (var prop in Entity.PropertyNames)
			{
				if (prop.StartsWith(outputName, StringComparison.OrdinalIgnoreCase))
				{
					var output = BspEntityOutput.Parse(outputName, Entity.GetRawPropertyValue(prop));
					output.Activator = activator;
					if(output.Delay > 0)
                    {
						_pendingOutputs.Add(output);
						continue;
                    }
					_Fire(output);
				}
			}
		}

		private void _Fire(BspEntityOutput output)
        {
			if (!output.TargetEntity.StartsWith("!"))
			{
				foreach (var ent in FindBspEntities(output.TargetEntity))
				{
					ent.Input(output);
				}
			}

            if (output.Activator != null)
            {
                BaseActivatorStuff(output);
            }

            OnOutputFired(output);
		}

		protected virtual void OnOutputFired(BspEntityOutput output)
        {

        }

		protected virtual void _Input(BspEntityOutput output)
        {

        }

        public virtual void Damage(DamageInfo dmgInfo)
        {
            var gameLoop = FSGameLoop.GetGameInstance(dmgInfo.Server);
            NetEntity activator = null;
            if(gameLoop)
            {
                activator = gameLoop.EntityManager.FindEntity(dmgInfo.AttackerEntityId);
            }
            Fire("OnDamaged", activator);
		}

		private void BaseActivatorStuff(BspEntityOutput output)
        {
            if (string.IsNullOrWhiteSpace(output.Parameter))
            {
                return;
            }

            if (output.Parameter.StartsWith("classname", StringComparison.OrdinalIgnoreCase))
            {
                if (output.Activator == null)
                {
                    return;
                }

                var s = output.Parameter.Split(' ');
                if (s.Length != 2)
                {
                    return;
                }

                output.Activator.ClassName = s[1];
            }

            if (output.Parameter.StartsWith("targetname", StringComparison.OrdinalIgnoreCase))
            {
                if (output.Activator == null)
                {
                    return;
                }

                var s = output.Parameter.Split(' ');
                if (s.Length != 2)
                {
                    return;
                }

                output.Activator.EntityName = s[1];
            }

            if (output.Parameter.StartsWith("gravity", StringComparison.OrdinalIgnoreCase))
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

	public class GenericBspEntityMonoBehaviour<T> : BspEntityMonoBehaviour
		where T : Entity
	{
		public new T Entity => base.Entity as T;
	}
}