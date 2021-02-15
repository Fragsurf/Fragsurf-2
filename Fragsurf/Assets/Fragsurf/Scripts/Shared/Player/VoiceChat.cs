using System;
using System.Runtime.InteropServices;
using Fragsurf.Shared.Packets;
using Steamworks;
using Fragsurf.UI;

namespace Fragsurf.Shared.Player
{
    public class VoiceChat : FSSharedScript
    {

        private byte[] _data = new byte[1024 * 64];
        private float _readTime = 0.25f;
        private bool _loopback;
        private const float _audioReadTime = 0.4f;

        protected override void _Start()
        {
            base._Start();

            DevConsole.RegisterCommand("+voicerecord", "", this, (e) =>
            {
                if(UGuiManager.Instance.HasFocusedInput())
                {
                    return;
                }
                if(!SteamUser.VoiceRecord)
                {
                    SteamUser.VoiceRecord = true;
                    _readTime = _audioReadTime;
                }
            }, true);

            DevConsole.RegisterCommand("-voicerecord", "", this, (e) =>
            {
                SteamUser.VoiceRecord = false;
            }, true);

            DevConsole.RegisterVariable("voice.loopback", "", () => _loopback, v => _loopback = v, this);
        }

        protected override void _Update()
        {
            if(Game.IsHost || !SteamUser.VoiceRecord)
            {
                return;
            }

            _readTime -= UnityEngine.Time.deltaTime;
            if(_readTime > 0)
            {
                return;
            }

            _readTime = _audioReadTime;

            var data = SteamUser.ReadVoiceDataBytes();
            if(data != null && data.Length > 0)
            {
                var voicePacket = PacketUtility.TakePacket<CompressedVoiceData>();
                voicePacket.ClientIndex = Game.ClientIndex;
                voicePacket.SetData(data, data.Length);

                var cl = FSGameLoop.GetGameInstance(false);
                if (cl)
                {
                    cl.Network.BroadcastPacket(voicePacket);
                }
            }
        }

        protected unsafe override void OnPlayerPacketReceived(IPlayer player, IBasePacket packet)
        {
            if(!(packet is CompressedVoiceData voicePacket))
            {
                return;
            }

            if(Game.IsHost)
            {
                var packetToSend = PacketUtility.TakePacket<CompressedVoiceData>();
                packetToSend.SetData(voicePacket.Data, voicePacket.DataLength);
                packetToSend.ClientIndex = player.ClientIndex;
                Server.GameServer.Instance.Socket.BroadcastPacket(packetToSend);
            }
            else
            {
                var vol = DevConsole.GetVariable<float>("audio.voiceoutput");
                if (vol == 0)
                {
                    return;
                }

                var clientIndex = voicePacket.ClientIndex;

                if (clientIndex == Game.ClientIndex && !_loopback)
                {
                    return;
                }

                int uncompressedDataLength = 0;
                var speakingPlayer = Game.PlayerManager.FindPlayer(clientIndex);
                fixed (byte* src = voicePacket.Data)
                {
                    fixed(byte* dst = _data)
                    {
                        uncompressedDataLength = SteamUser.DecompressVoice((IntPtr)src, voicePacket.DataLength, (IntPtr)dst, _data.Length);
                    }
                }
                if(uncompressedDataLength == 0 || speakingPlayer == null)
                {
                    return;
                }

                throw new System.NotImplementedException();
                // _data to audio..

                //UnityEngine.Debug.Log($"size: {data.Length} - group: {getChannelGroupResult} - create: {createSoundResult} - play: {playSoundResult}");
            }
        }

    }
}

