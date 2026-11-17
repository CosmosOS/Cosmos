// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// The two schedules: the skeleton built once in the probe (the async
/// head and the eight interval heads the frame list points at), the queue
/// head and transfer descriptor writers, the insertion of a queue head
/// after a head, its removal through the async doorbell or the periodic
/// frame settle, and the quiesce that empties an endpoint's queue head
/// while it is unlinked. Every link pointer the driver writes names a
/// queue head: the controller never sees an iTD, siTD or FSTN.
/// </summary>
internal sealed partial class EhciState
{
    // --- The skeleton ---

    /// <summary>
    /// Writes the async head (H set, a halted overlay so every traversal
    /// skips it, linked to itself), the interval heads (halted overlays,
    /// head k linked to head k - 1, head 0 terminating) and the frame list
    /// (frame i to the head of index min(trailing zeros of i, 7), frame 0
    /// to head 7), so a queue head behind head k runs every 2^k frames
    /// (EHCI 1.0 sections 4.7 and 4.8). Thread context, from the probe,
    /// before the controller runs.
    /// </summary>
    private void BuildSchedules()
    {
        EhciQueueHeadSlot asyncHead = AsyncHead;
        WriteQueueHead(ref asyncHead.QueueHead, asyncHead.LinkPointer, EhciProtocol.QhHead, EhciProtocol.QhMultOne,
            EhciProtocol.LinkTerminate, EhciProtocol.TokenHalted);

        for (int k = 0; k < _intervalHeads.Length; k++)
        {
            uint horizontal = k == 0 ? EhciProtocol.LinkTerminate : _intervalHeads[k - 1].LinkPointer;
            WriteQueueHead(ref _intervalHeads[k].QueueHead, horizontal, 0, EhciProtocol.QhMultOne,
                EhciProtocol.LinkTerminate, EhciProtocol.TokenHalted);
        }

        Span<uint> entries = MemoryMarshal.Cast<byte, uint>(FrameList.Span);
        for (int frame = 0; frame < EhciProtocol.FrameListEntries; frame++)
        {
            int k = frame == 0
                ? EhciProtocol.MaxIntervalIndex
                : Math.Min(BitOperations.TrailingZeroCount(frame), EhciProtocol.MaxIntervalIndex);
            entries[frame] = _intervalHeads[k].LinkPointer;
        }

        DmaBuffer.WriteBarrier();
    }

    // --- Writers ---

    /// <summary>
    /// Writes a whole queue head for one it is not linked: the horizontal
    /// link, the two endpoint dwords, a zero current pointer and an overlay
    /// of the given next pointer, a terminating alternate next, the given
    /// token and zero buffers. Mult must be set: a zero yields undefined
    /// results (EHCI 1.0 section 3.6.2).
    /// </summary>
    /// <exception cref="InvalidOperationException">The capabilities carry Mult 0.</exception>
    private static void WriteQueueHead(ref EhciQueueHead queueHead, uint horizontal, uint characteristics, uint capabilities, uint overlayNext, uint overlayToken)
    {
        if ((capabilities & EhciProtocol.QhMultMask) == 0)
        {
            throw new InvalidOperationException("A queue head needs a non-zero Mult.");
        }

        queueHead.HorizontalLink = horizontal;
        queueHead.Characteristics = characteristics;
        queueHead.Capabilities = capabilities;
        queueHead.CurrentTransfer = 0;
        queueHead.NextTransfer = overlayNext;
        queueHead.AlternateNextTransfer = EhciProtocol.LinkTerminate;
        queueHead.Token = overlayToken;
        queueHead.Buffer0 = 0;
        queueHead.Buffer1 = 0;
        queueHead.Buffer2 = 0;
        queueHead.Buffer3 = 0;
        queueHead.Buffer4 = 0;
    }

