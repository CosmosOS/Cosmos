/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Network dictionary
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections;
using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System.Network.IPv4;

namespace Cosmos.System.Network.Config
{
    /// <summary>
    /// Network Configuration (link network device to an ip address)
    /// </summary>
    internal class NetworkConfig
    {
        /// <summary>
        /// Network device
        /// </summary>
        internal NetworkDevice Device;

        /// <summary>
        /// IPv4 Configuration
        /// </summary>
        internal IPConfig IPConfig;

        /// <summary>
        /// NetworkConfig ctor
        /// </summary>
        /// <param name="device">Network device.</param>
        /// <param name="config">IP Config</param>
        internal NetworkConfig(NetworkDevice device, IPConfig config)
        {
            Device = device;
            IPConfig = config;
        }
    }

    /// <summary>
    /// Network stack configuration
    /// </summary>
    public static class NetworkConfiguration
    {
        /// <summary>
        /// Current network configuration used by the network stack
        /// </summary>
        private static NetworkConfig CurrentNetworkConfig { get; set; }

        /// <summary>
        /// Current network configuration used by the network stack
        /// </summary>
        private static List<NetworkConfig> NetworkConfigs = new List<NetworkConfig>();

        /// <summary>
        /// Network congiruations count
        /// </summary>
        public static int Count
        {
            get { return NetworkConfigs.Count; }
        }

        /// <summary>
        /// Current IPv4 address
        /// </summary>
        public static Address CurrentAddress
        {
            get { return CurrentNetworkConfig.IPConfig.IPAddress; }
        }

        /// <summary>
        /// Set current network config
        /// </summary>
        /// <param name="device">Network device.</param>
        /// <param name="config">IP Config</param>
        internal static void SetCurrentConfig(NetworkDevice device, IPConfig config)
        {
            CurrentNetworkConfig = new NetworkConfig(device, config);
        }

        /// <summary>
        /// Add new network config
        /// </summary>
        /// <param name="device">Network device.</param>
        /// <param name="config">IP Config</param>
        internal static void AddConfig(NetworkDevice device, IPConfig config)
        {
            NetworkConfigs.Add(new NetworkConfig(device, config));
        }

        /// <summary>
        /// Network stack contains device
        /// </summary>
        /// <param name="device">Network device.</param>
        public static bool ConfigsContainsDevice(NetworkDevice k)
        {
            if (NetworkConfigs == null)
            {
                return false;
            }
            else
            {
                foreach (var device in NetworkConfigs)
                {
                    if (k == device.Device)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Clear network configurations
        /// </summary>
        internal static void ClearConfigs()
        {
            NetworkConfigs.Clear();
        }

        /// <summary>
        /// Get ip config for network device
        /// </summary>
        /// <param name="device">Network device.</param>
        internal static IPConfig Get(NetworkDevice device)
        {
            foreach (var networkConfig in NetworkConfigs)
            {
                if (device == networkConfig.Device)
                {
                    return networkConfig.IPConfig;
                }
            }

            return null;
        }

        /// <summary>
        /// Remove Config for network device
        /// </summary>
        /// <param name="device">Network device.</param>
        public static void Remove(NetworkDevice key)
        {
            int index = 0;

            foreach (var networkConfig in NetworkConfigs)
            {
                if (key == networkConfig.Device)
                {
                    break;
                }
                index++;
            }
            NetworkConfigs.RemoveAt(index);
        }
    }
}
