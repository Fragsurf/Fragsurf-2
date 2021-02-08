using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace HighlightPlus {
				
	public class TransparentWithDepth {

		static Material bmDepthOnly;


		[MenuItem ("GameObject/Effects/Highlight Plus/Add Depth To Transparent Object", false, 100)]
		static void AddDepthOption () {
			Renderer renderer = GetRenderer ();
			if (renderer == null)
				return;

			if (!EditorUtility.DisplayDialog ("Add Depth To Transparent Object", "This option will force the transparent object to write to the depth buffer by adding a new special material to the renderer (existing materials are preserved) so it can occlude and allow See-Through effect.\nOnly use on transparent objects.\n\nProceed?", "Yes", "No")) {
				return;
			}

			Material[] materials = renderer.sharedMaterials;
			for (int k = 0; k < materials.Length; k++) {
				if (materials [k] == bmDepthOnly) {
					EditorUtility.DisplayDialog ("Depth Support", "Already set! Nothing to do.", "Ok");
					return;
				}
			}
			if (materials == null) {
				renderer.sharedMaterial = bmDepthOnly;
			} else {
				List<Material> newMaterials = new List<Material> (materials);
				newMaterials.Insert (0, bmDepthOnly);
				renderer.sharedMaterials = newMaterials.ToArray ();
			}
		}

		[MenuItem ("GameObject/Effects/Highlight Plus/Remove Depth Compatibility", false, 101)]
		static void RemoveDepthOption () {

			Renderer renderer = GetRenderer ();
			if (renderer == null)
				return;

			Material[] materials = renderer.sharedMaterials;
			for (int k = 0; k < materials.Length; k++) {
				if (materials [k] == bmDepthOnly) {
					List<Material> newMaterials = new List<Material> (renderer.sharedMaterials);
					newMaterials.RemoveAt (k);
					renderer.sharedMaterials = newMaterials.ToArray ();
					return;
				}
			}

			for (int k = 0; k < materials.Length; k++) {
				if (materials [k] == bmDepthOnly) {
					EditorUtility.DisplayDialog ("Depth Support", "This object was not previously modified! Nothing to do.", "Ok");
					return;
				}
			}

		}


		static Renderer GetRenderer () {

			if (Selection.activeGameObject == null) {
				EditorUtility.DisplayDialog ("Depth Support", "This option can only be used on GameObjects.", "Ok");
				return null;
			}
			Renderer renderer = Selection.activeGameObject.GetComponent<Renderer> ();
			if (renderer == null) {
				EditorUtility.DisplayDialog ("Depth Support", "This option can only be used on GameObjects with a Renderer component attached.", "Ok");
				return null;
			}

			if (bmDepthOnly == null) {
				bmDepthOnly = Resources.Load<Material> ("HighlightPlus/HighlightPlusDepthWrite");
				if (bmDepthOnly == null) {
					EditorUtility.DisplayDialog ("Depth Support", "HighlightPlusDepthWrite material not found!", "Ok");
					return null;
				}
			}

			return renderer;
		}


	}
}