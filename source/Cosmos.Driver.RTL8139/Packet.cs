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
        private PacketHeader head;
        public PacketHeader Head { get; private set; }

        public Packet(PacketHeader newhead)
        {
            head = newhead;
        }

        public byte[] PacketBody()
        {
            return new byte[10]; //TODO: Redo this completely! Hardcoded to some bogus value now.
        }
    }
}
