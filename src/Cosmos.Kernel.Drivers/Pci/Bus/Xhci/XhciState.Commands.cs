// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Xhci;

/// <summary>
/// The command ring and the commands built on it: Enable Slot, Address
/// Device and Evaluate Context of the address path, Configure Endpoint of
/// the hub registration and the pipe opens, the Stop Endpoint, Reset
/// Endpoint, Set TR Dequeue Pointer and drop-context Configure Endpoint of
/// a pipe close, and the Disable Slot of a device release.
/// </summary>
public sealed partial class XhciState
{
    /// <summary>The device descriptor's first 8 bytes carry bMaxPacketSize0, and any packet size moves them (USB 2.0 section 9.6.1).</summary>
    private const int DeviceDescriptorHeadLength = 8;
    private const int MaxPacketSize0Offset = 7;

    /// <summary>SuperSpeed devices give bMaxPacketSize0 as a power of two; 2^15 is past any valid value.</summary>
    private const int MaxPacketSize0MaxExponent = 15;

    /// <summary>Periodic endpoint interval bounds, in the Endpoint Context's 2^n x 125 us encoding (xHCI 1.2 section 6.2.3.6).</summary>
    private const int FullSpeedMinIntervalExponent = 3;
    private const int FullSpeedMaxIntervalExponent = 10;
    private const int HighSpeedMaxInterval = 16;

    /// <summary>Set TR Dequeue Pointer parameter bit 0: the dequeue cycle state.</summary>
    private const ulong DequeueCycleState = 1;

    // --- The command ring ---

    /// <summary>
    /// Queues one command, rings the command doorbell and waits for its
    /// completion event: the one-command flag is claimed, the TRB enqueued
    /// and the doorbell written under the lock, the completion awaited
    /// through <see cref="WaitCompletion"/>, and the flag released in a
    /// finally. Thread context only.
    /// </summary>
    /// <param name="parameter">The command TRB's parameter: an input context address, a dequeue pointer, or 0.</param>
    /// <param name="control">The control dword less the cycle bit, from <see cref="SlotCommand"/> or <see cref="EndpointCommand"/>.</param>
    /// <param name="slotId">The slot id the completion carried (what Enable Slot returns), 0 on a timeout.</param>
    /// <returns>The completion code, or <see cref="XhciCompletionCode.Invalid"/> when no completion arrived within <see cref="XhciProtocol.CommandTimeoutMs"/> or the flag could not be claimed.</returns>
    internal XhciCompletionCode ExecuteCommand(ulong parameter, uint control, out byte slotId)
    {
        slotId = 0;
        if (!ClaimCommand())
        {
            return XhciCompletionCode.Invalid;
        }

        try
        {
            using (Lock.Acquire())
            {
                Volatile.Write(ref _commandCompleted, false);
                _pendingCommand = CommandRing.Enqueue(parameter, 0, control);
                RingDoorbell(0, 0);
            }

            _commandsIssued++;
            if (!WaitCompletion(ref _commandCompleted, _commandEvent, null, XhciProtocol.CommandTimeoutMs))
            {
                using (Lock.Acquire())
                {
                    _pendingCommand = 0;
                }

                _timeouts++;
                return XhciCompletionCode.Invalid;
            }

            using (Lock.Acquire())
            {
                _pendingCommand = 0;
                slotId = _commandSlotId;
                return _commandCode;
            }
        }
        finally
        {
            using (Lock.Acquire())
            {
                _commandBusy = false;
            }
        }
    }

    /// <summary>Control dword of a command that targets a slot.</summary>
    private static uint SlotCommand(XhciTrbType type, byte slotId) =>
        XhciTrb.TypeField(type) | ((uint)slotId << XhciTrb.SlotIdShift);

    /// <summary>Control dword of a command that targets one endpoint of a slot.</summary>
    private static uint EndpointCommand(XhciTrbType type, byte slotId, byte endpointId) =>
        SlotCommand(type, slotId) | ((uint)endpointId << XhciTrb.EndpointIdShift);

