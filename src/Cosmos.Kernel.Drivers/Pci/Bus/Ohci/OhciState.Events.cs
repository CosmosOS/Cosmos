// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Interrupts;
using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// The done queue consumer: the line interrupt handler and the polled
/// drain, the completions of the control, bulk and interrupt endpoints,
/// the root hub status change, the unrecoverable error, and the start of
/// an interrupt pipe recovery. Nothing here logs: the fault report work
/// item logs in thread context what the drain recorded.
/// </summary>
internal sealed partial class OhciState
{
    /// <summary>
    /// The line interrupt's handler: drains the handled sources until none
    /// is set (the I/O APIC entry is edge-triggered, so a bit that lands
    /// during the handler would otherwise never be seen again) and counts
    /// the interrupt. Interrupt context; allocation-free; no lock, since a
    /// lock holder runs with interrupts disabled and the two never overlap.
    /// </summary>
    /// <param name="context">What a handler may do.</param>
    internal void OnInterrupt(InterruptContext context)
    {
        DrainCompletions(context);
        _interruptCount++;
    }

    /// <summary>
    /// Consumes every handled source of HcInterruptStatus: WDH drains the
    /// done queue, then the bits are written back (W1C, after the done
    /// head was consumed, or the controller writes the next head over the
    /// old one), RHSC marks the devices of a vanished port disconnected
    /// and wakes the hot-plug thread, UE stops everything. RHSC may be
    /// level-triggered (set until the port change bits are cleared), so on
    /// an interrupt-driven controller the handler disables it and leaves
    /// the loop mask without it; the hot-plug thread re-enables it after
    /// writing the port bits back. The mask is
    /// <see cref="OhciProtocol.HandledSources"/> whatever the enable
    /// register holds, so the polled drain sees the same bits with every
    /// enable off. Interrupt context from the handler (no lock), or thread
    /// context under the lock from a polled wait, the hot-plug thread and
    /// the quiesce, with <paramref name="context"/> null. Allocation-free.
    /// </summary>
    /// <param name="context">The handler's context, to signal events and schedule the fault report; null on the polled path.</param>
    private void DrainCompletions(InterruptContext? context)
    {
        uint mask = OhciProtocol.HandledSources;
        uint status = Read(OhciProtocol.HcInterruptStatus) & mask;
        for (int pass = 0; status != 0 && pass < OhciProtocol.MaxDrainPasses; pass++)
        {
            if ((status & OhciProtocol.WritebackDoneHead) != 0)
            {
                ProcessDoneQueue(context);
            }

            bool portChange = (status & OhciProtocol.RootHubStatusChange) != 0;
            if (portChange && _hasInterrupt)
            {
                Write(OhciProtocol.HcInterruptDisable, OhciProtocol.RootHubStatusChange);
            }

            Write(OhciProtocol.HcInterruptStatus, status);

            if (portChange)
            {
                HandlePortChangeInHandler(context);
                mask &= ~OhciProtocol.RootHubStatusChange;
            }

            if ((status & OhciProtocol.UnrecoverableError) != 0)
            {
                // The controller stopped itself: nothing more will land.
                HaltOnUnrecoverableError(context);
                return;
            }

            status = Read(OhciProtocol.HcInterruptStatus) & mask;
        }
    }

