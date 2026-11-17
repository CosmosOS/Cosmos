// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Numerics;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// The controller's lists and the descriptors in them: building an ED,
/// linking it after a permanent head and taking it out again, queueing TDs
/// on an ED by the tail pointer method, and the quiesce that returns an
/// ED to "nothing queued" with every TD slot accounted for (OHCI 1.0a
/// sections 5.2.7 and 5.2.8).
/// </summary>
internal sealed partial class OhciState
{
    /// <summary>Dword 0 of an ED: the function address, endpoint number, direction, speed and packet size; K and F clear.</summary>
    private static uint EndpointControl(byte address, byte endpointNumber, uint direction, UsbSpeed speed, ushort maxPacketSize)
    {
        return (address & OhciProtocol.FunctionAddressMask)
            | ((uint)endpointNumber << OhciProtocol.EndpointNumberShift)
            | (direction << OhciProtocol.EndpointDirectionShift)
            | (speed == UsbSpeed.Low ? OhciProtocol.EndpointLowSpeed : 0)
            | ((maxPacketSize & OhciProtocol.EndpointMaxPacketSizeMask) << OhciProtocol.EndpointMaxPacketSizeShift);
    }

    /// <summary>Dword 0 of a TD the driver builds: CC NotAccessed, EC 0, the toggle, the delay, the PID and the rounding.</summary>
    private static uint TransferControl(uint pid, uint toggle, bool rounding, uint delayInterrupt)
    {
        return OhciProtocol.ConditionCodeNotAccessed
            | (toggle << OhciProtocol.DataToggleShift)
            | (delayInterrupt << OhciProtocol.DelayInterruptShift)
            | (pid << OhciProtocol.DirectionPidShift)
            | (rounding ? OhciProtocol.BufferRounding : 0);
    }

    /// <summary>The condition code a retired TD carries.</summary>
    private static OhciConditionCode ConditionCodeOf(uint control)
    {
        return (OhciConditionCode)((control >> OhciProtocol.ConditionCodeShift) & OhciProtocol.ConditionCodeMask);
    }

    /// <summary>
    /// The interval list an interrupt endpoint belongs in: bInterval in
    /// frames (0 read as 1), rounded down to a power of two and clamped to
    /// 1..32, as an exponent 0..5. A hub's 255 becomes 32.
    /// </summary>
    private static int IntervalExponent(byte interval)
    {
        int frames = Math.Clamp(interval == 0 ? 1 : interval, 1, OhciProtocol.MaxIntervalFrames);
        return Math.Min(BitOperations.Log2((uint)frames), OhciProtocol.IntervalHeadCount - 1);
    }

    // --- Endpoints ---

    /// <summary>
    /// Writes a set's ED for an endpoint: dword 0 from the parameters, head
    /// and tail both at the dummy TD (nothing queued, H and C clear), no
    /// next. Thread context; the ED is not in any list.
    /// </summary>
    private static void BuildEndpoint(OhciDescriptorSet set, byte address, byte endpointNumber, uint direction, UsbSpeed speed, ushort maxPacketSize)
    {
        uint dummy = set.TdSlots[set.DummyIndex].PhysicalAddress;
        ref OhciEndpointDescriptor ed = ref Ed(set.Endpoint.Descriptor);
        ed.Control = EndpointControl(address, endpointNumber, direction, speed, maxPacketSize);
        ed.TailPointer = dummy;
        ed.HeadPointer = dummy;
        ed.NextEndpoint = 0;
        DmaBuffer.WriteBarrier();
    }

    /// <summary>Sets or clears a set's K bit, the one dword 0 write the controller tolerates on a linked ED. Under the lock.</summary>
    private static void SetSkip(OhciDescriptorSet set, bool skip)
    {
        ref OhciEndpointDescriptor ed = ref Ed(set.Endpoint.Descriptor);
        uint control = ed.Control;
        Volatile.Write(ref ed.Control, skip ? control | OhciProtocol.EndpointSkip : control & ~OhciProtocol.EndpointSkip);
    }

