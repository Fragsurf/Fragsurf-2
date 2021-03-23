using UnityEngine;

[System.Serializable]
public class bl_IndicatorInfo
{

    public Vector3 Direction = Vector3.zero;
    public Color Color = new Color(1, 1, 1, 0);
    public GameObject Sender;
    public Vector2 Size;
    public bool ShowDistance = false;
    [HideInInspector]public float TimeToShow = 7;
    [HideInInspector]public float PivotSize = 20;

    public bl_IndicatorInfo(Vector3 vector)
    {
        Direction = vector;
    }

}