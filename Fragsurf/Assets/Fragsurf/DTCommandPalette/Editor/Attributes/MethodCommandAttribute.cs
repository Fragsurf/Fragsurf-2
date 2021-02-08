using System;
using System.Collections;
using UnityEngine;

namespace DTCommandPalette {
	/// <summary>
	/// Marking a method with this attribute adds it to the list
	/// that the CommandPalette queries from
	///
	/// Ex. [MethodCommand]
	///     public void Give5HealthToShip() {
	///
	/// Can also specify a display name for the entry:
	/// Ex. [MethodCommand("Give Five Health")]
	///     public void Give5HealthToShip() {
	/// </summary>
	[AttributeUsageAttribute(AttributeTargets.Method)]
	public class MethodCommandAttribute : Attribute {
		public MethodCommandAttribute(string methodDisplayName) {
			this.methodDisplayName = methodDisplayName;
		}

		public MethodCommandAttribute() {
			this.methodDisplayName = null;
		}

		public string methodDisplayName;
	}
}