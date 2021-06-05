using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fragsurf.Maps
{
    public class SceneMap : BaseMap
    {

        public override Texture LoadCoverImage()
        {
            return Resources.Load<Texture>(Name);
        }

        protected override async Task<MapLoadState> _LoadAsync()
        {
            try
            {
                var ao = SceneManager.LoadSceneAsync(Name, LoadSceneMode.Single);
                while (!ao.isDone)
                {
                    await Task.Delay(50);
                }
                return MapLoadState.Loaded;
            }
            catch(Exception e)
            {
                Debug.LogError("Failed to load SceneMap: " + Name + "\n" + e.Message);
            }
            return MapLoadState.Failed;
        }

        protected override async Task _UnloadAsync()
        {
            SceneManager.UnloadScene(Name);
            //try
            //{
                //var ao = SceneManager.UnloadSceneAsync(Name, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                //while (!ao.isDone)
                //{
                //    await Task.Delay(50);
                //}
            //}
            //catch(Exception e)
            //{
            //    Debug.LogError("Failed to unloaded SceneMap: " + Name + "\n" + e.Message);
            //}
        }
    }
}

