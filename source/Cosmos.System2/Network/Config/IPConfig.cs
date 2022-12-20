/*
* PROJECT:          Aura Operating System Development
* CONTENT:          List of all IPs / Utils
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Collections.Generic;
using System;
using Cosmos.System.Network.IPv4;
using Cosmos.HAL;

namespace Cosmos.System.Network.Config
{
    /// <summary>
    /// Contains a IPv4 configuration
    /// </summary>
    public class IPConfig
    {
        /// <summary>
        /// IPv4 configurations list.
        /// </summary>
        private static readonly List<IPConfig> ipConfigs = new List<IPConfig>();

        /// <summary>
        /// Add IPv4 configuration.
        /// </summary>
        /// <param name="config"></param>
        internal static void Add(IPConfig config)
        {
            ipConfigs.Add(config);
        }

        /// <summary>
        /// Remove IPv4 configuration.
        /// </summary>
        /// <param name="config"></param>
        internal static void Remove(IPConfig config)
        {
            int counter = 0;

            foreach (var ipconfig in ipConfigs)
            {
                if (ipconfig == config)
                {
                    ipConfigs.RemoveAt(counter);
                    return;
                }
                counter++;
            }
        }

        /// <summary>
        /// Remove All IPv4 configuration.
        /// </summary>
        internal static void RemoveAll()
        {
            ipConfigs.Clear();
        }

        /// <summary>
        /// Find network.
        /// </summary>
        /// <param name="destIP">Destination IP address.</param>
        /// <returns>Address value.</returns>
        /// <exception cref="ArgumentException">Thrown on fatal error (contact support).</exception>
        public static Address FindNetwork(Address destIP)
        {
            Address default_gw = null;

            foreach (IPConfig ipConfig in ipConfigs)
            {
                if ((ipConfig.IPAddress.Hash & ipConfig.SubnetMask.Hash) ==
                    (destIP.Hash & ipConfig.SubnetMask.Hash))
                {
                    return ipConfig.IPAddress;
                }
                if (default_gw == null && ipConfig.DefaultGateway.CompareTo(Address.Zero) != 0)
                {
                    default_gw = ipConfig.IPAddress;
                }

                if (!IsLocalAddress(destIP))
                {
                    return ipConfig.IPAddress;
                }
            }

            return default_gw;
        }

        public static bool Enable(NetworkDevice device, Address ip, Address subnet, Address gw)
        {
            if (device != null)
            {
                var config = new IPConfig(ip, subnet, gw);
                NetworkStack.ConfigIP(device, config);
                Global.mDebugger.Send("Config OK.");
                return true;
            }
            return false;
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
                //if (ipConfigs[c].DefaultGateway.CompareTo(Address.Zero) != 0)
                //{
                    return ipConfigs[c].DefaultGateway;
                //}
            }

            return null;
        }

        /// <summary>
        /// Create a IPv4 Configuration with no default gateway
        /// </summary>
        /// <param name="ip">IP Address</param>
        /// <param name="subnet">Subnet Mask</param>
        public IPConfig(Address ip, Address subnet)
            : this(ip, subnet, Address.Zero)
        {
        }

        /// <summary>
        /// Create a IPv4 Configuration
        /// </summary>
        /// <param name="ip">IP Address</param>
        /// <param name="subnet">Subnet Mask</param>
        /// <param name="gw">Default gateway</param>
        public IPConfig(Address ip, Address subnet, Address gw)
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