    /// <summary>Endpoint characteristics, queue head dword 1: the address, the endpoint, the speed, DTC, the packet size and the C bit; I, H and RL stay 0.</summary>
    /// <param name="address">The device address, 0 before SET_ADDRESS.</param>
    /// <param name="endpoint">The endpoint number.</param>
    /// <param name="speed">The device speed, as EPS.</param>
    /// <param name="toggleFromTransfer">DTC: the toggle comes from each qTD (control endpoints) rather than the overlay (bulk and interrupt).</param>
    /// <param name="maxPacket">The maximum packet length.</param>
    /// <param name="controlEndpoint">C: a full or low-speed control endpoint.</param>
    private static uint EndpointCharacteristics(byte address, byte endpoint, UsbSpeed speed, bool toggleFromTransfer, ushort maxPacket, bool controlEndpoint)
    {
        uint value = (address & EhciProtocol.QhDeviceAddressMask)
            | ((uint)(endpoint & EhciProtocol.QhEndpointMask) << EhciProtocol.QhEndpointShift)
            | (SpeedField(speed) << EhciProtocol.QhSpeedShift)
            | ((uint)(maxPacket & EhciProtocol.QhMaxPacketMask) << EhciProtocol.QhMaxPacketShift);
        if (toggleFromTransfer)
        {
            value |= EhciProtocol.QhDataToggleControl;
        }

        if (controlEndpoint)
        {
            value |= EhciProtocol.QhControlEndpoint;
        }

        return value;
    }

    /// <summary>Endpoint capabilities, queue head dword 2: the split masks, the transaction translator's hub and port, and Mult.</summary>
    /// <param name="startMask">S-mask, the micro-frames a periodic endpoint starts in; 0 for the async schedule.</param>
    /// <param name="completeMask">C-mask, the micro-frames of the complete-splits; 0 for high speed.</param>
    /// <param name="hubAddress">The high-speed hub whose TT serves a full or low-speed device, 0 otherwise.</param>
    /// <param name="hubPort">That hub's port, 0 otherwise.</param>
    /// <param name="mult">Transactions per micro-frame, 1 to 3.</param>
    private static uint EndpointCapabilities(uint startMask, uint completeMask, byte hubAddress, byte hubPort, uint mult)
    {
        return (startMask << EhciProtocol.QhStartMaskShift)
            | (completeMask << EhciProtocol.QhCompleteMaskShift)
            | ((uint)hubAddress << EhciProtocol.QhHubAddressShift)
            | ((uint)hubPort << EhciProtocol.QhHubPortShift)
            | (mult << EhciProtocol.QhMultShift);
    }

    /// <summary>EPS of a device speed: low 01b, high 10b, anything else full 00b.</summary>
    private static uint SpeedField(UsbSpeed speed)
    {
        return speed switch
        {
            UsbSpeed.Low => EhciProtocol.QhSpeedLow,
            UsbSpeed.High => EhciProtocol.QhSpeedHigh,
            _ => EhciProtocol.QhSpeedFull
        };
    }

    /// <summary>
    /// Fills a qTD except its token: the next and alternate next pointers
    /// and the buffer pointers, page 0 with the offset and as many
    /// following pages as the length needs. The caller writes the token,
    /// Active set, after a write barrier.
    /// </summary>
    /// <param name="transfer">The qTD, a slot the controller does not hold.</param>
    /// <param name="next">The next qTD pointer.</param>
    /// <param name="alternateNext">The alternate next qTD pointer, T unless a short packet should branch.</param>
    /// <param name="buffer">Physical address of the data; the low 12 bits are the offset in page 0.</param>
    /// <param name="length">Bytes to transfer, at most <see cref="EhciProtocol.QtdMaxTransferBytes"/> from a page boundary.</param>
    private static void FillTransfer(ref EhciTransferDescriptor transfer, uint next, uint alternateNext, ulong buffer, int length)
    {
        uint page0 = (uint)buffer;
        uint pageBase = page0 & ~EhciProtocol.QtdBufferOffsetMask;
        int offset = (int)(page0 & EhciProtocol.QtdBufferOffsetMask);
        int pages = length == 0 ? 1 : (offset + length + PageBytes - 1) / PageBytes;

        transfer.Next = next;
        transfer.AlternateNext = alternateNext;
        transfer.Token = 0;
        transfer.Buffer0 = page0;
        transfer.Buffer1 = pages > 1 ? pageBase + (uint)PageBytes : 0;
        transfer.Buffer2 = pages > 2 ? pageBase + (2 * (uint)PageBytes) : 0;
        transfer.Buffer3 = pages > 3 ? pageBase + (3 * (uint)PageBytes) : 0;
        transfer.Buffer4 = pages > 4 ? pageBase + (4 * (uint)PageBytes) : 0;
    }

