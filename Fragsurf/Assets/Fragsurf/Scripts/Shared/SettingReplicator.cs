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
            var locked = (FSGameLoop.GetGameInstance(true) == null) || Game.GamemodeLoader.Gamemode.LockVars;

            DevConsole.LockFlags(locked, ConVarFlags.Replicator);
            DevConsole.LockFlags(locked, ConVarFlags.Cheat);
            DevConsole.LockFlags(locked, ConVarFlags.Gamemode);
        }

        private void FSConsole_OnVariableChanged(string varName)
        {
            if (!Game.IsHost 
                || !DevConsole.VariableHasFlags(varName, ConVarFlags.Replicator))
            {
                return;
            }

            var replStr = varName + " " + DevConsole.GetVariableAsString(varName);
            var cp = PacketUtility.TakePacket<CustomPacket>();
            cp.Sc = SendCategory.UI_Important;
            cp.AddString(replStr);
            cp.Label = "Replicate";
            Game.Network.BroadcastPacket(cp);
        }

        protected override void OnPlayerIntroduced(IPlayer player)
        {
            if(!Game.IsHost)
            {
                return;
            }

            var replStr = string.Empty;
            var vars = DevConsole.GetVariablesWithFlags(ConVarFlags.Replicator);
            foreach(var var in vars)
            {
                var str = var + " " + DevConsole.GetVariableAsString(var);
                if(replStr == string.Empty)
                {
                    replStr = str;
                }
                else
                {
                    replStr += "&" + str;
                }
            }
            var cp = PacketUtility.TakePacket<CustomPacket>();
            cp.Sc = SendCategory.UI_Important;
            cp.AddString(replStr);
            cp.Label = "Replicate";
            Game.Network.SendPacket(player.AccountId, cp);
        }

        protected override void OnPlayerPacketReceived(IPlayer player, IBasePacket packet)
        {
            if(Game.IsHost
                || FSGameLoop.GetGameInstance(true) != null
                || !(packet is CustomPacket cp && cp.Label == "Replicate"))
            {
                return;
            }

            var replStr = cp.GetString(0);
            var cmds = replStr.Split('&');
            if(cmds != null && cmds.Length > 0)
            {
                foreach(var cmd in cmds)
                {
                    DevConsole.ExecuteLine(cmd);
                }
            }
        }

    }
}

