using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using System.Threading.Tasks;

namespace Fragsurf.Maps
{
    public class SceneMapProvider : IMapProvider
    {
        public async Task<List<BaseMap>> GetMapsAsync()
        {
            await Task.Delay(1); // poop
            var result = new List<BaseMap>();

            var sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                var sceneName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                if(sceneName.IndexOf('_') == -1)
                {
                    continue;
                }
                result.Add(new SceneMap()
                {
                    Author = "Fragsurf",
                    Cover = null,
                    Name = sceneName
                });
            }
            return result;
        }
    }
}

