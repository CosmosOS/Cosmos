// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// What a device set and a pipe set share: the ED and its software list
/// entry, the TD slots the ED's queue rotates through with the state,
/// generation and queue order of each, and the index of the dummy TD the
/// queue ends at (OHCI 1.0a section 5.2.8.2). Written under the
/// controller's lock or in its handler. A slot a quarantine took away is
/// <see cref="OhciDescriptorSlot.None"/> until the quiesce replaces it; a
/// set left with too few slots to queue a transfer is dropped by the host
/// instead of pooled again.
/// </summary>
internal abstract class OhciDescriptorSet
{
    /// <summary>Creates the shared part over the slots the host took from the pool. Thread context.</summary>
    /// <param name="endpoint">The ED's slot.</param>
    /// <param name="transfers">The TD slots.</param>
    protected OhciDescriptorSet(OhciDescriptorSlot endpoint, OhciDescriptorSlot[] transfers)
    {
        Endpoint = new OhciEndpointSlot(endpoint);
        TdSlots = transfers;
        TdStates = new OhciTdState[transfers.Length];
        TdTags = new int[transfers.Length];
        TdOrder = new int[transfers.Length];
    }

    /// <summary>The ED and its place in a list.</summary>
    public OhciEndpointSlot Endpoint { get; }

    /// <summary>The TD slots; one is the dummy, the others carry transfers or are free.</summary>
    public OhciDescriptorSlot[] TdSlots { get; }

    /// <summary>The state of each TD slot.</summary>
    public OhciTdState[] TdStates { get; }

    /// <summary>The generation each TD slot was last queued with, compared with the pool's by the drain.</summary>
    public int[] TdTags { get; }

    /// <summary>The order each TD slot was last queued in on the ED, from <see cref="NextOrder"/>: the controller retires an ED's TDs in that order. Compared as a wrap-safe difference.</summary>
    public int[] TdOrder { get; }

    /// <summary>The queue order the last TD was given; the next one takes the following value, wrapping.</summary>
    public int NextOrder { get; set; }

    /// <summary>The fewest usable TD slots a transfer can be queued with: one TD and the dummy.</summary>
    protected virtual int MinimumUsableSlots => OhciProtocol.MinimumPipeTransferSlots;

    /// <summary>True while the set has slots enough to queue a transfer; false once quarantines took too many with no replacement to be had. Any context.</summary>
    public bool IsUsable => UsableSlotCount >= MinimumUsableSlots;

    /// <summary>Index of the dummy TD, the one TailP names; -1 when every slot was quarantined.</summary>
    public int DummyIndex { get; set; }

    /// <summary>Index of <paramref name="slot"/> in the set, or -1. Any context.</summary>
    public int IndexOf(OhciDescriptorSlot slot)
    {
        for (int i = 0; i < TdSlots.Length; i++)
        {
            if (TdSlots[i] == slot)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Index of the TD slot at <paramref name="physicalAddress"/>, or -1. Any context.</summary>
    public int IndexOfPhysical(uint physicalAddress)
    {
        for (int i = 0; i < TdSlots.Length; i++)
        {
            if (!TdSlots[i].IsNone && TdSlots[i].PhysicalAddress == physicalAddress)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Lists the free TD slots other than the dummy, in slot order, as
    /// many as <paramref name="indices"/> holds: what a chain may be
    /// built from. Under the lock.
    /// </summary>
    /// <param name="indices">Receives the indices.</param>
    /// <returns>How many were written.</returns>
    public int FreeIndices(Span<int> indices)
    {
        int count = 0;
        for (int i = 0; i < TdSlots.Length && count < indices.Length; i++)
        {
            if (i != DummyIndex && !TdSlots[i].IsNone && TdStates[i] == OhciTdState.Free)
            {
                indices[count++] = i;
            }
        }

        return count;
    }

    /// <summary>True while any TD slot waits for its done entry. Under the lock.</summary>
    public bool AnyInFlight()
    {
        for (int i = 0; i < TdStates.Length; i++)
        {
            if (TdStates[i] == OhciTdState.InFlight)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The in-flight slot queued earliest among those queued before
    /// <paramref name="index"/>, or -1 when none is left: a done entry for
    /// a TD proves the controller retired every TD queued before it on the
    /// same ED, so one still in flight there was retired without a done
    /// entry. The order comparison is a wrap-safe difference. Under the
    /// lock or in the drain.
    /// </summary>
    /// <param name="index">The slot a done entry named.</param>
    public int OldestInFlightBefore(int index)
    {
        int oldest = -1;
        for (int i = 0; i < TdStates.Length; i++)
        {
            if (i == index || TdStates[i] != OhciTdState.InFlight || unchecked(TdOrder[i] - TdOrder[index]) >= 0)
            {
                continue;
            }

            if (oldest < 0 || unchecked(TdOrder[i] - TdOrder[oldest]) < 0)
            {
                oldest = i;
            }
        }

        return oldest;
    }

    /// <summary>How many TD slots the set still has, the dummy included. Any context.</summary>
    public int UsableSlotCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < TdSlots.Length; i++)
            {
                if (!TdSlots[i].IsNone)
                {
                    count++;
                }
            }

            return count;
        }
    }

    /// <summary>
    /// Zeroes the ED (skipped, no queue, no next) and every TD (CC
    /// NotAccessed, no pointers), marks every slot free, restarts the
    /// queue order and picks the first usable slot as the dummy: for a set
    /// taken from the pool, before the ED is built. Thread context; the ED
    /// is not in any list.
    /// </summary>
    public void Reset()
    {
        if (Endpoint.Descriptor.Page is OhciDescriptorPage endpointPage)
        {
            ref OhciEndpointDescriptor ed = ref endpointPage.Endpoint(Endpoint.Descriptor.Index);
            ed.Control = OhciProtocol.EndpointSkip;
            ed.TailPointer = 0;
            ed.HeadPointer = 0;
            ed.NextEndpoint = 0;
        }

        DummyIndex = -1;
        NextOrder = 0;
        for (int i = 0; i < TdSlots.Length; i++)
        {
            TdStates[i] = OhciTdState.Free;
            TdTags[i] = 0;
            TdOrder[i] = 0;
            if (TdSlots[i].Page is not OhciDescriptorPage page)
            {
                continue;
            }

            ref OhciTransferDescriptor td = ref page.Transfer(TdSlots[i].Index);
            td.Control = OhciProtocol.ConditionCodeNotAccessed;
            td.CurrentBufferPointer = 0;
            td.NextTransfer = 0;
            td.BufferEnd = 0;
            if (DummyIndex < 0)
            {
                DummyIndex = i;
            }
        }
    }
}
