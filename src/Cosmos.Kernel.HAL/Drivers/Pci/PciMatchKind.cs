// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.Pci;

/// <summary>
/// What a <see cref="PciMatch"/> compares. Ordered from least to most
/// specific: the driver pass offers a function to the candidate whose
/// matching entry has the highest kind first.
/// </summary>
internal enum PciMatchKind : byte
{
    /// <summary>A <c>default(PciMatch)</c>, which matches nothing and which a registration refuses.</summary>
    None = 0,

    /// <summary>Base class and subclass.</summary>
    Class = 1,

    /// <summary>Base class, subclass and programming interface.</summary>
    ClassWithInterface = 2,

    /// <summary>Vendor and device ID.</summary>
    Device = 3
}