    /// <summary>Logs a command that timed out or completed with an error. Thread context only; a failure seen in interrupt context is counted, not logged.</summary>
    /// <param name="command">The command's name.</param>
    /// <param name="code">Its completion code.</param>
    private void LogCommandFailure(string command, XhciCompletionCode code)
    {
        if (code == XhciCompletionCode.Invalid)
        {
            _binding.Log($"{command} timed out");
        }
        else
        {
            _binding.Log($"{command} failed, completion code {(uint)code}");
        }
    }

    // --- The address path ---

    /// <inheritdoc/>
    protected override UsbDevice? AddressDeviceCore(UsbDevice? parentHub, byte port, UsbSpeed speed)
    {
        XhciCompletionCode code = ExecuteCommand(0, XhciTrb.TypeField(XhciTrbType.EnableSlotCommand), out byte slotId);
        if (code != XhciCompletionCode.Success || slotId == 0 || slotId > _maxSlots)
        {
            LogCommandFailure("Enable Slot", code);
            return null;
        }

        XhciSlotMemory? memory = TakeSlotMemory();
        if (memory is null)
        {
            // The controller's slot is given back: Enable Slot succeeded and
            // no XhciSlot exists to release it through.
            _binding.Log($"slot {slotId}: no DMA memory for the endpoint");
            ExecuteCommand(0, SlotCommand(XhciTrbType.DisableSlotCommand, slotId), out _);
            return null;
        }

        XhciSlot slot = new(this, parentHub as XhciSlot, port, speed, slotId, memory);
        using (Lock.Acquire())
        {
            _slots[slotId] = slot;
            Span<ulong> entries = MemoryMarshal.Cast<byte, ulong>(Dcbaa.Span);
            entries[slotId] = memory.OutputContext.PhysicalAddress;
        }

        XhciContext.SetFlags(memory.InputControl(), 0, XhciContext.SlotFlag | XhciContext.EndpointFlag(XhciSlot.ControlEndpointId));
        XhciContext.WriteSlot(memory.InputSlot(), slot.RouteString, speed, XhciSlot.ControlEndpointId,
            slot.RootPortNumber, slot.TtHubSlotId, slot.TtPortNumber);
        WriteControlEndpoint(slot, slot.MaxPacketSize0);

        code = ExecuteCommand(memory.InputContext.PhysicalAddress, SlotCommand(XhciTrbType.AddressDeviceCommand, slotId), out _);
        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Address Device", code);
            ReleaseDeviceCore(slot, hostPresent: true);
            return null;
        }

        _binding.Delay(XhciProtocol.SetAddressRecoveryMs * XhciProtocol.MicrosecondsPerMillisecond);

        Span<byte> head = stackalloc byte[DeviceDescriptorHeadLength];
        UsbSetupPacket getDescriptor = new(UsbRequestType.DeviceToHost | UsbRequestType.Standard | UsbRequestType.Device,
            (byte)UsbStandardRequest.GetDescriptor, (ushort)((byte)UsbDescriptorType.Device << 8), 0, DeviceDescriptorHeadLength);
        if (ControlTransfer(slot, getDescriptor, head) != UsbTransferStatus.Success)
        {
            _binding.Log($"slot {slotId}: the addressed device did not return its descriptor");
            ReleaseDeviceCore(slot, hostPresent: true);
            return null;
        }

        int packetSizeField = head[MaxPacketSize0Offset];
        ushort maxPacketSize = speed >= UsbSpeed.Super
            ? (ushort)(packetSizeField <= MaxPacketSize0MaxExponent ? 1 << packetSizeField : 0)
            : (ushort)packetSizeField;
        if (maxPacketSize != 0 && maxPacketSize != slot.MaxPacketSize0 && !UpdateControlMaxPacketSize(slot, maxPacketSize))
        {
            ReleaseDeviceCore(slot, hostPresent: true);
            return null;
        }

