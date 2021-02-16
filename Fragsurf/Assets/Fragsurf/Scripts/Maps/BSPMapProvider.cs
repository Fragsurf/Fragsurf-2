using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Maps
{
    public class BSPMapProvider : IMapProvider
    {
        public async Task<List<BaseMap>> GetMapsAsync()
        {
            var result = new List<BaseMap>();

            result.Add(new BSPMap()
            {
                Author = "Unknown",
                FilePath = "D:\\Gemes\\steamapps\\common\\Counter-Strike Source\\cstrike\\download\\maps\\surf_aqua_fix.bsp",
                Name = "Stonework3"
            });

            await Task.Delay(100);

            return result;
        }
    }
}
