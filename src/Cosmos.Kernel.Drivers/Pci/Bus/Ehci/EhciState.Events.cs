// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Interrupts;
using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// The status consumer: the line interrupt handler and the polled
/// service of USBSTS, the drain that reads every pending descriptor back
/// (the control chains, the bulk qTDs, the interrupt rings with their
/// re-queue), the port change path of the handler, the host system error
/// and the fault report. Nothing here logs: the fault report work item
/// logs in thread context what the handler recorded.
/// </summary>
internal sealed partial class EhciState
{
    /// <summary>
    /// The line interrupt's handler: services USBSTS until no handled bit
    /// is set, since the I/O APIC entry is edge-triggered and a bit that
    /// stays set after the acknowledge would never be seen again, and
    /// counts the run. Interrupt context; allocation-free; no lock, since
    /// a lock holder runs with interrupts disabled and the two never
    /// overlap.
    /// </summary>
    /// <param name="context">What a handler may do.</param>
    internal void OnInterrupt(InterruptContext context)
    {
        ServiceStatus(context);
        _interruptCount++;
    }

    /// <summary>
    /// Reads USBSTS, writes the handled bits back (RW1C) and acts on them,
    /// again until none is set: a port change marks the devices of a root
    /// port that lost its connection disconnected (only once the boot scan
    /// is done: its own resets raise port changes too); USBINT and
    /// USBERRINT drain the completions; IAA completes the doorbell wait
    /// (the async schedule idles while IAA is set, so it is cleared as soon
    /// as it is seen); HSE halts everything. A register that reads all
    /// ones belongs to a function that vanished. Interrupt context from
    /// the handler (no lock), or thread context under the lock from a
    /// polled wait and the hot-plug thread, with <paramref name="context"/>
    /// null. Allocation-free.
    /// </summary>
    /// <param name="context">The handler's context, to signal events and schedule the fault report; null on the polled path.</param>
    private void ServiceStatus(InterruptContext? context)
    {
        uint raw = ReadOperational(EhciProtocol.UsbSts);
        uint status = raw & EhciProtocol.InterruptMask;
        while (status != 0)
        {
            if (raw == uint.MaxValue)
            {
                HaltOnHostError(context);
                return;
            }

            WriteOperational(EhciProtocol.UsbSts, status);

            if ((status & EhciProtocol.UsbStsPortChangeDetect) != 0 && _rootPortsProbed)
            {
                HandlePortChangeInHandler(context);
            }

            if ((status & (EhciProtocol.UsbStsInterrupt | EhciProtocol.UsbStsErrorInterrupt)) != 0)
            {
                DrainCompletions(context);
            }

            if ((status & EhciProtocol.UsbStsAsyncAdvance) != 0)
            {
                Volatile.Write(ref _doorbellCompleted, true);
                Signal(context, _doorbellEvent);
            }

            if ((status & EhciProtocol.UsbStsHostSystemError) != 0)
            {
                HaltOnHostError(context);
                return;
            }

            raw = ReadOperational(EhciProtocol.UsbSts);
            status = raw & EhciProtocol.InterruptMask;
        }
    }

    /// <summary>
    /// The handler's port change path: every root port the controller
    /// owns whose connection changed and is gone has its devices marked
    /// disconnected, so their waiters fail at once; the change bits are
    /// left for the hot-plug thread, which is signalled. In the handler or
    /// under the lock; allocation-free.
    /// </summary>
    private void HandlePortChangeInHandler(InterruptContext? context)
    {
        for (int i = 1; i <= _portCount; i++)
        {
            byte port = (byte)i;
            uint portsc = ReadPortSc(port);
            if ((portsc & EhciProtocol.PortOwner) != 0)
            {
                continue;
            }

            if ((portsc & EhciProtocol.PortConnectChange) != 0 && (portsc & EhciProtocol.PortConnected) == 0)
            {
                MarkRootPortDisconnected(port);
            }
        }

        Signal(context, _portChangeEvent);
    }

    /// <summary>
    /// Reads every pending descriptor back: for each device, the control
    /// chain a thread waits on, the bulk qTD each pipe has in flight and
    /// the completed qTDs at the head of each running interrupt pipe's
    /// ring. Controller-written fields are read through <c>Volatile</c>
    /// after a read barrier; a completion's result fields are written
    /// before its flag. In the handler or under the lock; allocation-free.
    /// </summary>
    /// <param name="context">The handler's context, or null on the polled path.</param>
    private void DrainCompletions(InterruptContext? context)
    {
        if (!_running)
        {
            return;
        }

        for (int address = 1; address < _devices.Length; address++)
        {
            EhciDevice? device = _devices[address];
            if (device is null)
            {
                continue;
            }

            DrainControl(device, context);

            EhciBulkPipe?[] bulkPipes = device.BulkPipes;
            for (int id = 0; id < bulkPipes.Length; id++)
            {
                EhciBulkPipe? pipe = bulkPipes[id];
                if (pipe is not null)
                {
                    DrainBulk(pipe, context);
                }
            }

            EhciInterruptPipe?[] interruptPipes = device.InterruptPipes;
            for (int id = 0; id < interruptPipes.Length; id++)
            {
                EhciInterruptPipe? pipe = interruptPipes[id];
                if (pipe is not null && pipe.State == EhciPipeState.Running)
                {
                    DrainInterrupt(device, pipe, context);
                }
            }
        }
    }

