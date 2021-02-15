using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fragsurf.Shared;
using Fragsurf.Utility;
using Fragsurf.Actors;

namespace Fragsurf.Maps
{
    public class PlayTestMap : BaseMap
    {
        public override MapLoadState State { get; set; }

        public override void GetSpawnPoint(out Vector3 position, out Vector3 angles, int teamNumber = 255)
        {
            position = Vector3.zero;
            angles = Vector3.zero;

            var sps = GameObject.FindObjectsOfType<FSMSpawnPoint>();
            if (sps.Length > 0)
            {
                var rnd = sps[UnityEngine.Random.Range(0, sps.Length)];
                position = rnd.transform.position;
                angles = rnd.transform.eulerAngles;
            }

            var tp = GameObject.FindObjectOfType<PlayTest>();
            if (tp && tp.SpawnPoint != Vector3.zero)
            {
                position = tp.SpawnPoint;
                angles = Vector3.zero;
            }
        }

        public override async Task UnloadAsync()
        {
            await Task.Delay(100);
        }

        protected override async Task<MapLoadState> _LoadAsync()
        {
            await Task.Delay(100);
            return MapLoadState.Loaded;
        }
    }
}
