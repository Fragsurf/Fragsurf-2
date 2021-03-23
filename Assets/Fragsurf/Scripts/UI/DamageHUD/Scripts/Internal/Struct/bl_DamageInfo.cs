using UnityEngine;

[System.Serializable]
public class bl_DamageInfo  {

    public float Damage = 0;
    public GameObject Sender;

    public bl_DamageInfo(float _damage)
    {
        Damage = _damage;
    }
}