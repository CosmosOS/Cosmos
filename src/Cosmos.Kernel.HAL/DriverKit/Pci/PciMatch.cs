// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// A match over PCI identities: every field given must equal the
/// function's, every field left null is not looked at, and the specificity
/// is the number of fields given. A match that constrains nothing is
/// refused: until the HAL's USB host controller driver moves into the
/// kit, a catch-all driver would quiesce the function it operates.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class PciMatch : DeviceMatch
{
    private readonly ushort? _vendorId;
    private readonly ushort? _deviceId;
    private readonly ushort? _subsystemVendorId;
    private readonly ushort? _subsystemId;
    private readonly byte? _classCode;
    private readonly byte? _subclass;
    private readonly byte? _progIf;
    private readonly byte? _revision;

    /// <summary>Creates a match over the fields given; each null field is unconstrained.</summary>
    /// <param name="vendorId">The vendor id, or null.</param>
    /// <param name="deviceId">The device id, or null.</param>
    /// <param name="subsystemVendorId">The subsystem vendor id, or null.</param>
    /// <param name="subsystemId">The subsystem id, or null.</param>
    /// <param name="classCode">The base class code, or null.</param>
    /// <param name="subclass">The subclass, or null.</param>
    /// <param name="progIf">The programming interface, or null.</param>
    /// <param name="revision">The revision id, or null.</param>
    /// <exception cref="ArgumentException">Every field is null.</exception>
    public PciMatch(ushort? vendorId = null, ushort? deviceId = null, ushort? subsystemVendorId = null, ushort? subsystemId = null,
        byte? classCode = null, byte? subclass = null, byte? progIf = null, byte? revision = null)
    {
        _vendorId = vendorId;
        _deviceId = deviceId;
        _subsystemVendorId = subsystemVendorId;
        _subsystemId = subsystemId;
        _classCode = classCode;
        _subclass = subclass;
        _progIf = progIf;
        _revision = revision;

        int specificity = 0;
        specificity += vendorId is null ? 0 : 1;
        specificity += deviceId is null ? 0 : 1;
        specificity += subsystemVendorId is null ? 0 : 1;
        specificity += subsystemId is null ? 0 : 1;
        specificity += classCode is null ? 0 : 1;
        specificity += subclass is null ? 0 : 1;
        specificity += progIf is null ? 0 : 1;
        specificity += revision is null ? 0 : 1;
        if (specificity == 0)
        {
            throw new ArgumentException("a PCI match constrains at least one field");
        }

        Specificity = specificity;
    }

    /// <inheritdoc/>
    public override int Specificity { get; }

    /// <inheritdoc/>
    public override bool Matches(DeviceIdentity identity) =>
        identity is PciIdentity pci
        && (_vendorId is null || _vendorId.Value == pci.VendorId)
        && (_deviceId is null || _deviceId.Value == pci.DeviceId)
        && (_subsystemVendorId is null || _subsystemVendorId.Value == pci.SubsystemVendorId)
        && (_subsystemId is null || _subsystemId.Value == pci.SubsystemId)
        && (_classCode is null || _classCode.Value == pci.ClassCode)
        && (_subclass is null || _subclass.Value == pci.Subclass)
        && (_progIf is null || _progIf.Value == pci.ProgIf)
        && (_revision is null || _revision.Value == pci.Revision);
}
