// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// Base class codes a class driver matches an interface against
/// (usb.org "Defined Class Codes").
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public static class UsbClassCode
{
    /// <summary>Human interface device.</summary>
    public const byte Hid = 0x03;

    /// <summary>Mass storage.</summary>
    public const byte MassStorage = 0x08;

    /// <summary>Hub.</summary>
    public const byte Hub = 0x09;
}