    /// <summary>A qTD token with Active set, CERR 3 and C_Page 0: the PID, the total bytes, the toggle and IOC.</summary>
    /// <param name="pid">OUT, IN or SETUP.</param>
    /// <param name="totalBytes">Bytes to transfer.</param>
    /// <param name="dataToggle">DT, read when the queue head has DTC set.</param>
    /// <param name="interruptOnComplete">IOC: the last qTD of every chain.</param>
    private static uint TransferToken(uint pid, int totalBytes, bool dataToggle, bool interruptOnComplete)
    {
        uint token = EhciProtocol.TokenActive
            | (pid << EhciProtocol.TokenPidShift)
            | (EhciProtocol.TokenErrorCounterMax << EhciProtocol.TokenErrorCounterShift)
            | ((uint)totalBytes << EhciProtocol.TokenTotalBytesShift);
        if (interruptOnComplete)
        {
            token |= EhciProtocol.TokenInterruptOnComplete;
        }

        if (dataToggle)
        {
            token |= EhciProtocol.TokenDataToggle;
        }

        return token;
    }

    /// <summary>Bytes a retired qTD's token says were left untransferred.</summary>
    private static int RemainingBytes(uint token)
    {
        return (int)((token >> EhciProtocol.TokenTotalBytesShift) & EhciProtocol.TokenTotalBytesMask);
    }

    /// <summary>
    /// Whether a halted qTD's token records a device STALL rather than an
    /// error: Babble halts with CERR intact but is fatal, a missed
    /// complete-split on an IN halts the queue (Table 4-13), otherwise a
    /// STALL leaves CERR non-zero while the transaction errors that halt a
    /// queue drive it to 0. XactErr, Data Buffer Error and Missed
    /// Micro-Frame are sticky across retries (Table 3-16), so their
    /// presence proves nothing about the handshake that ended the qTD.
    /// </summary>
    private static bool IsStall(uint token)
    {
        if ((token & EhciProtocol.TokenHalted) == 0 || (token & EhciProtocol.TokenBabble) != 0)
        {
            return false;
        }

        if ((token & EhciProtocol.TokenMissedMicroFrame) != 0 && ((token >> EhciProtocol.TokenPidShift) & EhciProtocol.TokenPidMask) == EhciProtocol.PidIn)
        {
            return false;
        }

        return ((token >> EhciProtocol.TokenErrorCounterShift) & EhciProtocol.TokenErrorCounterMask) != 0;
    }

    // --- Linking ---

    /// <summary>Inserts a queue head right after the async head (EHCI 1.0 section 4.8.1): its link takes the head's, a write barrier, then the head's link takes it. Under the lock.</summary>
    private void LinkAsync(EhciQueueHeadSlot queueHead)
    {
        EhciQueueHeadSlot head = AsyncHead;
        queueHead.QueueHead.HorizontalLink = head.QueueHead.HorizontalLink;
        DmaBuffer.WriteBarrier();
        Volatile.Write(ref head.QueueHead.HorizontalLink, queueHead.LinkPointer);
        _asyncQueueHeads.Insert(0, queueHead);
        queueHead.ListIndex = EhciQueueHeadSlot.AsyncList;
    }

    /// <summary>Inserts a queue head right after interval head <paramref name="interval"/>, so it runs every 2^interval frames. Under the lock.</summary>
    private void LinkPeriodic(EhciQueueHeadSlot queueHead, int interval)
    {
        EhciQueueHeadSlot head = _intervalHeads[interval];
        queueHead.QueueHead.HorizontalLink = head.QueueHead.HorizontalLink;
        DmaBuffer.WriteBarrier();
        Volatile.Write(ref head.QueueHead.HorizontalLink, queueHead.LinkPointer);
        _periodicQueueHeads[interval].Insert(0, queueHead);
        queueHead.ListIndex = interval;
    }

    /// <summary>Inserts a queue head into the list <paramref name="list"/> names, the async schedule or an interval head's. Under the lock.</summary>
    private void Link(EhciQueueHeadSlot queueHead, int list)
    {
        if (list == EhciQueueHeadSlot.AsyncList)
        {
            LinkAsync(queueHead);
        }
        else
        {
            LinkPeriodic(queueHead, list);
        }
    }

