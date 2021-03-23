public static class bl_DamageDelegate
{
    //Delegate for invoke a new / update indicator.
    public delegate void NewIndicator(bl_IndicatorInfo info);
    public static NewIndicator OnIndicator;

    //Delegate for invoke a new damage effect.
    public delegate void NewDamageDelegate(bl_DamageInfo info);
    public static NewDamageDelegate OnDamageEvent;

    //Delegate for invoke when player die;
    public delegate void DieDelegate();
    public static DieDelegate OnDieEvent;

    /// <summary>
    /// Invoke this for send a new indicator info
    /// </summary>
    /// <param name="_info"></param>
    public static void NewEntry(bl_IndicatorInfo _info)
    {
        if (OnIndicator != null)
            OnIndicator(_info);
    }

    /// <summary>
    /// Invoke this for call Damage HUD.
    /// </summary>
    public static void OnDamage(bl_DamageInfo info)
    {
        if (OnDamageEvent != null)
            OnDamageEvent(info);
    }

    /// <summary>
    /// Call this when player die.
    /// </summary>
    public static void OnDie()
    {
        if (OnDieEvent != null)
            OnDieEvent();
    }

}