    /// <summary>
    /// Consumes the done queue: the head read from the HCCA (bit 0, "another
    /// interrupt pending", masked off) and zeroed. The hardware list runs
    /// newest first, and order matters (two reports of one pipe delivered
    /// newest first would swap a press and its release, and a STATUS stage
    /// must not be seen before its DATA stage was recorded), so the list is
    /// reversed in place: each TD's NextTD is read into a local before
    /// anything touches the TD, then overwritten with the previous TD's
    /// address (the controller is done with a retired TD's NextTD once the
    /// list is in the HCCA). An address outside every pool page stops the
    /// walk, and so does a list longer than the pool has slots (one that
    /// revisits a slot would otherwise be walked for ever); both count as
    /// a stray. Each entry is then handed to its owner, oldest first, with
    /// the reversed link read before the owner may re-queue the slot, the
    /// pass bounded the same way. In the handler or under the lock;
    /// allocation-free.
    /// </summary>
    private void ProcessDoneQueue(InterruptContext? context)
    {
        DmaBuffer.ReadBarrier();
        ref OhciHcca hcca = ref Hcca();
        uint head = Volatile.Read(ref hcca.DoneHead) & ~OhciProtocol.DoneHeadInterruptPending;
        Volatile.Write(ref hcca.DoneHead, 0);
        if (head == 0)
        {
            return;
        }

        int limit = _pool.PageCount * OhciProtocol.DescriptorsPerPage;
        int walked = 0;
        uint previous = 0;
        for (uint current = head; current != 0;)
        {
            if (walked++ >= limit || !_pool.TryFindByPhysicalAddress(current, out OhciDescriptorSlot slot, out _, out _))
            {
                _strayDoneEntries++;
                break;
            }

            ref OhciTransferDescriptor td = ref Td(slot);
            uint next = Volatile.Read(ref td.NextTransfer) & OhciProtocol.DescriptorPointerMask;
            td.NextTransfer = previous;
            previous = current;
            current = next;
        }

        walked = 0;
        for (uint current = previous; current != 0 && walked++ < limit;)
        {
            if (!_pool.TryFindByPhysicalAddress(current, out OhciDescriptorSlot slot, out object? owner, out int tag))
            {
                break;
            }

            uint next = Td(slot).NextTransfer;
            HandleRetiredTransfer(slot, owner, tag, context);
            current = next;
        }
    }

    /// <summary>
    /// One retired TD: dropped when its owner no longer waits for the slot
    /// (not in flight, or a generation other than the one stored) and
    /// otherwise handed to the owner's completion path, after every TD of
    /// the same ED queued before it and still in flight. The controller
    /// retires an ED's TDs in queue order, so a done entry proves its
    /// predecessors retired too; some controllers (NVIDIA MCP) occasionally
    /// leave a retired TD off the done queue, and without this take-back
    /// (the rule of Linux ohci-q.c, dl_done_list) the slot would stay in
    /// flight for the life of the set, since a successful transfer never
    /// quiesces: a device set would run out of slots for a data stage, an
    /// interrupt ring would shrink by one per forgotten TD. A taken-back
    /// predecessor is handled as forgotten, oldest first (a press keeps its
    /// place before its release), and counted; only an entry the set still
    /// waits for proves anything, so a stale entry takes nothing back. On
    /// a compliant controller the rule is a no-op: the reversed done list
    /// is handled oldest first, so every predecessor is already free. A
    /// bulk set holds one TD and has no predecessor. A forgotten final TD
    /// (STATUS, a bulk TD, the last TD of a ring) is not covered, as in
    /// Linux, and is recovered by the timeout, quiesce and quarantine path.
    /// Review-only: QEMU links every retired TD onto the done queue. In the
    /// handler or under the lock.
    /// </summary>
    private void HandleRetiredTransfer(OhciDescriptorSlot slot, object? owner, int tag, InterruptContext? context)
    {
        switch (owner)
        {
            case OhciDevice device:
                if (TryRetire(device.Memory, slot, tag, out int stage))
                {
                    for (int earlier = device.Memory.OldestInFlightBefore(stage); earlier >= 0; earlier = device.Memory.OldestInFlightBefore(stage))
                    {
                        device.Memory.TdStates[earlier] = OhciTdState.Free;
                        _forgottenDoneEntries++;
                        HandleControlStage(device, earlier, context, forgotten: true);
                    }

                    HandleControlStage(device, stage, context, forgotten: false);
                }

                break;

            case OhciBulkPipe bulkPipe:
                if (TryRetire(bulkPipe.Memory, slot, tag, out int bulkIndex))
                {
                    HandleBulkCompletion(bulkPipe, bulkIndex, context);
                }

                break;

            case OhciInterruptPipe interruptPipe:
                if (TryRetire(interruptPipe.Memory, slot, tag, out int interruptIndex))
                {
                    // Looked up again every pass: the re-queue of a taken-back
                    // slot gives the old dummy a newer order, so it is never picked.
                    for (int earlier = interruptPipe.Memory.OldestInFlightBefore(interruptIndex); earlier >= 0; earlier = interruptPipe.Memory.OldestInFlightBefore(interruptIndex))
                    {
                        interruptPipe.Memory.TdStates[earlier] = OhciTdState.Free;
                        _forgottenDoneEntries++;
                        HandleInterruptCompletion(interruptPipe, earlier, context, forgotten: true);
                    }

                    HandleInterruptCompletion(interruptPipe, interruptIndex, context, forgotten: false);
                }

                break;

            default:
                _staleDoneEntries++;
                break;
        }
    }

