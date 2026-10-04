// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The event ring consumer: the message interrupt handler and the polled
/// drain, the command completions, the transfer events of the control,
/// bulk and interrupt endpoints, the port status changes, the host
/// controller events, and the interrupt pipe recovery state machine driven
/// by command completions. Nothing here logs: the fault report work item
/// logs in thread context what the handler recorded.
/// </summary>
public sealed partial class XhciState
{
    /// <summary>
    /// The message interrupt's handler: acknowledges USBSTS.EINT and
    /// IMAN.IP (both RW1C; IE written back as set), counts the interrupt
    /// and drains the event ring. Interrupt context; allocation-free; no
    /// lock, since a lock holder runs with interrupts disabled and the two
    /// never overlap.
    /// </summary>
    /// <param name="context">What a handler may do.</param>
    internal void OnInterrupt(InterruptContext context)
    {
        WriteOperational(XhciProtocol.UsbSts, XhciProtocol.UsbStsEventInterrupt);
        WriteInterrupter(XhciProtocol.Iman, XhciProtocol.ImanInterruptEnable | XhciProtocol.ImanInterruptPending);
        _interruptCount++;
        DrainEvents(context);
    }

    /// <summary>
    /// Consumes every pending event, then moves ERDP past them with EHB
    /// set. Interrupt context from the handler (no lock), or thread context
    /// under the lock from a polled wait and the hot-plug thread, with
    /// <paramref name="context"/> null. Allocation-free.
    /// </summary>
    /// <param name="context">The handler's context, to signal events and schedule the fault report; null on the polled path.</param>
    private void DrainEvents(InterruptContext? context)
    {
        XhciEventRing? eventRing = _eventRing;
        if (eventRing is null)
        {
            return;
        }

        bool consumed = false;
        while (eventRing.TryDequeue(out XhciTrb trb))
        {
            consumed = true;
            switch (trb.Type)
            {
                case XhciTrbType.CommandCompletionEvent:
                    HandleCommandCompletion(trb, context);
                    break;

                case XhciTrbType.TransferEvent:
                    HandleTransferEvent(trb, context);
                    break;

                case XhciTrbType.PortStatusChangeEvent:
                    // Port resets during the boot probe raise these too;
                    // only a change after it is a (re)plug. The port itself
                    // is handled on the hot-plug thread.
                    if (_rootPortsProbed)
                    {
                        MarkRootPortDisconnected(trb.PortId);
                        Signal(context, _portChangeEvent);
                    }

                    break;

                case XhciTrbType.HostControllerEvent:
                    _hostControllerEvents++;
                    _lastHostControllerEventCode = (byte)trb.CompletionCode;
                    _hostEventCode = trb.CompletionCode;
                    _hostEventPending = true;
                    Schedule(context, _faultReport);
                    break;
            }
        }

        if (consumed)
        {
            WriteInterrupter64(XhciProtocol.Erdp, eventRing.DequeuePointer | XhciProtocol.ErdpEventHandlerBusy);
        }
    }

    /// <summary>
    /// A Command Completion Event: the synchronous command's when the TRB
    /// address matches <see cref="_pendingCommand"/>; else the recovery
    /// step of the interrupt pipe whose <see cref="XhciInterruptPipe.PendingCommand"/>
    /// matches; a completion matching neither is dropped (a pipe closed
    /// while its command was on the ring left its address 0).
    /// </summary>
    private void HandleCommandCompletion(XhciTrb trb, InterruptContext? context)
    {
        if (_pendingCommand != 0 && trb.Parameter == _pendingCommand)
        {
            _commandCode = trb.CompletionCode;
            _commandSlotId = trb.SlotId;
            Volatile.Write(ref _commandCompleted, true);
            Signal(context, _commandEvent);
            return;
        }

        XhciSlot? slot = trb.SlotId < _slots.Length ? _slots[trb.SlotId] : null;
        if (slot is null)
        {
            return;
        }

        XhciInterruptPipe?[] pipes = slot.InterruptPipes;
        for (int dci = 0; dci < pipes.Length; dci++)
        {
            XhciInterruptPipe? pipe = pipes[dci];
            if (pipe is not null && pipe.PendingCommand != 0 && pipe.PendingCommand == trb.Parameter)
            {
                ContinuePipeRecovery(slot, pipe, trb.CompletionCode);
                return;
            }
        }
    }

