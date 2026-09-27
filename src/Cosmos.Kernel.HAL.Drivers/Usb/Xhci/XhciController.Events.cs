// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Pipes;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Rings;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci;

internal sealed partial class XhciController
{
    /// <summary>Recoveries of an interrupt pipe without a good transfer in between before it is given up.</summary>
    private const int MaxPipeRecoveries = 3;

    /// <summary>ENDPOINT_HALT feature selector (USB 2.0 table 9-6).</summary>
    private const ushort EndpointHaltFeature = 0;

    /// <summary>CLEAR_FEATURE (USB 2.0 table 9-4).</summary>
    private const byte ClearFeatureRequest = 0x01;

    /// <summary>
    /// Interrupter 0's handler, which the kit calls on the MSI-X message or
    /// from the timer. Allocation-free.
    /// </summary>
    private void OnInterrupt(int vector)
    {
        using (_eventLock.EnterScope())
        {
            // Both are RW1C; IMAN.IE has to be written back as set.
            _registers.UsbSts = UsbStsEventInterrupt;
            _registers.Iman = ImanInterruptEnable | ImanInterruptPending;
            DrainEvents();
        }
    }

    /// <summary>Consumes every pending event, then moves ERDP past them. The caller holds the event lock.</summary>
    private void DrainEvents()
    {
        bool consumed = false;
        while (_eventRing.TryDequeue(out Trb trb))
        {
            consumed = true;
            switch (trb.Type)
            {
                case TrbType.CommandCompletionEvent:
                    HandleCommandCompletion(trb);
                    break;

                case TrbType.TransferEvent:
                    HandleTransferEvent(trb);
                    break;

                case TrbType.PortStatusChangeEvent:
                    // Port resets during the first root port probe raise
                    // these too; only a change after it is a (re)plug. The
                    // port itself is handled on the hot-plug thread.
                    if (_rootPortsProbed)
                    {
                        MarkRootPortDisconnected(trb.PortId);
                        _bus?.NotifyPortChange();
                    }

                    break;

                case TrbType.HostControllerEvent:
                    RecordFault(XhciFaultKind.HostControllerEvent, 0, 0, trb.CompletionCode);
                    break;
            }
        }

        if (consumed)
        {
            _registers.Erdp = _eventRing.DequeuePointer | ErdpEventHandlerBusy;
        }
    }

    private void HandleCommandCompletion(Trb trb)
    {
        if (_pendingCommand != 0 && trb.Parameter == _pendingCommand)
        {
            _commandCode = trb.CompletionCode;
            _commandSlotId = trb.SlotId;
            _commandCompleted = true;
            return;
        }

        XhciDevice? device = trb.SlotId < _devices.Length ? _devices[trb.SlotId] : null;
        InterruptPipe? pipe = device?.FindPipeByCommand(trb.Parameter);
        if (device is not null && pipe is not null)
        {
            ContinuePipeRecovery(device, pipe, trb.CompletionCode);
        }
    }

    private void HandleTransferEvent(Trb trb)
    {
        if (trb.SlotId >= _devices.Length || _devices[trb.SlotId] is not XhciDevice device)
        {
            return;
        }

        CompletionCode code = trb.CompletionCode;
        bool succeeded = code is CompletionCode.Success or CompletionCode.ShortPacket;
        if (trb.EndpointId == XhciDevice.ControlEndpointId)
        {
            // The device answered a device-to-host request with less than
            // it was asked for. Not the end of the transfer: the Status
            // Stage runs next and reports it; this only says how much of
            // the data stage was left unfilled.
            if (device == _transferDevice && code == CompletionCode.ShortPacket
                && _transferDataTrb != 0 && trb.Parameter == _transferDataTrb)
            {
                _transferResidual = trb.ResidualLength;
                return;
            }

            // An error ends a transfer on whichever stage it hit; success
            // only counts once the Status Stage completes.
            if (device == _transferDevice && (trb.Parameter == _transferStatusTrb || !succeeded))
            {
                _transferCode = code;
                _transferCompleted = true;
            }
            else if (!succeeded)
            {
                // A fire-and-forget transfer failed: the next synchronous
                // one resets the endpoint first.
                device.ControlEndpointHalted = true;
            }

            return;
        }

        BulkPipe? bulkPipe = device.GetBulkPipe(trb.EndpointId);
        if (bulkPipe is not null)
        {
            // Events for a TRB nobody waits on any more (a Stop Endpoint
            // after a timeout reports the abandoned one) are dropped.
            if (bulkPipe.PendingTrb != 0 && trb.Parameter == bulkPipe.PendingTrb)
            {
                bulkPipe.CompletionCode = code;
                bulkPipe.ResidualLength = trb.ResidualLength;
                bulkPipe.Completed = true;
            }

            return;
        }

        InterruptPipe? pipe = device.GetPipe(trb.EndpointId);
        if (pipe is null || pipe.State != InterruptPipeState.Running || !pipe.TryGetBuffer(trb.Parameter, out ulong buffer))
        {
            return;
        }

        if (succeeded)
        {
            pipe.ConsecutiveErrors = 0;
            pipe.Deliver(buffer, trb.ResidualLength);
            using (_ringLock.EnterScope())
            {
                pipe.Queue(buffer);
            }

            _registers.RingDoorbell(device.SlotId, pipe.EndpointId);
            return;
        }

        if (code is not (CompletionCode.Stopped or CompletionCode.StoppedLengthInvalid or CompletionCode.StoppedShortPacket))
        {
            StartPipeRecovery(device, pipe, code, buffer);
        }
    }

