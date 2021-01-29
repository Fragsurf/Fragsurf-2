using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public partial class EntityManager
    {

        public List<NetEntity> FindEntitiesInRadius(Vector3 point, float radius)
        {
            var result = new List<NetEntity>();
            foreach(var ent in Entities)
            {
                if(ent.IsValid() && Vector3.Distance(point, ent.Origin) <= radius)
                {
                    result.Add(ent);
                }
            }
            return result;
        }

        public NetEntity FindEntity(GameObject obj)
        {
            var entObj = obj.GetComponentInParent<EntityGameObject>();
            if (entObj == null)
            {
                return null;
            }
            return entObj.Entity;
        }

        public NetEntity FindEntity(string targetName)
        {
            return Entities.Find(x => x.EntityName == targetName);
        }

        public bool TryFindEntity(int entityId, out NetEntity entity)
        {
            entity = FindEntity(entityId);
            return entity != null;
        }

        public NetEntity FindEntity(int entityId)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].EntityId == entityId)
                    return Entities[i];
            }
            return null;
        }

        public T FindEntity<T>(int entityId)
            where T : NetEntity
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (!(Entities[i] is T))
                    continue;
                if (Entities[i].EntityId == entityId)
                    return (T)Entities[i];
            }
            return null;
        }

        public IEnumerable OfType<T>()
            where T : NetEntity
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i] is T t)
                    yield return t;
            }
        }

    }
}

