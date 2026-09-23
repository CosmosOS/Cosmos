// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// bmRequestType of a setup packet (USB 2.0 §9.3.1): one direction, one
/// type and one recipient, OR-ed together.
/// </summary>
[Flags]
internal enum UsbRequestType : byte
{
    HostToDevice = 0x00,
    DeviceToHost = 0x80,

    Standard = 0x00,
    Class = 0x20,
    Vendor = 0x40,

    Device = 0x00,
    Interface = 0x01,
    Endpoint = 0x02,
    Other = 0x03
}
