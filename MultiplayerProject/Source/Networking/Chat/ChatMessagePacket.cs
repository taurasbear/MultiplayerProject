namespace MultiplayerProject.Source.Networking.Chat
{
    public enum ChatMessageType
    {
        Global,
        Private
    }

    public class ChatMessagePacket
    {
        public ChatMessageType Type { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; } // For private messages
        public string Message { get; set; }
    }
}