    /// <summary>
    /// A Transfer Event. An unknown slot is dropped. DCI 1, in order: (a)
    /// while a recovery's CLEAR_FEATURE owns the control ring, its Status
    /// Stage or any failure ends it, finishes the recovering pipe and runs
    /// the hand-off check; (b) else the thread's Status Stage or any failure
    /// completes the thread's transfer; (c) else a failure halts the
    /// endpoint for the next transfer to recover. A bulk pipe's pending TRB
    /// completes its transfer. An interrupt pipe's buffer is delivered and
    /// re-queued, or its recovery started on an error other than a stop.
    /// </summary>
    private void HandleTransferEvent(XhciTrb trb, InterruptContext? context)
    {
        if (trb.SlotId >= _slots.Length || _slots[trb.SlotId] is not XhciSlot slot)
        {
            return;
        }

        XhciCompletionCode code = trb.CompletionCode;
        bool succeeded = code is XhciCompletionCode.Success or XhciCompletionCode.ShortPacket;
        if (trb.EndpointId == XhciSlot.ControlEndpointId)
        {
            HandleControlEvent(slot, trb, succeeded, context);
            return;
        }

        if (trb.EndpointId > XhciSlot.MaxEndpointId)
        {
            return;
        }

        XhciBulkPipe? bulkPipe = slot.BulkPipes[trb.EndpointId];
        if (bulkPipe is not null)
        {
            // Events for a TRB nobody waits on any more (a Stop Endpoint
            // after a timeout reports the abandoned one) are dropped.
            if (bulkPipe.PendingTrb != 0 && trb.Parameter == bulkPipe.PendingTrb)
            {
                bulkPipe.CompletionCode = code;
                bulkPipe.ResidualLength = trb.ResidualLength;
                Volatile.Write(ref bulkPipe.Completed, true);
                Signal(context, bulkPipe.Event);
            }

            return;
        }

        XhciInterruptPipe? pipe = slot.InterruptPipes[trb.EndpointId];
        if (pipe is null || pipe.State != XhciPipeState.Running || !pipe.TryGetBuffer(trb.Parameter, out ulong buffer))
        {
            return;
        }

        if (succeeded)
        {
            pipe.ConsecutiveErrors = 0;
            pipe.Deliver(buffer, trb.ResidualLength);
            pipe.Queue(buffer);
            RingDoorbell(slot.SlotId, pipe.EndpointId);
            return;
        }

        if (code is not (XhciCompletionCode.Stopped or XhciCompletionCode.StoppedLengthInvalid or XhciCompletionCode.StoppedShortPacket))
        {
            StartPipeRecovery(slot, pipe, code, buffer, context);
        }
    }

    /// <summary>The DCI 1 rules (a), (b) and (c) of <see cref="HandleTransferEvent"/>.</summary>
    private void HandleControlEvent(XhciSlot slot, XhciTrb trb, bool succeeded, InterruptContext? context)
    {
        // (a) A recovery's CLEAR_FEATURE owns the control ring, so no
        //     thread's transfer is on it. An error ends the TD on the failing
        //     Setup or Data Stage and its Status Stage never completes, which
        //     is why a failure on any TRB counts (xHCI 1.2 sections 4.10.2
        //     and 4.11.2.2). A success for another TRB is dropped.
        if (slot.RecoveryStatusTrb != 0)
        {
            if (trb.Parameter == slot.RecoveryStatusTrb || !succeeded)
            {
                slot.RecoveryStatusTrb = 0;
                if (!succeeded)
                {
                    slot.ControlEndpointHalted = true;
                }

                XhciInterruptPipe? recovering = slot.RecoveringPipe;
                if (recovering is not null && recovering.State == XhciPipeState.SettingDequeuePointer)
                {
                    if (succeeded)
                    {
                        recovering.State = XhciPipeState.Running;
                        recovering.QueueAll();
                        RingDoorbell(slot.SlotId, recovering.EndpointId);
                    }
                    else
                    {
                        recovering.State = XhciPipeState.Stopped;
                    }
                }

                slot.RecoveringPipe = null;
                HandOffControlRing(slot);
            }

            return;
        }

        // (b) The thread's transfer: its Status Stage, or any failure.
        if (slot.ControlStatusTrb != 0 && (trb.Parameter == slot.ControlStatusTrb || !succeeded))
        {
            slot.ControlCode = trb.CompletionCode;
            Volatile.Write(ref slot.ControlCompleted, true);
            Signal(context, slot.ControlEvent);
            return;
        }

        // (c) A failure nobody waits on: the next transfer recovers the
        //     endpoint first. A success nobody waits on is dropped.
        if (!succeeded)
        {
            slot.ControlEndpointHalted = true;
        }
    }

    // --- Interrupt pipe recovery ---

    /// <summary>
    /// First step of an interrupt pipe's recovery after a transfer error:
    /// counted, the fault recorded for the report; a disconnected slot or
    /// one error past <see cref="XhciProtocol.MaxPipeRecoveries"/> stops
    /// the pipe; otherwise Reset Endpoint goes on the command ring directly
    /// (no claim: a recovery command rides beside a synchronous one and is
    /// told apart by its TRB address) and the pipe waits in
    /// <see cref="XhciPipeState.ResettingEndpoint"/>. In the handler or
    /// under the lock.
    /// </summary>
    private void StartPipeRecovery(XhciSlot slot, XhciInterruptPipe pipe, XhciCompletionCode code, ulong failedBuffer, InterruptContext? context)
    {
        _pipeRecoveries++;
        if (slot.IsDisconnected)
        {
            pipe.State = XhciPipeState.Stopped;
            return;
        }

        _transferFaultSlot = slot.SlotId;
        _transferFaultEndpoint = pipe.EndpointId;
        _transferFaultCode = code;
        _transferFaultPending = true;
        Schedule(context, _faultReport);

        pipe.ConsecutiveErrors++;
        if (pipe.ConsecutiveErrors > XhciProtocol.MaxPipeRecoveries)
        {
            pipe.State = XhciPipeState.Stopped;
            return;
        }

        pipe.State = XhciPipeState.ResettingEndpoint;
        pipe.FailedBuffer = failedBuffer;
        pipe.StalledByDevice = code == XhciCompletionCode.StallError;
        QueuePipeCommand(pipe, 0, EndpointCommand(XhciTrbType.ResetEndpointCommand, slot.SlotId, pipe.EndpointId));
    }

