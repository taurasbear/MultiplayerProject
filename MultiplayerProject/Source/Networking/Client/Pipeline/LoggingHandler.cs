using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source.Networking.Client.Pipeline
{
    public class LoggingHandler : BaseHandler
    {
        public override void Handle(RequestContext requestContext)
        {
            Console.WriteLine("Processing packet of type: " + requestContext.Packet.GetType().Name);
            this.Next.Handle(requestContext);
            Console.WriteLine("Finished processing packet of type: " + requestContext.Packet.GetType().Name);
        }
    }
}
