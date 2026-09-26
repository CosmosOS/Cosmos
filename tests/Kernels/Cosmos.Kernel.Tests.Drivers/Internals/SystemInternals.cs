// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.IPv6;
using SampleDrivers;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// What the mouse manager and the network stack hold about a device a
/// driver published, which the ring does not report because a kernel never
/// holds the device itself, and the stack's per-device configuration, which
/// the race cells drive directly. The cells check with these that an
/// unplugged mouse or link left its manager and the stack entirely. Thread
/// context.
/// </summary>
/// <remarks>
/// Reached through <see cref="UnsafeAccessorAttribute"/> and
/// <see cref="UnsafeAccessorTypeAttribute"/>, as <see cref="PciFunctions"/>
/// describes: an accessor whose target moved throws, failing its cell. The
/// device each member takes is the internal one a
/// <see cref="PublishedMouseView"/> or <see cref="PublishedLinkView"/> holds.
/// </remarks>
internal static class SystemInternals
{
    private const string MouseManagerType = "Cosmos.Kernel.System.Mouse.MouseManager, Cosmos.Kernel.System";
    private const string NetworkStackType = "Cosmos.Kernel.System.Network.NetworkStack, Cosmos.Kernel.System";
    private const string MouseDeviceInterfaceType = "Cosmos.Kernel.HAL.Interfaces.Devices.IMouseDevice, Cosmos.Kernel.HAL.Interfaces";
    private const string NetworkDeviceInterfaceType = "Cosmos.Kernel.HAL.Interfaces.Devices.INetworkDevice, Cosmos.Kernel.HAL.Interfaces";

    /// <summary>True while the mouse manager holds <paramref name="mouse"/>'s device among its mice.</summary>
    /// <param name="mouse">A published mouse.</param>
    /// <returns>What the manager answers.</returns>
    /// <exception cref="MissingMethodException">The mouse manager no longer has the method.</exception>
    public static bool IsRegistered(PublishedMouseView mouse) => IsMouseRegistered(null, mouse.Device);

    /// <summary>The link-local IPv6 address the network stack maps to <paramref name="link"/>'s device, or null.</summary>
    /// <param name="link">A published link.</param>
    /// <returns>The address, or null while the stack maps none to the device.</returns>
    /// <exception cref="MissingMethodException">The network stack no longer has the method.</exception>
    public static Address6? LinkLocalOf(PublishedLinkView link) => LinkLocalOfDevice(null, link.Device);

    /// <summary>The IPv4 configuration the stack holds for <paramref name="link"/>'s device, or null.</summary>
    /// <param name="link">A published link.</param>
    /// <returns>The configuration.</returns>
    /// <exception cref="MissingMethodException">The configuration list no longer has the method.</exception>
    public static IPConfig? GetConfig(PublishedLinkView link) => GetConfigOf(null, link.Device);

    /// <summary>Records <paramref name="config"/> for <paramref name="link"/>'s device in the stack's configuration list.</summary>
    /// <param name="link">A link.</param>
    /// <param name="config">Its configuration.</param>
    /// <exception cref="MissingMethodException">The configuration list no longer has the method.</exception>
    public static void SetConfig(PublishedLinkView link, IPConfig config) => SetConfigOf(null, link.Device, config);

    /// <summary>Takes <paramref name="link"/>'s device's configuration out of the stack's configuration list.</summary>
    /// <param name="link">A link.</param>
    /// <exception cref="MissingMethodException">The configuration list no longer has the method.</exception>
    public static void RemoveConfig(PublishedLinkView link) => RemoveConfigOf(null, link.Device);

    /// <summary>
    /// The source address the stack picks for a packet to
    /// <paramref name="destination"/>: the address of the first configured
    /// interface on its network, or null.
    /// </summary>
    /// <param name="destination">The destination.</param>
    /// <returns>The source address.</returns>
    /// <exception cref="MissingMethodException">The configuration list no longer has the method.</exception>
    public static Address? FindNetwork(Address destination) => FindNetworkOf(null, destination);

    /// <summary>A configuration for no device yet, as the stack builds one for a device it configures.</summary>
    /// <param name="address">The interface's address.</param>
    /// <param name="subnetMask">Its network's mask.</param>
    /// <param name="defaultGateway">Its gateway.</param>
    /// <returns>The configuration.</returns>
    /// <exception cref="MissingMethodException">The configuration's constructor changed.</exception>
    public static IPConfig NewConfig(Address address, Address subnetMask, Address defaultGateway) =>
        NewIPConfig(address, subnetMask, defaultGateway);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "IsRegistered")]
    private static extern bool IsMouseRegistered([UnsafeAccessorType(MouseManagerType)] object? manager,
        [UnsafeAccessorType(MouseDeviceInterfaceType)] object mouse);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "LinkLocalOf")]
    private static extern Address6? LinkLocalOfDevice([UnsafeAccessorType(NetworkStackType)] object? stack,
        [UnsafeAccessorType(NetworkDeviceInterfaceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Get")]
    private static extern IPConfig? GetConfigOf(IPConfig? configs, [UnsafeAccessorType(NetworkDeviceInterfaceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Set")]
    private static extern void SetConfigOf(IPConfig? configs, [UnsafeAccessorType(NetworkDeviceInterfaceType)] object device, IPConfig config);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Remove")]
    private static extern void RemoveConfigOf(IPConfig? configs, [UnsafeAccessorType(NetworkDeviceInterfaceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "FindNetwork")]
    private static extern Address? FindNetworkOf(IPConfig? configs, Address destination);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern IPConfig NewIPConfig(Address address, Address subnetMask, Address defaultGateway);
}
