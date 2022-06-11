/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Network dictionary
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System.Collections.Generic;
using Cosmos.HAL;
using Cosmos.System.Network.IPv4;

namespace Cosmos.System.Network.Config;

/// <summary>
///     Network Configuration (link network device to an ip address)
/// </summary>
public class NetworkConfig
{
    /// <summary>
    ///     Network device
    /// </summary>
    public NetworkDevice Device;

    /// <summary>
    ///     IPv4 Configuration
    /// </summary>
    public IPConfig IPConfig;

    /// <summary>
    ///     NetworkConfig ctor
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
///     Network stack configuration
/// </summary>
public static class NetworkConfiguration
{
    /// <summary>
    ///     Current network configuration used by the network stack
    /// </summary>
    public static List<NetworkConfig> NetworkConfigs = new();

    /// <summary>
    ///     Current network configuration used by the network stack
    /// </summary>
    public static NetworkConfig CurrentNetworkConfig { get; set; }

    /// <summary>
    ///     Network congiruations count
    /// </summary>
    public static int Count => NetworkConfigs.Count;

    /// <summary>
    ///     Current IPv4 address
    /// </summary>
    public static Address CurrentAddress => CurrentNetworkConfig.IPConfig.IPAddress;

    /// <summary>
    ///     Set current network config
    /// </summary>
    /// <param name="device">Network device.</param>
    /// <param name="config">IP Config</param>
    public static void SetCurrentConfig(NetworkDevice device, IPConfig config) =>
        CurrentNetworkConfig = new NetworkConfig(device, config);

    /// <summary>
    ///     Add new network config
    /// </summary>
    /// <param name="device">Network device.</param>
    /// <param name="config">IP Config</param>
    public static void AddConfig(NetworkDevice device, IPConfig config) =>
        NetworkConfigs.Add(new NetworkConfig(device, config));

    /// <summary>
    ///     Network stack contains device
    /// </summary>
    /// <param name="device">Network device.</param>
    public static bool ConfigsContainsDevice(NetworkDevice k)
    {
        if (NetworkConfigs == null)
        {
            return false;
        }

        foreach (var device in NetworkConfigs)
        {
            if (k == device.Device)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Clear network configurations
    /// </summary>
    public static void ClearConfigs() => NetworkConfigs.Clear();

    /// <summary>
    ///     Get ip config for network device
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
    ///     Remove Config for network device
    /// </summary>
    /// <param name="device">Network device.</param>
    public static void Remove(NetworkDevice key)
    {
        var index = 0;

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