    /// <summary>
    /// A device's control chain in flight: complete when any of its qTDs
    /// is halted (the chain stopped there and the status is that qTD's)
    /// or when the last one, the STATUS stage, is no longer active. The
    /// finishing qTD's whole token (the status mapping reads CERR as well
    /// as the status byte) and the DATA stage's remaining bytes are
    /// recorded, the chain dropped, the flag set and the event signalled.
    /// </summary>
    private void DrainControl(EhciDevice device, InterruptContext? context)
    {
        int count = device.ControlPendingCount;
        if (count == 0)
        {
            return;
        }

        EhciDescriptorSlot[] slots = device.Memory.Transfers;
        int first = device.ControlFirstSlot;
        int slotCount = slots.Length;

        DmaBuffer.ReadBarrier();
        uint finishing = 0;
        bool done = false;
        for (int i = 0; i < count; i++)
        {
            uint token = Volatile.Read(ref slots[(first + i) % slotCount].TransferDescriptor.Token);
            if ((token & EhciProtocol.TokenHalted) != 0)
            {
                finishing = token;
                done = true;
                break;
            }

            if (i == count - 1 && (token & EhciProtocol.TokenActive) == 0)
            {
                finishing = token;
                done = true;
            }
        }

        if (!done)
        {
            return;
        }

        int remaining = 0;
        if (device.ControlHasData)
        {
            uint dataToken = Volatile.Read(ref slots[(first + 1) % slotCount].TransferDescriptor.Token);
            remaining = RemainingBytes(dataToken);
        }

        device.ControlStatus = finishing;
        device.ControlRemaining = remaining;
        device.ControlPendingCount = 0;
        Volatile.Write(ref device.ControlCompleted, true);
        Signal(context, device.ControlEvent);
    }

    /// <summary>A bulk pipe's qTD in flight: complete when it is no longer active (a halt clears Active too). The token is recorded, the flag set and the event signalled.</summary>
    private void DrainBulk(EhciBulkPipe pipe, InterruptContext? context)
    {
        if (!pipe.PendingTransfer)
        {
            return;
        }

        DmaBuffer.ReadBarrier();
        uint token = Volatile.Read(ref pipe.Memory.Transfers[pipe.TransferSlot].TransferDescriptor.Token);
        if ((token & EhciProtocol.TokenActive) != 0)
        {
            return;
        }

        pipe.CompletionToken = token;
        pipe.PendingTransfer = false;
        Volatile.Write(ref pipe.Completed, true);
        Signal(context, pipe.Event);
    }

    /// <summary>
    /// A running interrupt pipe's ring: every retired qTD at its head, in
    /// order, is delivered and its buffer re-queued with the dummy
    /// technique, until the head is active again; a halted qTD starts the
    /// recovery instead. A retired qTD that only carries a sticky error
    /// bit from a retried transaction (Table 3-16) completed and is
    /// delivered.
    /// </summary>
    private void DrainInterrupt(EhciDevice device, EhciInterruptPipe pipe, InterruptContext? context)
    {
        EhciDescriptorSlot[] slots = pipe.Memory.Transfers;
        for (int processed = 0; processed < pipe.BufferCount && pipe.State == EhciPipeState.Running; processed++)
        {
            int head = pipe.Head;
            DmaBuffer.ReadBarrier();
            uint token = Volatile.Read(ref slots[head].TransferDescriptor.Token);
            if ((token & EhciProtocol.TokenActive) != 0)
            {
                return;
            }

            if ((token & EhciProtocol.TokenHalted) != 0)
            {
                StartPipeRecovery(device, pipe, token, context);
                return;
            }

            DmaBuffer.ReadBarrier();
            pipe.ConsecutiveErrors = 0;
            int length = pipe.TransferSize - Math.Min(RemainingBytes(token), pipe.TransferSize);
            pipe.Deliver(pipe.BufferOf(head), length);
            RequeueInterruptBuffer(pipe, head);
        }
    }

    /// <summary>
    /// Re-queues a completed interrupt qTD's buffer with the dummy
    /// technique (EHCI 1.0 section 4.10.2): the completed slot is zeroed
    /// as the new dummy, the old dummy is filled with the completed
    /// buffer's transfer pointing at it, a write barrier, then the filled
    /// qTD's token is written Active last. The chain rotates through the
    /// slots in index order: the head advances to the next slot. In the
    /// handler or under the lock; allocation-free.
    /// </summary>
    private static void RequeueInterruptBuffer(EhciInterruptPipe pipe, int completedSlot)
    {
        EhciDescriptorSlot[] slots = pipe.Memory.Transfers;
        int buffer = pipe.BufferOf(completedSlot);
        int dummy = pipe.Dummy;
        int ringLength = pipe.BufferCount + 1;

        slots[completedSlot].Clear();

        ref EhciTransferDescriptor transfer = ref slots[dummy].TransferDescriptor;
        FillTransfer(ref transfer, slots[completedSlot].TransferPointer, EhciProtocol.LinkTerminate, pipe.BufferAddress(buffer), pipe.TransferSize);
        pipe.SetBufferOf(dummy, buffer);
        pipe.Dummy = completedSlot;
        pipe.Head = (completedSlot + 1) % ringLength;

        DmaBuffer.WriteBarrier();
        Volatile.Write(ref transfer.Token, TransferToken(EhciProtocol.PidIn, pipe.TransferSize, dataToggle: false, interruptOnComplete: true));
    }

