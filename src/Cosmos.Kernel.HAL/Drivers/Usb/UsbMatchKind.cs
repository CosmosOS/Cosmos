// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// What a <see cref="UsbMatch"/> compares. Ordered from least to most
/// specific: the kit offers an interface to the candidate whose matching
/// entry has the highest kind first.
/// </summary>
internal enum UsbMatchKind : byte
{
    /// <summary>A <c>default(UsbMatch)</c>, which matches nothing and which a registration refuses.</summary>
    None = 0,

    /// <summary>Interface class.</summary>
    Class = 1,

    /// <summary>Interface class and subclass.</summary>
    ClassWithSubclass = 2,

    /// <summary>Interface class, subclass and protocol.</summary>
    ClassWithProtocol = 3,

    /// <summary>The device's vendor and product ID, whatever the interface.</summary>
    Device = 4
}
