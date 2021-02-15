using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Maps
{
    public class SceneMap : BaseMap
    {
        protected override async Task<MapLoadState> _LoadAsync()
        {
            await Task.Delay(1);
            return MapLoadState.Loaded;
        }

        protected override async Task _UnloadAsync()
        {
            await Task.Delay(1);
        }
    }
}

