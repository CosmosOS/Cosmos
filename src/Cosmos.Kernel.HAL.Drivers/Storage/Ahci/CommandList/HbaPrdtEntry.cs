// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Ahci.CommandList;

/// <summary>
/// One PRDT entry of a command table: a buffer the HBA moves the command's
/// data through (AHCI 1.3.1 s4.2.3.3). Creating one clears it.
/// </summary>
internal sealed class HbaPrdtEntry
{
    /// <summary>Size of one PRDT entry in bytes; entry N starts at PRDT base + N * 0x10 (AHCI spec 4.2.3.3).</summary>
    private const int PrdtEntrySizeBytes = 0x10;

    /// <summary>DBA - Data Base Address field offset (AHCI spec 4.2.3.3).</summary>
    private const int DbaOffset = 0x00;

    /// <summary>DBAU - Data Base Address Upper 32-bits field offset (AHCI spec 4.2.3.3).</summary>
    private const int DbauOffset = 0x04;

    /// <summary>DBC - Data Byte Count field offset within DW3 (AHCI spec 4.2.3.3).</summary>
    private const int DbcOffset = 0x0C;

    /// <summary>Offset of the byte holding the Interrupt on Completion bit (top byte of DW3, AHCI spec 4.2.3.3).</summary>
    private const int InterruptOnCompletionByteOffset = 0x0F;

    /// <summary>DBC - Data Byte Count mask, bits 21:0 of DW3 (AHCI spec 4.2.3.3).</summary>
    private const uint DbcMask = 0x3FFFFF;

    /// <summary>I - Interrupt on Completion bit position within its byte (bit 31 of DW3, AHCI spec 4.2.3.3).</summary>
    private const int InterruptOnCompletionBitShift = 7;

    private readonly DmaBuffer _memory;
    private readonly int _offset;

    /// <summary>DBA - low dword of the data buffer's address.</summary>
    internal uint DBA
    {
        get => BinaryPrimitives.ReadUInt32LittleEndian(Bytes.Slice(DbaOffset));
        set => BinaryPrimitives.WriteUInt32LittleEndian(Bytes.Slice(DbaOffset), value);
    }

    /// <summary>DBAU - high dword of the data buffer's address.</summary>
    internal uint DBAU
    {
        get => BinaryPrimitives.ReadUInt32LittleEndian(Bytes.Slice(DbauOffset));
        set => BinaryPrimitives.WriteUInt32LittleEndian(Bytes.Slice(DbauOffset), value);
    }

    /// <summary>DBC - bytes to move, minus one. Setting it rewrites all of DW3, clearing I.</summary>
    internal uint DBC
    {
        get => BinaryPrimitives.ReadUInt32LittleEndian(Bytes.Slice(DbcOffset)) & DbcMask;
        set => BinaryPrimitives.WriteUInt32LittleEndian(Bytes.Slice(DbcOffset), value);
    }

    /// <summary>I - Interrupt on Completion. Setting it rewrites the top byte of DW3.</summary>
    internal byte InterruptOnCompletion
    {
        get => (byte)(Bytes[InterruptOnCompletionByteOffset] >> InterruptOnCompletionBitShift);
        set => Bytes[InterruptOnCompletionByteOffset] = (byte)(value << InterruptOnCompletionBitShift);
    }

    private Span<byte> Bytes => _memory.Span.Slice(_offset, PrdtEntrySizeBytes);

    /// <summary>Clears entry <paramref name="entry"/> of the PRDT at <paramref name="prdtOffset"/> in <paramref name="memory"/>, and views it.</summary>
    /// <param name="memory">The controller's command region.</param>
    /// <param name="prdtOffset">Where the PRDT starts in it.</param>
    /// <param name="entry">The entry index.</param>
    internal HbaPrdtEntry(DmaBuffer memory, int prdtOffset, uint entry)
    {
        _memory = memory;
        _offset = prdtOffset + PrdtEntrySizeBytes * (int)entry;
        Bytes.Clear();
    }
}
