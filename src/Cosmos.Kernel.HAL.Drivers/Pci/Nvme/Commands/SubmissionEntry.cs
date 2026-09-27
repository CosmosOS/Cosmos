// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Nvme.Commands;

/// <summary>
/// One 64-byte entry of a submission queue, in the queue's DMA buffer: the
/// command the controller fetches once the tail doorbell moves past it
/// (NVM Express 1.4 s4.2). Creating one clears the entry, so every field
/// this driver does not set, PRP2 and the fused, PSDT and metadata fields
/// among them, is zero.
/// </summary>
internal readonly ref struct SubmissionEntry
{
    /// <summary>Size of one entry in bytes: CC.IOSQES = 6, and the fixed admin entry size.</summary>
    internal const int Size = 64;

    /// <summary>Byte offset of CDW0: opcode, fused operation, PSDT and command identifier.</summary>
    private const int CommandDword0Offset = 0x00;

    /// <summary>Byte offset of the Namespace Identifier.</summary>
    private const int NamespaceIdOffset = 0x04;

    /// <summary>Byte offset of PRP Entry 1, the data pointer's first page.</summary>
    private const int DataPointerOffset = 0x18;

    /// <summary>Byte offset of CDW10.</summary>
    private const int CommandDword10Offset = 0x28;

    /// <summary>Byte offset of CDW11.</summary>
    private const int CommandDword11Offset = 0x2C;

    /// <summary>Bit position of the command identifier in CDW0 (bits 31:16).</summary>
    private const int CommandIdShift = 16;

    private readonly Span<byte> _entry;

    /// <summary>Clears entry <paramref name="index"/> of <paramref name="queue"/> and views it.</summary>
    /// <param name="queue">The submission queue's memory.</param>
    /// <param name="index">The entry, the queue's tail.</param>
    internal SubmissionEntry(Span<byte> queue, uint index)
    {
        _entry = queue.Slice((int)index * Size, Size);
        _entry.Clear();
    }

    /// <summary>Sets an admin command's opcode and the identifier its completion entry echoes.</summary>
    internal void SetCommand(AdminOpcode opcode, ushort commandId) => SetCommandDword0((byte)opcode, commandId);

    /// <summary>Sets an I/O command's opcode and the identifier its completion entry echoes.</summary>
    internal void SetCommand(IoOpcode opcode, ushort commandId) => SetCommandDword0((byte)opcode, commandId);

    /// <summary>Sets the namespace the command applies to; 0 for commands that name none.</summary>
    internal void SetNamespace(uint namespaceId) =>
        BinaryPrimitives.WriteUInt32LittleEndian(_entry[NamespaceIdOffset..], namespaceId);

    /// <summary>
    /// Sets PRP Entry 1, the device address of the command's data page. PRP
    /// Entry 2 stays zero: no command this driver builds moves more than
    /// the one page PRP1 names.
    /// </summary>
    internal void SetDataPointer(ulong deviceAddress) =>
        BinaryPrimitives.WriteUInt64LittleEndian(_entry[DataPointerOffset..], deviceAddress);

    /// <summary>Sets command dword 10, whose meaning depends on the command.</summary>
    internal void SetCommandDword10(uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(_entry[CommandDword10Offset..], value);

    /// <summary>Sets command dword 11, whose meaning depends on the command.</summary>
    internal void SetCommandDword11(uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(_entry[CommandDword11Offset..], value);

    private void SetCommandDword0(byte opcode, ushort commandId) =>
        BinaryPrimitives.WriteUInt32LittleEndian(_entry[CommandDword0Offset..], opcode | ((uint)commandId << CommandIdShift));
}
