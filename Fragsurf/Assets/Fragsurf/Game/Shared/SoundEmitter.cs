using UnityEngine;
using Fragsurf.Shared.Player;
using Fragsurf.Shared.Packets;
using FMODUnity;

namespace Fragsurf.Shared
{
    public class SoundEmitter : FSSharedScript
    {

        protected override void OnPlayerPacketReceived(IPlayer player, IBasePacket packet)
        {
            if(!Game.IsHost && packet is PlaySound playSound)
            {
                Emit(playSound.Event, playSound.Twod, playSound.Position);
            }
        }

        public void Emit(string eventPath, bool twod, Vector3 position = default)
        {
            if (Game.IsHost)
            {
                throw new System.NotImplementedException();
            }

            var fixedPath = FixPath(eventPath);
            if (!twod)
            {
                RuntimeManager.PlayOneShot(fixedPath, position);
            }
            else
            {
                RuntimeManager.PlayOneShotAttached(fixedPath, Camera.main.gameObject);
            }
        }

        public void EmitToAll(string eventPath, Vector3 position = default)
        {
            if(!Game.IsHost)
            {
                return;
            }
            var sound = PacketUtility.TakePacket<PlaySound>();
            sound.Event = eventPath;
            sound.Position = position;
            sound.Twod = position == Vector3.zero;
            Game.Network.BroadcastPacket(sound);
        }

        public void EmitToPlayer(IPlayer player, string eventPath, Vector3 position = default)
        {
            if (!Game.IsHost)
            {
                return;
            }
            var sound = PacketUtility.TakePacket<PlaySound>();
            sound.Event = eventPath;
            sound.Position = position;
            sound.Twod = position == Vector3.zero;
            Game.Network.SendPacket(player.AccountId, sound);
        }

        public static string FixPath(string path)
        {
            if (!path.StartsWith("event:/"))
            {
                return "event:/" + path;
            }
            return path;
        }
    }
}

