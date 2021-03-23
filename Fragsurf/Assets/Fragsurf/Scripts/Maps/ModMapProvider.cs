using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ModTool;
using System.IO;

namespace Fragsurf.Maps
{
    public class ModMapProvider : IMapProvider
    {

        private static bool _searchDirsAdded;

        public async Task<List<BaseMap>> GetMapsAsync()
        {
            await Task.Delay(10); // idk

            if (!_searchDirsAdded)
            {
                if (Directory.Exists(Structure.MapsFolder))
                {
                    ModManager.instance.AddSearchDirectory(Structure.MapsFolder, true);
                }

                _searchDirsAdded = true;
            }

            var result = new List<BaseMap>();

            foreach(var mod in ModManager.instance.mods)
            {
                result.Add(new ModMap(mod));
            }

            return result;
        }

    }
}