    /// <summary>
    /// Puts an ED right after a permanent head (OHCI 1.0a section
    /// 5.2.7.1.1): its NextED takes the head's, a barrier, then the head's
    /// NextED names it; the software links follow. The head register is
    /// never rewritten, since the head ED is permanent. Under the lock.
    /// </summary>
    private static void LinkEndpoint(OhciEndpointSlot head, OhciEndpointSlot endpoint)
    {
        ref OhciEndpointDescriptor headEd = ref Ed(head.Descriptor);
        ref OhciEndpointDescriptor ed = ref Ed(endpoint.Descriptor);
        ed.NextEndpoint = headEd.NextEndpoint;
        endpoint.Next = head.Next;
        endpoint.Head = head;
        DmaBuffer.WriteBarrier();
        Volatile.Write(ref headEd.NextEndpoint, endpoint.PhysicalAddress);
        head.Next = endpoint;
    }

    /// <summary>
    /// Takes an ED out of its list (OHCI 1.0a section 5.2.7.1.2): under
    /// the lock its K bit is set and its predecessor's NextED skips it,
    /// every dword of the ED itself left alone (a controller positioned on
    /// it still follows its NextED); then, outside the lock, two frames
    /// pass before the caller may write the ED or reuse its slot. A no-op
    /// for an ED that is in no list. Thread context.
    /// </summary>
    /// <returns>True when the ED was in a list and the frames were waited out.</returns>
    private bool UnlinkEndpoint(OhciEndpointSlot endpoint)
    {
        bool linked;
        using (Lock.Acquire())
        {
            linked = UnlinkEndpointCore(endpoint);
        }

        if (!linked)
        {
            return false;
        }

        SettleFrames(OhciProtocol.EndpointRemovalFrames);
        return true;
    }

    /// <summary>
    /// The locked half of <see cref="UnlinkEndpoint"/>: K set, the
    /// predecessor's NextED rewritten past the ED, the software links
    /// dropped; the removed ED's own dwords are left alone. On its own
    /// (no settle) only for a controller that no longer runs. Under the
    /// lock.
    /// </summary>
    /// <returns>False for an ED in no list.</returns>
    private static bool UnlinkEndpointCore(OhciEndpointSlot endpoint)
    {
        OhciEndpointSlot? head = endpoint.Head;
        if (head is null)
        {
            return false;
        }

        ref OhciEndpointDescriptor ed = ref Ed(endpoint.Descriptor);
        Volatile.Write(ref ed.Control, ed.Control | OhciProtocol.EndpointSkip);

        OhciEndpointSlot predecessor = head;
        while (predecessor.Next is OhciEndpointSlot next && !ReferenceEquals(next, endpoint))
        {
            predecessor = next;
        }

        if (ReferenceEquals(predecessor.Next, endpoint))
        {
            ref OhciEndpointDescriptor previousEd = ref Ed(predecessor.Descriptor);
            Volatile.Write(ref previousEd.NextEndpoint, ed.NextEndpoint);
            predecessor.Next = endpoint.Next;
        }

        endpoint.Next = null;
        endpoint.Head = null;
        return true;
    }

    // --- Transfer descriptors ---

    /// <summary>
    /// Writes a TD into a set's slot and marks it queued with a fresh
    /// generation and the set's next queue order, so a chain is filled in
    /// the order the ED runs it (the drain's take-back of forgotten
    /// predecessors reads that order). Under the lock or in the drain; the
    /// caller publishes the tail afterwards.
    /// </summary>
    private static void FillTransfer(OhciDescriptorSet set, int index, uint control, uint bufferStart, uint bufferEnd, uint next)
    {
        OhciDescriptorSlot slot = set.TdSlots[index];
        ref OhciTransferDescriptor td = ref Td(slot);
        td.Control = control;
        td.CurrentBufferPointer = bufferStart;
        td.NextTransfer = next;
        td.BufferEnd = bufferEnd;
        set.TdTags[index] = OhciDescriptorPool.NextTag(slot);
        set.TdStates[index] = OhciTdState.InFlight;
        set.TdOrder[index] = unchecked(++set.NextOrder);
    }

