// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// bmRequestType of a setup packet (USB 2.0 section 9.3.1): one direction,
/// one type and one recipient, OR-ed together.
/// </summary>
[Flags]
[Experimental(Experimentals.DriverKitSeamDiagId)]
public enum UsbRequestType : byte
{
    /// <summary>Direction: the data stage, if any, goes to the device.</summary>
    HostToDevice = 0x00,

    /// <summary>Direction: the data stage comes from the device.</summary>
    DeviceToHost = 0x80,

    /// <summary>Type: a standard request of chapter 9.</summary>
    Standard = 0x00,

    /// <summary>Type: a request a device class defines.</summary>
    Class = 0x20,

    /// <summary>Type: a request a vendor defines.</summary>
    Vendor = 0x40,

    /// <summary>Recipient: the device.</summary>
    Device = 0x00,

    /// <summary>Recipient: an interface, named in wIndex.</summary>
    Interface = 0x01,

    /// <summary>Recipient: an endpoint, named in wIndex.</summary>
    Endpoint = 0x02,

    /// <summary>Recipient: other, a hub port for the hub class.</summary>
    Other = 0x03
}
