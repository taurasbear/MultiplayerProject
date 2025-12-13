using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source.Networking.Client.Pipeline
{
    public class FinalHandler : BaseHandler
    {
        public override void Handle(RequestContext requestContext)
        {
            requestContext.Client.ProcessServerPacket(requestContext.Packet);
        }
    }
}
