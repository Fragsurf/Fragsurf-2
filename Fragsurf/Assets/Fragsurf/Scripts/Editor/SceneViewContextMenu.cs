using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using InternalRealtimeCSG;
using System.Collections.Generic;
using RealtimeCSG.Components;
using RealtimeCSG;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;

[InitializeOnLoad]
public class SceneContextMenu : Editor
{

    private static double _downTime;
    private static Vector2 _downPos;

    static SceneContextMenu()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void GetHit(SceneView sceneView, out GameObject hitObject, out Vector3 hitPoint, out Vector3 hitNormal)
    {
        hitObject = HandleUtility.PickGameObject(Event.current.mousePosition, out int materialIndex);
        hitPoint = Vector3.zero;
        hitNormal = Vector3.zero;

        if (hitObject)
        {
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var mf = hitObject.GetComponentInChildren<MeshFilter>();
            if (mf)
            {
                EditorHandles_UnityInternal.IntersectRayMesh(ray, mf, out RaycastHit hit);
                hitPoint = hit.point;
                hitNormal = hit.normal;
            }
        }
        else
        {
            if (SceneQueryUtility.FindClickWorldIntersection(sceneView.camera, Event.current.mousePosition, out hitObject))
            {
                var intersection = SceneQueryUtility.FindMeshIntersection(sceneView.camera, Event.current.mousePosition);
                hitPoint = intersection.worldIntersection;
                hitNormal = intersection.worldPlane.normal;
            }
        }
    }

    static void OnSceneGUI(SceneView sceneview)
    {
        if (Event.current.type == EventType.MouseUp
            && Event.current.button == 1
            && EditorApplication.timeSinceStartup - _downTime < .09
            && Vector2.Distance(Event.current.mousePosition, _downPos) < 5)
        {
            GetHit(sceneview, out GameObject hitObject, out Vector3 hitPoint, out Vector3 hitNormal);
            Event.current.Use();
            ShowMenu(hitObject, hitPoint, hitNormal);
        }
        else if (Event.current.type == EventType.MouseDown
            && Event.current.button == 1)
        {
            _downTime = EditorApplication.timeSinceStartup;
            _downPos = Event.current.mousePosition;
        }
    }

    static void ShowMenu(GameObject obj, Vector3 hitPoint, Vector3 hitNormal)
    {
        var menu = new GenericMenu();

        if (obj)
        {
            menu.AddDisabledItem(new GUIContent(obj.name));
        }
        menu.AddDisabledItem(new GUIContent($"Position: {hitPoint}"));
        menu.AddDisabledItem(new GUIContent($"Normal: {hitNormal}"));
        menu.AddSeparator(string.Empty);

        AddCreationOptions(menu, hitPoint, hitNormal);

        menu.AddItem(new GUIContent("Play From Here"), false, () =>
        {
            if (!EditorApplication.isPlaying)
            {
                PlayStateNotifier.PlayFrom = hitPoint;
                EditorApplication.EnterPlaymode();
            }
            else
            {
                var gs = FSGameLoop.GetGameInstance(true);
                if (gs 
                    && Human.Local != null
                    && gs.EntityManager.TryFindEntity(Human.Local.EntityId, out NetEntity pl)
                    && pl is Human hu)
                {
                    hu.Origin = hitPoint;
                    hu.Velocity = Vector3.zero;
                    hu.BaseVelocity = Vector3.zero;
                }
            }
        });
        menu.AddSeparator(string.Empty);

        menu.AddItem(new GUIContent("Close"), false, () => { });
        menu.ShowAsContext();
    }

