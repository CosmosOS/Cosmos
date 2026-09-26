// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// Manages network devices.
/// </summary>
public static class NetworkManager
{
    /// <summary>
    /// One registered device and the generation stamp it was registered
    /// under, which the handles to it carry.
    /// </summary>
    private sealed class Registration
    {
        internal INetworkDevice Device { get; }

        internal int Generation { get; }

        internal Registration(INetworkDevice device, int generation)
        {
            Device = device;
            Generation = generation;
        }
    }

    /// <summary>
    /// The registered devices in registration order, up to
    /// <see cref="s_deviceCount"/>. A slot holds a device together with the
    /// generation stamp it was registered under, in one object, so a reader
    /// never pairs a device with another's stamp.
    /// </summary>
    private static Registration?[]? s_devices;
    private static int s_deviceCount;

    /// <summary>
    /// The primary device's registration, or null with no device. Held as
    /// the registration rather than an index: unregistering a device moves
    /// the ones after it down a slot, which must not make the primary name
    /// another device.
    /// </summary>
    private static Registration? s_primary;

    /// <summary>
    /// The last generation stamp handed out. Each registration takes the
    /// next, so a stamp names one registration for the life of the kernel,
    /// and 0, a default <see cref="NetworkAdapter"/>'s, names none.
    /// </summary>
    private static int s_lastGeneration;

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
    internal static INetworkDevice? PrimaryDevice => s_primary?.Device;

    /// <summary>
    /// The adapter the ring uses when no other is named: the target of
    /// <see cref="Send"/>, of the primary shortcuts on this class, and of
    /// <see cref="Config.IPConfig.Enable(Address, Address, Address)"/>.
    /// It starts as the first device HAL enumeration registered. When the
    /// primary device is unregistered, which only a USB device a driver
    /// published can be, the first device left becomes primary.
    /// </summary>
    /// <exception cref="InvalidOperationException">Network support is disabled.</exception>
    /// <exception cref="ArgumentException">Thrown when the assigned handle names no registered device.</exception>
    public static NetworkAdapter Primary
    {
        get => s_primary is { } primary ? new NetworkAdapter(primary.Generation) : default;
        set
        {
            ThrowIfDisabled();

            s_primary = Find(value.Generation)
                ?? throw new ArgumentException("Handle names no registered network device", nameof(value));
        }
    }

