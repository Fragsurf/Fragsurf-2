using Fragsurf.Shared.Player;
using Fragsurf.Shared.Packets;

namespace Fragsurf.Shared
{
    public class SettingReplicator : FSSharedScript
    {

        protected override void _Hook()
        {
            base._Hook();

            DevConsole.OnVariableChanged += FSConsole_OnVariableChanged;
        }

        protected override void _Unhook()
        {
            base._Unhook();

            DevConsole.OnVariableChanged -= FSConsole_OnVariableChanged;
        }

        protected override void OnGameLoaded()
        {
            var locked = (!Game.IsHost && Server.GameServer.Instance == null) || Game.GamemodeLoader.Gamemode.LockVars;

            DevConsole.LockFlags(locked, ConVarFlags.Replicator);
            DevConsole.LockFlags(locked, ConVarFlags.Cheat);
        }

        private void FSConsole_OnVariableChanged(string varName)
        {
            if (!Game.IsHost || !DevConsole.VariableHasFlags(varName, ConVarFlags.Replicator))
            {
                return;
            }

            // Broadcast new value for varName
        }

        protected override void OnPlayerIntroduced(IPlayer player)
        {
            if(!Game.IsHost)
            {
                return;
            }

            // Send all replicators to player
        }

        protected override void OnPlayerPacketReceived(IPlayer player, IBasePacket packet)
        {
            if(Game.IsHost)
            {
                return;
            }

            // if packet is replicate
        }

    }
}

