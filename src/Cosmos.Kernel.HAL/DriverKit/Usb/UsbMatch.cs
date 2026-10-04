// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// A match over USB interface identities: every field given must equal the
/// identity's, every field left null is not looked at, and the specificity
/// is the number of fields given. The port path and the speed are identity,
/// not match fields. A match that constrains nothing is refused. So a
/// vendor and product match (2) outranks a class-only match (1) and a full
/// interface triplet (3) outranks both.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class UsbMatch : DeviceMatch
{
    private readonly ushort? _vendorId;
    private readonly ushort? _productId;
    private readonly byte? _deviceClass;
    private readonly byte? _deviceSubclass;
    private readonly byte? _deviceProtocol;
    private readonly byte? _interfaceClass;
    private readonly byte? _interfaceSubclass;
    private readonly byte? _interfaceProtocol;

    /// <summary>Creates a match over the fields given; each null field is unconstrained.</summary>
    /// <param name="vendorId">The vendor id, or null.</param>
    /// <param name="productId">The product id, or null.</param>
    /// <param name="deviceClass">The device class, or null.</param>
    /// <param name="deviceSubclass">The device subclass, or null.</param>
    /// <param name="deviceProtocol">The device protocol, or null.</param>
    /// <param name="interfaceClass">The interface class, or null.</param>
    /// <param name="interfaceSubclass">The interface subclass, or null.</param>
    /// <param name="interfaceProtocol">The interface protocol, or null.</param>
    /// <exception cref="ArgumentException">Every field is null.</exception>
    public UsbMatch(ushort? vendorId = null, ushort? productId = null, byte? deviceClass = null, byte? deviceSubclass = null,
        byte? deviceProtocol = null, byte? interfaceClass = null, byte? interfaceSubclass = null, byte? interfaceProtocol = null)
    {
        _vendorId = vendorId;
        _productId = productId;
        _deviceClass = deviceClass;
        _deviceSubclass = deviceSubclass;
        _deviceProtocol = deviceProtocol;
        _interfaceClass = interfaceClass;
        _interfaceSubclass = interfaceSubclass;
        _interfaceProtocol = interfaceProtocol;

        int specificity = 0;
        specificity += vendorId is null ? 0 : 1;
        specificity += productId is null ? 0 : 1;
        specificity += deviceClass is null ? 0 : 1;
        specificity += deviceSubclass is null ? 0 : 1;
        specificity += deviceProtocol is null ? 0 : 1;
        specificity += interfaceClass is null ? 0 : 1;
        specificity += interfaceSubclass is null ? 0 : 1;
        specificity += interfaceProtocol is null ? 0 : 1;
        if (specificity == 0)
        {
            throw new ArgumentException("a USB match constrains at least one field");
        }

        Specificity = specificity;
    }

    /// <inheritdoc/>
    public override int Specificity { get; }

    /// <inheritdoc/>
    public override bool Matches(DeviceIdentity identity) =>
        identity is UsbIdentity usb
        && (_vendorId is null || _vendorId.Value == usb.VendorId)
        && (_productId is null || _productId.Value == usb.ProductId)
        && (_deviceClass is null || _deviceClass.Value == usb.DeviceClass)
        && (_deviceSubclass is null || _deviceSubclass.Value == usb.DeviceSubclass)
        && (_deviceProtocol is null || _deviceProtocol.Value == usb.DeviceProtocol)
        && (_interfaceClass is null || _interfaceClass.Value == usb.InterfaceClass)
        && (_interfaceSubclass is null || _interfaceSubclass.Value == usb.InterfaceSubclass)
        && (_interfaceProtocol is null || _interfaceProtocol.Value == usb.InterfaceProtocol);
}
