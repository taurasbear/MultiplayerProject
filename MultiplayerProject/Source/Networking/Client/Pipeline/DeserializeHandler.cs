using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source.Networking.Client.Pipeline
{
    public class DeserializeHandler : BaseHandler
    {
        public override void Handle(RequestContext requestContext)
        {
            BasePacket packet = Serializer.DeserializeWithLengthPrefix<BasePacket>(requestContext.RawStream, PrefixStyle.Base128);
            if (packet != null)
            {
                requestContext.Packet = packet;
                this.Next.Handle(requestContext);
            }
        }
    }
}
