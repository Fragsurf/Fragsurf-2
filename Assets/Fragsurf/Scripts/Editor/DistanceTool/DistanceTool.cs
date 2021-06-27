using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DistanceTool : EditorWindow
{
    [MenuItem("Tools/3d Distance Tool")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(DistanceTool), false, "3d Distance Tool");
    }

    private bool _nearbyDist = true;
    private bool _groupDist = true;
    private bool _xDimension = true;
    private bool _yDimension = true;
    private bool _zDimension = true;
    private Color _labelColor = Color.black;
    private Color _distanceLineColor = Color.black;

    private bool _allDimensionsDisabled => !_xDimension && !_yDimension && !_zDimension;

    private void OnGUI()
    {
        _nearbyDist = EditorGUILayout.Toggle("Nearby Distance", _nearbyDist);
        _groupDist = EditorGUILayout.Toggle("Selected Distance", _groupDist);
        _xDimension = EditorGUILayout.Toggle("X Dimension", _xDimension);
        _yDimension = EditorGUILayout.Toggle("Y Dimension", _yDimension);
        _zDimension = EditorGUILayout.Toggle("Z Dimension", _zDimension);
        _labelColor = EditorGUILayout.ColorField("Label Color", _labelColor);
        _distanceLineColor = EditorGUILayout.ColorField("Distance Line Color", _distanceLineColor);
        SceneView.RepaintAll();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += SceneView_duringSceneGui;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= SceneView_duringSceneGui;
    }

    private Vector3[] ScatterDirections = new Vector3[]
    {
        Vector3.up,
        Vector3.left,
        Vector3.down,
        Vector3.right,
        Vector3.forward,
        Vector3.back
    };

    private void SceneView_duringSceneGui(SceneView obj)
    {
        var len = Selection.gameObjects.Length;
        if (len > 1)
        {
            if (_groupDist)
            {
                for (int i = 0; i < len; i++)
                {
                    for (int j = i + 1; j < len; j++)
                    {
                        MeasureDistance(Selection.gameObjects[i], Selection.gameObjects[j]);
                    }
                }
            }
            MeasureBounds(Selection.activeGameObject);
        }
        else if(len == 1)
        {
            if (_nearbyDist)
            {
                foreach (var dir in ScatterDirections)
                {

                    var startPos = Selection.gameObjects[0].transform.position;
                    var r = Selection.gameObjects[0].GetComponent<Renderer>();
                    if (r)
                    {
                        startPos = r.bounds.center;
                    }
                    if (Physics.Raycast(startPos, dir, out RaycastHit hit))
                    {
                        MeasureDistance(Selection.gameObjects[0], hit.collider.gameObject);
                    }
                }
            }
            MeasureBounds(Selection.gameObjects[0]);
        }
    }

    private void MeasureBounds(GameObject a)
    {
        if (_allDimensionsDisabled)
        {
            return;
        }
        var renderer = a.GetComponent<Renderer>();
        if(renderer == null)
        {
            return;
        }
        var center = renderer.bounds.center;
        var min = renderer.bounds.min;
        var max = renderer.bounds.max;
        var height = Mathf.Abs(max.y - min.y);
        var lengthRight = Mathf.Abs(max.x - min.x);
        var lengthFwd = Mathf.Abs(max.z - min.z);

        if (_yDimension)
        {
            MeasureDistance(min + new Vector3(-.1f, 0, -.1f), min + new Vector3(-.1f, height, -.1f), Color.green);
        }
        if (_xDimension)
        {
            MeasureDistance(min + new Vector3(0, 0, -.1f), min + new Vector3(lengthRight, 0, -.1f), Color.red);
        }
        if (_zDimension)
        {
            MeasureDistance(min + new Vector3(-.1f, 0, 0), min + new Vector3(-.1f, 0, lengthFwd), Color.blue);
        }
    }

    private void MeasureDistance(Vector3 a, Vector3 b, Color color)
    {
        Handles.color = color;
        Handles.DrawLine(a, b, 2f);
        var dist = (int)(Vector3.Distance(a, b) / .0254f);
        var center = (a + b) / 2f;
        var col = ColorUtility.ToHtmlStringRGB(_labelColor);
        Handles.Label(center, $"<color=#{col}>{dist}\"</color>", new GUIStyle()
        {
            fontSize = 16,
            richText = true
        });
    }

    private void MeasureDistance(GameObject a, GameObject b)
    {
        var colA = a.GetComponent<Collider>();
        var colB = b.GetComponent<Collider>();
        if(!colA || !colB)
        {
            return;
        }
        var startB = colB.transform.position;
        var pointA = SuperCollider.ClosestPointOnSurface(colA, startB, 0f);
        var pointB = SuperCollider.ClosestPointOnSurface(colB, pointA, 0f);
        pointA = SuperCollider.ClosestPointOnSurface(colA, pointB, 0f);
        Handles.color = _distanceLineColor;
        Handles.DrawLine(pointA, pointB, 2f);
        var midpoint = (pointA + pointB) / 2;
        var dist = (int)(Vector3.Distance(pointA, pointB) / .0254f);
        var col = ColorUtility.ToHtmlStringRGB(_labelColor);
        Handles.Label(midpoint, $"<color={col}>{dist}\"</color>", new GUIStyle()
        {
            fontSize = 16,
            richText = true
        });
    }
}

