using UnityEngine;
using System.Threading.Tasks;

namespace Fragsurf.Maps
{
    public interface IFragsurfMap
    {
        MapData Data { get; }
        MapLoadState State { get; }
        void Tick();
        Task<MapLoadState> LoadAsync();
        Task UnloadAsync();
        void Hotload();
        void GetSpawnPoint(out Vector3 position, out Vector3 angles, int teamNumber = 255);
    }
}
