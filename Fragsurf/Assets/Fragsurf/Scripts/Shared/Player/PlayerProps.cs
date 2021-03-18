using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;
using System.Collections.Generic;

namespace Fragsurf.Shared
{
    [Inject(InjectRealm.Shared)]
    public class PlayerProps : FSSharedScript
    {

        private Dictionary<int, Dictionary<string, float>> _playerProps = new Dictionary<int, Dictionary<string, float>>();
        private const string _cpLabel = "_PlayerProp";

        public float GetProp(int clientIndex, string key)
        {
            if (!_playerProps.ContainsKey(clientIndex)
                || !_playerProps[clientIndex].ContainsKey(key))
            {
                return 0f;
            }
            return _playerProps[clientIndex][key];
        }

        public void SetProp(int clientIndex, string key, float value)
        {
            if (!_playerProps.ContainsKey(clientIndex))
            {
                _playerProps[clientIndex] = new Dictionary<string, float>();
            }
            _playerProps[clientIndex][key] = value;

            if (Game.IsServer)
            {
                BroadcastProp(clientIndex, key, value);
            }
        }

        public void IncrementProp(int clientIndex, string key, float value)
        {
            var val = GetProp(clientIndex, key);
            SetProp(clientIndex, key, val + value);
        }

        protected override void OnPlayerPacketReceived(BasePlayer player, IBasePacket packet)
        {
            if(Game.IsServer 
                || !(packet is CustomPacket cp) 
                || !cp.Label.Equals(_cpLabel))
            {
                return;
            }

            var client = cp.GetInt();
            var key = cp.GetString();
            var value = cp.GetFloat();

            SetProp(client, key, value);
        }

        protected override void OnPlayerIntroduced(BasePlayer player)
        {
            foreach(var kvp in _playerProps)
            {
                var client = kvp.Key;
                foreach(var kvp2 in kvp.Value)
                {
                    var kkey = kvp2.Key;
                    var kvalue = kvp2.Value;
                    SendProp(player, client, kkey, kvalue);
                }
            }
        }

        private void BroadcastProp(int clientIndex, string key, float value)
        {
            var packet = GetPacket(clientIndex, key, value);
            Game.Network.BroadcastPacket(packet);
        }

        private void SendProp(BasePlayer player, int clientIndex, string key, float value)
        {
            var packet = GetPacket(clientIndex, key, value);
            Game.Network.SendPacket(player.ClientIndex, packet);
        }

        private CustomPacket GetPacket(int clientIndex, string key, float value)
        {
            var cp = PacketUtility.TakePacket<CustomPacket>();
            cp.Sc = SendCategory.UI_Important;
            cp.Label = _cpLabel;
            cp.AddInt(clientIndex);
            cp.AddString(key);
            cp.AddFloat(value);
            return cp;
        }

    }
}

