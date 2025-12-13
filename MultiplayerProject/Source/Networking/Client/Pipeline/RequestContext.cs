using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source.Networking.Client.Pipeline
{
    public class RequestContext
    {
        public BasePacket Packet { get; set; }

        public NetworkStream RawStream { get; set; }

        public MultiplayerProject.Client Client { get; set; }
    }
}