    /// <summary>Takes a slot out of flight when the set still waits for it with the stored generation; counts a stale entry otherwise.</summary>
    private bool TryRetire(OhciDescriptorSet set, OhciDescriptorSlot slot, int tag, out int index)
    {
        index = set.IndexOf(slot);
        if (index < 0 || set.TdStates[index] != OhciTdState.InFlight || set.TdTags[index] != tag)
        {
            _staleDoneEntries++;
            return false;
        }

        set.TdStates[index] = OhciTdState.Free;
        return true;
    }

    /// <summary>
    /// A stage of a device's control transfer: the DATA stage's final CBP
    /// gives the bytes not moved; the STATUS stage, or any stage with an
    /// error (the ED is then halted with the later stages still on it),
    /// completes the transfer for the waiting thread, the result fields
    /// written before the flag. A failure nobody waits for still marks the
    /// endpoint halted, so the next transfer quiesces it first. A stage
    /// taken back as forgotten (retired without a done entry) whose CC the
    /// controller never wrote back reads as a success: NotAccessed there is
    /// a skipped write-back, not an error that would halt the ED or end
    /// the transfer early.
    /// </summary>
    /// <param name="device">The device whose control TD retired.</param>
    /// <param name="index">The retired TD's slot in the device's memory set.</param>
    /// <param name="context">The interrupt context, or null from the polled drain under the lock.</param>
    /// <param name="forgotten">True for a predecessor taken back by <see cref="HandleRetiredTransfer"/>, never for the TD a done entry named.</param>
    private static void HandleControlStage(OhciDevice device, int index, InterruptContext? context, bool forgotten)
    {
        ref OhciTransferDescriptor td = ref Td(device.Memory.TdSlots[index]);
        uint control = Volatile.Read(ref td.Control);
        OhciConditionCode code = ConditionCodeOf(control);
        if (forgotten && code == OhciConditionCode.NotAccessed)
        {
            code = OhciConditionCode.NoError;
        }

        bool pending = device.ControlPending;

        if (pending && index == device.ControlDataIndex)
        {
            uint bufferPointer = td.CurrentBufferPointer;
            device.ControlRemaining = bufferPointer == 0 ? 0 : (int)(td.BufferEnd + 1 - bufferPointer);
        }

        if (code != OhciConditionCode.NoError)
        {
            device.ControlHalted = true;
        }

        if (!pending)
        {
            return;
        }

        if (code != OhciConditionCode.NoError || index == device.ControlStatusIndex)
        {
            device.ControlCode = code;
            device.ControlPending = false;
            Volatile.Write(ref device.ControlCompleted, true);
            Signal(context, device.ControlEvent);
        }
    }

    /// <summary>A bulk pipe's TD: the one the thread waits for completes its transfer; any other is dropped.</summary>
    private static void HandleBulkCompletion(OhciBulkPipe pipe, int index, InterruptContext? context)
    {
        if (pipe.PendingIndex != index)
        {
            return;
        }

        ref OhciTransferDescriptor td = ref Td(pipe.Memory.TdSlots[index]);
        uint control = Volatile.Read(ref td.Control);
        pipe.CompletionCode = ConditionCodeOf(control);
        pipe.CompletionBufferPointer = td.CurrentBufferPointer;
        pipe.PendingIndex = -1;
        Volatile.Write(ref pipe.Completed, true);
        Signal(context, pipe.Event);
    }

