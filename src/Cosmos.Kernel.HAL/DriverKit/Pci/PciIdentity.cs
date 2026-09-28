// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// The identity of a PCI function as its configuration header states it:
/// where it is (segment, bus, device, function) and what it is (vendor,
/// device, subsystem, class, revision, header layout). Read once by the
/// host that described the function; never changes. Path is
/// <c>pci:ssss:bb:dd.f</c>, all hexadecimal.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class PciIdentity : DeviceIdentity
{
    internal PciIdentity(ushort segment, byte bus, byte device, byte function, ushort vendorId, ushort deviceId,
        ushort subsystemVendorId, ushort subsystemId, byte classCode, byte subclass, byte progIf, byte revision, byte headerType)
    {
        Segment = segment;
        Bus = bus;
        Device = device;
        Function = function;
        VendorId = vendorId;
        DeviceId = deviceId;
        SubsystemVendorId = subsystemVendorId;
        SubsystemId = subsystemId;
        ClassCode = classCode;
        Subclass = subclass;
        ProgIf = progIf;
        Revision = revision;
        HeaderType = headerType;
        Address = $"{segment:x4}:{bus:x2}:{device:x2}.{function:x}";
    }

    /// <summary>The PCI segment group the function's host serves.</summary>
    public ushort Segment { get; }

    /// <summary>The bus number.</summary>
    public byte Bus { get; }

    /// <summary>The device number on the bus, 0 to 31.</summary>
    public byte Device { get; }

    /// <summary>The function number of the device, 0 to 7.</summary>
    public byte Function { get; }

    /// <summary>The vendor id at offset 0x00.</summary>
    public ushort VendorId { get; }

    /// <summary>The device id at offset 0x02.</summary>
    public ushort DeviceId { get; }

    /// <summary>The subsystem vendor id at offset 0x2C of a type 0 header; zero for the other header types.</summary>
    public ushort SubsystemVendorId { get; }

    /// <summary>The subsystem id at offset 0x2E of a type 0 header; zero for the other header types.</summary>
    public ushort SubsystemId { get; }

    /// <summary>The base class code at offset 0x0B.</summary>
    public byte ClassCode { get; }

    /// <summary>The subclass at offset 0x0A.</summary>
    public byte Subclass { get; }

    /// <summary>The programming interface at offset 0x09.</summary>
    public byte ProgIf { get; }

    /// <summary>The revision id at offset 0x08.</summary>
    public byte Revision { get; }

    /// <summary>The header layout: 0 for a device, 1 for a PCI-to-PCI bridge, 2 for a CardBus bridge; the multi-function bit is stripped.</summary>
    public byte HeaderType { get; }

    /// <inheritdoc/>
    public override string BusName => "pci";

    /// <inheritdoc/>
    public override string Address { get; }

    /// <inheritdoc/>
    public override string Describe() =>
        $"{VendorId:x4}:{DeviceId:x4} class {ClassCode:x2}.{Subclass:x2}.{ProgIf:x2}";
}
