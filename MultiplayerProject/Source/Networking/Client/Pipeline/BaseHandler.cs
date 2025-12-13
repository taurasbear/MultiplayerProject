using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source.Networking.Client.Pipeline
{
    public abstract class BaseHandler
    {
        protected BaseHandler Next { get; set; }

        public abstract void Handle(RequestContext requestContext);

        public virtual void SetNext(BaseHandler handler)
        {
            Next = handler;
        }
    }
}