    /// <summary>
    /// Takes a queue head out of its list: the predecessor's link takes its
    /// link, a write barrier, and the software list drops it. No dword of
    /// the removed queue head is written: the controller may be positioned
    /// on it and follows its horizontal link (EHCI 1.0 section 4.8.2). A
    /// queue head on no list is left alone. Under the lock.
    /// </summary>
    /// <returns>True when the queue head was in a list.</returns>
    private bool RemoveFromList(EhciQueueHeadSlot queueHead)
    {
        int list = queueHead.ListIndex;
        if (list == EhciQueueHeadSlot.NoList)
        {
            return false;
        }

        List<EhciQueueHeadSlot> entries = list == EhciQueueHeadSlot.AsyncList ? _asyncQueueHeads : _periodicQueueHeads[list];
        EhciQueueHeadSlot head = list == EhciQueueHeadSlot.AsyncList ? AsyncHead : _intervalHeads[list];
        int index = entries.IndexOf(queueHead);
        if (index < 0)
        {
            queueHead.ListIndex = EhciQueueHeadSlot.NoList;
            return false;
        }

        EhciQueueHeadSlot predecessor = index == 0 ? head : entries[index - 1];
        Volatile.Write(ref predecessor.QueueHead.HorizontalLink, queueHead.QueueHead.HorizontalLink);
        DmaBuffer.WriteBarrier();
        entries.RemoveAt(index);
        queueHead.ListIndex = EhciQueueHeadSlot.NoList;
        return true;
    }

    // --- Unlinking ---

    /// <summary>
    /// Takes a queue head out of the async schedule and waits for the
    /// controller to let go of it (EHCI 1.0 section 4.8.2): the doorbell
    /// claimed, the predecessor rewritten under the lock, then under the
    /// lock again any USBSTS.IAA an earlier doorbell left behind written
    /// back (a doorbell that timed out may be acknowledged late, and that
    /// acknowledge must not pass for this one), the flag cleared and
    /// USBCMD.IAAD set; the acknowledge is awaited on the event or the
    /// polled service within <see cref="EhciProtocol.DoorbellTimeoutMs"/>
    /// (a controller wait: no device, so a disconnected device's queue head
    /// is still waited out) and every <see cref="EhciProtocol.DoorbellWatchdogMs"/>
    /// read back from USBSTS.IAA and USBCMD.IAAD, since a controller may
    /// ring IAA without raising the line or clear IAAD without setting IAA.
    /// Between the predecessor write and the acknowledge no dword of the
    /// queue head is written. A queue head on no list, or a controller that
    /// does not run (halted, or a function that vanished), needs no
    /// doorbell. Thread context.
    /// </summary>
    /// <returns>True when the controller let go; false when the doorbell was not claimed or not acknowledged, so the caller retires the memory set.</returns>
    private bool UnlinkAsyncQueueHead(EhciQueueHeadSlot queueHead)
    {
        bool claimed = ClaimDoorbell();
        try
        {
            bool wasLinked;
            using (Lock.Acquire())
            {
                wasLinked = RemoveFromList(queueHead);
            }

            if (!wasLinked || !ControllerRuns)
            {
                return true;
            }

            if (!claimed)
            {
                NoteDoorbellTimeout();
                return false;
            }

            using (Lock.Acquire())
            {
                // A doorbell that timed out may be acknowledged late: its IAA
                // is written back here so the one the wait consumes is ours.
                WriteOperational(EhciProtocol.UsbSts, EhciProtocol.UsbStsAsyncAdvance);
                Volatile.Write(ref _doorbellCompleted, false);
                WriteOperational(EhciProtocol.UsbCmd, ReadOperational(EhciProtocol.UsbCmd) | EhciProtocol.UsbCmdAsyncAdvanceDoorbell);
            }

            // The acknowledge is also read back from the registers between
            // short waits: a controller may set IAA without raising the line,
            // or clear IAAD without setting IAA (Linux's 10 ms IAA watchdog).
            long deadline = DeadlineAfter(EhciProtocol.DoorbellTimeoutMs);
            while (true)
            {
                if (WaitCompletion(ref _doorbellCompleted, _doorbellEvent, null, EhciProtocol.DoorbellWatchdogMs))
                {
                    return true;
                }

                // A controller that halted meanwhile fetches nothing any more.
                if (!ControllerRuns)
                {
                    return true;
                }

                uint status = ReadOperational(EhciProtocol.UsbSts);
                uint command = ReadOperational(EhciProtocol.UsbCmd);
                if ((status & EhciProtocol.UsbStsAsyncAdvance) != 0 || (command & EhciProtocol.UsbCmdAsyncAdvanceDoorbell) == 0)
                {
                    using (Lock.Acquire())
                    {
                        // The handler or the polled service may have seen it
                        // meanwhile; only an acknowledge nobody saw counts.
                        if (!Volatile.Read(ref _doorbellCompleted))
                        {
                            WriteOperational(EhciProtocol.UsbSts, EhciProtocol.UsbStsAsyncAdvance);
                            Volatile.Write(ref _doorbellCompleted, true);
                            _lostDoorbellInterrupts++;
                        }
                    }

                    return true;
                }

                if (Stopwatch.GetTimestamp() >= deadline)
                {
                    break;
                }
            }

            NoteDoorbellTimeout();
            return false;
        }
        finally
        {
            if (claimed)
            {
                ReleaseDoorbell();
            }
        }
    }

