using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using InternalRealtimeCSG;
using RealtimeCSG.Components;

namespace LightingTools.LightProbesVolumes
{
    [CustomEditor(typeof(LightProbesVolumeSettings))]
    public class LightProbesVolumeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            var volume = (LightProbesVolumeSettings)target;
            base.DrawDefaultInspector();
            if (GUILayout.Button("Create Light Probes in Selected Volume"))
            {
                Populate(volume);
            }
        }

        [MenuItem("GameObject/Light/Lightprobes Volume", false, 10)]
        static void CreateCustomGameObject(MenuCommand menuCommand)
        {
            // Create a custom game object
            GameObject volume = new GameObject("LightprobeVolume");
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(volume, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(volume, "Create " + volume.name);
            Selection.activeObject = volume;
            volume.AddComponent<LightProbesVolumeSettings>();
            volume.GetComponent<BoxCollider>().size = new Vector3(5, 2, 5);
        }

        public static void Populate(LightProbesVolumeSettings settings)
        {
            //GameObject gameObject, float horizontalSpacing, float verticalSpacing, float offsetFromFloor, int numberOfLayers, bool drawDebug, bool fillVolume, bool discardInsideGeometry, bool followFloor
            BoxCollider boxCollider = settings.gameObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                Debug.LogWarning("Box collider not found on " + settings.gameObject.name);
                return;
            }
            //Make sure collider is a trigger
            boxCollider.isTrigger = true;

            //avoid division by 0
            settings.horizontalSpacing = Mathf.Max(settings.horizontalSpacing, 0.01f);
            settings.verticalSpacing = Mathf.Max(settings.verticalSpacing, 0.01f);

            //Check if there is already a lightprobegroup component
            // if there is destroy it
            LightProbeGroup oldLightprobes = settings.gameObject.GetComponent<LightProbeGroup>();

            //Calculate Start Points at the top of the collider
            Vector3[] startPositions = StartPoints(boxCollider.size, boxCollider.center, boxCollider.transform, settings.horizontalSpacing);

            float minY = boxCollider.bounds.min.y;
            float maxY = boxCollider.bounds.max.y;

            float sizeY = boxCollider.size.y;
            int ycount = Mathf.FloorToInt((sizeY - settings.offsetFromFloor) / settings.verticalSpacing) + 1;

            List<Vector3> VertPositions = new List<Vector3>();

            int currentTrace = 0;

            //if followFloor we raycast from top to down in order to follow the static geometry (with colliders)
            if (settings.followFloor)
            {
                foreach (Vector3 startPos in startPositions)
                {
                    //RaycastHit hit;
                    RaycastHit[] hits;
                    Ray ray = new Ray();
                    ray.origin = startPos;
                    ray.direction = -Vector3.up;
                    hits = Physics.RaycastAll(ray, sizeY + 1, -1, QueryTriggerInteraction.Ignore);

                    //Validate hits
                    foreach (var hit in hits)
                    {
                        if (!IsValid(hit.collider.gameObject))
                        {
                            break;
                        }
                        if (hit.point.y + settings.offsetFromFloor < maxY && hit.point.y + settings.offsetFromFloor > minY)
                            VertPositions.Add(hit.point + new Vector3(0, settings.offsetFromFloor, 0));

                        int maxLayer = settings.fillVolume ? ycount : settings.numberOfLayers;

                        for (int i = 1; i < maxLayer; i++)
                        {
                            if (hit.point.y + settings.offsetFromFloor + i * settings.verticalSpacing < maxY && hit.point.y + settings.offsetFromFloor + settings.verticalSpacing > minY)
                                VertPositions.Add(hit.point + new Vector3(0, settings.offsetFromFloor + i * settings.verticalSpacing, 0));
                        }
                    }
                    EditorUtility.DisplayProgressBar("Tracing floor collisions", currentTrace.ToString() + "/" + startPositions.Length.ToString(), (float)currentTrace / (float)startPositions.Length);
                    currentTrace++;
                }
                EditorUtility.ClearProgressBar();
            }

            else
            {
                int maxLayer = settings.fillVolume ? ycount : settings.numberOfLayers;

                for (int i = 0; i < maxLayer; i++)
                {
                    foreach (Vector3 position in startPositions)
                    {
                        VertPositions.Add(position + Vector3.up * settings.verticalSpacing * i - Vector3.up * sizeY + Vector3.up * settings.offsetFromFloor);
                    }
                }
            }

            if (settings.drawDebug)
            {
                foreach (Vector3 position in VertPositions)
                {
                    Debug.DrawLine(position, position + Vector3.up * 0.5f, Color.red, 3);
                }
            }

            List<Vector3> validVertPositions = new List<Vector3>();

            //Inside Geometry test : take an arbitrary position in space and trace from that position to the probe position and back from the probe position to the arbitrary position. If the number of hits is different for both raycasts the probe is considered to be inside an object.
            //When using Draw Debug the arbitrary position is the Green cross in the air.
            if (settings.discardInsideGeometry)
            {
                int j = 0;
                Vector3 insideTestPosition = settings.gameObject.transform.position + settings.gameObject.GetComponent<BoxCollider>().center + new Vector3(0, maxY / 2, 0);
                if (settings.drawDebug)
                {
                    Debug.DrawLine(insideTestPosition + Vector3.up, insideTestPosition - Vector3.up, Color.green, 5);
                    Debug.DrawLine(insideTestPosition + Vector3.right, insideTestPosition - Vector3.right, Color.green, 5);
                    Debug.DrawLine(insideTestPosition + Vector3.forward, insideTestPosition - Vector3.forward, Color.green, 5);
                }
                foreach (Vector3 positionCandidate in VertPositions)
                {
                    EditorUtility.DisplayProgressBar("Checking probes inside geometry", j.ToString() + "/" + VertPositions.Count, (float)j / (float)VertPositions.Count);

                    Ray forwardRay = new Ray(insideTestPosition, Vector3.Normalize(positionCandidate - insideTestPosition));
                    Ray backwardRay = new Ray(positionCandidate, Vector3.Normalize(insideTestPosition - positionCandidate));
                    RaycastHit[] hitsForward;
                    RaycastHit[] hitsBackward;
                    hitsForward = Physics.RaycastAll(forwardRay, Vector3.Distance(positionCandidate, insideTestPosition), -1, QueryTriggerInteraction.Ignore);
                    hitsBackward = Physics.RaycastAll(backwardRay, Vector3.Distance(positionCandidate, insideTestPosition), -1, QueryTriggerInteraction.Ignore);
                    if (hitsForward.Length == hitsBackward.Length) validVertPositions.Add(positionCandidate);
                    else if (settings.drawDebug)
                        Debug.DrawRay(backwardRay.origin, backwardRay.direction * Vector3.Distance(positionCandidate, insideTestPosition), Color.cyan, 5);
                    j++;
                }
                EditorUtility.ClearProgressBar();
            }
            else
                validVertPositions = VertPositions;


            // Check if we have any hits
            if (validVertPositions.Count < 1)
            {
                Debug.Log("no valid hit for " + settings.gameObject.name);
                return;
            }

            LightProbeGroup LPGroup = oldLightprobes != null ? oldLightprobes : settings.gameObject.AddComponent<LightProbeGroup>();

            // Feed lightprobe positions
            Vector3[] ProbePos = new Vector3[validVertPositions.Count];
            for (int i = 0; i < validVertPositions.Count; i++)
            {
                ProbePos[i] = settings.gameObject.transform.InverseTransformPoint(validVertPositions[i]);
            }
            LPGroup.probePositions = ProbePos;

            //Finish
            Debug.Log("Finished placing " + ProbePos.Length + " probes for " + settings.gameObject.name);
        }

        private static bool IsValid(GameObject obj)
        {
            if (GameObjectUtility.AreStaticEditorFlagsSet(obj, StaticEditorFlags.ContributeGI))
            {
                return true;
            }
            var csgModel = obj.GetComponentInParent<CSGModel>();
            if(csgModel && GameObjectUtility.AreStaticEditorFlagsSet(csgModel.gameObject, StaticEditorFlags.ContributeGI))
            {
                return true;
            }
            return false;
        }

        static Vector3[] StartPoints(Vector3 size, Vector3 offset, Transform transform, float horizontalSpacing)
        {
            // Calculate count and start offset
            int xCount = Mathf.FloorToInt(size.x / horizontalSpacing) + 1;
            int zCount = Mathf.FloorToInt(size.z / horizontalSpacing) + 1;
            float startxoffset = (size.x - (xCount - 1) * horizontalSpacing) / 2;
            float startzoffset = (size.z - (zCount - 1) * horizontalSpacing) / 2;

            //if lightprobe count fits exactly in bounds, I know the probes at the maximum bounds will be rejected, so add offset
            if (startxoffset == 0)
                startxoffset = horizontalSpacing / 2;
            if (startzoffset == 0)
                startzoffset = horizontalSpacing / 2;

            Vector3[] vertPositions = new Vector3[xCount * zCount];

            int vertexnumber = 0;

            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < zCount; j++)
                {
                    Vector3 position = new Vector3
                    {
                        y = size.y / 2,
                        x = startxoffset + (i * horizontalSpacing) - (size.x / 2),
                        z = startzoffset + (j * horizontalSpacing) - (size.z / 2)
                    };

                    vertPositions[vertexnumber] = transform.TransformPoint(position + offset);

                    vertexnumber++;
                }
            }

            return vertPositions;
        }

    }
}