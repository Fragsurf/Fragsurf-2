using System;
using System.Linq;
using Fragsurf.Shared.Packets;

namespace Fragsurf.Shared.Player
{
    public class TextChat : FSSharedScript
    {
        public event Action<ChatMessage> OnMessageReceived;

        public static string ServerName = "[Server]";
        public char CommandChar = '/';
        public int MaxMessageLength = 150;

        [ConCommand("say", "Say something", ConVarFlags.Silent)]
        public void SayCmd(string msg)
        {
            if (Game.IsLocalServer)
            {
                return;
            }
            MessageAll(msg);
        }

        protected override void OnPlayerPacketReceived(BasePlayer player, IBasePacket packet)
        {
            if (packet is ChatMessage chatMessage)
            {
                if (chatMessage.Message.Length < 1)
                {
                    return;
                }

                if(Game.IsHost)
                {
                    if (chatMessage.Message.Length > MaxMessageLength)
                    {
                        chatMessage.Message = chatMessage.Message.Substring(0, MaxMessageLength);
                    }

                    if (chatMessage.Message[0] == CommandChar)
                    {
                        ParseCommand(player, chatMessage.Message);
                    }
                    else
                    {
                        var msgToSend = PacketUtility.TakePacket<ChatMessage>();
                        msgToSend.ClientIndex = player.ClientIndex;
                        msgToSend.Name = player.DisplayName;
                        msgToSend.Message = chatMessage.Message;
                        msgToSend.Scope = chatMessage.Scope;
                        msgToSend.SupporterLevel = chatMessage.SupporterLevel;
                        Game.Network.BroadcastPacket(msgToSend);
                    }
                }

                OnMessageReceived?.Invoke(chatMessage);
            }
        }

        protected override void OnPlayerIntroduced(BasePlayer player)
        {
            OnMessageReceived?.Invoke(new ChatMessage()
            {
                Message = $"Player {player.DisplayName} has joined the game.",
                Name = ServerName
            });
        }

        protected override void OnPlayerDisconnected(BasePlayer player)
        {
            OnMessageReceived?.Invoke(new ChatMessage()
            {
                Message = $"Player {player.DisplayName} has disconnected.",
                Name = ServerName
            });
        }

        public void MessagePlayer(BasePlayer player, string message)
        {
            if(!Game.IsHost)
            {
                throw new NotImplementedException();
            }
            Game.Network.SendPacket(player.ClientIndex, GetChatPacket(message));
        }

        public void MessageAll(string message)
        {
            Game.Network.BroadcastPacket(GetChatPacket(message));
            if (message[0] == CommandChar)
            {
                var localPlayer = Game.PlayerManager.FindPlayer(Game.ClientIndex);
                ParseCommand(localPlayer, message);
            }
        }

        public void PrintChat(string name, string message)
        {
            OnMessageReceived?.Invoke(new ChatMessage()
            {
                Message = message,
                Name = name,
                Scope = ChatScope.Global,
                ClientIndex = Game.ClientIndex
            });
        }

        private void ParseCommand(BasePlayer player, string message)
        {
            var args = DevConsole.ParseArguments(message.Remove(0, 1), ' ', '"').ToArray();
            Game.PlayerManager.RaiseChatCommand(player, args); 
        }

        private ChatMessage GetChatPacket(string message)
        {
            var packet = PacketUtility.TakePacket<ChatMessage>();
            packet.ClientIndex = Game.ClientIndex;
            packet.Message = message;
            packet.Name = Game.IsHost ? ServerName : Steamworks.SteamClient.Name;
            packet.SupporterLevel = 0;
            return packet;
        }

    }
}
