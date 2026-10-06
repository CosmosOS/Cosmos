// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.HAL.Devices;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// Manages network devices: every interface a driver kit driver publishes,
/// which the manager's <see cref="KitNetworkConsumer"/> registers from the
/// kit worker and unregisters when it is withdrawn. The table is updated
/// with interrupts disabled, so a registration from the worker and a read
/// from the ring's thread never see it half-written.
/// </summary>
public static class NetworkManager
{
    private static INetworkDevice?[]? s_devices;
    private static int s_deviceCount;
    private static int s_primaryIndex = -1;

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

    /// <summary>
    /// Gets whether the network manager is initialized, which is what makes
    /// the device table exist.
    /// </summary>
    public static bool IsInitialized => s_devices is not null;

    /// <summary>
    /// Gets the primary network device. Internal: a kernel names a device with
    /// a <see cref="NetworkAdapter"/> rather than holding the contract.
    /// </summary>
    internal static INetworkDevice? PrimaryDevice =>
        s_primaryIndex >= 0 && s_devices is not null ? s_devices[s_primaryIndex] : null;

    /// <summary>
    /// The adapter the ring uses when no other is named: the target of
    /// <see cref="Send"/>, of the primary shortcuts on this class, and of
    /// <see cref="Config.IPConfig.Enable(Address, Address, Address)"/>.
    /// It starts as the first device registered: the first interface a kit
    /// driver publishes, in the order the driver stage binds them.
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
    /// Gets whether the primary device link is up.
    /// </summary>
    public static bool LinkUp => PrimaryDevice?.LinkUp ?? false;

    /// <summary>
    /// Gets the number of registered network devices.
    /// </summary>
    public static int DeviceCount => s_deviceCount;

    /// <summary>
    /// Initializes the network manager. Called once during boot, before the
    /// platform network device is registered and before the driver stage
    /// runs: the table exists from here on, and the kit's network consumer
    /// is installed so every interface a driver publishes lands in it.
    /// </summary>
    internal static void Initialize()
    {
        ThrowIfDisabled();

        if (s_devices is not null)
        {
            return;
        }

        s_deviceCount = 0;
        s_primaryIndex = -1;
        s_devices = new INetworkDevice[8];
        DeviceRegistry.SetConsumer(DeviceKind.Network, new KitNetworkConsumer());
    }

    /// <summary>
    /// Registers a network device with the manager; the first registered
    /// becomes the primary. Thread context, from the kit worker when a
    /// published interface is consumed; the table is updated with interrupts
    /// disabled.
    /// </summary>
    /// <param name="device">The network device to register.</param>
    /// <returns>False when the device is null, the manager is not initialized or the table's eight slots are taken; the device is not registered then.</returns>
    internal static bool RegisterDevice(INetworkDevice device)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (device is null || s_devices is null || s_deviceCount >= s_devices.Length)
            {
                return false;
            }

            s_devices[s_deviceCount++] = device;

            // First device becomes primary
            if (s_primaryIndex < 0)
            {
                s_primaryIndex = s_deviceCount - 1;
            }

            return true;
        }
    }

    /// <summary>
    /// Takes a device out of the table, compacting the entries after it
    /// and moving the primary index with them; a withdrawn primary falls
    /// back to the first remaining device. Thread context, from the kit
    /// worker when a published interface is withdrawn; the table is updated
    /// with interrupts disabled. The device's IP configuration is not
    /// removed and <see cref="NetworkAdapter"/> handles stay positional, so a
    /// handle taken before the withdrawal may name the device that moved
    /// into the slot.
    /// </summary>
    /// <param name="device">The network device to remove; nothing when it is not registered.</param>
    internal static void UnregisterDevice(INetworkDevice device)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            if (s_devices is null)
            {
                return;
            }

            int index = -1;
            for (int i = 0; i < s_deviceCount; i++)
            {
                if (ReferenceEquals(s_devices[i], device))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                return;
            }

            for (int i = index; i < s_deviceCount - 1; i++)
            {
                s_devices[i] = s_devices[i + 1];
            }

            s_deviceCount--;
            s_devices[s_deviceCount] = null;

            if (s_primaryIndex == index)
            {
                s_primaryIndex = s_deviceCount > 0 ? 0 : -1;
            }
            else if (s_primaryIndex > index)
            {
                s_primaryIndex--;
            }
        }
    }

    /// <summary>
    /// Gets a network device by index.
    /// </summary>
    /// <param name="index">The device index.</param>
    /// <returns>The network device, or null if not found.</returns>
    internal static INetworkDevice? GetDevice(int index)
    {
        if (s_devices is null || index < 0 || index >= s_deviceCount)
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
}
