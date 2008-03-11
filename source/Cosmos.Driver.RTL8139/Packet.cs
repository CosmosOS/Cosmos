using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Driver.RTL8139
{
    /// <summary>
    /// A network Packet used when transferring data over a network. 
    /// Consists of a body (with the data) and a head (with info about the packet)
    /// </summary>
    public class Packet
    {
        public PacketHeader Head { get; private set; }
        public byte[] PacketBody { get; set; }
    }
}
