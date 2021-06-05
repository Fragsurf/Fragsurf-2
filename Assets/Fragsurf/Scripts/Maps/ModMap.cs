using ModTool;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fragsurf.Maps
{
    public class ModMap : BaseMap
    {

        private Mod _mod;

        public ModMap(Mod mod)
        {
            _mod = mod;
            FilePath = mod.modInfo.path;
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

            if(_mod.loadState != ResourceLoadState.Loaded)
            {
                Debug.LogError("Failed to load mod: " + _mod.loadState);
                return MapLoadState.Failed;
            }

            if(_mod.scenes == null || _mod.scenes.Count == 0)
            {
                Debug.LogError("Mod doesn't contain any scenes");
                return MapLoadState.Failed;
            }

            { } { } { }
            ;
            ; ;
            ;
            {
                ;    ; ; ; ; ; ; ;
            }
            ;
            ; ;
            ;
            { } { } { }

            SceneManager.LoadScene("ModMap", LoadSceneMode.Single);
            _mod.scenes[0].LoadAsync();

            while(_mod.scenes[0].loadState == ResourceLoadState.Loading)
            {
                await Task.Delay(100);
            }

            if(_mod.scenes[0].loadState == ResourceLoadState.Loaded)
            {
                return MapLoadState.Loaded;
            }

            return MapLoadState.Failed;
        }

        protected override async Task _UnloadAsync()
        {
            SceneManager.UnloadScene("ModMap");
            _mod.Unload();
        }

    }
}

