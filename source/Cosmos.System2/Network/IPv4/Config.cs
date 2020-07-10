using System.Collections.Generic;
using Cosmos.HAL;
using System;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// Contains a IPv4 configuration
    /// </summary>
    public class Config
    {
        /// <summary>
        /// IPv4 configurations list.
        /// </summary>
        private static readonly List<Config> ipConfigs = new List<Config>();

        /// <summary>
        /// Add IPv4 configuration.
        /// </summary>
        /// <param name="config"></param>
        internal static void Add(Config config)
        {
            ipConfigs.Add(config);
        }

        /// <summary>
        /// Find network.
        /// </summary>
        /// <param name="destIP">Destination IP address.</param>
        /// <returns>Address value.</returns>
        /// <exception cref="ArgumentException">Thrown on fatal error (contact support).</exception>
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
            }

            return default_gw;
        }

        /// <summary>
        /// Check if address is local address.
        /// </summary>
        /// <param name="destIP">Address to check.</param>
        /// <returns>bool value.</returns>
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

        /// <summary>
        /// Find interface.
        /// </summary>
        /// <param name="sourceIP">Source IP.</param>
        /// <returns>NetworkDevice value.</returns>
        internal static NetworkDevice FindInterface(Address sourceIP)
        {
            return NetworkStack.AddressMap[sourceIP.Hash];
        }

        /// <summary>
        /// Find route to address.
        /// </summary>
        /// <param name="destIP">Destination IP.</param>
        /// <returns>Address value.</returns>
        /// <exception cref="ArgumentException">Thrown on fatal error (contact support).</exception>
        internal static Address FindRoute(Address destIP)
        {
            for (int c = 0; c < ipConfigs.Count; c++)
            {
                if (ipConfigs[c].DefaultGateway.CompareTo(Address.Zero) != 0)
                {
                    return ipConfigs[c].DefaultGateway;
                }
            }

            return null;
        }

        /// <summary>
        /// Create a IPv4 Configuration with no default gateway
        /// </summary>
        /// <param name="ip">IP Address</param>
        /// <param name="subnet">Subnet Mask</param>
        public Config(Address ip, Address subnet)
            : this(ip, subnet, Address.Zero)
        {
        }

        /// <summary>
        /// Create a IPv4 Configuration
        /// </summary>
        /// <param name="ip">IP Address</param>
        /// <param name="subnet">Subnet Mask</param>
        /// <param name="gw">Default gateway</param>
        public Config(Address ip, Address subnet, Address gw)
        {
            IPAddress = ip;
            SubnetMask = subnet;
            DefaultGateway = gw;
        }

        /// <summary>
        /// Get IP address.
        /// </summary>
        public Address IPAddress { get; }
        /// <summary>
        /// Get subnet mask.
        /// </summary>
        public Address SubnetMask { get; }
        /// <summary>
        /// Get default gateway.
        /// </summary>
        public Address DefaultGateway { get; }
    }
}
