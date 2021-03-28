using Fragsurf.Shared.Player;
using Fragsurf.Shared.Packets;
using UnityEngine;

namespace Fragsurf.Shared
{
    public class SettingReplicator : FSSharedScript
    {

        private const string _cpLabel = "repl";

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

        private void FSConsole_OnVariableChanged(DevConsoleEntry var)
        {
            if (!Game.IsHost 
                || !var.Flags.HasFlag(ConVarFlags.Replicator))
            {
                return;
            }

            var value = var.ToString();

            Game.TextChat.MessageAll($"Server setting changed: {var.Name} {value}");

            var replStr = $"{var.Name}={value}";
            var cp = PacketUtility.TakePacket<CustomPacket>();
            cp.Sc = SendCategory.UI_Important;
            cp.AddString(replStr);
            cp.Label = _cpLabel;
            Game.Network.BroadcastPacket(cp);
        }

        protected override void OnPlayerIntroduced(BasePlayer player)
        {
            if(!Game.IsHost)
            {
                return;
            }

            var replStr = string.Empty;
            var vars = DevConsole.GetVariablesWithFlags(ConVarFlags.Replicator);
            foreach(var var in vars)
            {
                var str = var + "=" + DevConsole.GetVariableAsString(var);
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
            cp.Label = _cpLabel;
            Game.Network.SendPacket(player.ClientIndex, cp);
        }

        protected override void OnPlayerPacketReceived(BasePlayer player, IBasePacket packet)
        {
            if(Game.IsHost
                || !(packet is CustomPacket cp)
                || !cp.Label.Equals(_cpLabel))
            {
                return;
            }

            var replStr = cp.GetString(0);
            var cmds = replStr.Split('&');

            if (cmds != null && cmds.Length > 0)
            {
                foreach(var cmd in cmds)
                {
                    var split = cmd.Split('=');
                    var varName = split[0];
                    var varValue = split[1];
                    DevConsole.SetVariable(varName, varValue, true, true);
                }
            }
        }

    }
}

