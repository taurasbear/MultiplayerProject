using System.Collections.Generic;

namespace MultiplayerProject.Source.Networking.Chat
{
    public interface IChatMediator
    {
        void RegisterClient(ChatClient client);
        void SendGlobalMessage(string senderId, string message);
        void SendPrivateMessage(string senderId, string receiverId, string message);
    }

    public class ChatMediator : IChatMediator
    {
        private readonly Dictionary<string, ChatClient> _clients = new Dictionary<string, ChatClient>();

        public void RegisterClient(ChatClient client)
        {
            if (!_clients.ContainsKey(client.Id))
                _clients.Add(client.Id, client);
        }

        public void SendGlobalMessage(string senderId, string message)
        {
            foreach (var client in _clients.Values)
                client.ReceiveMessage($"[Global] {senderId}: {message}");
        }

        public void SendPrivateMessage(string senderId, string receiverId, string message)
        {
            if (_clients.TryGetValue(receiverId, out var receiver))
                receiver.ReceiveMessage($"[DM] {senderId} -> {receiverId}: {message}");
        }
    }
}