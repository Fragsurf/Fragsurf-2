using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineHelperComponent : MonoBehaviour
{
    public float DestroyIn;

    private void Update()
    {
        if (DestroyIn > 0)
        {
            DestroyIn -= Time.deltaTime;
            if (DestroyIn <= 0)
            {
                GameObject.Destroy(gameObject);
            }
        }
    }
}
