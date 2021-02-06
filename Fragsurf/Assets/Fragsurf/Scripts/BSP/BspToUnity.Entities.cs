using SourceUtils.ValveBsp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fragsurf.BSP
{
	public partial class BspToUnity
	{

		public Dictionary<Entity, BspEntityMonoBehaviour> Entities { get; private set; }

		private void GenerateEntities()
		{
			Entities = new Dictionary<Entity, BspEntityMonoBehaviour>();

			EntityComponentCache.Registrations.Clear();

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var currentType in assembly.GetTypes().Where(_ => typeof(MonoBehaviour).IsAssignableFrom(_)))
				{
					var attributes = currentType.GetCustomAttributes(typeof(EntityComponentAttribute), false);
					if (attributes.Length > 0)
					{
						var targetAttribute = attributes.First() as EntityComponentAttribute;
						foreach(var className in targetAttribute.ClassNames)
						{
							if (EntityComponentCache.Registrations.ContainsKey(className))
							{
								Debug.LogError("Duplicate entity component: " + className);
								continue;
							}
							EntityComponentCache.Registrations.Add(className, currentType);
						}
					}
				}
			}

			var entityParent = CreateGameObject("[Entities]");

			foreach (var e in _bsp.Entities)
			{
				GameObject obj = null;

				var model = e.GetRawPropertyValue("model");
				if(model != null && model[0] == '*')
				{
					var modelIndex = int.Parse(model.Replace("*", null));
					obj = _models[modelIndex];
					obj.gameObject.SetActive(true);
				}

				if (obj == null)
				{
					obj = new GameObject();
					obj.transform.SetParent(entityParent.transform, true);
				}

				obj.transform.position = e.Origin.ToUVector() * Options.WorldScale;
                if (e.PropertyNames.Contains("angles", StringComparer.OrdinalIgnoreCase))
                {
					obj.transform.forward = e.Angles.TOUDirection();
				}
				//obj.transform.Rotate(new Vector3(-e.Angles.X, -e.Angles.Y, -e.Angles.Z));
				obj.name = $"{e.ClassName} [${e.TargetName}]";

				var entityComponentType = GetEntityComponentType(e.ClassName);
				var component = obj.AddComponent(entityComponentType);
				if (component is BspEntityMonoBehaviour bspEntity)
				{
					bspEntity.Entity = e;
					bspEntity.BspToUnity = this;
					Entities.Add(e, bspEntity);
				}
			}
		}

		private Type GetEntityComponentType(string className)
        {
			if (EntityComponentCache.Registrations.ContainsKey(className))
            {
				return EntityComponentCache.Registrations[className];
			}

			foreach (var entry in EntityComponentCache.Registrations)
			{
				if (entry.Key.EndsWith("*"))
				{
					var partial = entry.Key.Replace("*", null);
					if (className.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
					{
						return entry.Value;
					}
				}
			}

			return typeof(BspEntityMonoBehaviour);
        }

	}
}