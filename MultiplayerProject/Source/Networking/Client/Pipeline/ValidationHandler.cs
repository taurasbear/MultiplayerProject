using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source.Networking.Client.Pipeline
{
    public class ValidationHandler : BaseHandler
    {
        public override void Handle(RequestContext requestContext)
        {
            if (requestContext.Packet == null)
            {
                throw new InvalidOperationException("Packet is null. Cannot process request.");
            }

            bool isValidMessageType = Enum.IsDefined(typeof(MultiplayerProject.Source.MessageType), (byte)requestContext.Packet.MessageType);
            if (!isValidMessageType)
            {
                throw new InvalidOperationException($"Invalid MessageType: {requestContext.Packet.MessageType}.");
            }

            this.Next.Handle(requestContext);
        }
    }
}