    static void SnapDown(params GameObject[] gameObjects)
    {
        var transforms = gameObjects.Select(x => x.transform).ToArray();
        Undo.RecordObjects(transforms, "Snap Down");

        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (GetFullBounds(gameObjects[i], out Bounds bounds, out Vector3 worldCenter))
            {
                var ray = new Ray(worldCenter, Vector3.down);
                var wasActive = gameObjects[i].activeSelf;
                gameObjects[i].SetActive(false);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f))
                {
                    var offset = gameObjects[i].transform.position - worldCenter;
                    gameObjects[i].transform.position = hitInfo.point + new Vector3(0f, bounds.extents.y, 0f) + offset;
                }
                gameObjects[i].SetActive(wasActive);
            }
        }
    }

    static bool GetFullBounds(GameObject go, out Bounds bounds, out Vector3 worldCenter)
    {
        bounds = new Bounds();
        worldCenter = new Vector3();

        var rens = go.GetComponentsInChildren<Renderer>();
        if (rens.Length > 0)
        {
            for (int i = 0; i < rens.Length; i++)
                if (i == 0)
                    bounds = rens[i].bounds;
                else
                    bounds.Encapsulate(rens[i].bounds);
            worldCenter = bounds.center;
            return true;
        }

        var cols = go.GetComponentsInChildren<Collider>();
        if (cols.Length > 0)
        {
            for (int i = 0; i < cols.Length; i++)
                if (i == 0)
                    bounds = cols[i].bounds;
                else
                    bounds.Encapsulate(cols[i].bounds);
            worldCenter = go.transform.TransformPoint(bounds.center);
            return true;
        }

        var brush = go.GetComponent<CSGBrush>();
        if (brush)
        {
            var brushBounds = BoundsUtilities.GetBounds(brush);
            worldCenter = brushBounds.Center;
            bounds = new Bounds()
            {
                center = brushBounds.Center,
                extents = brushBounds.Size * .5f,
                max = brushBounds.Max,
                min = brushBounds.Min,
                size = brushBounds.Size
            };
            return true;
        }

        return false;
    }

    static void SpawnObject(Vector3 hitPoint, Vector3 hitNormal, string command)
    {
        EditorApplication.ExecuteMenuItem(command);

        SetPositionBoundsOffset(Selection.activeGameObject, hitPoint, hitNormal);
    }

    static void SpawnPrefab(Vector3 hitPoint, Vector3 hitNormal, string path)
    {
        var obj = Resources.Load<GameObject>(path);
        if (obj)
        {
            var instance = GameObject.Instantiate(obj);
            Selection.activeGameObject = instance;
            Undo.RegisterCreatedObjectUndo(instance, "FSM Prefab");
            SetPositionBoundsOffset(instance, hitPoint, hitNormal);
        }
        else
        {
            Debug.LogError("Missing context menu prefab: " + path);
        }
    }

    static void SetPositionBoundsOffset(GameObject gameObject, Vector3 position, Vector3 normal)
    {
        Bounds bounds;
        if (GetFullBounds(gameObject, out bounds, out Vector3 worldCenter))
        {
            gameObject.transform.position = position + normal * bounds.extents.y;
        }
        else
            gameObject.transform.position = position;

        gameObject.transform.up = normal;
    }

    static void AddCreationOptions(GenericMenu menu, Vector3 position, Vector3 normal)
    {
        var mainMenuOptions = new MenuSet();

        var menuString = EditorGUIUtility.SerializeMainMenuToString();
        var menus = menuString.Split('\n');
        var pathParts = new List<string>();
        var menuPaths = new List<string>();

        var contextOptions = new Dictionary<string, List<string[]>>();

        foreach (var m in menus)
        {
            var s = m.Split(new string[] { "    " }, System.StringSplitOptions.None);
            var n = s[s.Length - 1];

            // Add to path parts.
            if (pathParts.Count <= s.Length)
                pathParts.Add(n);
            else
                pathParts[s.Length - 1] = n;

            // Get full path.
            var path = "";
            var parts = new List<string>();
            var menuSet = mainMenuOptions;
            for (int i = 0; i < s.Length; i++)
            {
                var pp = pathParts[i];
                parts.Add(pp);
                path += pp;

                if (!menuSet.children.ContainsKey(pp))
                {
                    var ms = new MenuSet();
                    ms.fullPath = path;
                    ms.pathPart = pp;
                    menuSet.children.Add(pp, ms);
                }

                menuSet = menuSet.children[pp];

                if (i != s.Length - 1)
                    path += "/";
            }

            // Context menus.
            if (path.Contains("CONTEXT"))
            {
                var cParts = path.Split('/');
                if (cParts.Length >= 3)
                {
                    var component = cParts[1];
                    var label = cParts[2];

                    if (!contextOptions.ContainsKey(component))
                        contextOptions.Add(component, new List<string[]>());

                    contextOptions[component].Add(new string[] {
						// Nice label.
						component + "/" + label,
						// Actual menu item.
						path });
                }
            }

            menuPaths.Add(path);
        }

        // Options for creating an object.
        var go = mainMenuOptions.children["GameObject"];
        var list = new List<string>();
        list.AddRange(go.children["3D Object"].GetSubPaths());
        list.AddRange(go.children["Realtime-CSG"].GetSubPaths());
        list.AddRange(go.children["Effects"].GetSubPaths());
        list.AddRange(go.children["Light"].GetSubPaths());

        foreach (var path in list)
        {
            menu.AddItem(new GUIContent(path), false, () => SpawnObject(position, normal, path));
        }

        menu.AddItem(new GUIContent("FSM/Spawn Point"), false, () => SpawnPrefab(position, normal, "FSM/Prefabs/SpawnPoint"));
        menu.AddItem(new GUIContent("FSM/Teleport Destination"), false, () => SpawnPrefab(position, normal, "FSM/Prefabs/TeleportDestination"));
    }

    private class MenuSet
    {
        public string pathPart = null;
        public string fullPath = null;
        public Dictionary<string, MenuSet> children = new Dictionary<string, MenuSet>();

        public List<string> GetSubPaths()
        {
            var l = new List<string>();

            foreach (var c in children)
                if (c.Value.children.Count == 0)
                    l.Add(c.Value.fullPath);
                else
                    l.AddRange(c.Value.GetSubPaths());

            return l;
        }
    }

}
