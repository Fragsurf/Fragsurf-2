using Fragsurf.Shared.Packets;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public partial class EntityManager
    {

        private float _sendTimer;
        private int _entityRate = 30;

        [ConVar("net.entityrate", "How many times entities are networked per second")]
        public int EntityRate
        {
            get => _entityRate;
            set => _entityRate = Mathf.Clamp(value, 1, 64);
        }

        private void UpdateEntity(EntityUpdate update)
        {
            if (_deletedEntities.Contains(update.EntityId))
            {
                return;
            }

            var ent = FindEntity(update.EntityId);
            if(ent == null)
            {
                ent = NetEntity.CreateInstanceOfEntity(Game, update.EntityTypeId);
                if (ent == null)
                {
                    Debug.LogError("Failed to instantiate entity of type: " + update.EntityTypeId);
                    return;
                }
                ent.EntityId = update.EntityId;
                AddEntity(ent, false);
            }

            ent.LoadPropData(update.PropData);

            OnEntityUpdated?.Invoke(ent, update.ChangedTick, update.ChangedTime);
        }

        private void TrySendEntities()
        {
            if (!Game.Live || !Game.IsHost)
            {
                return;
            }

            _sendTimer -= Time.fixedDeltaTime;

            if (_sendTimer > 0)
            {
                return;
            }

            _sendTimer = 1f / _entityRate;

            foreach (var ent in Game.EntityManager.Entities)
            {
                if (ent.State != NetEntityState.Deleted
                    && ent.PropertyHasChanged())
                {
                    var entPacket = PacketUtility.TakePacket<EntityUpdate>();
                    entPacket.Load(ent);
                    Game.Network.BroadcastPacket(entPacket);
                }
            }
        }

        private void BroadcastEntityDeleted(NetEntity entity)
        {
            if (!Game.IsHost)
            {
                return;
            }

            var deletePacket = PacketUtility.TakePacket<DeleteEntity>();
            deletePacket.EntityId = entity.EntityId;
            Game.Network.BroadcastPacket(deletePacket);
        }

    }
}

