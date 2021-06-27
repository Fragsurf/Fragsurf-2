using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lidgren.Network;
using Fragsurf.Actors;
using System;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;

namespace Fragsurf.Shared.Entity
{

    public enum TriggerEventType
    {
        None,
        Enter,
        Stay,
        Exit
    }

    public delegate void TimedEntityEventHandler(NetEntity entity, int remoteTick, double remoteTime);
    public delegate void HumanTriggerHandler(Human hu, FSMTrigger trigger, TriggerEventType type, float offset = 0f);

    public partial class EntityManager : FSComponent
    {

        public event Action<NetEntity> OnEntityAdded;
        public event Action<NetEntity> OnEntityDestroyed;
        public event Action<Human> OnHumanSpawned;
        public event Action<Human> OnHumanKilled;
        public event Action<Human, DamageInfo> OnHumanDamaged;
        public event TimedEntityEventHandler OnEntityUpdated;
        public event HumanTriggerHandler OnHumanTrigger;

        private int _entityIndex;
        private HashSet<int> _deletedEntities = new HashSet<int>();

        public List<NetEntity> Entities { get; } = new List<NetEntity>();
        public int NextEntityIndex => ++_entityIndex;

        [ConVar("entity.nodamage", "", ConVarFlags.Replicator)]
        public bool NoDamage { get; set; }
        [ConVar("entity.infiniteammo", "", ConVarFlags.Replicator | ConVarFlags.Cheat)]
        public bool InfiniteAmmo { get; set; }
        [ConVar("entity.friendlyfire", "", ConVarFlags.Replicator)]
        public bool FriendlyFire { get; set; }
        [ConVar("entity.interpdelay", "", ConVarFlags.Replicator | ConVarFlags.Cheat)]
        public float InterpDelay { get; set; } = .2f;
        [ConVar("entity.decay", "", ConVarFlags.Replicator | ConVarFlags.Cheat)]
        public float EntityDecay { get; set; } = 5f;

        protected override void _Hook()
        {
            Game.PlayerManager.OnPlayerPacketReceived += OnPlayerPacketReceived;
            Game.GameLoader.GameLoaded += () =>
            {
                _deletedEntities.Clear();
            };
        }

        protected override void _Unhook()
        {
            Game.PlayerManager.OnPlayerPacketReceived -= OnPlayerPacketReceived;
        }

        private void OnPlayerPacketReceived(BasePlayer player, IBasePacket packet)
        {
            if (!Game.IsHost)
            {
                switch (packet)
                {
                    case DeleteEntity delEntity:
                        DeleteEntity(delEntity.EntityId);
                        break;
                    case EntityUpdate entityUpdate:
                        UpdateEntity(entityUpdate);
                        break;
                    case DamageInfoPacket dmgInfo:
                        var hu = FindEntity(dmgInfo.DamageInfo.VictimEntityId) as Human;
                        if(hu != null)
                        {
                            BroadcastHumanDamaged(hu, dmgInfo.DamageInfo);
                        }
                        break;
                }
            }

            if (packet is EntityCommand entCommand)
            {
                FindEntity(entCommand.EntityId)?.ReceiveCommand(entCommand);
            }
        }

        protected override void _Update()
		{
            for (int i = 0; i < Entities.Count; i++)
			{
                if (!Entities[i].IsLive)
                {
                    continue;
                }
				Entities[i].Update();
			}
		}

        protected override void _Tick()
		{
			for (int i = 0; i < Entities.Count; i++)
			{
                if(Entities[i].State == NetEntityState.Deleted)
                {
                    DeleteEntity(Entities[i].EntityId);
                    continue;
                }

                if (!Entities[i].IsLive)
                {
                    continue;
                }

                Entities[i].Tick();
			}

            TrySendEntities();
		}

        protected override void _LateUpdate()
		{
			for (int i = 0; i < Entities.Count; i++)
			{
                if (!Entities[i].IsLive)
                    continue;
                Entities[i].LateUpdate();
			}
		}

        protected override void _Destroy()
        {
            DeleteAllEntities();
        }

#if UNITY_EDITOR
        public override void DrawGizmos()
        {
            for(int i = 0; i < Entities.Count; i++)
            {
                Entities[i].OnDrawGizmos();
            }
        }
#endif

		public int AddEntity(NetEntity entity, bool setIndex = true)
		{
			if (Entities.Contains(entity) || entity.IsLive)
            {
                Debug.LogError("Adding duplicate or started entity to game..." + entity.GetType().Name);
                return 0;
            }

			Entities.Add(entity);

            if(setIndex)
            {
                entity.EntityId = NextEntityIndex;

                // do this so clients can create their own client-side entities
                // and its id wont overlap with server-side entities
                if (!Game.IsHost)
                {
                    entity.EntityId *= -1;
                }
            }

            entity.Initialize();

            OnEntityAdded?.Invoke(entity);

            return entity.EntityId;
		}

        public void DeleteAllEntities()
        {
            for(int i = Entities.Count - 1; i >= 0; i--)
            {
                DeleteEntity(Entities[i].EntityId);
            }
        }

        public void DeleteEntity(int entityId)
        {
            _deletedEntities.Add(entityId);
            var ent = FindEntity(entityId);
            if(ent != null)
            {
                if (ent.State != NetEntityState.Deleted)
                {
                    ent.Delete();
                }

                Entities.Remove(ent);

                if (Game.Live)
                {
                    BroadcastEntityDeleted(ent);
                    OnEntityDestroyed?.Invoke(ent);
                }
            }
        }

        public void RaiseHumanTrigger(Human hu, FSMTrigger trigger, TriggerEventType evType, float offset = 0f)
        {
            if(hu == null)
            {
                return;
            }
            OnHumanTrigger?.Invoke(hu, trigger, evType, 0f);
        }

        public void RaiseHumanKilled(Human hu)
        {
            if(hu == null)
            {
                return;
            }
            OnHumanKilled?.Invoke(hu);
        }

        public void RaiseHumanSpawned(Human hu)
        {
            if(hu == null)
            {
                return;
            }
            OnHumanSpawned?.Invoke(hu);
        }

        public void BroadcastHumanDamaged(Human hu, DamageInfo dmgInfo)
        {
            if (hu == null)
            {
                return;
            }

            OnHumanDamaged?.Invoke(hu, dmgInfo);

            if (Game.IsHost)
            {
                var packet = PacketUtility.TakePacket<DamageInfoPacket>();
                packet.DamageInfo = dmgInfo;
                Game.Network.BroadcastPacket(packet);
            }
        }

        [ConCommand("entity.count")]
        private void PrintEntityCount()
        {
            DevConsole.WriteLine("Entity count: " + Entities.Count);
        }

    }

}
