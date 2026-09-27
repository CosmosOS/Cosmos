// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.UsbMassStorage.Scsi;

/// <summary>
/// Standard INQUIRY data (SPC-4 s6.4.2): what kind of device a logical
/// unit is, and the vendor and product it names itself by.
/// </summary>
internal readonly ref struct InquiryData
{
    /// <summary>Bytes INQUIRY asks for: the standard data through the product revision level.</summary>
    internal const int Size = 36;

    /// <summary>Fewest bytes a unit may answer with: through the additional length byte.</summary>
    internal const int MinimumLength = 5;

    /// <summary>Bit position of the peripheral qualifier in byte 0 (bits 7:5).</summary>
    private const int PeripheralQualifierShift = 5;

    /// <summary>The peripheral device type in byte 0 (bits 4:0).</summary>
    private const byte PeripheralDeviceTypeMask = 0x1F;

    /// <summary>Peripheral qualifier of a unit that is connected: 0.</summary>
    private const int ConnectedQualifier = 0;

    /// <summary>Peripheral device type of a direct access block device, a disk (SBC-3).</summary>
    private const byte DirectAccessBlockDevice = 0x00;

    /// <summary>Peripheral device type of a simplified direct access device (RBC), which some card readers report.</summary>
    private const byte SimplifiedDirectAccessDevice = 0x0E;

    private const int VendorOffset = 8;
    private const int VendorLength = 8;
    private const int ProductOffset = 16;
    private const int ProductLength = 16;

    private readonly ReadOnlySpan<byte> _data;

    /// <summary>Byte 0, the peripheral qualifier and device type, for the log.</summary>
    internal byte Peripheral => _data[0];

    /// <summary>True for a connected unit this driver can drive: a direct access block device, simplified or not.</summary>
    internal bool IsDisk =>
        Peripheral >> PeripheralQualifierShift == ConnectedQualifier
        && (Peripheral & PeripheralDeviceTypeMask) is DirectAccessBlockDevice or SimplifiedDirectAccessDevice;

    /// <summary>The T10 vendor identification, without its padding. Thread context only: it builds a string.</summary>
    internal string Vendor => Text(_data.Slice(VendorOffset, VendorLength));

    /// <summary>The product identification, without its padding. Thread context only: it builds a string.</summary>
    internal string Product => Text(_data.Slice(ProductOffset, ProductLength));

    /// <summary>Views the first <see cref="Size"/> bytes of <paramref name="data"/>, the rest of which a short answer left zero.</summary>
    internal InquiryData(ReadOnlySpan<byte> data)
    {
        _data = data[..Size];
    }

    /// <summary>An INQUIRY text field: ASCII, padded with spaces, which are dropped; any other byte reads as '?'.</summary>
    private static string Text(ReadOnlySpan<byte> field)
    {
        int length = field.Length;
        while (length > 0 && field[length - 1] is (byte)' ' or 0)
        {
            length--;
        }

        Span<char> text = stackalloc char[ProductLength];
        for (int i = 0; i < length; i++)
        {
            byte b = field[i];
            text[i] = b is >= 0x20 and < 0x7F ? (char)b : '?';
        }

        return new string(text[..length]);
    }
}
