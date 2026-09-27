// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Ahci.CommandList;

/// <summary>
/// One command header of a port's command list, in the command region the
/// HBA reads by DMA (AHCI 1.3.1 s4.2.2). Creating one clears the header.
/// </summary>
internal sealed class HbaCommandHeader
{
    /// <summary>Size of one command header in bytes; slot N starts at CLB + N * 32 (AHCI spec 4.2.2).</summary>
    private const int CommandHeaderSizeBytes = 32;

    /// <summary>CFL - Command FIS Length mask, bits 4:0 of the header DW0 (AHCI spec 4.2.2).</summary>
    private const int CommandFisLengthMask = 0x1F;

    /// <summary>W - Write bit position in the header DW0 (AHCI spec 4.2.2).</summary>
    private const int WriteBitShift = 6;

    /// <summary>PRDTL - Physical Region Descriptor Table Length field offset (AHCI spec 4.2.2).</summary>
    private const int PrdtlOffset = 0x02;

    /// <summary>CTBA - Command Table Descriptor Base Address field offset (AHCI spec 4.2.2).</summary>
    private const int CtbaOffset = 0x08;

    /// <summary>CTBAU - Command Table Descriptor Base Address Upper 32-bits field offset (AHCI spec 4.2.2).</summary>
    private const int CtbauOffset = 0x0C;

    private readonly DmaBuffer _memory;
    private readonly int _offset;

    /// <summary>CFL - Command FIS Length in dwords. Setting it rewrites the whole first byte, clearing A and W.</summary>
    internal byte CFL
    {
        get => (byte)(Bytes[0] & CommandFisLengthMask);
        set => Bytes[0] = value;
    }

    /// <summary>W - Write: the command moves data from memory to the device. Setting it ORs the bit in.</summary>
    internal byte Write
    {
        get => (byte)((Bytes[0] >> WriteBitShift) & 1);
        set
        {
            Span<byte> bytes = Bytes;
            bytes[0] = (byte)(bytes[0] | (value << WriteBitShift));
        }
    }

    /// <summary>PRDTL - entries in the command table's PRDT.</summary>
    internal ushort PRDTL
    {
        get => BinaryPrimitives.ReadUInt16LittleEndian(Bytes.Slice(PrdtlOffset));
        set => BinaryPrimitives.WriteUInt16LittleEndian(Bytes.Slice(PrdtlOffset), value);
    }

    /// <summary>CTBA - low dword of the command table's address.</summary>
    internal uint CTBA
    {
        get => BinaryPrimitives.ReadUInt32LittleEndian(Bytes.Slice(CtbaOffset));
        set => BinaryPrimitives.WriteUInt32LittleEndian(Bytes.Slice(CtbaOffset), value);
    }

    /// <summary>CTBAU - high dword of the command table's address.</summary>
    internal uint CTBAU
    {
        get => BinaryPrimitives.ReadUInt32LittleEndian(Bytes.Slice(CtbauOffset));
        set => BinaryPrimitives.WriteUInt32LittleEndian(Bytes.Slice(CtbauOffset), value);
    }

    private Span<byte> Bytes => _memory.Span.Slice(_offset, CommandHeaderSizeBytes);

    /// <summary>
    /// Clears header <paramref name="slot"/> of the command list at
    /// <paramref name="commandListOffset"/> in <paramref name="memory"/>, and
    /// views it.
    /// </summary>
    /// <param name="memory">The controller's command region.</param>
    /// <param name="commandListOffset">Where the port's command list starts in it.</param>
    /// <param name="slot">The command slot, 0 to 31.</param>
    internal HbaCommandHeader(DmaBuffer memory, int commandListOffset, uint slot)
    {
        _memory = memory;
        _offset = commandListOffset + CommandHeaderSizeBytes * (int)slot;
        Bytes.Clear();
    }
}
