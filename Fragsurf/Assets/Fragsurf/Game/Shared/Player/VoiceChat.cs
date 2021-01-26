using System;
using System.Runtime.InteropServices;
using Fragsurf.Shared.Packets;
using Steamworks;
using FMOD;
using FMODUnity;
using Fragsurf.UI;

namespace Fragsurf.Shared.Player
{
    public class VoiceChat : FSSharedScript
    {

        private byte[] _data = new byte[1024 * 64];
        private float _readTime = 0.25f;
        private ChannelGroup _chatChannelGroup;
        private bool _loopback;
        private const float _audioReadTime = 0.4f;

        protected override void _Start()
        {
            base._Start();

            RuntimeManager.LowlevelSystem.createChannelGroup("VoiceChatGroup", out _chatChannelGroup);

            DevConsole.RegisterCommand("+voicerecord", "", this, (e) =>
            {
                if(UIManager.Instance.HasFocusedInputField())
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
                Client.GameClient.Instance.Socket.BroadcastPacket(voicePacket);
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

                _chatChannelGroup.setVolume(vol);

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

                var soundInfo = new CREATESOUNDEXINFO();
                soundInfo.format = SOUND_FORMAT.PCM16;
                soundInfo.defaultfrequency = (int)SteamUser.SampleRate;
                soundInfo.numchannels = 1;
                soundInfo.cbsize = Marshal.SizeOf(soundInfo);
                soundInfo.length = (uint)uncompressedDataLength;

                var createSoundResult = RuntimeManager.LowlevelSystem.createStream(_data, MODE._2D | MODE.OPENMEMORY | MODE.OPENRAW, ref soundInfo, out Sound newSound);
                var playSoundResult = RuntimeManager.LowlevelSystem.playSound(newSound, _chatChannelGroup, false, out Channel channel);

                //UnityEngine.Debug.Log($"size: {data.Length} - group: {getChannelGroupResult} - create: {createSoundResult} - play: {playSoundResult}");
            }
        }

    }
}

