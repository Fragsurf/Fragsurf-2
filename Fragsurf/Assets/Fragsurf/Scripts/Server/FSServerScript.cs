using Fragsurf.Shared;

namespace Fragsurf.Server
{
    public class FSServerScript : FSSharedScript
    {
        protected override void _Hook()
        {
            base._Hook();

            Game.GetFSComponent<SocketManager>().OnServerStatusChanged += OnServerStatusChanged;
        }

        protected override void _Unhook()
        {
            base._Unhook();

            Game.GetFSComponent<SocketManager>().OnServerStatusChanged -= OnServerStatusChanged;
        }

        protected virtual void OnServerStatusChanged(ServerStatus newStatus) { }

    }
}
