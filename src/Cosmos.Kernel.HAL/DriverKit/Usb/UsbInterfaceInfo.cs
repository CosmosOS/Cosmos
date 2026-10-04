// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// One interface of a device's first configuration (alternate setting 0)
/// and its endpoints, as the enumeration core parsed them (USB 2.0 section
/// 9.6.5). The unit a class driver binds: each one becomes a node. Any
/// context once parsed.
/// </summary>
internal sealed class UsbInterfaceInfo
{
    internal UsbInterfaceInfo(byte number, byte interfaceClass, byte subclass, byte protocol)
    {
        Number = number;
        Class = interfaceClass;
        Subclass = subclass;
        Protocol = protocol;
    }

    /// <summary>bInterfaceNumber.</summary>
    internal byte Number { get; }

    /// <summary>bInterfaceClass.</summary>
    internal byte Class { get; }

    /// <summary>bInterfaceSubClass.</summary>
    internal byte Subclass { get; }

    /// <summary>bInterfaceProtocol.</summary>
    internal byte Protocol { get; }

    /// <summary>The endpoints of alternate setting 0, in descriptor order.</summary>
    internal List<UsbEndpoint> Endpoints { get; } = [];
}
