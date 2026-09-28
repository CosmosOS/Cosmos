// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>
/// The kinds of device a driver can publish. Closed by design: each kind has
/// one contract the driver implements, one sink the kit hands back, and one
/// consumer slot the ring's manager of that kind occupies.
/// </summary>
internal enum DeviceKind
{
    /// <summary>A keyboard: <see cref="IKeyboard"/>, reported through a <see cref="KeyboardSink"/>.</summary>
    Keyboard,

    /// <summary>A mouse or touchpad: <see cref="IPointer"/>, reported through a <see cref="PointerSink"/>.</summary>
    Pointer,

    /// <summary>A network interface: <see cref="INetworkInterface"/>, reported through a <see cref="NetworkSink"/>.</summary>
    Network,

    /// <summary>A block device: the existing <c>IBlockDevice</c> contract, no sink.</summary>
    Block,

    /// <summary>A display: <see cref="IDisplay"/>, reported through a <see cref="DisplaySink"/>.</summary>
    Display,
}
