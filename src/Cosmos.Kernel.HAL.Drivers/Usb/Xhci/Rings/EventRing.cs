// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Rings;

/// <summary>
/// The event ring of interrupter 0 (xHCI 1.2 §4.9.4): one segment the
/// controller produces into and the driver consumes, described to the
/// controller by a one-entry Event Ring Segment Table. Callers serialize
/// access (the controller's event lock).
/// </summary>
internal sealed class EventRing
{
    internal const int TrbCount = XhciMemory.PageSize / Trb.Size;
    internal const uint SegmentCount = 1;

    /// <summary>Byte offset of an ERST entry's Ring Segment Size, after its 64-bit base address (xHCI 1.2 §6.5).</summary>
    private const int SegmentSizeOffset = 8;

    private readonly DmaBuffer _segment;
    private readonly DmaBuffer _segmentTable;
    private int _dequeueIndex;
    private bool _cycleState = true;

    /// <summary>Device address of the Event Ring Segment Table, the value ERSTBA is written with.</summary>
    internal ulong SegmentTableAddress => _segmentTable.DeviceAddress;

    /// <summary>Device address of the next TRB to consume, the value ERDP is written with.</summary>
    internal ulong DequeuePointer => _segment.DeviceAddress + ((ulong)_dequeueIndex * Trb.Size);

    /// <summary>Allocates the segment and its table. Probe only.</summary>
    /// <exception cref="InvalidOperationException">No DMA memory the controller can reach is free.</exception>
    internal EventRing(XhciMemory memory)
    {
        _segment = memory.Allocate(1);
        _segmentTable = memory.Allocate(1);

        // One ERST entry: the segment's base address, then its size in TRBs.
        Span<byte> entry = _segmentTable.Span;
        BinaryPrimitives.WriteUInt64LittleEndian(entry, _segment.DeviceAddress);
        BinaryPrimitives.WriteUInt32LittleEndian(entry[SegmentSizeOffset..], (uint)TrbCount);
    }

    /// <summary>Consumes the next event when the controller has written one.</summary>
    internal bool TryDequeue(out Trb trb)
    {
        ReadOnlySpan<byte> current = _segment.Span.Slice(_dequeueIndex * Trb.Size, Trb.Size);
        if (((Trb.ReadControl(current) & Trb.Cycle) != 0) != _cycleState)
        {
            trb = default;
            return false;
        }

        // The controller writes the cycle bit last: nothing else of the TRB
        // may be read ahead of it on a weakly ordered CPU.
        DmaBuffer.ReadBarrier();
        trb = Trb.Read(current);

        _dequeueIndex++;
        if (_dequeueIndex == TrbCount)
        {
            _dequeueIndex = 0;
            _cycleState = !_cycleState;
        }

        return true;
    }
}