    // --- Interrupt pipe recovery ---

    /// <summary>
    /// First step of an interrupt pipe's recovery after a transfer error:
    /// a disconnected device stops the pipe quietly; otherwise the fault
    /// is recorded for the report, the error counted, and the pipe stopped
    /// past <see cref="EhciProtocol.MaxPipeRecoveries"/> or when no
    /// hot-plug thread exists to recover it, else counted as a recovery,
    /// marked recovering with whether the device STALLed, and the hot-plug
    /// thread asked for. In the handler or under the lock.
    /// </summary>
    private void StartPipeRecovery(EhciDevice device, EhciInterruptPipe pipe, uint token, InterruptContext? context)
    {
        if (device.IsDisconnected)
        {
            pipe.State = EhciPipeState.Stopped;
            return;
        }

        _transferFaultAddress = device.Address;
        _transferFaultEndpoint = pipe.EndpointId;
        _transferFaultToken = token;
        _transferFaultPending = true;
        Schedule(context, _faultReport);

        pipe.ConsecutiveErrors++;
        if (pipe.ConsecutiveErrors > EhciProtocol.MaxPipeRecoveries || !_hotPlugRunning)
        {
            StopPipeFromDrain(device, pipe, context);
            return;
        }

        _pipeRecoveries++;
        pipe.StalledByDevice = IsStall(token);
        pipe.State = EhciPipeState.Recovering;
        _recoveryRequested = true;
        Signal(context, _portChangeEvent);
    }

    /// <summary>Stops an interrupt pipe for good from the drain: counted, and recorded for the report to log. In the handler or under the lock.</summary>
    private void StopPipeFromDrain(EhciDevice device, EhciInterruptPipe pipe, InterruptContext? context)
    {
        pipe.State = EhciPipeState.Stopped;
        _pipesStopped++;
        _pipeStopAddress = device.Address;
        _pipeStopEndpoint = pipe.EndpointId;
        _pipeStopPending = true;
        Schedule(context, _faultReport);
    }

    // --- Host system error ---

    /// <summary>
    /// A host system error, or a function that reads all ones: the
    /// controller halted itself (EHCI 1.0 section 4.15.2.4). Counted, the
    /// controller marked not running so every wait fails at once and the
    /// hot-plug thread leaves the ports alone, every device marked
    /// disconnected (which wakes its waiters), every running interrupt
    /// pipe stopped, the doorbell and port change waiters woken and the
    /// report scheduled. Recovery is out of scope. In the handler or under
    /// the lock; allocation-free.
    /// </summary>
    private void HaltOnHostError(InterruptContext? context)
    {
        if (!_running)
        {
            return;
        }

        _running = false;
        _hostControllerEvents++;

        for (int address = 1; address < _devices.Length; address++)
        {
            EhciDevice? device = _devices[address];
            if (device is null)
            {
                continue;
            }

            device.MarkDisconnected();
            EhciInterruptPipe?[] pipes = device.InterruptPipes;
            for (int id = 0; id < pipes.Length; id++)
            {
                EhciInterruptPipe? pipe = pipes[id];
                if (pipe is not null && pipe.State is EhciPipeState.Running or EhciPipeState.Recovering)
                {
                    pipe.State = EhciPipeState.Stopped;
                    _pipesStopped++;
                }
            }
        }

        _hostErrorPending = true;
        Schedule(context, _faultReport);
        Signal(context, _doorbellEvent);
        Signal(context, _portChangeEvent);
    }

    // --- The fault report ---

    /// <summary>
    /// The fault report: logs, in thread context on the kit worker, what
    /// the handler recorded and could not log itself: the last transfer
    /// error that started a recovery, the last pipe stopped from the
    /// drain and the host system error.
    /// </summary>
    private void ReportFaults()
    {
        if (_transferFaultPending)
        {
            _transferFaultPending = false;
            _binding.Log($"address {_transferFaultAddress} endpoint {_transferFaultEndpoint}: transfer error, token status 0x{_transferFaultToken & EhciProtocol.TokenStatusMask:x}");
        }

        if (_pipeStopPending)
        {
            _pipeStopPending = false;
            _binding.Log($"address {_pipeStopAddress} endpoint {_pipeStopEndpoint}: pipe stopped");
        }

        if (_hostErrorPending)
        {
            _hostErrorPending = false;
            _binding.Log("host system error, controller halted");
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
