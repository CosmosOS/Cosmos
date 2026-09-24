// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// Base class codes a class driver matches an interface against
/// (usb.org "Defined Class Codes").
/// </summary>
internal static class UsbClassCode
{
    public const byte Hid = 0x03;
    public const byte MassStorage = 0x08;
    public const byte Hub = 0x09;
}
