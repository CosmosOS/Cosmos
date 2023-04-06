using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System.Network.IPv4;

namespace Cosmos.System.Network.Config
{
    /// <summary>
    /// Represents the global network stack configuration.
    /// </summary>
    public static class NetworkConfiguration
    {
        /// <summary>
        /// The current network configuration used by the network stack.
        /// </summary>
        public static NetworkConfig CurrentNetworkConfig { get; set; }

        /// <summary>
        /// The current network configuration list used by the network stack.
        /// </summary>
        public readonly static List<NetworkConfig> NetworkConfigs = new();

        /// <summary>
        /// Gets the amount of available network configurations.
        /// </summary>
        public static int Count => NetworkConfigs.Count;

        /// <summary>
        /// Gets the current IPv4 address.
        /// </summary>
        public static Address CurrentAddress => CurrentNetworkConfig.IPConfig.IPAddress;

        /// <summary>
        /// Sets the configuration of the current network.
        /// </summary>
        /// <param name="device">The network device to use.</param>
        /// <param name="config">The IPv4 configuration associated with the device to use.</param>
        public static void SetCurrentConfig(NetworkDevice device, IPConfig config)
        {
            CurrentNetworkConfig = new NetworkConfig(device, config);
        }

        /// <summary>
        /// Adds a new network configuration.
        /// </summary>
        /// <param name="device">The network device to use.</param>
        /// <param name="config">The IPv4 configuration associated with the device to use.</param>
        public static void AddConfig(NetworkDevice device, IPConfig config)
        {
            NetworkConfigs.Add(new NetworkConfig(device, config));
        }

        /// <summary>
        /// Returns whether the network stack contains the given network device.
        /// </summary>
        public static bool ConfigsContainsDevice(NetworkDevice targetDevice)
        {
            if (NetworkConfigs == null)
            {
                return false;
            }
            else
            {
                foreach (var device in NetworkConfigs)
                {
                    if (targetDevice == device.Device)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Clears network configurations, removing each configuration.
        /// </summary>
        public static void ClearConfigs()
        {
            NetworkConfigs.Clear();
        }

        /// <summary>
        /// Get the IPv4 configuration for the given network device.
        /// </summary>
        /// <param name="device">Network device.</param>
        public static IPConfig Get(NetworkDevice device)
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
        /// Remove the configuration for the given network device.
        /// </summary>
        /// <param name="key">The target network device.</param>
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