    /// <summary>Zeroes a set's slot into a dummy: CC NotAccessed, no pointers, free. Under the lock or in the drain.</summary>
    private static void ZeroTransfer(OhciDescriptorSet set, int index)
    {
        ref OhciTransferDescriptor td = ref Td(set.TdSlots[index]);
        td.Control = OhciProtocol.ConditionCodeNotAccessed;
        td.CurrentBufferPointer = 0;
        td.NextTransfer = 0;
        td.BufferEnd = 0;
        set.TdStates[index] = OhciTdState.Free;
    }

    /// <summary>
    /// Hands a queued chain to the controller (OHCI 1.0a section 5.2.8.2):
    /// a barrier over the TDs written before, then the one 32-bit write of
    /// TailP that moves the tail to the new dummy. Under the lock or in the
    /// drain; a control or bulk caller sets CLF or BLF afterwards.
    /// </summary>
    private static void PublishTail(OhciDescriptorSet set, int newDummyIndex)
    {
        DmaBuffer.WriteBarrier();
        ref OhciEndpointDescriptor ed = ref Ed(set.Endpoint.Descriptor);
        Volatile.Write(ref ed.TailPointer, set.TdSlots[newDummyIndex].PhysicalAddress);
        set.DummyIndex = newDummyIndex;
    }

    /// <summary>Tells the controller the control list has work: HcCommandStatus ORs the bit in, so it never clears BLF. Any context.</summary>
    private void SetControlListFilled()
    {
        Write(OhciProtocol.HcCommandStatus, OhciProtocol.ControlListFilled);
    }

    /// <summary>Tells the controller the bulk list has work. Any context.</summary>
    private void SetBulkListFilled()
    {
        Write(OhciProtocol.HcCommandStatus, OhciProtocol.BulkListFilled);
    }

    // --- Quiesce ---

    /// <summary>
    /// Returns an ED to "nothing queued" with every TD slot accounted for,
    /// from whatever a timeout, a halt, a reset, a recovery or a close
    /// found it in. Under the lock the ED is skipped; outside it two
    /// frames pass; under the lock again the TDs from HeadP to TailP, which
    /// the controller never retired, become free at once, HeadP is moved
    /// to TailP with H clear and C kept or cleared, and K is cleared. The
    /// TDs before HeadP were retired and stay in flight until the done
    /// queue delivers them, which a drain per frame waits for over
    /// <see cref="OhciProtocol.DoneQueueFlushFrames"/> frames (DI 6
    /// flushes them within seven). A slot still in flight after that is
    /// quarantined: parked on the retired list for good, replaced in the
    /// set by a fresh pool slot when one can be had, counted and logged.
    /// On a controller that no longer runs nothing is waited for: nothing
    /// will be delivered, so every slot is free. Thread context; the
    /// caller owns the ED.
    /// </summary>
    /// <param name="set">The ED's memory set.</param>
    /// <param name="keepToggle">Keep the toggle carry (an error or timeout that did not move the device's toggle) or clear it (a reset to DATA0).</param>
    /// <param name="address">The device address, for the log.</param>
    /// <param name="endpointAddress">bEndpointAddress, for the log.</param>
    private void QuiesceEndpoint(OhciDescriptorSet set, bool keepToggle, byte address, byte endpointAddress)
    {
        bool runs = ControllerRuns;
        if (runs)
        {
            using (Lock.Acquire())
            {
                SetSkip(set, true);
            }

            SettleFrames(OhciProtocol.EndpointRemovalFrames);
        }

        using (Lock.Acquire())
        {
            ref OhciEndpointDescriptor ed = ref Ed(set.Endpoint.Descriptor);
            uint head = Volatile.Read(ref ed.HeadPointer);
            uint tail = ed.TailPointer & OhciProtocol.DescriptorPointerMask;
            for (uint pointer = head & OhciProtocol.DescriptorPointerMask; pointer != 0 && pointer != tail;)
            {
                int index = set.IndexOfPhysical(pointer);
                if (index < 0)
                {
                    break;
                }

                set.TdStates[index] = OhciTdState.Free;
                pointer = Td(set.TdSlots[index]).NextTransfer & OhciProtocol.DescriptorPointerMask;
            }

            if (!runs)
            {
                for (int index = 0; index < set.TdStates.Length; index++)
                {
                    set.TdStates[index] = OhciTdState.Free;
                }
            }

            uint carry = keepToggle ? head & OhciProtocol.EndpointToggleCarry : 0;
            Volatile.Write(ref ed.HeadPointer, tail | carry);
            DmaBuffer.WriteBarrier();
            if (runs)
            {
                SetSkip(set, false);
            }
        }

        for (uint frame = 0; frame < OhciProtocol.DoneQueueFlushFrames; frame++)
        {
            bool inFlight;
            using (Lock.Acquire())
            {
                DrainCompletions(null);
                inFlight = set.AnyInFlight();
            }

            if (!inFlight)
            {
                return;
            }

            SettleFrames(1);
        }

        QuarantineInFlight(set, address, endpointAddress);
    }

