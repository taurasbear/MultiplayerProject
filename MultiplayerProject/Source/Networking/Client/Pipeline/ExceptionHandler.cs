using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source.Networking.Client.Pipeline
{
    public class ExceptionHandler : BaseHandler
    {
        public override void Handle(RequestContext requestContext)
        {
            try
            {
                this.Next.Handle(requestContext);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in pipeline: " + ex.Message);
            }
        }
    }
}
