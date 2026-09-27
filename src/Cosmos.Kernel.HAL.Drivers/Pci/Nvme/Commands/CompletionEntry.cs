// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Nvme.Commands;

/// <summary>
/// One 16-byte entry of a completion queue, in the queue's DMA buffer,
/// which the controller writes (NVM Express 1.4 s4.6). An entry is new
/// once its phase tag differs from the one the queue's previous pass left.
/// Reading one allocates nothing, so the interrupt handler may.
/// </summary>
internal readonly ref struct CompletionEntry
{
    /// <summary>Size of one entry in bytes: CC.IOCQES = 4, and the fixed admin entry size.</summary>
    internal const int Size = 16;

    /// <summary>Byte offset of the command identifier the entry completes.</summary>
    private const int CommandIdOffset = 0x0C;

    /// <summary>Byte offset of the status field, whose bit 0 is the phase tag.</summary>
    private const int StatusFieldOffset = 0x0E;

    /// <summary>The phase tag bit of the status field.</summary>
    private const ushort PhaseTagMask = 0x1;

    /// <summary>Bit position of the status code in the status field (bit 0 is the phase tag).</summary>
    private const int StatusCodeShift = 1;

    /// <summary>The 15 status bits once the phase tag is shifted out.</summary>
    private const ushort StatusCodeMask = 0x7FFF;

    private readonly ReadOnlySpan<byte> _entry;

    /// <summary>The phase tag the controller wrote with the entry.</summary>
    internal bool Phase => (ReadStatusField() & PhaseTagMask) != 0;

    /// <summary>
    /// The status code and type: 0 is success. Read only once
    /// <see cref="Phase"/> said the entry is new, behind a read barrier.
    /// </summary>
    internal uint StatusCode => (uint)((ReadStatusField() >> StatusCodeShift) & StatusCodeMask);

    /// <summary>The identifier of the command the entry completes. Read as <see cref="StatusCode"/> is.</summary>
    internal ushort CommandId => BinaryPrimitives.ReadUInt16LittleEndian(_entry[CommandIdOffset..]);

    /// <summary>Views entry <paramref name="index"/> of <paramref name="queue"/>.</summary>
    /// <param name="queue">The completion queue's memory.</param>
    /// <param name="index">The entry, the queue's head.</param>
    internal CompletionEntry(ReadOnlySpan<byte> queue, uint index)
    {
        _entry = queue.Slice((int)index * Size, Size);
    }

    /// <summary>
    /// Reads the status field. Volatile, because a poll reads it again and
    /// again with nothing in between that the compiler would treat as a
    /// write to it: a plain load could be hoisted out of the loop, and the
    /// poll would never see the controller's write. Both architectures
    /// are little-endian, as the field is.
    /// </summary>
    private ushort ReadStatusField()
    {
        ref byte field = ref MemoryMarshal.GetReference(_entry[StatusFieldOffset..]);
        return Volatile.Read(ref Unsafe.As<byte, ushort>(ref field));
    }
}
