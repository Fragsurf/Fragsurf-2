using UnityEngine;
using System.Collections.Generic;

public static class bl_IndicatorUtils
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newSender"></param>
    /// <param name="newEntrys"></param>
    /// <returns></returns>
    public static bool CheckIfHaveSender(GameObject newSender, List<bl_Indicator> newEntrys)
    {
        //Verify is sender already exits
        if (newEntrys.Exists(x => x.Sender == newSender))
        {
            return true;
        }

        return false;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="newSender"></param>
    /// <param name="newEntrys"></param>
    /// <returns></returns>
    public static int GetSenderInList(GameObject newSender, List<bl_Indicator> newEntrys)
    {
        for (int i = 0; i < newEntrys.Count; i++)
        {
            if (newEntrys[i].Sender != null)
            {
                if (newEntrys[i].Sender.GetInstanceID() == newSender.GetInstanceID())
                {
                    return i;
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dot"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static float AngleCircumference(float dot, float angle)
    {
        float ac = angle;
        float circumference = 360f;
        ac = angle - 10;
        if (dot < 0)
        {
            ac = circumference - angle;
        }
        return ac;
    }


    /// <summary>
    /// Send a new indicator from a gameobject
    /// direction will be take from the go position.
    /// </summary>
    /// <param name="go"></param>
    public static void SetIndicator(this GameObject go)
    {
        //Just in case that go is destroy
        if (go == null)
            return;

        bl_IndicatorInfo info = new bl_IndicatorInfo(go.transform.position);
        info.Sender = go;
        bl_DamageDelegate.OnIndicator(info);
    }

    /// <summary>
    /// Send a new indicator with custom direction
    /// </summary>
    /// <param name="direction">custom direction</param>
    public static void SetIndicator(this GameObject go, Vector3 direction)
    {
        bl_IndicatorInfo info = new bl_IndicatorInfo(direction);
        info.Sender = go;
        bl_DamageDelegate.OnIndicator(info);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    /// <param name="customColor"></param>
    public static void SetIndicator(this GameObject go,Color customColor)
    {
        //Just in case that go is destroy
        if (go == null)
            return;

        bl_IndicatorInfo info = new bl_IndicatorInfo(go.transform.position);
        info.Sender = go;
        info.Color = customColor;
        bl_DamageDelegate.OnIndicator(info);
    }

    /// <summary>
    /// 
    /// </summary>
    public static Camera UseCamera
    {
        get
        {
            if (Camera.main != null)
            {
                return Camera.main;
            }
            else if (Camera.current != null)
            {
                return Camera.current;
            }
            else
            {
                return null;
            }
        }
    }
}