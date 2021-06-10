using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;
using Lidgren.Network;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.UI;

namespace Fragsurf.Shared
{
    [Inject(InjectRealm.Shared)]
    public class SpectateController : FSSharedScript
    {

        private static Human _targetHuman;
        // todo: a netprop for dictionaries would be nice.
        private Dictionary<int, int> _specTargets = new Dictionary<int, int>();

        public static event Action ScoreboardUpdateNotification;

        public Human TargetHuman
        {
            get => _targetHuman;
            set => Spectate(value);
        }

        public static Human SpecTarget => _targetHuman;

        public bool IsSpectating(int clientIndex)
        {
            var pl = Game.PlayerManager.FindPlayer(clientIndex);
            if (pl != null && pl.Team == 0)
            {
                return true;
            }

            if (!_specTargets.ContainsKey(clientIndex)
                || _specTargets[clientIndex] <= 0)
            {
                return true;
            }

            var entId = _specTargets[clientIndex];
            var targetEnt = Game.EntityManager.FindEntity(entId);
            if (targetEnt == null || !(targetEnt is Human targetHu))
            {
                return true;
            }

            return targetHu.OwnerId != clientIndex;
        }

        public bool CanSpectate(Human hu)
        {
            if(hu == null 
                || (hu.Dead && hu.TimeDead > 5f)
                || (!hu.Enabled && hu != Human.Local))
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

        protected override void OnPlayerChangedTeam(BasePlayer player)
        {
            if (!Game.IsHost)
            {
                if(player.ClientIndex == Game.ClientIndex
                    && player.Team > 0
                    && player.Entity is Human hu)
                {
                    Spectate(hu);
                }
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

        protected override void OnHumanSpawned(Human hu)
        {
            if (Game.IsHost)
            {
                return;
            }

            if(hu == Human.Local)
            {
                Spectate(hu);
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

            if (IsSpectating(Game.ClientIndex)
                || (Human.Local != null && (Human.Local.Dead || !Human.Local.Enabled)))
            {
                UpdateSpectateCycle();
            }
        }

        private bool _guiHadFocus;
        private void UpdateSpectateCycle()
        {
            if (!UGuiManager.Instance.HasCursor()
                && !UGuiManager.Instance.HasFocusedInput())
            {
                // do this to skip the first click after toying around in menus
                // it's a better ux imo
                if (_guiHadFocus)
                {
                    _guiHadFocus = false;
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        SpectateNext();
                    }
                    else if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        SpectateNext(true);
                    }
                }
            }
            else
            {
                _guiHadFocus = true;
            }
        }

        public void SpectateNext(bool backwards = false)
        {
            var spectatable = Game.EntityManager
                .OfType<Human>()
                .Where(x => CanSpectate(x) && x != Human.Local)
                .ToList();

            if(spectatable.Count == 0)
            {
                return;
            }

            if(spectatable.Count == 1)
            {
                Spectate(spectatable[0]);
                return;
            }

            var nextTarget = backwards
                ? spectatable.PreviousOf(_targetHuman)
                : spectatable.NextOf(_targetHuman);

            if(nextTarget != null)
            {
                Spectate(nextTarget);
            }
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

                if(player.Entity is Human hu 
                    && !hu.Dead
                    && hu.EntityId != spec.TargetEntityId)
                {
                    Game.PlayerManager.SetPlayerTeam(player, 0);
                }
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

