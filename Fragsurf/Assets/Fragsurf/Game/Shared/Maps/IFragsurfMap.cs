using UnityEngine;
using System.Threading.Tasks;

namespace Fragsurf.Shared.Maps
{
    public interface IFragsurfMap
    {
        string Name { get; }
        MapLoadState State { get; }
        void Tick();
        Task<MapLoadState> LoadAsync();
        Task UnloadAsync();
        void Hotload();
        void GetSpawnPoint(out Vector3 position, out Vector3 angles, int teamNumber = 255);
    }
}
