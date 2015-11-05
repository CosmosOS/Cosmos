using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Sys.Network
{
    public class IPv4Config
    {
        protected IPv4Address address;
        protected IPv4Address defaultGateway;
        protected IPv4Address subnetMask;

        public IPv4Config(IPv4Address ip, IPv4Address subnet)
            : this(ip, subnet, IPv4Address.Zero)
        {}

        public IPv4Config(IPv4Address ip, IPv4Address subnet, IPv4Address gw)
        {
            this.address = ip;
            this.subnetMask = subnet;
            this.defaultGateway = gw;
        }

        public IPv4Address IPAddress
        {
            get { return this.address; }
        }
        public IPv4Address SubnetMask
        {
            get { return this.subnetMask; }
        }
        public IPv4Address DefaultGateway
        {
            get { return this.defaultGateway; }
        }
    }
}
