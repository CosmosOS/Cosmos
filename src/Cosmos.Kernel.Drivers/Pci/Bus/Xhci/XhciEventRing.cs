// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Xhci;

/// <summary>
/// The event ring of interrupter 0 (xHCI 1.2 section 4.9.4): one segment
/// page the controller produces into and software consumes, described to
/// the controller by a one-entry Event Ring Segment Table in a second page.
/// Consumed under the controller's lock or in its handler.
/// </summary>
internal sealed class XhciEventRing
{
    /// <summary>TRBs in the segment.</summary>
    public const int TrbCount = 256;

    /// <summary>Entries in the segment table.</summary>
    public const uint SegmentCount = 1;

    private readonly DmaBuffer _segment;
    private int _dequeueIndex;
    private bool _cycleState = true;

    /// <summary>Wraps the two pages the host allocated and fills the table: the segment's address, then its size in TRBs (xHCI 1.2 section 6.5). Thread context.</summary>
    /// <param name="segment">One page of DMA memory for the TRBs.</param>
    /// <param name="table">One page of DMA memory for the Event Ring Segment Table.</param>
    public XhciEventRing(DmaBuffer segment, DmaBuffer table)
    {
        _segment = segment;
        Span<ulong> entries = MemoryMarshal.Cast<byte, ulong>(table.Span);
        entries[0] = segment.PhysicalAddress;
        entries[1] = TrbCount;
        DmaBuffer.WriteBarrier();
        SegmentTableAddress = table.PhysicalAddress;
    }

    /// <summary>Physical address of the Event Ring Segment Table, the value ERSTBA takes.</summary>
    public ulong SegmentTableAddress { get; }

    /// <summary>Physical address of the next TRB to consume, the value ERDP is written with.</summary>
    public ulong DequeuePointer => _segment.PhysicalAddress + ((ulong)_dequeueIndex * XhciTrb.Size);

    /// <summary>
    /// Takes the next event when the controller has written it: the control
    /// dword is read first, and its cycle bit has to match the consumer
    /// cycle state; a read barrier then orders the rest of the TRB behind
    /// it, since the controller writes the cycle bit last. Any context,
    /// allocation-free.
    /// </summary>
    /// <param name="trb">The event, when one was there.</param>
    public bool TryDequeue(out XhciTrb trb)
    {
        Span<XhciTrb> trbs = MemoryMarshal.Cast<byte, XhciTrb>(_segment.Span);
        ref XhciTrb current = ref trbs[_dequeueIndex];
        uint control = Volatile.Read(ref current.Control);
        if (((control & XhciTrb.Cycle) != 0) != _cycleState)
        {
            trb = default;
            return false;
        }

        DmaBuffer.ReadBarrier();
        trb = current;

        _dequeueIndex++;
        if (_dequeueIndex == TrbCount)
        {
            _dequeueIndex = 0;
            _cycleState = !_cycleState;
        }

        return true;
    }
}
