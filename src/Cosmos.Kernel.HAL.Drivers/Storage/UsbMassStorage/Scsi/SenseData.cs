// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.UsbMassStorage.Scsi;

/// <summary>
/// Fixed-format sense data (SPC-4 s4.5.3), as REQUEST SENSE returns it:
/// why the command before it failed. All zero, which reads as no sense
/// key, when the transport could not fetch it.
/// </summary>
internal readonly ref struct SenseData
{
    /// <summary>Bytes of sense data REQUEST SENSE asks for: the fixed format through the sense key specific field.</summary>
    internal const int Size = 18;

    /// <summary>Byte offset of the sense key, in the low four bits.</summary>
    private const int SenseKeyOffset = 2;

    private const byte SenseKeyMask = 0x0F;

    /// <summary>Byte offset of the additional sense code (ASC).</summary>
    private const int AdditionalSenseCodeOffset = 12;

    /// <summary>Byte offset of the additional sense code qualifier (ASCQ).</summary>
    private const int AdditionalSenseCodeQualifierOffset = 13;

    /// <summary>ASC MEDIUM NOT PRESENT, under a <see cref="SenseKey.NotReady"/> key.</summary>
    private const byte MediumNotPresent = 0x3A;

    private readonly ReadOnlySpan<byte> _data;

    /// <summary>The general reason the command failed.</summary>
    internal SenseKey Key => (SenseKey)(_data[SenseKeyOffset] & SenseKeyMask);

    /// <summary>The additional sense code, which details the key.</summary>
    internal byte AdditionalSenseCode => _data[AdditionalSenseCodeOffset];

    /// <summary>The additional sense code qualifier, which details the code.</summary>
    internal byte AdditionalSenseCodeQualifier => _data[AdditionalSenseCodeQualifierOffset];

    /// <summary>True when the unit has no medium, such as a card reader slot with no card in it.</summary>
    internal bool IsMediumNotPresent => Key == SenseKey.NotReady && AdditionalSenseCode == MediumNotPresent;

    /// <summary>Views the first <see cref="Size"/> bytes of <paramref name="data"/>.</summary>
    internal SenseData(ReadOnlySpan<byte> data)
    {
        _data = data[..Size];
    }

    /// <summary>The key, code and qualifier for the log. Thread context only: it builds a string.</summary>
    internal string Describe() =>
        $"sense key 0x{(byte)Key:X}, ASC 0x{AdditionalSenseCode:X2}, ASCQ 0x{AdditionalSenseCodeQualifier:X2}";
}
