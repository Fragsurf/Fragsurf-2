using Fragsurf.Gamemodes.Bunnyhop;
using UnityEngine;

public class btimestest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var p = new BTimesSpeedrunMapDataProvider();
        p.CreateZones("bhop_white");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
