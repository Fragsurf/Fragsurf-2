using Fragsurf.Shared;

namespace Fragsurf.Client
{
    public class FSClientScript : FSSharedScript
    {
        protected override void _Hook()
        {
            base._Hook();

            Game.GetFSComponent<ClientSocketManager>().OnStatusChanged += OnClientStatusChanged;
        }

        protected override void _Unhook()
        {
            base._Unhook();
            Game.GetFSComponent<ClientSocketManager>().OnStatusChanged -= OnClientStatusChanged;
        }

        protected virtual void OnClientStatusChanged(ClientSocketStatus status, string reason) { }
    }
}
