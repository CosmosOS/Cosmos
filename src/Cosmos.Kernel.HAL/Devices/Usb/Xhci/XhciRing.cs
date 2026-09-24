// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

/// <summary>
/// A producer ring: the command ring or one endpoint's transfer ring
/// (xHCI 1.2 §4.9). One page of TRBs whose last entry is a Link TRB back to
/// the start; the producer cycle state flips on every wrap. Callers
/// serialize access (the controller's ring lock) and ring the doorbell.
/// </summary>
internal sealed unsafe class XhciRing
{
    public const int TrbCount = XhciDma.PageSize / XhciTrb.Size;

    private readonly XhciTrb* _trbs;
    private int _enqueueIndex;

    public ulong PhysicalAddress { get; }

    /// <summary>The producer cycle state, which a Set TR Dequeue Pointer command passes as DCS.</summary>
    public bool CycleState { get; private set; } = true;

    /// <summary>Physical address of the next TRB <see cref="Enqueue"/> will write.</summary>
    public ulong EnqueuePointer => PhysicalAddress + ((ulong)_enqueueIndex * XhciTrb.Size);

    public XhciRing()
    {
        _trbs = (XhciTrb*)XhciDma.AllocPages(1, out ulong physicalAddress);
        PhysicalAddress = physicalAddress;

        // The link keeps cycle 0 until the first wrap publishes it.
        XhciTrb* link = &_trbs[TrbCount - 1];
        link->Parameter = physicalAddress;
        link->Control = XhciTrb.TypeField(XhciTrbType.Link) | XhciTrb.ToggleCycle;
    }

    /// <summary>
    /// Writes one TRB and hands it to the controller. The cycle bit in
    /// <paramref name="control"/> is replaced by the ring's.
    /// </summary>
    /// <returns>The TRB's physical address, which its completion event points back to.</returns>
    public ulong Enqueue(ulong parameter, uint status, uint control)
    {
        XhciTrb* trb = &_trbs[_enqueueIndex];
        ulong address = EnqueuePointer;
        trb->Parameter = parameter;
        trb->Status = status;
        Publish(trb, control);

        _enqueueIndex++;
        if (_enqueueIndex == TrbCount - 1)
        {
            // Chain carries across the link so a TD the wrap splits stays one TD (xHCI 1.2 §4.11.5.1).
            Publish(&_trbs[_enqueueIndex], XhciTrb.TypeField(XhciTrbType.Link) | XhciTrb.ToggleCycle | (control & XhciTrb.Chain));
            _enqueueIndex = 0;
            CycleState = !CycleState;
        }

        return address;
    }

    /// <summary>Reads back the TRB at <paramref name="address"/> when it lies in this ring.</summary>
    public bool TryRead(ulong address, out XhciTrb trb)
    {
        ulong offset = address - PhysicalAddress;
        if (address < PhysicalAddress || offset >= (ulong)XhciDma.PageSize || offset % XhciTrb.Size != 0)
        {
            trb = default;
            return false;
        }

        trb = _trbs[offset / XhciTrb.Size];
        return true;
    }

    public void Free() => XhciDma.Free(_trbs);

    private void Publish(XhciTrb* trb, uint control)
    {
        // The cycle bit is what hands the TRB over, so the rest of the TRB
        // has to be visible before the control dword lands.
        XhciDma.Barrier();
        Volatile.Write(ref trb->Control, (control & ~XhciTrb.Cycle) | (CycleState ? XhciTrb.Cycle : 0));
    }
}
