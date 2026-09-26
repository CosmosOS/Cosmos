// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Pci;

namespace Cosmos.Kernel.HAL.Drivers.Pci;

/// <summary>
/// One entry of a PCI driver's match table: the functions a registration is
/// offered. When several registrations match a function, a
/// <see cref="Device"/> entry beats a three-part <see cref="Class(byte, byte, byte)"/>
/// entry, which beats a two-part <see cref="Class(byte, byte)"/> entry, and
/// the earlier registration wins a tie. A <c>default(PciMatch)</c> matches
/// nothing, and a registration refuses it.
/// </summary>
internal readonly struct PciMatch
{
    private readonly ushort _vendorId;
    private readonly ushort _deviceId;
    private readonly byte _baseClass;
    private readonly byte _subclass;
    private readonly byte _programmingInterface;

    /// <summary>What this entry compares, and so how it ranks; <see cref="PciMatchKind.None"/> for a default value.</summary>
    internal PciMatchKind Kind { get; }

    private PciMatch(PciMatchKind kind, ushort vendorId, ushort deviceId, byte baseClass, byte subclass, byte programmingInterface)
    {
        Kind = kind;
        _vendorId = vendorId;
        _deviceId = deviceId;
        _baseClass = baseClass;
        _subclass = subclass;
        _programmingInterface = programmingInterface;
    }

    /// <summary>Matches the functions with this vendor and device ID.</summary>
    /// <param name="vendorId">The vendor ID, config offset 0x00.</param>
    /// <param name="deviceId">The device ID, config offset 0x02.</param>
    public static PciMatch Device(ushort vendorId, ushort deviceId) =>
        new(PciMatchKind.Device, vendorId, deviceId, 0, 0, 0);

    /// <summary>Matches the functions of this base class and subclass, whatever their programming interface.</summary>
    /// <param name="baseClass">The base class code, config offset 0x0B.</param>
    /// <param name="subclass">The subclass code, config offset 0x0A.</param>
    public static PciMatch Class(byte baseClass, byte subclass) =>
        new(PciMatchKind.Class, 0, 0, baseClass, subclass, 0);

    /// <summary>Matches the functions of this base class, subclass and programming interface.</summary>
    /// <param name="baseClass">The base class code, config offset 0x0B.</param>
    /// <param name="subclass">The subclass code, config offset 0x0A.</param>
    /// <param name="programmingInterface">The programming interface, config offset 0x09.</param>
    public static PciMatch Class(byte baseClass, byte subclass, byte programmingInterface) =>
        new(PciMatchKind.ClassWithInterface, 0, 0, baseClass, subclass, programmingInterface);

    /// <summary>True when <paramref name="function"/> is one this entry names.</summary>
    internal bool Matches(PciDevice function) => Kind switch
    {
        PciMatchKind.Device => function.VendorId == _vendorId && function.DeviceId == _deviceId,
        PciMatchKind.ClassWithInterface => function.ClassCode == _baseClass && function.Subclass == _subclass
            && function.ProgIf == _programmingInterface,
        PciMatchKind.Class => function.ClassCode == _baseClass && function.Subclass == _subclass,
        _ => false
    };
}
