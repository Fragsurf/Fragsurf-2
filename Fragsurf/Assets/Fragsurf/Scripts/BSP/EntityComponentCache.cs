using System;
using System.Collections.Generic;

namespace Fragsurf.BSP
{
	public class EntityComponentCache
	{
		public static Dictionary<string, Type> Registrations = new Dictionary<string, Type>();
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class EntityComponentAttribute : Attribute
	{
		public EntityComponentAttribute(params string[] classNames)
		{
			ClassNames = classNames;
		}
		public readonly string[] ClassNames;
	}
}