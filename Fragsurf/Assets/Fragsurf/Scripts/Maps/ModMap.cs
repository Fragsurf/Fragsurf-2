using ModTool;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Maps
{
    public class ModMap : BaseMap
    {

        private Mod _mod;

        public ModMap(Mod mod)
        {
            _mod = mod;
            Name = mod.name;
        }

        protected override async Task<MapLoadState> _LoadAsync()
        {
            if(_mod == null 
                || !_mod.canLoad
                || !_mod.isValid)
            {
                return MapLoadState.Failed;
            }

            _mod.LoadAsync();

            while (_mod.loadState == ResourceLoadState.Loading)
            {
                await Task.Delay(100);
            }

            if(_mod.loadState == ResourceLoadState.Loaded)
            {
                return MapLoadState.Loaded;
            }

            return MapLoadState.Failed;
        }

        protected override async Task _UnloadAsync()
        {
            _mod.Unload();
        }

    }
}