    /// <summary>
    /// The next step of a recovery, on its command's completion. A pipe
    /// closed meanwhile only loses its pending address. Reset Endpoint
    /// done: Context State Error means the endpoint was not halted and the
    /// failed buffer alone goes back; success moves to Set TR Dequeue
    /// Pointer. That done: a stalled pipe needs CLEAR_FEATURE on the
    /// control ring, taken at once when free and not halted, else waited
    /// for with <see cref="XhciInterruptPipe.PendingClearHalt"/> until the
    /// holder's hand-off check; any other pipe runs again. A failed command stops
    /// the pipe, as does a slot that disconnected meanwhile. In the handler
    /// or under the lock.
    /// </summary>
    private void ContinuePipeRecovery(XhciSlot slot, XhciInterruptPipe pipe, XhciCompletionCode code)
    {
        pipe.PendingCommand = 0;
        if (pipe.State is XhciPipeState.Closing or XhciPipeState.Stopped)
        {
            return;
        }

        // A device that left fails every transfer; there is nothing to recover.
        if (slot.IsDisconnected)
        {
            pipe.State = XhciPipeState.Stopped;
            return;
        }

        switch (pipe.State)
        {
            case XhciPipeState.ResettingEndpoint:
                if (code == XhciCompletionCode.ContextStateError)
                {
                    pipe.State = XhciPipeState.Running;
                    pipe.Queue(pipe.FailedBuffer);
                    RingDoorbell(slot.SlotId, pipe.EndpointId);
                    return;
                }

                if (code != XhciCompletionCode.Success)
                {
                    pipe.State = XhciPipeState.Stopped;
                    return;
                }

                pipe.State = XhciPipeState.SettingDequeuePointer;
                XhciRing ring = pipe.Memory.Ring;
                QueuePipeCommand(pipe, ring.EnqueuePointer | (ring.CycleState ? DequeueCycleState : 0),
                    EndpointCommand(XhciTrbType.SetTrDequeuePointerCommand, slot.SlotId, pipe.EndpointId));
                return;

            case XhciPipeState.SettingDequeuePointer:
                if (code != XhciCompletionCode.Success)
                {
                    pipe.State = XhciPipeState.Stopped;
                    return;
                }

                // A STALL means the device halted its endpoint as well: the
                // pipe stays here until the CLEAR_FEATURE completes. A
                // halted control endpoint cannot carry it: the pipe waits
                // for the next thread transfer, which recovers the endpoint
                // first and hands the ring over from its finally.
                if (pipe.StalledByDevice)
                {
                    if (!slot.ControlBusy && !slot.ControlEndpointHalted)
                    {
                        slot.ControlBusy = true;
                        StartClearHalt(slot, pipe);
                    }
                    else
                    {
                        pipe.PendingClearHalt = true;
                    }

                    return;
                }

                pipe.State = XhciPipeState.Running;
                pipe.QueueAll();
                RingDoorbell(slot.SlotId, pipe.EndpointId);
                return;
        }
    }

    /// <summary>Puts a recovery command on the command ring, keeps its address on the pipe and rings doorbell 0. In the handler or under the lock.</summary>
    private void QueuePipeCommand(XhciInterruptPipe pipe, ulong parameter, uint control)
    {
        pipe.PendingCommand = CommandRing.Enqueue(parameter, 0, control);
        RingDoorbell(0, 0);
    }

    /// <summary>
    /// The fault report: logs, in thread context on the kit worker, what the
    /// handler recorded and could not log itself, the last transfer error
    /// that started a recovery and the last host controller event.
    /// </summary>
    private void ReportFaults()
    {
        if (_transferFaultPending)
        {
            _transferFaultPending = false;
            _binding.Log($"slot {_transferFaultSlot} endpoint {_transferFaultEndpoint}: transfer error, completion code {(uint)_transferFaultCode}");
        }

        if (_hostEventPending)
        {
            _hostEventPending = false;
            _binding.Log($"host controller event, completion code {(uint)_hostEventCode}");
        }
    }

    /// <summary>Signals an event from the handler through its context, or directly in thread context. Any context.</summary>
    private static void Signal(InterruptContext? context, DeviceEvent evt)
    {
        if (context is null)
        {
            evt.Signal();
        }
        else
        {
            context.Signal(evt);
        }
    }

    /// <summary>Schedules a work item from the handler through its context, or directly in thread context. Any context.</summary>
    private static void Schedule(InterruptContext? context, WorkItem item)
    {
        if (context is null)
        {
            item.Schedule();
        }
        else
        {
            context.Schedule(item);
        }
    }
}
