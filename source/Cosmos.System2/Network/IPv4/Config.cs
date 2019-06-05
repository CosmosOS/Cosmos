/*
* PROJECT:          Aura Operating System Development
* CONTENT:          List of all IPs / Utils
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System.Network;
using Cosmos.System.Network.IPv4;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// Contains a IPv4 configuration
    /// </summary>
    public class Config
    {
        internal static List<Config> ipConfigs;

        static Config()
        {
            ipConfigs = new List<Config>();
        }

        internal static void Add(Config config)
        {
            ipConfigs.Add(config);
        }

        internal static void Remove(Config config)
        {
            ipConfigs.Remove(config);
        }

        internal static void RemoveAll()
        {
            ipConfigs.Clear();
        }

        internal static Address FindNetwork(Address destIP)
        {
            Address default_gw = null;

            for (int c = 0; c < ipConfigs.Count; c++)
            {
                if ((ipConfigs[c].IPAddress.Hash & ipConfigs[c].SubnetMask.Hash) ==
                    (destIP.Hash & ipConfigs[c].SubnetMask.Hash))
                {
                    return ipConfigs[c].IPAddress;
                }
                if ((default_gw == null) && (ipConfigs[c].DefaultGateway.CompareTo(Address.Zero) != 0))
                {
                    default_gw = ipConfigs[c].IPAddress;
                }

                if (!IsLocalAddress(destIP))
                {
                    return ipConfigs[c].IPAddress;
                }
            }

            return default_gw;
        }

        internal static bool IsLocalAddress(Address destIP)
        {
            for (int c = 0; c < ipConfigs.Count; c++)
            {
                if ((ipConfigs[c].IPAddress.Hash & ipConfigs[c].SubnetMask.Hash) ==
                    (destIP.Hash & ipConfigs[c].SubnetMask.Hash))
                {
                    return true;
                }
            }

            return false;
        }

        internal static NetworkDevice FindInterface(Address sourceIP)
        {
            return NetworkStack.AddressMap[sourceIP.Hash];
        }

        internal static Address FindRoute(Address destIP)
        {
            for (int c = 0; c < ipConfigs.Count; c++)
            {
                //if (ipConfigs[c].DefaultGateway.CompareTo(Address.Zero) != 0)
                //{
                //    return ipConfigs[c].DefaultGateway;
                //}
                return ipConfigs[c].DefaultGateway;
            }

            return null;
        }

        protected Address address;
        protected Address defaultGateway;
        protected Address subnetMask;
        protected Address preferreddns;

        /// <summary>
        /// Create a IPv4 Configuration with no default gateway
        /// </summary>
        /// <param name="ip">IP Address</param>
        /// <param name="subnet">Subnet Mask</param>
        public Config(Address ip, Address subnet)
            : this(ip, subnet, Address.Zero)
        { }

        /// <summary>
        /// Create a IPv4 Configuration
        /// </summary>
        /// <param name="ip">IP Address</param>
        /// <param name="subnet">Subnet Mask</param>
        /// <param name="gw">Default gateway</param>
        public Config(Address ip, Address subnet, Address gw)
        {
            this.address = ip;
            this.subnetMask = subnet;
            this.defaultGateway = gw;
            this.PreferredDNS = new Address(0, 0, 0, 0);
        }

        public Address IPAddress
        {
            get { return this.address; }
            set { this.address = value; }
        }
        public Address SubnetMask
        {
            get { return this.subnetMask; }
            set { this.subnetMask = value; }
        }
        public Address DefaultGateway
        {
            get { return this.defaultGateway; }
            set { this.defaultGateway = value; }
        }

        public Address PreferredDNS
        {
            get { return this.preferreddns; }
            set { this.preferreddns = value; }
        }
    }
}
