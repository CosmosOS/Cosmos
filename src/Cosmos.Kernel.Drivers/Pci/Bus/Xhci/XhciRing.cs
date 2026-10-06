// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Xhci;

/// <summary>
/// A producer ring: the command ring or one endpoint's transfer ring (xHCI
/// 1.2 section 4.9). One DMA page of TRBs whose last entry is a Link TRB
/// back to the start; the producer cycle state flips on every wrap. The
/// caller serializes access (the controller's lock, or the handler, which
/// runs exclusively) and rings the doorbell. The page is reached through
/// <c>MemoryMarshal.Cast</c>, never a pointer.
/// </summary>
internal sealed class XhciRing
{
    /// <summary>TRBs in one page, the Link TRB included.</summary>
    public const int TrbCount = 256;

    /// <summary>Bytes of one page.</summary>
    private const int PageBytes = 4096;

    private readonly DmaBuffer _buffer;
    private int _enqueueIndex;

    /// <summary>Wraps a page the host allocated and writes its Link TRB with cycle 0. Thread context.</summary>
    /// <param name="buffer">One page of DMA memory, page aligned.</param>
    public XhciRing(DmaBuffer buffer)
    {
        _buffer = buffer;
        PhysicalAddress = buffer.PhysicalAddress;
        CycleState = true;
        WriteLink(XhciTrb.TypeField(XhciTrbType.Link) | XhciTrb.ToggleCycle, publish: false);
    }

    /// <summary>Physical address of the first TRB.</summary>
    public ulong PhysicalAddress { get; }

    /// <summary>The producer cycle state, which a Set TR Dequeue Pointer command passes as DCS.</summary>
    public bool CycleState { get; private set; }

    /// <summary>Physical address of the next TRB <see cref="Enqueue"/> will write.</summary>
    public ulong EnqueuePointer => PhysicalAddress + ((ulong)_enqueueIndex * XhciTrb.Size);

    /// <summary>
    /// Writes one TRB and hands it to the controller: Parameter and Status
    /// first, a write barrier, then the control dword with the ring's cycle
    /// bit in place of the one in <paramref name="control"/>. At the last
    /// slot the Link TRB is published with the same cycle, carrying Chain so
    /// a TD the wrap splits stays one TD (xHCI 1.2 section 4.11.5.1), and
    /// the cycle state flips. Under the controller's lock or in its handler.
    /// </summary>
    /// <param name="parameter">Dwords 0 and 1.</param>
    /// <param name="status">Dword 2.</param>
    /// <param name="control">Dword 3, less the cycle bit.</param>
    /// <returns>The TRB's physical address, which its completion event points back to.</returns>
    public ulong Enqueue(ulong parameter, uint status, uint control)
    {
        Span<XhciTrb> trbs = MemoryMarshal.Cast<byte, XhciTrb>(_buffer.Span);
        ulong address = EnqueuePointer;
        trbs[_enqueueIndex].Parameter = parameter;
        trbs[_enqueueIndex].Status = status;
        Publish(ref trbs[_enqueueIndex], control);

        _enqueueIndex++;
        if (_enqueueIndex == TrbCount - 1)
        {
            WriteLink(XhciTrb.TypeField(XhciTrbType.Link) | XhciTrb.ToggleCycle | (control & XhciTrb.Chain), publish: true);
            _enqueueIndex = 0;
            CycleState = !CycleState;
        }

        return address;
    }

    /// <summary>Reads back the TRB at <paramref name="address"/> when it lies in this ring. Any context, allocation-free.</summary>
    /// <param name="address">A TRB's physical address, from a Transfer Event.</param>
    /// <param name="trb">The TRB as written.</param>
    public bool TryRead(ulong address, out XhciTrb trb)
    {
        ulong offset = address - PhysicalAddress;
        if (address < PhysicalAddress || offset >= (ulong)PageBytes || offset % XhciTrb.Size != 0)
        {
            trb = default;
            return false;
        }

        Span<XhciTrb> trbs = MemoryMarshal.Cast<byte, XhciTrb>(_buffer.Span);
        trb = trbs[(int)(offset / XhciTrb.Size)];
        return true;
    }

    /// <summary>
    /// Zeroes the TRBs, rewrites the Link TRB with cycle 0, and starts
    /// again at index 0 with cycle state 1: for a pooled ring reused by
    /// another endpoint. Thread context, before the endpoint is configured.
    /// </summary>
    public void Reset()
    {
        _buffer.Span.Clear();
        _enqueueIndex = 0;
        CycleState = true;
        WriteLink(XhciTrb.TypeField(XhciTrbType.Link) | XhciTrb.ToggleCycle, publish: false);
    }

    /// <summary>Writes the Link TRB at the last index, with the ring's cycle when <paramref name="publish"/>, cycle 0 otherwise.</summary>
    private void WriteLink(uint control, bool publish)
    {
        Span<XhciTrb> trbs = MemoryMarshal.Cast<byte, XhciTrb>(_buffer.Span);
        ref XhciTrb link = ref trbs[TrbCount - 1];
        link.Parameter = PhysicalAddress;
        link.Status = 0;
        if (publish)
        {
            Publish(ref link, control);
        }
        else
        {
            DmaBuffer.WriteBarrier();
            Volatile.Write(ref link.Control, control & ~XhciTrb.Cycle);
        }
    }

    /// <summary>The cycle bit is what hands the TRB over, so the rest of the TRB is made visible before the control dword lands.</summary>
    private void Publish(ref XhciTrb trb, uint control)
    {
        DmaBuffer.WriteBarrier();
        Volatile.Write(ref trb.Control, (control & ~XhciTrb.Cycle) | (CycleState ? XhciTrb.Cycle : 0));
    }
}
