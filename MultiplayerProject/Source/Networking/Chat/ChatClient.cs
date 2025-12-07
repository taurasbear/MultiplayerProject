using System;

namespace MultiplayerProject.Source.Networking.Chat
{
    public class ChatClient
    {
        public string Id { get; }
        private readonly IChatMediator _mediator;

        public ChatClient(string id, IChatMediator mediator)
        {
            Id = id;
            _mediator = mediator;
            _mediator.RegisterClient(this);
        }

        public void SendGlobal(string message)
        {
            _mediator.SendGlobalMessage(Id, message);
        }

        public void SendPrivate(string receiverId, string message)
        {
            _mediator.SendPrivateMessage(Id, receiverId, message);
        }

        public void ReceiveMessage(string message)
        {
            // Display message in chat UI
            Console.WriteLine(message);
        }
    }
}