    /// <summary>
    /// The adapter registered at <paramref name="index"/>. Enumerate with
    /// <see cref="DeviceCount"/>.
    /// </summary>
    /// <param name="index">Position among the registered devices, from 0 to <see cref="DeviceCount"/> - 1.</param>
    /// <returns>A handle to that device, or one whose <see cref="NetworkAdapter.IsValid"/> is false when there is none.</returns>
    public static NetworkAdapter GetAdapter(int index)
    {
        return GetRegistration(index) is { } registration ? new NetworkAdapter(registration.Generation) : default;
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
    public static int DeviceCount => Volatile.Read(ref s_deviceCount);

    /// <summary>
    /// Initializes the network manager. Called once during boot, before the
    /// platform network device is registered.
    /// </summary>
    internal static void Initialize()
    {
        ThrowIfDisabled();

        if (s_devices is not null)
        {
            return;
        }

        s_deviceCount = 0;
        s_primary = null;
        s_devices = new Registration?[8];
    }

    /// <summary>
    /// Registers a network device with the manager.
    /// </summary>
    /// <param name="device">The network device to register.</param>
    internal static void RegisterDevice(INetworkDevice device)
    {
        Registration?[]? devices = s_devices;
        int index = s_deviceCount;
        if (device is null || devices is null || index >= devices.Length)
        {
            return;
        }

        // The slot, and the primary when this is the first device, before
        // the count that makes them visible: a device a driver publishes
        // registers from the driver pass or the USB hot-plug thread, while
        // other code may already walk the table up to the count.
        Registration registration = new(device, ++s_lastGeneration);
        devices[index] = registration;
        s_primary ??= registration;

        Volatile.Write(ref s_deviceCount, index + 1);
    }

    /// <summary>
    /// Takes a device that is gone out of the manager and the stack: a
    /// network link a USB driver published, whose device was pulled out.
    /// The devices registered after it move down a slot, so their indexes
    /// change, while every <see cref="NetworkAdapter"/> keeps naming the
    /// device it named: a handle to the one that left names none from now
    /// on, and never the device that takes its slot. The stack forgets the
    /// device's addresses, its MAC address and its IPv4 configuration. A
    /// device that is not registered is left alone.
    /// </summary>
    /// <param name="device">The device to take out.</param>
    internal static void UnregisterDevice(INetworkDevice device)
    {
        Registration?[]? devices = s_devices;
        if (devices is null)
        {
            return;
        }

        // Interrupts off for the whole change: the stack's receive path runs
        // in interrupt handlers and takes no lock, and on this single CPU no
        // other thread runs meanwhile either, so nothing sees the table or
        // the stack's maps half changed.
        using (InternalCpu.DisableInterruptsScope())
        {
            int count = s_deviceCount;
            int index = -1;
            for (int i = 0; i < count; i++)
            {
                if (devices[i] is { } registered && ReferenceEquals(registered.Device, device))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                return;
            }

            // A new primary before the device leaves, so the primary never
            // names a device that is gone: the first one left, or none.
            if (s_primary == devices[index])
            {
                if (index > 0)
                {
                    s_primary = devices[0];
                }
                else
                {
                    s_primary = count > 1 ? devices[1] : null;
                }
            }

            // Down a slot, each slot written whole; then the count; then the
            // slot past it cleared. A reader bounded by an earlier count
            // finds a device there, possibly one it already saw, never a
            // torn slot, and a reader that finds a null slot skips it, as
            // GetDevice's callers already do.
            for (int i = index; i < count - 1; i++)
            {
                devices[i] = devices[i + 1];
            }

            Volatile.Write(ref s_deviceCount, count - 1);
            devices[count - 1] = null;

            NetworkStack.RemoveDevice(device);
        }

        Serial.WriteString($"[NetworkManager] Unregistered {device.Name}, total: {DeviceCount}\n");
    }

    /// <summary>
    /// Gets a network device by index.
    /// </summary>
    /// <param name="index">The device index.</param>
    /// <returns>The network device, or null if not found.</returns>
    internal static INetworkDevice? GetDevice(int index) => GetRegistration(index)?.Device;

    /// <summary>
    /// The device registered under <paramref name="generation"/>, the stamp
    /// a <see cref="NetworkAdapter"/> carries, or null when none is: a
    /// default handle's, or one whose device was unregistered.
    /// </summary>
    internal static INetworkDevice? FindDevice(int generation) => Find(generation)?.Device;

    /// <summary>
    /// The index the device registered under <paramref name="generation"/>
    /// sits at now, or -1 when none is registered under it.
    /// </summary>
    internal static int IndexOf(int generation) => Locate(generation, out _);

    private static Registration? GetRegistration(int index)
    {
        Registration?[]? devices = s_devices;
        if (devices is null || index < 0 || index >= DeviceCount)
        {
            return null;
        }

        return devices[index];
    }

    private static Registration? Find(int generation)
    {
        _ = Locate(generation, out Registration? registration);
        return registration;
    }

    /// <summary>
    /// Finds the registration stamped <paramref name="generation"/> in one
    /// walk of the table, so its index and the registration agree.
    /// </summary>
    /// <returns>Its index, or -1 when no device is registered under the stamp.</returns>
    private static int Locate(int generation, out Registration? registration)
    {
        registration = null;
        Registration?[]? devices = s_devices;
        if (devices is null || generation == 0)
        {
            return -1;
        }

        int count = DeviceCount;
        for (int i = 0; i < count; i++)
        {
            if (devices[i] is { } registered && registered.Generation == generation)
            {
                registration = registered;
                return i;
            }
        }

        return -1;
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