    /// <summary>
    /// An interrupt pipe's TD: on a running pipe a good one is delivered
    /// and its buffer re-queued (the retired slot becomes the new dummy,
    /// the old dummy carries the buffer again); a failed one starts the
    /// recovery. A pipe that is closing, recovering or stopped only loses
    /// the slot's in-flight state: its quiesce accounts for the rest. A TD
    /// taken back as forgotten (retired without a done entry) whose CC the
    /// controller never wrote back reads as a success: NotAccessed there is
    /// a skipped write-back, not an error to recover from.
    /// </summary>
    /// <param name="pipe">The pipe whose TD retired.</param>
    /// <param name="index">The retired TD's slot in the pipe's memory set.</param>
    /// <param name="context">The interrupt context, or null from the polled drain under the lock.</param>
    /// <param name="forgotten">True for a predecessor taken back by <see cref="HandleRetiredTransfer"/>, never for the TD a done entry named.</param>
    private void HandleInterruptCompletion(OhciInterruptPipe pipe, int index, InterruptContext? context, bool forgotten)
    {
        OhciPipeMemory memory = pipe.Memory;
        int buffer = pipe.TdBuffer[index];
        pipe.TdBuffer[index] = -1;
        if (pipe.State != OhciPipeState.Running || buffer < 0)
        {
            return;
        }

        ref OhciTransferDescriptor td = ref Td(memory.TdSlots[index]);
        uint control = Volatile.Read(ref td.Control);
        OhciConditionCode code = ConditionCodeOf(control);
        if (forgotten && code == OhciConditionCode.NotAccessed)
        {
            code = OhciConditionCode.NoError;
        }

        if (code != OhciConditionCode.NoError)
        {
            StartPipeRecovery(pipe, code, context);
            return;
        }

        uint bufferPointer = td.CurrentBufferPointer;
        uint start = pipe.BufferAddress(buffer);
        int length = bufferPointer == 0 ? pipe.TransferSize : (int)(bufferPointer - start);
        pipe.ConsecutiveErrors = 0;
        pipe.Deliver(buffer, length);

        int dummy = memory.DummyIndex;
        if (dummy < 0)
        {
            return;
        }

        ZeroTransfer(memory, index);
        FillTransfer(memory, dummy,
            TransferControl(OhciProtocol.DirectionPidIn, OhciProtocol.DataToggleFromCarry, rounding: true, OhciProtocol.DelayInterruptImmediate),
            start, start + (uint)pipe.TransferSize - 1, memory.TdSlots[index].PhysicalAddress);
        pipe.TdBuffer[dummy] = buffer;
        PublishTail(memory, index);
    }

    /// <summary>
    /// The devices behind a root port that lost its connection are marked
    /// disconnected so their waiters fail at once, and the hot-plug thread
    /// is woken to write the change bits back, detach and probe. Only once
    /// the boot scan is done: its own port resets raise RHSC too. In the
    /// handler or under the lock; allocation-free.
    /// </summary>
    private void HandlePortChangeInHandler(InterruptContext? context)
    {
        if (!_rootPortsProbed)
        {
            return;
        }

        for (int i = 1; i <= _portCount; i++)
        {
            byte port = (byte)i;
            uint status = ReadPortStatus(port);
            if ((status & OhciProtocol.ConnectStatusChange) != 0 && (status & OhciProtocol.CurrentConnectStatus) == 0)
            {
                MarkRootPortDisconnected(port);
            }
        }

        Signal(context, _portChangeEvent);
    }

    /// <summary>
    /// The controller raised an unrecoverable error (OHCI 1.0a section
    /// 7.1.4): it is stopped here for good, every interrupt off and HCFS
    /// back to USBReset with RWC preserved (the detach hook's
    /// <see cref="Stop"/> repeats the writes harmlessly), since a
    /// controller that resumed its lists once the status bit was written
    /// back would fetch EDs whose slots the teardown frees and reuses with
    /// no settle; then every wait fails fast, every device is marked
    /// disconnected (allocation-free table walk), the hot-plug thread
    /// leaves the ports alone and the fault report logs it. A later
    /// sighting of the bit, once stopped, counts and logs nothing. Recovery
    /// is out of scope. In the handler or under the lock.
    /// </summary>
    private void HaltOnUnrecoverableError(InterruptContext? context)
    {
        if (!_running)
        {
            return;
        }

        Write(OhciProtocol.HcInterruptDisable, OhciProtocol.AllInterrupts);
        Write(OhciProtocol.HcControl, (Read(OhciProtocol.HcControl) & OhciProtocol.RemoteWakeupConnected) | OhciProtocol.FunctionalStateReset);
        _running = false;
        _hostControllerEvents++;
        for (int address = 1; address < _devices.Length; address++)
        {
            _devices[address]?.MarkDisconnected();
        }

        _hostEventPending = true;
        Schedule(context, _faultReport);
        Signal(context, _portChangeEvent);
    }