    /// <summary>
    /// First step of an interrupt pipe's recovery after a transfer error:
    /// Reset Endpoint. It runs asynchronously, driven by command
    /// completions, because it starts in interrupt context.
    /// </summary>
    private void StartPipeRecovery(XhciDevice device, InterruptPipe pipe, CompletionCode code, ulong failedBuffer)
    {
        // A device that left fails every transfer; there is nothing to recover.
        if (device.IsDisconnected)
        {
            pipe.State = InterruptPipeState.Stopped;
            return;
        }

        pipe.ConsecutiveErrors++;
        if (pipe.ConsecutiveErrors > MaxPipeRecoveries)
        {
            pipe.State = InterruptPipeState.Stopped;
            RecordFault(XhciFaultKind.EndpointAbandoned, device.SlotId, pipe.EndpointId, code);
            return;
        }

        RecordFault(XhciFaultKind.EndpointReset, device.SlotId, pipe.EndpointId, code);
        pipe.State = InterruptPipeState.ResettingEndpoint;
        pipe.FailedBuffer = failedBuffer;
        pipe.StalledByDevice = code == CompletionCode.StallError;
        QueuePipeCommand(pipe, 0, EndpointCommand(TrbType.ResetEndpointCommand, device.SlotId, pipe.EndpointId));
    }

    private void ContinuePipeRecovery(XhciDevice device, InterruptPipe pipe, CompletionCode code)
    {
        pipe.PendingCommand = 0;
        switch (pipe.State)
        {
            case InterruptPipeState.ResettingEndpoint:
                if (code == CompletionCode.ContextStateError)
                {
                    // The endpoint was not halted: only the failed transfer
                    // left the ring, so it alone goes back.
                    pipe.State = InterruptPipeState.Running;
                    using (_ringLock.EnterScope())
                    {
                        pipe.Queue(pipe.FailedBuffer);
                    }

                    _registers.RingDoorbell(device.SlotId, pipe.EndpointId);
                    return;
                }

                if (code != CompletionCode.Success)
                {
                    StopPipe(device, pipe, code);
                    return;
                }

                pipe.State = InterruptPipeState.SettingDequeuePointer;
                ulong dequeuePointer;
                using (_ringLock.EnterScope())
                {
                    dequeuePointer = pipe.Ring.EnqueuePointer | (pipe.Ring.CycleState ? DequeueCycleState : 0);
                }

                QueuePipeCommand(pipe, dequeuePointer, EndpointCommand(TrbType.SetTrDequeuePointerCommand, device.SlotId, pipe.EndpointId));
                return;

            case InterruptPipeState.SettingDequeuePointer:
                if (code != CompletionCode.Success)
                {
                    StopPipe(device, pipe, code);
                    return;
                }

                // A STALL means the device halted its endpoint as well.
                if (pipe.StalledByDevice)
                {
                    SubmitControlTransfer(device, new UsbSetupPacket(UsbDirection.Out, UsbRequestKind.Standard, UsbRecipient.Endpoint,
                        ClearFeatureRequest, EndpointHaltFeature, pipe.EndpointAddress, 0), []);
                }

                pipe.State = InterruptPipeState.Running;
                using (_ringLock.EnterScope())
                {
                    pipe.QueueAll();
                }

                _registers.RingDoorbell(device.SlotId, pipe.EndpointId);
                return;
        }
    }

    private void QueuePipeCommand(InterruptPipe pipe, ulong parameter, uint control)
    {
        using (_ringLock.EnterScope())
        {
            pipe.PendingCommand = _commandRing.Enqueue(parameter, 0, control);
        }

        _registers.RingDoorbell(0, 0);
    }

    private void StopPipe(XhciDevice device, InterruptPipe pipe, CompletionCode code)
    {
        pipe.State = InterruptPipeState.Stopped;
        RecordFault(XhciFaultKind.RecoveryFailed, device.SlotId, pipe.EndpointId, code);
    }

    /// <summary>
    /// Keeps a fault for the hot-plug thread to log, and wakes it for it:
    /// the event handler cannot build the line itself. The caller holds the
    /// event lock. Allocation-free.
    /// </summary>
    private void RecordFault(XhciFaultKind kind, byte slotId, byte endpointId, CompletionCode code)
    {
        if (_faultCount < _faults.Length)
        {
            _faults[_faultCount++] = new XhciFault(kind, slotId, endpointId, code);
        }
        else
        {
            _faultsDropped++;
        }

        _bus?.NotifyPortChange();
    }

    /// <summary>Logs the faults the event handler recorded since the last call. Thread context.</summary>
    private void WriteFaults()
    {
        Span<XhciFault> faults = stackalloc XhciFault[MaxPendingFaults];
        int count;
        int dropped;
        using (_eventLock.EnterScope())
        {
            count = _faultCount;
            dropped = _faultsDropped;
            _faults.AsSpan(0, count).CopyTo(faults);
            _faultCount = 0;
            _faultsDropped = 0;
        }

        for (int i = 0; i < count; i++)
        {
            WriteLog(faults[i].Describe());
        }

        if (dropped != 0)
        {
            WriteLog($"{dropped} more endpoint faults were not kept");
        }
    }
}
