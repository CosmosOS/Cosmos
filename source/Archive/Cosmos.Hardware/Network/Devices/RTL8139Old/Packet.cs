using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware2.Network.Devices.RTL8139
{
    /// <summary>
    /// A network Packet used when transferring data over a network. 
    /// Consists of a body (with the data) and a head (with info about the packet)
    /// </summary>
    [Obsolete("Use Ethernet2Frame instead.")]
    public class Packet
    {
        private PacketHeader head;
        private byte[] body;
        public PacketHeader Head { get; private set; }

        public Packet(PacketHeader newhead, byte[] data)
        {
            head = newhead;
            body = data;
        }

        public byte[] PacketBody
        {
            get
            {
                return body;
            }
            //return new byte[10]; //TODO: Redo this completely! Hardcoded to some bogus value now.
        }
    }
}