    // --- Interrupt pipe recovery ---

    /// <summary>
    /// First step of an interrupt pipe's recovery after a transfer error:
    /// the fault recorded for the report; a disconnected device or one
    /// error past <see cref="OhciProtocol.MaxPipeRecoveries"/> stops the
    /// pipe, as does a controller without a hot-plug thread (nobody would
    /// recover it, and the kit worker must not wait behind USB budgets);
    /// otherwise the pipe goes <see cref="OhciPipeState.Recovering"/>,
    /// the recovery is counted and the hot-plug thread is asked for a
    /// recovery pass. The ED is halted by the controller, so nothing runs
    /// on it meanwhile. In the handler or under the lock.
    /// </summary>
    private void StartPipeRecovery(OhciInterruptPipe pipe, OhciConditionCode code, InterruptContext? context)
    {
        OhciDevice? device = DeviceOf(pipe);
        if (device is null || device.IsDisconnected)
        {
            StopPipe(pipe);
            return;
        }

        _transferFaultAddress = device.Address;
        _transferFaultEndpoint = pipe.Endpoint.Address;
        _transferFaultCode = code;
        _transferFaultPending = true;
        Schedule(context, _faultReport);

        pipe.ConsecutiveErrors++;
        if (pipe.ConsecutiveErrors > OhciProtocol.MaxPipeRecoveries || !_hotPlugRunning)
        {
            _pipeStoppedAddress = device.Address;
            _pipeStoppedEndpoint = pipe.Endpoint.Address;
            _pipeStoppedPending = true;
            Schedule(context, _faultReport);
            StopPipe(pipe);
            return;
        }

        _pipeRecoveries++;
        pipe.State = OhciPipeState.Recovering;
        pipe.StalledByDevice = code == OhciConditionCode.Stall;
        _recoveryRequested = true;
        Signal(context, _portChangeEvent);
    }

    /// <summary>Stops a pipe for good and counts it. Any context.</summary>
    private void StopPipe(OhciInterruptPipe pipe)
    {
        pipe.State = OhciPipeState.Stopped;
        _pipesStopped++;
    }

    /// <summary>The device an interrupt pipe is open on, found by the pipe tables. In the handler or under the lock; allocation-free.</summary>
    private OhciDevice? DeviceOf(OhciInterruptPipe pipe)
    {
        for (int address = 1; address < _devices.Length; address++)
        {
            OhciDevice? device = _devices[address];
            if (device is not null && ReferenceEquals(device.InterruptPipes[pipe.Index], pipe))
            {
                return device;
            }
        }

        return null;
    }

    /// <summary>
    /// The fault report: logs, in thread context on the kit worker, what the
    /// drain recorded and could not log itself: the last transfer error
    /// that started a recovery, the last pipe stopped for good and the
    /// unrecoverable error.
    /// </summary>
    private void ReportFaults()
    {
        if (_transferFaultPending)
        {
            _transferFaultPending = false;
            _binding.Log($"address {_transferFaultAddress} endpoint {_transferFaultEndpoint}: transfer error, condition code {(uint)_transferFaultCode}");
        }

        if (_pipeStoppedPending)
        {
            _pipeStoppedPending = false;
            _binding.Log($"address {_pipeStoppedAddress} endpoint {_pipeStoppedEndpoint}: pipe stopped");
        }

        if (_hostEventPending)
        {
            _hostEventPending = false;
            _binding.Log("unrecoverable error, controller stopped");
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