    /// <summary>Counts an unacknowledged doorbell and logs the first one. Thread context.</summary>
    private void NoteDoorbellTimeout()
    {
        _doorbellTimeouts++;
        if (!_doorbellTimeoutLogged)
        {
            _doorbellTimeoutLogged = true;
            _binding.Log("async doorbell timed out");
        }
    }

    /// <summary>
    /// Takes a queue head out of the periodic schedule and waits two
    /// frames for the controller to be off it; no doorbell exists for the
    /// periodic list. The frames are waited whenever the controller runs,
    /// linked or not: a close may follow a recovery's own unlink, and the
    /// controller may still be positioned on the queue head either way.
    /// Thread context.
    /// </summary>
    private void UnlinkPeriodicQueueHead(EhciQueueHeadSlot queueHead)
    {
        using (Lock.Acquire())
        {
            RemoveFromList(queueHead);
        }

        if (ControllerRuns)
        {
            WaitFrames(EhciProtocol.PeriodicUnlinkFrames);
        }
    }

    /// <summary>
    /// Empties an endpoint's queue head (a control timeout, a halted or
    /// timed-out bulk transfer, an endpoint reset, an interrupt pipe
    /// recovery): unlinked through the doorbell or the frame settle, then,
    /// only once the controller let go, every qTD slot of the set zeroed
    /// (nothing stays queued) and the overlay rewritten with the set's
    /// dummy as next, a terminating alternate next, zero buffers and a
    /// token that keeps the toggle or returns it to DATA0, and linked
    /// again where it was. Thread context; the caller owns the endpoint.
    /// </summary>
    /// <param name="queueHead">The endpoint's queue head.</param>
    /// <param name="transfers">The set's qTD slots.</param>
    /// <param name="dummy">Index of the slot the overlay idles on afterwards.</param>
    /// <param name="keepToggle">Keep the overlay's DT: a bulk error or timeout left the device's toggle where it was; false returns the endpoint to DATA0.</param>
    /// <returns>False when the controller may still hold the queue head, which the caller then retires with its set.</returns>
    private bool QuiesceQueueHead(EhciQueueHeadSlot queueHead, EhciDescriptorSlot[] transfers, int dummy, bool keepToggle)
    {
        int list;
        using (Lock.Acquire())
        {
            list = queueHead.ListIndex;
        }

        if (list == EhciQueueHeadSlot.AsyncList)
        {
            if (!UnlinkAsyncQueueHead(queueHead))
            {
                return false;
            }
        }
        else if (list != EhciQueueHeadSlot.NoList)
        {
            UnlinkPeriodicQueueHead(queueHead);
        }

        using (Lock.Acquire())
        {
            for (int i = 0; i < transfers.Length; i++)
            {
                transfers[i].Clear();
            }

            ref EhciQueueHead head = ref queueHead.QueueHead;
            DmaBuffer.ReadBarrier();
            uint token = keepToggle ? Volatile.Read(ref head.Token) & EhciProtocol.TokenDataToggle : 0;
            head.CurrentTransfer = 0;
            head.NextTransfer = transfers[dummy].TransferPointer;
            head.AlternateNextTransfer = EhciProtocol.LinkTerminate;
            head.Token = token;
            head.Buffer0 = 0;
            head.Buffer1 = 0;
            head.Buffer2 = 0;
            head.Buffer3 = 0;
            head.Buffer4 = 0;
            DmaBuffer.WriteBarrier();

            if (list != EhciQueueHeadSlot.NoList)
            {
                Link(queueHead, list);
            }
        }

        return true;
    }
}
