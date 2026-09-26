using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv6;

namespace Cosmos.Kernel.System.Network.Config;

/// <summary>
/// Represents IPv4 configuration.
/// </summary>
public class IPConfig
{
    /// <summary>
    /// One device and the configuration in force on it.
    /// </summary>
    private sealed class Entry
    {
        internal Entry(INetworkDevice device, IPConfig config)
        {
            Device = device;
            Config = config;
        }

        internal INetworkDevice Device { get; }

        internal IPConfig Config { get; }
    }

    /// <summary>
    /// Every configured interface. This is the only store: routing lookups and
    /// per-device lookups read the same list, so neither can drift from the
    /// other. Replaced whole on every change, never changed in place: a
    /// network device a USB driver published leaves on the USB hot-plug
    /// thread, which takes its entry out while a kernel thread may be halfway
    /// through a walk choosing a source address, and the array that walk read
    /// stays as it was.
    /// </summary>
    private static Entry[] s_configs = [];

    /// <summary>
    /// Record the configuration now in force on a device, replacing any
    /// earlier one so a reconfigured device leaves no stale route behind.
    /// </summary>
    /// <param name="device">The configured device.</param>
    /// <param name="config">The configuration applied to it.</param>
    internal static void Set(INetworkDevice device, IPConfig config)
    {
        Entry entry = new(device, config);

        // Interrupts off from the read to the publication, so on this single
        // CPU no other writer, such as the hot-plug thread taking an unplugged
        // device out, runs in between and has its change lost.
        using (InternalCpu.DisableInterruptsScope())
        {
            Entry[] configs = s_configs;
            for (int i = 0; i < configs.Length; i++)
            {
                if (configs[i].Device == device)
                {
                    Entry[] replaced = [.. configs];
                    replaced[i] = entry;
                    s_configs = replaced;
                    return;
                }
            }

            s_configs = [.. configs, entry];
        }
    }