        _devicesAddressed++;
        return slot;
    }

    /// <summary>Tells the controller the default endpoint's real packet size once the device descriptor gave it: Evaluate Context with the control endpoint's context rewritten, under the input context claim. Thread context.</summary>
    private bool UpdateControlMaxPacketSize(XhciSlot slot, ushort maxPacketSize)
    {
        XhciCompletionCode code = XhciCompletionCode.Invalid;
        if (ClaimInput(slot))
        {
            try
            {
                slot.Memory.ClearInput();
                XhciContext.SetFlags(slot.Memory.InputControl(), 0, XhciContext.EndpointFlag(XhciSlot.ControlEndpointId));
                WriteControlEndpoint(slot, maxPacketSize);
                code = ExecuteCommand(slot.Memory.InputContext.PhysicalAddress, SlotCommand(XhciTrbType.EvaluateContextCommand, slot.SlotId), out _);
            }
            finally
            {
                ReleaseInput(slot);
            }
        }

        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Evaluate Context", code);
            return false;
        }

        slot.SetMaxPacketSize0(maxPacketSize);
        return true;
    }

    /// <summary>Writes the default control endpoint's input context: type Control, the packet size, no burst, no interval, the control ring and its cycle, average TRB length 8, no ESIT payload.</summary>
    private static void WriteControlEndpoint(XhciSlot slot, ushort maxPacketSize) =>
        XhciContext.WriteEndpoint(slot.Memory.InputEndpoint(XhciSlot.ControlEndpointId), XhciEndpointType.Control,
            maxPacketSize, 0, 0, slot.Memory.ControlRing.PhysicalAddress, slot.Memory.ControlRing.CycleState,
            XhciProtocol.ControlAverageTrbLength, 0);

    // --- Hubs ---

    /// <summary>
    /// Registers the slot as a hub in its Slot Context: the output Slot
    /// Context copied into the input one with the Hub bit, the port count
    /// and the think time, then Configure Endpoint on a controller from
    /// 0.96 and Evaluate Context before. Thread context; the body of
    /// <see cref="XhciSlot.ConfigureAsHubCore"/>.
    /// </summary>
    /// <param name="slot">The hub's slot.</param>
    /// <param name="portCount">bNbrPorts.</param>
    /// <param name="thinkTime">TT think time.</param>
    internal bool ConfigureAsHub(XhciSlot slot, byte portCount, byte thinkTime)
    {
        XhciCompletionCode code = XhciCompletionCode.Invalid;
        if (ClaimInput(slot))
        {
            try
            {
                slot.Memory.ClearInput();
                XhciContext.SetFlags(slot.Memory.InputControl(), 0, XhciContext.SlotFlag);
                XhciContext.CopySlot(slot.Memory.OutputSlot(), slot.Memory.InputSlot());
                XhciContext.SetHub(slot.Memory.InputSlot(), portCount, thinkTime);

                XhciTrbType command = _version >= XhciProtocol.HubConfigureEndpointMinVersion
                    ? XhciTrbType.ConfigureEndpointCommand
                    : XhciTrbType.EvaluateContextCommand;
                code = ExecuteCommand(slot.Memory.InputContext.PhysicalAddress, SlotCommand(command, slot.SlotId), out _);
            }
            finally
            {
                ReleaseInput(slot);
            }
        }

        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Configure hub", code);
            return false;
        }

        return true;
    }

    // --- Interrupt pipes ---

    /// <summary>
    /// Adds an interrupt IN endpoint to the slot and starts its transfers:
    /// a pooled memory set, a fresh pipe object, the endpoint's input
    /// context and Configure Endpoint under the input context claim, then
    /// the table entry, every buffer queued and the doorbell under the
    /// lock. Thread context; the body of
    /// <see cref="XhciSlot.OpenInterruptPipeCore"/>.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="endpoint">An interrupt IN endpoint of the device.</param>
    /// <param name="handler">Receives every report.</param>
    /// <returns>The pipe; null when the endpoint is not interrupt IN, its DCI is already open, no DMA memory was left or the controller refused.</returns>
    internal XhciInterruptPipe? OpenInterruptPipe(XhciSlot slot, UsbEndpoint endpoint, UsbReportHandler handler)
    {
        if (endpoint.Type != UsbEndpointType.Interrupt || !endpoint.IsIn)
        {
            return null;
        }

        byte dci = XhciSlot.EndpointId(endpoint);
        if (dci <= XhciSlot.ControlEndpointId || dci > XhciSlot.MaxEndpointId)
        {
            return null;
        }

        using (Lock.Acquire())
        {
            if (slot.InterruptPipes[dci] is not null || slot.BulkPipes[dci] is not null)
            {
                return null;
            }
        }

        XhciPipeMemory? memory = TakePipeMemory(bulk: false);
        if (memory is null)
        {
            _binding.Log($"slot {slot.SlotId}: no DMA memory for the endpoint");
            return null;
        }

        XhciInterruptPipe pipe = new(endpoint, handler, memory, dci);
        XhciCompletionCode code = XhciCompletionCode.Invalid;
        if (ClaimInput(slot))
        {
            try
            {
                PrepareEndpointInput(slot, dci, dropFlags: 0);
                uint maxEsitPayload = (uint)pipe.TransferSize;
                XhciContext.WriteEndpoint(slot.Memory.InputEndpoint(dci), XhciEndpointType.InterruptIn,
                    endpoint.MaxPacketSize, endpoint.AdditionalTransactions, InterruptInterval(slot.Speed, endpoint.Interval),
                    memory.Ring.PhysicalAddress, memory.Ring.CycleState, (ushort)maxEsitPayload, maxEsitPayload);
                code = ExecuteCommand(slot.Memory.InputContext.PhysicalAddress, SlotCommand(XhciTrbType.ConfigureEndpointCommand, slot.SlotId), out _);
            }
            finally
            {
                ReleaseInput(slot);
            }
        }

        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Configure Endpoint", code);
            using (Lock.Acquire())
            {
                ReturnPipeMemory(memory);
            }

            return null;
        }

        using (Lock.Acquire())
        {
            slot.InterruptPipes[dci] = pipe;
            pipe.State = XhciPipeState.Running;
            pipe.QueueAll();
            RingDoorbell(slot.SlotId, dci);
        }

        return pipe;
    }

    /// <summary>
    /// Fills the input context of a Configure Endpoint command that adds
    /// the endpoint at <paramref name="endpointId"/>, dropping it first
    /// when <paramref name="dropFlags"/> names it: the input context
    /// cleared, the flags set, the Slot Context copied from the
    /// controller's with its Context Entries raised to cover the endpoint.
    /// The caller writes the Endpoint Context. Thread context, under the
    /// input context claim.
    /// </summary>
    private static void PrepareEndpointInput(XhciSlot slot, byte endpointId, uint dropFlags)
    {
        slot.Memory.ClearInput();
        XhciContext.SetFlags(slot.Memory.InputControl(), dropFlags, XhciContext.SlotFlag | XhciContext.EndpointFlag(endpointId));
        XhciContext.CopySlot(slot.Memory.OutputSlot(), slot.Memory.InputSlot());
        if (XhciContext.GetContextEntries(slot.Memory.InputSlot()) < endpointId)
        {
            XhciContext.SetContextEntries(slot.Memory.InputSlot(), endpointId);
        }
    }

    /// <summary>
    /// Converts bInterval into the Endpoint Context's 2^n x 125 us
    /// encoding. Low and full-speed devices count 1 ms frames, rounded down
    /// to a power of two from exponent 3, capped at 10; faster ones already
    /// use 2^(bInterval-1) microframes.
    /// </summary>
    private static byte InterruptInterval(UsbSpeed speed, byte interval)
    {
        if (speed is UsbSpeed.Low or UsbSpeed.Full)
        {
            int exponent = FullSpeedMinIntervalExponent;
            for (int frames = Math.Max((int)interval, 1); frames > 1; frames >>= 1)
            {
                exponent++;
            }

            return (byte)Math.Min(exponent, FullSpeedMaxIntervalExponent);
        }

        return (byte)(Math.Clamp((int)interval, 1, HighSpeedMaxInterval) - 1);
    }

    // --- Closing pipes ---

    /// <summary>
    /// Closes a pipe, the design's missing controller command, in three
    /// steps. 1, under the lock: a closed pipe returns; an interrupt pipe
    /// goes <see cref="XhciPipeState.Closing"/> so the handler neither
    /// re-queues its buffers nor starts a recovery; a bulk pipe's claim is
    /// awaited and taken. 2, when the slot is connected and the controller
    /// runs (USBSTS.HCH clear: a halted controller, or a PCI function that
    /// vanished and reads all ones, answers no command), whether or not the
    /// host binding detaches: the stop sequence (Stop Endpoint if Running,
    /// Reset Endpoint if Halted, Set TR Dequeue Pointer), then, under the
    /// input context claim, one Configure Endpoint that drops the
    /// endpoint's context with the Slot Context's Context Entries set to the
    /// highest DCI still open. 3, under the lock: the table entry cleared,
    /// the pipe marked closed, its recovery state dropped, its memory
    /// returned to the pool, or, on a disconnected slot, held back on the
    /// slot until its Disable Slot ran (the controller's endpoint context
    /// still names the ring until then). On a disconnected slot or a halted
    /// controller only 1 and 3 run: Disable Slot ends every endpoint of the
    /// slot. Thread context; the body of <see cref="XhciSlot.ClosePipeCore"/>.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="pipe">A pipe this slot opened.</param>
    internal void ClosePipe(XhciSlot slot, UsbPipe pipe)
    {
        XhciInterruptPipe? interruptPipe = pipe as XhciInterruptPipe;
        XhciBulkPipe? bulkPipe = pipe as XhciBulkPipe;
        if (interruptPipe is null && bulkPipe is null)
        {
            return;
        }

        byte dci = interruptPipe is not null ? interruptPipe.EndpointId : bulkPipe!.EndpointId;
        XhciRing ring = interruptPipe is not null ? interruptPipe.Memory.Ring : bulkPipe!.Memory.Ring;

        // 1.
        long deadline = DeadlineAfter(XhciProtocol.BulkTimeoutMs);
        while (true)
        {
            bool claimed;
            using (Lock.Acquire())
            {
                if (pipe.IsClosed)
                {
                    return;
                }

                if (interruptPipe is not null)
                {
                    interruptPipe.State = XhciPipeState.Closing;
                    claimed = true;
                }
                else
                {
                    claimed = !bulkPipe!.Busy;
                    if (claimed)
                    {
                        bulkPipe.Busy = true;
                    }
                }
            }

            if (claimed)
            {
                break;
            }

            // Bounded by the holder's own BulkTimeoutMs: a transfer that
            // outlives it has already been logged by RunBulkTransfer.
            if (Stopwatch.GetTimestamp() >= deadline)
            {
                break;
            }

            _binding.Delay(XhciProtocol.PollMicroseconds);
        }

        // 2.
        if (!slot.IsDisconnected && ControllerRuns)
        {
            StopEndpoint(slot, dci, ring);

            XhciCompletionCode code = XhciCompletionCode.Invalid;
            if (ClaimInput(slot))
            {
                try
                {
                    byte contextEntries;
                    using (Lock.Acquire())
                    {
                        contextEntries = slot.HighestOpenEndpointId(excluding: dci);
                    }

                    slot.Memory.ClearInput();
                    XhciContext.SetFlags(slot.Memory.InputControl(), XhciContext.EndpointFlag(dci), XhciContext.SlotFlag);
                    XhciContext.CopySlot(slot.Memory.OutputSlot(), slot.Memory.InputSlot());
                    XhciContext.SetContextEntries(slot.Memory.InputSlot(), contextEntries);
                    code = ExecuteCommand(slot.Memory.InputContext.PhysicalAddress, SlotCommand(XhciTrbType.ConfigureEndpointCommand, slot.SlotId), out _);
                }
                finally
                {
                    ReleaseInput(slot);
                }
            }

            if (code != XhciCompletionCode.Success)
            {
                LogCommandFailure("Configure Endpoint (close)", code);
            }
        }

        // 3.
        using (Lock.Acquire())
        {
            if (interruptPipe is not null)
            {
                slot.InterruptPipes[dci] = null;
                interruptPipe.MarkClosed();
                interruptPipe.State = XhciPipeState.Stopped;
                interruptPipe.PendingCommand = 0;
                interruptPipe.PendingClearHalt = false;
                if (ReferenceEquals(slot.RecoveringPipe, interruptPipe))
                {
                    slot.RecoveringPipe = null;
                }

                RetireOrReturnPipeMemory(slot, dci, interruptPipe.Memory);
            }
            else
            {
                slot.BulkPipes[dci] = null;
                bulkPipe!.MarkClosed();
                bulkPipe.PendingTrb = 0;
                RetireOrReturnPipeMemory(slot, dci, bulkPipe.Memory);
            }
        }

        _pipesClosed++;
    }

    /// <summary>
    /// Step 3's memory step: a connected slot's set goes back to its pool
    /// (the stop sequence took the endpoint off the ring); a disconnected
    /// slot's is held on the slot until <see cref="ReleaseDeviceCore"/> ran
    /// Disable Slot, since no command ran and the controller's endpoint
    /// context still names the ring. A set already held for the same DCI
    /// was superseded by a later open's Configure Endpoint and is pooled.
    /// Under the lock.
    /// </summary>
    private void RetireOrReturnPipeMemory(XhciSlot slot, byte dci, XhciPipeMemory memory)
    {
        if (!slot.IsDisconnected)
        {
            ReturnPipeMemory(memory);
            return;
        }

        XhciPipeMemory? superseded = slot.RetiredPipeMemory[dci];
        if (superseded is not null)
        {
            ReturnPipeMemory(superseded);
        }

        slot.RetiredPipeMemory[dci] = memory;
    }

    /// <summary>
    /// Leaves an endpoint Stopped with its dequeue pointer past every
    /// abandoned TRB, from whatever state a close, a failed transfer or a
    /// reset found it in: a Running one is stopped (Context State Error
    /// accepted: it halted or stopped meanwhile), a Halted one reset, then
    /// Set TR Dequeue Pointer to the ring's enqueue pointer and cycle (xHCI
    /// 1.2 sections 4.6.8, 4.6.9 and 4.6.10). Every failure is logged and
    /// the sequence goes on; a controller that does not run (halted, or a
    /// function that vanished) gets no command and fails at once. Thread
    /// context; the caller owns the pipe.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="dci">The endpoint's DCI.</param>
    /// <param name="ring">The endpoint's ring.</param>
    /// <returns>True when every step succeeded.</returns>
    private bool StopEndpoint(XhciSlot slot, byte dci, XhciRing ring)
    {
        if (!ControllerRuns)
        {
            return false;
        }

        bool succeeded = true;
        XhciCompletionCode code;
        if (XhciContext.GetEndpointState(slot.Memory.OutputEndpoint(dci)) == XhciEndpointState.Running)
        {
            code = ExecuteCommand(0, EndpointCommand(XhciTrbType.StopEndpointCommand, slot.SlotId, dci), out _);
            if (code is not (XhciCompletionCode.Success or XhciCompletionCode.ContextStateError))
            {
                LogCommandFailure("Stop Endpoint", code);
                succeeded = false;
            }
        }

        if (XhciContext.GetEndpointState(slot.Memory.OutputEndpoint(dci)) == XhciEndpointState.Halted)
        {
            code = ExecuteCommand(0, EndpointCommand(XhciTrbType.ResetEndpointCommand, slot.SlotId, dci), out _);
            if (code != XhciCompletionCode.Success)
            {
                LogCommandFailure("Reset Endpoint", code);
                succeeded = false;
            }
        }

        ulong dequeuePointer;
        using (Lock.Acquire())
        {
            dequeuePointer = ring.EnqueuePointer | (ring.CycleState ? DequeueCycleState : 0);
        }

        code = ExecuteCommand(dequeuePointer, EndpointCommand(XhciTrbType.SetTrDequeuePointerCommand, slot.SlotId, dci), out _);
        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Set TR Dequeue Pointer", code);
            succeeded = false;
        }

        return succeeded;
    }

    // --- Releasing devices ---

    /// <inheritdoc/>
    protected override void ReleaseDeviceCore(UsbDevice device, bool hostPresent)
    {
        if (device is not XhciSlot slot || !ReferenceEquals(slot.Controller, this))
        {
            return;
        }

        // Idempotent; the attach failure paths release a device nobody
        // marked, and the mark wakes every waiter through OnDisconnectedCore.
        slot.MarkDisconnected();

        if (hostPresent)
        {
            // A recovery's CLEAR_FEATURE owns the control ring, and on a
            // pulled device its completion may never come: the Disable Slot
            // below discards the stage, and a completion that still arrives
            // is dropped since both addresses are 0.
            using (Lock.Acquire())
            {
                if (slot.RecoveryStatusTrb != 0)
                {
                    slot.RecoveryStatusTrb = 0;
                    slot.RecoveringPipe = null;
                    for (int dci = 0; dci < slot.InterruptPipes.Length; dci++)
                    {
                        XhciInterruptPipe? pipe = slot.InterruptPipes[dci];
                        if (pipe is null)
                        {
                            continue;
                        }

                        pipe.PendingClearHalt = false;
                    }

                    slot.ControlBusy = false;
                }
            }

            // A thread's wait ends at once on a disconnected slot, so the
            // bound only covers a thread between its claim and its wait.
            long deadline = DeadlineAfter(XhciProtocol.BulkTimeoutMs);
            while (true)
            {
                bool busy;
                using (Lock.Acquire())
                {
                    busy = slot.ControlBusy;
                    for (int dci = 0; dci < slot.BulkPipes.Length && !busy; dci++)
                    {
                        XhciBulkPipe? pipe = slot.BulkPipes[dci];
                        busy = pipe is not null && pipe.Busy;
                    }
                }

                if (!busy)
                {
                    break;
                }

                if (Stopwatch.GetTimestamp() >= deadline)
                {
                    _binding.Log($"slot {slot.SlotId}: a transfer did not release the slot");
                    break;
                }

                _binding.Delay(XhciProtocol.PollMicroseconds);
            }
        }

        // Every open pipe is marked closed: steps 1 and 3 of ClosePipe, the
        // slot being disconnected, so its memory is held on the slot until
        // the Disable Slot below took the endpoints off their rings.
        for (int dci = 0; dci < slot.InterruptPipes.Length; dci++)
        {
            XhciInterruptPipe? pipe = slot.InterruptPipes[dci];
            if (pipe is not null)
            {
                ClosePipe(slot, pipe);
            }
        }

        for (int dci = 0; dci < slot.BulkPipes.Length; dci++)
        {
            XhciBulkPipe? pipe = slot.BulkPipes[dci];
            if (pipe is not null)
            {
                ClosePipe(slot, pipe);
            }
        }

        if (hostPresent)
        {
            ExecuteCommand(0, SlotCommand(XhciTrbType.DisableSlotCommand, slot.SlotId), out _);
        }

        using (Lock.Acquire())
        {
            _slots[slot.SlotId] = null;
            Span<ulong> entries = MemoryMarshal.Cast<byte, ulong>(Dcbaa.Span);
            entries[slot.SlotId] = 0;
            _freeSlotSets.Add(slot.Memory);

            // No endpoint of the slot can fetch a ring any more (the slot is
            // disabled, or no controller is left to fetch): the held pipe
            // memory joins the pools.
            XhciPipeMemory?[] retired = slot.RetiredPipeMemory;
            for (int dci = 0; dci < retired.Length; dci++)
            {
                XhciPipeMemory? memory = retired[dci];
                if (memory is not null)
                {
                    retired[dci] = null;
                    ReturnPipeMemory(memory);
                }
            }
        }
    }
}
