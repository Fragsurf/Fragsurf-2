using System.Threading.Tasks;
using UnityEngine;

namespace Fragsurf.Maps
{
    public class PlayTestMap : BaseMap
    {

        public override Texture LoadCoverImage()
        {
            return null;
        }

        protected override async Task<MapLoadState> _LoadAsync()
        {
            await Task.Delay(100);
            return MapLoadState.Loaded;
        }

        protected override async Task _UnloadAsync()
        {
            await Task.Delay(100);
        }
    }
}
