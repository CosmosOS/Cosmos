// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

/// <summary>
/// The event ring of interrupter 0 (xHCI 1.2 §4.9.4): one segment the
/// controller produces into and software consumes, described to the
/// controller by a one-entry Event Ring Segment Table.
/// </summary>
internal sealed unsafe class XhciEventRing
{
    public const int TrbCount = XhciDma.PageSize / XhciTrb.Size;
    public const uint SegmentCount = 1;

    private readonly XhciTrb* _trbs;
    private readonly ulong _segmentAddress;
    private int _dequeueIndex;
    private bool _cycleState = true;

    public ulong SegmentTableAddress { get; }

    /// <summary>Physical address of the next TRB to consume, the value ERDP is written with.</summary>
    public ulong DequeuePointer => _segmentAddress + ((ulong)_dequeueIndex * XhciTrb.Size);

    public XhciEventRing()
    {
        _trbs = (XhciTrb*)XhciDma.AllocPages(1, out _segmentAddress);

        // One ERST entry: segment base address, then its size in TRBs (xHCI 1.2 §6.5).
        ulong* table = (ulong*)XhciDma.AllocPages(1, out ulong tableAddress);
        table[0] = _segmentAddress;
        table[1] = TrbCount;
        SegmentTableAddress = tableAddress;
    }

    public bool TryDequeue(out XhciTrb trb)
    {
        XhciTrb* current = &_trbs[_dequeueIndex];
        uint control = Volatile.Read(ref current->Control);
        if (((control & XhciTrb.Cycle) != 0) != _cycleState)
        {
            trb = default;
            return false;
        }

        // The controller writes the cycle bit last: nothing else of the TRB
        // may be read ahead of it on a weakly ordered CPU.
        XhciDma.Barrier();
        trb = *current;

        _dequeueIndex++;
        if (_dequeueIndex == TrbCount)
        {
            _dequeueIndex = 0;
            _cycleState = !_cycleState;
        }

        return true;
    }
}
