using UnityEngine;
using UnityEditor;
using InternalRealtimeCSG;
using System;
using System.Collections.Generic;
using System.Collections;
using RealtimeCSG.Helpers;
using RealtimeCSG.Components;

namespace RealtimeCSG
{
    internal sealed class SceneViewEventHandler
    {
        static bool mousePressed;

        static int prevFocusControl;

        private static bool TryGetSelectionBounds(out Bounds bounds)
        {
            bounds = default;
            var obj = Selection.activeGameObject;

            if (obj == null)
            {
                return false;
            }

            var brushes = obj.GetComponentsInChildren<CSGBrush>();
            if(brushes.Length > 0)
            {
                var csgBounds = BoundsUtilities.GetBounds(brushes);
                bounds = new Bounds()
                {
                    center = csgBounds.Center,
                    extents = csgBounds.Size * .5f,
                    min = csgBounds.Min,
                    max = csgBounds.Max
                };
                return true;
            }

            var renderers = obj.GetComponentsInChildren<Renderer>();
            for(int i = 0; i < renderers.Length; i++)
            {
                if(i == 0)
                {
                    bounds = renderers[i].bounds;
                }
                else
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                return true;
            }

            bounds = new Bounds()
            {
                center = Selection.activeGameObject.transform.position,
                size = Vector3.one * 3f
            };

            return false;
        }

        public static bool FastApproximately(float a, float b, float threshold)
        {
            return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
        }

        internal static void OnScene(SceneView sceneView)
        {
            if (Event.current.type == EventType.MouseMove)
            {
                sceneView.Repaint();
            }

            sceneView.cameraSettings.dynamicClip = false;
            sceneView.cameraSettings.easingEnabled = false;
            sceneView.cameraSettings.accelerationEnabled = false;

            //if (sceneView.orthographic)
            //{
            //    sceneView.camera.nearClipPlane = 1;
            //    sceneView.camera.farClipPlane = 1001f;

            //    var camPos = sceneView.pivot;
            //    var camForward = sceneView.camera.transform.forward;
            //    for (int i = 0; i < 3; i++)
            //    {
            //        if (!FastApproximately(camForward[i], 0, .01f))
            //        {
            //            camPos[i] = 1000;
            //        }
            //    }
            //    sceneView.pivot = camPos;
            //}

            //if (sceneView.orthographic)
            //{
            //    if ((Event.current.type == EventType.KeyDown
            //        || Event.current.type == EventType.KeyUp)
            //        && Event.current.keyCode == KeyCode.F)
            //    {
            //        Event.current.Use();
            //        sceneView.pivot = Vector3.zero;
            //        if (TryGetSelectionBounds(out Bounds bounds))
            //        {
            //            var sz = bounds.extents.magnitude;
            //            if (float.IsInfinity(sz))
            //            {
            //                sz = 10;
            //            }
            //            else if(sz < .05f)
            //            {
            //                sz = .05f;
            //            }
            //            sceneView.pivot = bounds.center;
            //            sceneView.size = sz;
            //        }
            //    }

            //    if(sceneView.size > 500)
            //    {
            //        sceneView.size = 500;
            //    }

            //    if(sceneView.size < .05f)
            //    {
            //        sceneView.size = .05f;
            //    }
            //}

            CSGSettings.RegisterSceneView(sceneView);

            if (!RealtimeCSG.CSGSettings.EnableRealtimeCSG
                || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            UpdateLoop.UpdateOnSceneChange();

            if (!RealtimeCSG.CSGSettings.EnableRealtimeCSG)
            {
                ColorSettings.isInitialized = false;
            }
            else if (!ColorSettings.isInitialized)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    ColorSettings.Update();
                }
            }

            if (!UpdateLoop.IsActive())
            {
                UpdateLoop.ResetUpdateRoutine();
            }

            if (Event.current.type == EventType.MouseDown
                || Event.current.type == EventType.MouseDrag)
            {
                mousePressed = true;
            }
            else if (Event.current.type == EventType.MouseUp
                || Event.current.type == EventType.MouseMove)
            {
                mousePressed = false;
            }

            SceneDragToolManager.OnHandleDragAndDrop(sceneView);
            RectangleSelectionManager.Update(sceneView);
            EditModeManager.InitSceneGUI(sceneView);

            if (Event.current.type == EventType.Repaint)
            {
                MeshInstanceManager.UpdateHelperSurfaces();
                SceneToolRenderer.OnPaint(sceneView);
            }
            else
            {
                SceneViewBottomBarGUI.ShowGUI(sceneView);
                SceneViewInfoGUI.DrawInfoGUI( sceneView );
            }

            //if(EditorWindow.mouseOverWindow == sceneView)
            {
                EditModeManager.OnSceneGUI(sceneView);

                TooltipUtility.InitToolTip(sceneView);

                if (!mousePressed)
                {
                    Handles.BeginGUI();
                    TooltipUtility.DrawToolTip(getLastRect: false);
                    Handles.EndGUI();
                }

                if (Event.current.type == EventType.Layout)
                {
                    var currentFocusControl = CSGHandles.FocusControl;
                    if (prevFocusControl != currentFocusControl)
                    {
                        prevFocusControl = currentFocusControl;
                        HandleUtility.Repaint();
                    }
                }
            }
        }
    }
}