    /// <summary>
    /// Parks every slot of a set still in flight after the flush on the
    /// retired list, never to be pooled again (the controller may still
    /// name it on a later done queue), and gives the set a fresh slot in
    /// its place when the pool has one; without one the set keeps an empty
    /// slot and builds smaller chains. Thread context.
    /// </summary>
    private void QuarantineInFlight(OhciDescriptorSet set, byte address, byte endpointAddress)
    {
        object? owner;
        using (Lock.Acquire())
        {
            owner = OwnerOf(set);
        }

        int quarantined = 0;
        for (int index = 0; index < set.TdStates.Length; index++)
        {
            bool inFlight;
            using (Lock.Acquire())
            {
                inFlight = set.TdStates[index] == OhciTdState.InFlight;
            }

            if (!inFlight)
            {
                continue;
            }

            OhciDescriptorSlot replacement = TakeDescriptorSlot();
            using (Lock.Acquire())
            {
                if (set.TdStates[index] != OhciTdState.InFlight)
                {
                    _pool.Return(replacement);
                    continue;
                }

                _retiredSlots.Add(set.TdSlots[index]);
                set.TdSlots[index] = replacement;
                set.TdStates[index] = OhciTdState.Free;
                set.TdTags[index] = 0;
                if (!replacement.IsNone)
                {
                    OhciDescriptorPool.SetOwner(replacement, owner);
                    ZeroTransfer(set, index);
                }

                if (set.DummyIndex == index)
                {
                    set.DummyIndex = -1;
                    for (int i = 0; i < set.TdSlots.Length; i++)
                    {
                        if (!set.TdSlots[i].IsNone && set.TdStates[i] == OhciTdState.Free)
                        {
                            set.DummyIndex = i;
                            break;
                        }
                    }
                }

                _quarantinedSlots++;
                quarantined++;
            }
        }

        if (quarantined > 0)
        {
            _binding.Log($"address {address} endpoint {endpointAddress}: a descriptor was quarantined");
        }
    }

    /// <summary>The device or pipe a set's TDs are recorded for in the pool: the owner its first present slot carries. Under the lock.</summary>
    private static object? OwnerOf(OhciDescriptorSet set)
    {
        for (int i = 0; i < set.TdSlots.Length; i++)
        {
            if (set.TdSlots[i].Page is OhciDescriptorPage page)
            {
                return page.Owners[set.TdSlots[i].Index];
            }
        }

        return null;
    }

    /// <summary>Records <paramref name="owner"/> for every TD slot of a set, so the drain maps its done entries back. Under the lock.</summary>
    private static void SetOwner(OhciDescriptorSet set, object? owner)
    {
        for (int i = 0; i < set.TdSlots.Length; i++)
        {
            OhciDescriptorPool.SetOwner(set.TdSlots[i], owner);
        }
    }
}
