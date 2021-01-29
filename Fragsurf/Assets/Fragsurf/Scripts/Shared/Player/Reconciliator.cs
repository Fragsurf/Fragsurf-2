using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Shared.Packets;

namespace Fragsurf.Shared.Player
{
    public class Reconciliator
    {

        public Reconciliator(Entity.Human player)
        {
            _player = player;
        }

        private Entity.Human _player;
        private List<UserCmd.CmdFields> _localHistory = new List<UserCmd.CmdFields>(512);

        public void StashLocalCmd(UserCmd.CmdFields cmd)
        {
            _localHistory.Add(cmd);

            if(_localHistory.Count > 512)
            {
#if UNITY_EDITOR
                Debug.LogError("It can't be so!");
#endif
                _localHistory.Clear();
            }
        }

        public void StashServerCmd(UserCmd.CmdFields hostFrame)
        {
            var clientFrameIndex = GetClientFrameIndex(hostFrame.PacketId);
            if(clientFrameIndex == -1)
            {
                return;
            }

            var clientFrame = _localHistory[clientFrameIndex];

            var errorPos = Vector3.Distance(hostFrame.Origin, clientFrame.Origin);
            var errorVel = Vector3.Distance(hostFrame.Velocity, clientFrame.Velocity);
            var errorAng = Vector3.Distance(hostFrame.Angles, clientFrame.Angles);

            if (errorAng > 0.001f)
            {
                _player.Angles = hostFrame.Angles;
            }

            var error = errorPos > 0.001f
                || errorVel > 0.001f;

            if (error)
            {
#if UNITY_EDITOR && FALSE
                Debug.LogFormat("Prediction error: pos-{0}, vel-{1}, ang-{2}", errorPos, errorVel, errorAng);
                //Debug.Log($"{Time.frameCount} - BTN - {hostFrame.Buttons}:{clientFrame.Buttons} ID - {hostFrame.PacketId}:{clientFrame.PacketId} POS - {hostFrame.Origin}:{clientFrame.Origin}");
#endif
                var prevAngles = _player.Angles;
                _player.Origin = hostFrame.Origin;
                _player.Angles = hostFrame.Angles;
                _player.Velocity = hostFrame.Velocity;
                _player.BaseVelocity = hostFrame.BaseVelocity;
                for(int i = _localHistory.Count - 1; i >= 0; i--)
                {
                    if(_localHistory[i].PacketId <= hostFrame.PacketId)
                    {
                        _localHistory.RemoveAt(i);
                    }
                }
                foreach (var u in _localHistory)
                {
                    _player.MovementController?.ExecuteMovement(u);
                }
                _player.Angles = prevAngles;
            }
            else
            {
                _localHistory.RemoveRange(0, clientFrameIndex);
            }
        }

        private int GetClientFrameIndex(int packetId)
        {
            for(int i = 0; i < _localHistory.Count; i++)
            {
                if(_localHistory[i].PacketId == packetId)
                {
                    return i;
                }
            }
            return -1;
        }

    }
}
