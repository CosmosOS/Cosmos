// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Rings;

/// <summary>
/// A ring the driver produces into: the command ring or one endpoint's
/// transfer ring (xHCI 1.2 §4.9). One page of TRBs whose last entry is a
/// Link TRB back to the start; the producer cycle state flips on every
/// wrap. Callers serialize access (the controller's ring lock) and ring the
/// doorbell.
/// </summary>
internal sealed class ProducerRing
{
    internal const int TrbCount = XhciMemory.PageSize / Trb.Size;

    private readonly DmaBuffer _page;
    private int _enqueueIndex;

    /// <summary>The ring's first TRB as the controller addresses it.</summary>
    internal ulong DeviceAddress => _page.DeviceAddress;

    /// <summary>The producer cycle state, which a Set TR Dequeue Pointer command passes as DCS.</summary>
    internal bool CycleState { get; private set; } = true;

    /// <summary>Device address of the next TRB <see cref="Enqueue"/> will write.</summary>
    internal ulong EnqueuePointer => DeviceAddress + ((ulong)_enqueueIndex * Trb.Size);

    /// <summary>Allocates the ring's page and links its last TRB back to the first.</summary>
    /// <exception cref="InvalidOperationException">No DMA memory the controller can reach is free.</exception>
    internal ProducerRing(XhciMemory memory)
    {
        _page = memory.Allocate(1);

        // The link keeps cycle 0 until the first wrap publishes it.
        Span<byte> link = TrbAt(TrbCount - 1);
        Trb.WriteBody(link, DeviceAddress, 0);
        Trb.WriteControl(link, Trb.TypeField(TrbType.Link) | Trb.ToggleCycle);
    }

    /// <summary>
    /// Writes one TRB and hands it to the controller. The cycle bit in
    /// <paramref name="control"/> is replaced by the ring's.
    /// </summary>
    /// <returns>The TRB's device address, which its completion event points back to.</returns>
    internal ulong Enqueue(ulong parameter, uint status, uint control)
    {
        Span<byte> trb = TrbAt(_enqueueIndex);
        ulong address = EnqueuePointer;
        Trb.WriteBody(trb, parameter, status);
        Publish(trb, control);

        _enqueueIndex++;
        if (_enqueueIndex == TrbCount - 1)
        {
            // Chain carries across the link so a TD the wrap splits stays one TD (xHCI 1.2 §4.11.5.1).
            Publish(TrbAt(_enqueueIndex), Trb.TypeField(TrbType.Link) | Trb.ToggleCycle | (control & Trb.Chain));
            _enqueueIndex = 0;
            CycleState = !CycleState;
        }

        return address;
    }

    /// <summary>Reads back the TRB at device address <paramref name="address"/> when it lies in this ring.</summary>
    internal bool TryRead(ulong address, out Trb trb)
    {
        ulong offset = address - DeviceAddress;
        if (address < DeviceAddress || offset >= XhciMemory.PageSize || offset % Trb.Size != 0)
        {
            trb = default;
            return false;
        }

        trb = Trb.Read(TrbAt((int)(offset / Trb.Size)));
        return true;
    }

    /// <summary>Gives the ring's page back, once the controller no longer fetches from it.</summary>
    internal void Free(XhciMemory memory) => memory.Free(_page);

    private Span<byte> TrbAt(int index) => _page.Span.Slice(index * Trb.Size, Trb.Size);

    private void Publish(Span<byte> trb, uint control)
    {
        // The cycle bit is what hands the TRB over, so the rest of the TRB
        // has to be visible before the control dword lands.
        DmaBuffer.WriteBarrier();
        Trb.WriteControl(trb, (control & ~Trb.Cycle) | (CycleState ? Trb.Cycle : 0));
    }
}
