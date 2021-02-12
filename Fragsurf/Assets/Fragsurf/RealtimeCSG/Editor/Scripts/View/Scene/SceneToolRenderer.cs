using RealtimeCSG.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RealtimeCSG
{
	internal static class SceneToolRenderer
	{
		static int meshGeneration = -1;

		static LineMeshManager lineMeshManager = new LineMeshManager();

		internal static void Cleanup()
		{
			meshGeneration = -1;
			lineMeshManager.Destroy();
		}

		static bool forceOutlineUpdate = false;
		internal static void SetOutlineDirty() { forceOutlineUpdate = false; }
		
		internal static void OnPaint(SceneView sceneView)
        {
            if (!sceneView)
            {
				return;
            }

			SceneDragToolManager.OnPaint(sceneView);

			if (Event.current.type != EventType.Repaint)
            {
				return;
            }

            if (RealtimeCSG.CSGSettings.GridVisible)
            {
				sceneView.showGrid = false;
				RealtimeCSG.CSGGrid.RenderGrid(sceneView);
			}
				
			if (RealtimeCSG.CSGSettings.IsWireframeShown(sceneView))
			{
				if (forceOutlineUpdate || meshGeneration != InternalCSGModelManager.MeshGeneration)
				{
					forceOutlineUpdate = false;
					meshGeneration = InternalCSGModelManager.MeshGeneration;
					lineMeshManager.Begin();
					for (int i = 0; i < InternalCSGModelManager.Brushes.Count; i++)
					{
						var brush = InternalCSGModelManager.Brushes[i];

						if (!brush)
                        {
							continue;
                        }

						//if (!brush.outlineColor.HasValue)
						//                  {
						//	//brush.outlineColor = ColorSettings.GetBrushOutlineColor(brush);
						//	brush.outlineColor = ColorSettings.SimpleOutlineColor;
						//}

						var color = Color.white;
						var comparison = brush.OperationType;
						var op = brush.GetComponentInParent<CSGOperation>();
                        if (op)
                        {
							comparison = op.OperationType;
                        }
                        switch (comparison)
                        {
							case Foundation.CSGOperationType.Additive:
								color = ColorSettings.SimpleOutlineAdditiveColor;
								break;
							case Foundation.CSGOperationType.Subtractive:
								color = ColorSettings.SimpleOutlineSubtractiveColor;
								break;
							case Foundation.CSGOperationType.Intersecting:
								color = ColorSettings.SimpleOutlineIntersectingColor;
								break;
                        }

						var brushTransformation = brush.compareTransformation.localToWorldMatrix;
						CSGRenderer.DrawSimpleOutlines(lineMeshManager, brush.brushNodeID, brushTransformation, color);
						CSGRenderer.DrawPolygonCenters(lineMeshManager, brush);
					}
					lineMeshManager.End();
				}

				MaterialUtility.LineDashMultiplier = 1.0f;
				MaterialUtility.LineThicknessMultiplier = 1.0f;
				MaterialUtility.LineAlphaMultiplier = 1.0f;
				lineMeshManager.Render(MaterialUtility.NoZTestGenericLine);
			}
		}
	}
}
