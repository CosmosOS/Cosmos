// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// Manages network devices.
/// </summary>
public static class NetworkManager
{
    /// <summary>
    /// Whether network support is enabled. Uses centralized feature flag.
    /// </summary>
    public static bool IsEnabled => CosmosFeatures.NetworkEnabled;

    /// <summary>
    /// Throws when network support is compiled out. Guards actions, not reads:
    /// a read answers honestly (0, null, false, empty) so a kernel can branch
    /// on it, and an action names the switch to set instead of failing
    /// silently.
    /// </summary>
    private static void ThrowIfDisabled()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Network support is disabled. Set CosmosEnableNetwork=true in your csproj to enable it.");
        }
    }

    private static INetworkDevice?[]? s_devices;
    private static int s_deviceCount;
    private static int s_primaryIndex = -1;

    /// <summary>
    /// Gets whether the network manager is initialized, which is what makes
    /// the device table exist.
    /// </summary>
    public static bool IsInitialized => s_devices != null;

    /// <summary>
    /// Gets the primary network device. Internal: a kernel names a device with
    /// a <see cref="NetworkAdapter"/> rather than holding the contract.
    /// </summary>
    internal static INetworkDevice? PrimaryDevice =>
        s_primaryIndex >= 0 && s_devices != null ? s_devices[s_primaryIndex] : null;

    /// <summary>
    /// The adapter the ring uses when no other is named: the target of
    /// <see cref="Send"/>, of the primary shortcuts on this class, and of
    /// <see cref="Config.IPConfig.Enable(IPv4.Address, IPv4.Address, IPv4.Address)"/>.
    /// It starts as the first device HAL enumeration registered.
    /// </summary>
    /// <exception cref="InvalidOperationException">Network support is disabled.</exception>
    /// <exception cref="ArgumentException">Thrown when the assigned handle names no registered device.</exception>
    public static NetworkAdapter Primary
    {
        get => s_primaryIndex >= 0 ? new NetworkAdapter(s_primaryIndex) : default;
        set
        {
            ThrowIfDisabled();

            if (!value.IsValid)
            {
                throw new ArgumentException("Handle names no registered network device", nameof(value));
            }

            s_primaryIndex = value.Index;
        }
    }

    /// <summary>
    /// The adapter registered at <paramref name="index"/>. Enumerate with
    /// <see cref="DeviceCount"/>.
    /// </summary>
    /// <param name="index">Registration index, from 0 to <see cref="DeviceCount"/> - 1.</param>
    /// <returns>A handle to that device, or one whose <see cref="NetworkAdapter.IsValid"/> is false when there is none.</returns>
    public static NetworkAdapter GetAdapter(int index)
    {
        return index >= 0 && index < s_deviceCount ? new NetworkAdapter(index) : default;
    }

    /// <summary>
    /// The primary device's name, or null when there is no device.
    /// </summary>
    public static string? Name => PrimaryDevice?.Name;

    /// <summary>
    /// The primary device's MAC address, or null when there is no device. A
    /// device that has not finished initializing reports
    /// <see cref="MACAddress.None"/>, the all-zero address, rather than null;
    /// see <see cref="Ready"/>.
    /// </summary>
    public static MACAddress? MacAddress => PrimaryDevice?.MacAddress;

    /// <summary>
    /// Whether the primary device finished initializing and can carry traffic.
    /// </summary>
    public static bool Ready => PrimaryDevice?.Ready ?? false;

    /// <summary>
    /// Gets the number of registered network devices.
    /// </summary>
    public static int DeviceCount => s_deviceCount;

    /// <summary>
    /// Initializes the network manager. Called once during boot, before the
    /// platform network device is registered.
    /// </summary>
    internal static void Initialize()
    {
        ThrowIfDisabled();

        if (s_devices != null)
        {
            return;
        }

        s_deviceCount = 0;
        s_primaryIndex = -1;
        s_devices = new INetworkDevice[8];
    }

    /// <summary>
    /// Registers a network device with the manager.
    /// </summary>
    /// <param name="device">The network device to register.</param>
    internal static void RegisterDevice(INetworkDevice device)
    {
        if (device == null || s_devices == null || s_deviceCount >= s_devices.Length)
        {
            return;
        }

        s_devices[s_deviceCount++] = device;

        // First device becomes primary
        if (s_primaryIndex < 0)
        {
            s_primaryIndex = s_deviceCount - 1;
        }
    }

    /// <summary>
    /// Gets a network device by index.
    /// </summary>
    /// <param name="index">The device index.</param>
    /// <returns>The network device, or null if not found.</returns>
    internal static INetworkDevice? GetDevice(int index)
    {
        if (s_devices == null || index < 0 || index >= s_deviceCount)
        {
            return null;
        }

        return s_devices[index];
    }

    /// <summary>
    /// Sends a packet using the primary network device.
    /// </summary>
    /// <param name="data">The packet data.</param>
    /// <param name="length">The packet length.</param>
    /// <returns>True if the packet was sent successfully, false when there is
    /// no primary device or network support is compiled out.</returns>
    public static bool Send(byte[] data, int length)
    {
        // No ThrowIfDisabled: this bool already means "it did not happen", so
        // the middle row of the compiled-out table applies and a switched-off
        // build answers false like any other unsendable state.
        return IsEnabled && (PrimaryDevice?.Send(data, length) ?? false);
    }

    /// <summary>
    /// Gets whether the primary device link is up.
    /// </summary>
    public static bool LinkUp => PrimaryDevice?.LinkUp ?? false;
}