    /// <summary>
    /// Forget the configuration of a device that left the network manager,
    /// so no route leaves through it any more.
    /// </summary>
    /// <param name="device">The device that left.</param>
    internal static void Remove(INetworkDevice device)
    {
        // As in Set: no other writer runs between the read and the publication.
        using (InternalCpu.DisableInterruptsScope())
        {
            Entry[] configs = s_configs;
            for (int i = 0; i < configs.Length; i++)
            {
                if (configs[i].Device == device)
                {
                    Entry[] kept = new Entry[configs.Length - 1];
                    configs.AsSpan(0, i).CopyTo(kept);
                    configs.AsSpan(i + 1).CopyTo(kept.AsSpan(i));
                    s_configs = kept;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Forget every configured interface. Internal: this drops the routing
    /// list alone, leaving the stack's address and MAC maps behind.
    /// <see cref="NetworkStack.RemoveAllConfigIP"/> is the complete reset and
    /// the only caller.
    /// </summary>
    internal static void RemoveAll()
    {
        s_configs = [];
    }

    /// <summary>
    /// The configuration in force on a device, or null when it has none.
    /// </summary>
    /// <param name="device">The device to look up.</param>
    internal static IPConfig? Get(INetworkDevice device)
    {
        foreach (Entry entry in s_configs)
        {
            if (entry.Device == device)
            {
                return entry.Config;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the source address to send to the specified destination from.
    /// Internal: every caller is a client inside this assembly choosing its own
    /// source address, and a kernel names an interface with a
    /// <see cref="NetworkAdapter"/> instead.
    /// </summary>
    /// <param name="destination">The destination IP address.</param>
    internal static Address? FindNetwork(Address destination)
    {
        if (destination is Address6)
        {
            return FindNetwork6();
        }

        Address? defaultGw = null;

        foreach (Entry entry in s_configs)
        {
            IPConfig ipConfig = entry.Config;

            if ((ipConfig.Address & ipConfig.SubnetMask) ==
                (destination & ipConfig.SubnetMask))
            {
                return ipConfig.Address;
            }
            if (defaultGw is null && !ipConfig.DefaultGateway.IsZero)
            {
                defaultGw = ipConfig.Address;
            }

            if (!IsLocalAddress(destination))
            {
                return ipConfig.Address;
            }
        }

        return defaultGw;
    }

    /// <summary>
    /// The IPv6 address to send from. There is no IPv6 routing table and no
    /// address configuration beyond the link-local address every device comes
    /// up with, so every IPv6 destination is sourced from the primary
    /// device's link-local address.
    /// </summary>
    /// <returns>The link-local address, or null when no device has one yet.</returns>
    private static Address? FindNetwork6()
    {
        INetworkDevice? device = NetworkManager.PrimaryDevice;
        return device is null ? null : NetworkStack.LinkLocalOf(device);
    }

    /// <summary>
    /// Enables a network device with the specified IP configuration.
    /// </summary>
    /// <param name="device">The network device to enable.</param>
    /// <param name="address">The IP address to assign to the device.</param>
    /// <param name="subnetMask">The subnet mask to use for the device.</param>
    /// <param name="defaultGateway">The default gateway address to use for the device.</param>
    /// <returns><see langword="true"/> if the device was successfully enabled, <see langword="false"/> otherwise.</returns>
    internal static bool Enable(INetworkDevice device, Address address, Address subnetMask, Address defaultGateway)
    {
        if (device is not null)
        {
            IPConfig config = new(address, subnetMask, defaultGateway);
            NetworkStack.ConfigIP(device, config);
            Serial.WriteString("[IPConfig] Config OK.\n");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Assign an IPv4 configuration to the primary network device.
    /// </summary>
    /// <param name="address">The IP address to assign.</param>
    /// <param name="subnetMask">The subnet mask to use.</param>
    /// <param name="defaultGateway">The default gateway address to use.</param>
    /// <returns><see langword="true"/> if the configuration was applied, <see langword="false"/> when there is no device.</returns>
    public static bool Enable(Address address, Address subnetMask, Address defaultGateway)
    {
        INetworkDevice? device = NetworkManager.PrimaryDevice;
        if (device is null)
        {
            return false;
        }

        return Enable(device, address, subnetMask, defaultGateway);
    }

    /// <summary>
    /// Assign an IPv4 configuration to a named network device.
    /// </summary>
    /// <param name="adapter">Handle to the device, from <see cref="NetworkManager.GetAdapter(int)"/>.</param>
    /// <param name="address">The IP address to assign.</param>
    /// <param name="subnetMask">The subnet mask to use.</param>
    /// <param name="defaultGateway">The default gateway address to use.</param>
    /// <returns><see langword="true"/> if the configuration was applied, <see langword="false"/> when the handle names no device.</returns>
    public static bool Enable(NetworkAdapter adapter, Address address, Address subnetMask, Address defaultGateway)
    {
        INetworkDevice? device = adapter.Device;
        if (device is null)
        {
            return false;
        }

        return Enable(device, address, subnetMask, defaultGateway);
    }

    /// <summary>
    /// Check if the given address is a local address.
    /// </summary>
    /// <param name="destIP">The address to check.</param>
    internal static bool IsLocalAddress(Address destIP)
    {
        foreach (Entry entry in s_configs)
        {
            IPConfig ipConfig = entry.Config;

            if ((ipConfig.Address & ipConfig.SubnetMask) ==
                (destIP & ipConfig.SubnetMask))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Find the interface by the given IP address.
    /// </summary>
    /// <param name="sourceIP">Source IP.</param>
    internal static INetworkDevice? FindInterface(Address sourceIP)
    {
        return NetworkStack.AddressMap.TryGetValue(sourceIP, out INetworkDevice? device) ? device : null;
    }

    /// <summary>
    /// Find route to address.
    /// </summary>
    /// <param name="destIP">Destination IP.</param>
    /// <returns>Address value.</returns>
    internal static Address? FindRoute(Address destIP)
    {
        // There is no routing table: every non-local destination leaves
        // through the first configured interface's default gateway. One read
        // of the list, so the length checked is the one indexed.
        Entry[] configs = s_configs;
        return configs.Length > 0 ? configs[0].Config.DefaultGateway : null;
    }

    /// <summary>
    /// Creates a IPv4 Configuration. Internal: a kernel reads a configuration
    /// back from <see cref="NetworkAdapter.IPConfig"/> and applies one with
    /// <see cref="Enable(Address, Address, Address)"/>; it never supplies the
    /// object itself, and the only caller is this class's own Enable.
    /// </summary>
    /// <param name="address">The IPv4 address to assign.</param>
    /// <param name="subnetMask">The subnet mask.</param>
    /// <param name="defaultGateway">The default gateway.</param>
    internal IPConfig(Address address, Address subnetMask, Address defaultGateway)
    {
        Address = address;
        SubnetMask = subnetMask;
        DefaultGateway = defaultGateway;
    }

    /// <summary>
    /// The IPv4 address assigned to the device this configuration belongs to.
    /// </summary>
    public Address Address { get; }

    /// <summary>
    /// The subnet mask.
    /// </summary>
    public Address SubnetMask { get; }

    /// <summary>
    /// The default gateway address.
    /// </summary>
    public Address DefaultGateway { get; }
}
