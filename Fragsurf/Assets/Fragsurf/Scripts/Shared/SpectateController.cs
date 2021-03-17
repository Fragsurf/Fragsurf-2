using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;
using Lidgren.Network;
using System;
using System.Collections.Generic;

namespace Fragsurf.Shared
{
    [Inject(InjectRealm.Shared)]
    public class SpectateController : FSSharedScript
    {

        private Human _targetHuman;
        // todo: a netprop for scripts and dictionaries would be nice.
        private Dictionary<int, int> _specTargets = new Dictionary<int, int>();

        public static event Action ScoreboardUpdateNotification;

        public Human TargetHuman
        {
            get => _targetHuman;
            set => Spectate(value);
        }

        public bool IsSpectating(int clientIndex)
        {
            if (!_specTargets.ContainsKey(clientIndex)
                || _specTargets[clientIndex] <= 0)
            {
                return true;
            }
            var entId = _specTargets[clientIndex];
            var ent = Game.EntityManager.FindEntity(entId);
            if(ent == null || !(ent is Human hu))
            {
                return true;
            }
            return hu.OwnerId != clientIndex;
        }

        public bool CanSpectate(Human hu)
        {
            if(hu == null
                || !hu.IsLive
                || !hu.Enabled)
            {
                return false;
            }

            var owner = Game.PlayerManager.FindPlayer(hu.OwnerId);
            if(owner != null && owner.Team == 0)
            {
                return false;
            }

            return true;
        }

        public int GetPlayersSpectating(int entityId, int[] clients)
        {
            if(clients == null || clients.Length == 0)
            {
                return 0;
            }

            var idx = 0;
            foreach(var kvp in _specTargets)
            {
                if(kvp.Value == entityId)
                {
                    clients[idx] = kvp.Key;
                    idx++;
                    if(idx >= clients.Length)
                    {
                        return idx;
                    }
                }
            }

            return idx;
        }

        protected override void OnPlayerDisconnected(BasePlayer player)
        {
            _specTargets.Remove(player.ClientIndex);

            if (!Game.IsHost)
            {
                ScoreboardUpdateNotification?.Invoke();
            }
        }

        protected override void OnPlayerConnected(BasePlayer player)
        {
            _specTargets[player.ClientIndex] = 0;

            if (Game.IsHost)
            {
                foreach (var kvp in _specTargets)
                {
                    SendSpecId(player.ClientIndex, kvp.Key, kvp.Value);
                }
            }
            else
            {
                ScoreboardUpdateNotification?.Invoke();
            }
        }

        protected override void _Tick()
        {
            if (Game.IsHost)
            {
                return;
            }

            if (!CanSpectate(_targetHuman))
            {
                Spectate(FirstSpectatableHuman());
            }
        }

        private Human FirstSpectatableHuman()
        {
            if (CanSpectate(Human.Local))
            {
                return Human.Local;
            }

            foreach(Human hu in Game.EntityManager.OfType<Human>())
            {
                if (CanSpectate(hu))
                {
                    return hu;
                }
            }

            return null;
        }

        protected override void _Update()
        {
            if (Game.IsHost)
            {
                return;
            }
            _targetHuman?.CameraController?.Update();
        }

        protected override void OnPlayerPacketReceived(BasePlayer player, IBasePacket packet)
        {
            if(!(packet is SpecIdPacket spec))
            {
                return;
            }

            _specTargets[spec.ClientIndex] = spec.TargetEntityId;

            if (Game.IsHost)
            {
                BroadcastSpecId(spec.ClientIndex, spec.TargetEntityId);
            }
            else
            {
                ScoreboardUpdateNotification?.Invoke();
            }
        }

        public void Spectate(Human hu)
        {
            if (Game.IsHost
                || hu == null
                || !CanSpectate(hu)
                || _targetHuman == hu)
            {
                return;
            }

            if (_targetHuman != null)
            {
                _targetHuman.IsFirstPerson = false;
                _targetHuman.CameraController.Deactivate();
            }

            _targetHuman = hu;
            _targetHuman.IsFirstPerson = true;
            _targetHuman.CameraController.Activate(GameCamera.Camera);
            _specTargets[Game.ClientIndex] = hu.EntityId;

            BroadcastSpecId(Game.ClientIndex, _targetHuman.EntityId);
        }

        private void SendSpecId(int clientIndex, int spectatorIndex, int targetEntity)
        {
            var packet = PacketUtility.TakePacket<SpecIdPacket>();
            packet.ClientIndex = spectatorIndex;
            packet.TargetEntityId = targetEntity;
            Game.Network.SendPacket(clientIndex, packet);
        }

        private void BroadcastSpecId(int spectatorIndex, int targetEntity)
        {
            var packet = PacketUtility.TakePacket<SpecIdPacket>();
            packet.ClientIndex = spectatorIndex;
            packet.TargetEntityId = targetEntity;
            Game.Network.BroadcastPacket(packet);
        }

    }

    public class SpecIdPacket : IBasePacket
    {
        public SendCategory Sc => SendCategory.UI_Important;
        public int ByteSize => 8;
        public bool DisableAutoPool => false;

        public int ClientIndex;
        public int TargetEntityId;

        public void Read(NetBuffer buffer)
        {
            ClientIndex = buffer.ReadInt32();
            TargetEntityId = buffer.ReadInt32();
        }

        public void Reset()
        {
            ClientIndex = 0;
            TargetEntityId = 0;
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write(ClientIndex);
            buffer.Write(TargetEntityId);
        }
    }
}

