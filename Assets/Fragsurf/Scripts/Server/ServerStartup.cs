using Fragsurf.Shared;
using UnityEngine;

namespace Fragsurf.Server
{
    [Inject(InjectRealm.Server)]
    public class ServerStartup : FSSharedScript
    {

        protected override async void _Start()
        {
            base._Start();

            if(!Structure.DedicatedServer)
            {
                return;
            }

            var gs = Game as GameServer;

            var result = await Game.GameLoader.CreateGameAsync(gs.DefaultMap, gs.DefaultGamemode);

            if(result != GameLoadResult.Success)
            {
                // something went wrong, notify
            }
        }

    }

}
