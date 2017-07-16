using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Sys.Network
{
    public class IPv4EndPoint
    {
        protected IPv4Address ipAddr;
        protected UInt16 port;

        public IPv4EndPoint(IPv4Address addr, UInt16 port)
        {
            this.ipAddr = addr;
            this.port = port;
        }

        public IPv4Address IPAddress
        {
            get { return this.ipAddr; }
        }

        public UInt16 Port
        {
            get { return this.port; }
        }

        public override string ToString()
        {
            return this.ipAddr.ToString() + ":" + this.port.ToString();
        }
    }
}
