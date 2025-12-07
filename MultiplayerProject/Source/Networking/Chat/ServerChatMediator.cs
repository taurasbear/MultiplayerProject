using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MultiplayerProject.Source;
using MultiplayerProject.Source.Networking;

namespace MultiplayerProject
{
    public class ServerChatMediator : IMessageable
    {
        private readonly Dictionary<string, ServerConnection> _clients = new Dictionary<string, ServerConnection>();
        private readonly object _lock = new object();

        // IMessageable members
        public MessageableComponent ComponentType { get; set; } = MessageableComponent.Chat;


        public List<ServerConnection> ComponentClients { get; set; } = new List<ServerConnection>();

        public void Update(GameTime gameTime)
        {
            // No per-frame update needed for chat
        }

        // Register client for broadcasting / private messaging
        public void RegisterClient(string clientId, ServerConnection client)
        {
            lock (_lock)
            {
                if (!_clients.ContainsKey(clientId))
                {
                    _clients.Add(clientId, client);
                    ComponentClients.Add(client);
                }
            }
        }

        public void RemoveClient(ServerConnection client)
        {
            lock (_lock)
            {
                if (_clients.ContainsKey(client.ID))
                {
                    _clients.Remove(client.ID);
                    ComponentClients.Remove(client);
                }
            }
        }

        // Called automatically when a client sends a packet
        public void RecieveClientMessage(ServerConnection sender, BasePacket packet)
        {
            if (packet is ChatMessagePacket chatPacket)
            {
                if (chatPacket.Type == ChatMessageType.Global)
                {
                    lock (_lock)
                    {
                        foreach (var client in _clients.Values)
                        {
                            client.SendPacketToClient(chatPacket, MessageType.ChatMessage);
                        }
                    }
                }
                else if (chatPacket.Type == ChatMessageType.Private && !string.IsNullOrEmpty(chatPacket.ReceiverId))
                {
                    lock (_lock)
                    {
                        if (_clients.TryGetValue(chatPacket.ReceiverId, out var receiver))
                        {
                            receiver.SendPacketToClient(chatPacket, MessageType.ChatMessage);
                        }
                        // Also send back to sender so they see their own DM
                        sender.SendPacketToClient(chatPacket, MessageType.ChatMessage);
                    }
                }
            }
        }

        public void SendGlobalMessage(string senderId, string message)
{
    var packet = new ChatMessagePacket
    {
        Type = ChatMessageType.Global,
        SenderId = senderId,
        Message = message
    };

    lock (_lock)
    {
        foreach (var client in _clients.Values)
        {
            client.SendPacketToClient(packet, MessageType.ChatMessage);
        }
    }
}

public void SendPrivateMessage(string senderId, string receiverId, string message)
{
    lock (_lock)
    {
        if (_clients.TryGetValue(receiverId, out var receiver))
        {
            var packet = new ChatMessagePacket
            {
                Type = ChatMessageType.Private,
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message
            };
            receiver.SendPacketToClient(packet, MessageType.ChatMessage);
            // Also send to sender so they see their own DM
            if (_clients.TryGetValue(senderId, out var sender))
            {
                sender.SendPacketToClient(packet, MessageType.ChatMessage);
            }
        }
    }
}
    }
}
