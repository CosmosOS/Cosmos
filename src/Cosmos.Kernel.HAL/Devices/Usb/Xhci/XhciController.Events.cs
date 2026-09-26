// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

internal sealed unsafe partial class XhciController
{
    /// <summary>Recoveries of an interrupt pipe without a good transfer in between before it is given up.</summary>
    private const int MaxPipeRecoveries = 3;

    /// <summary>MSI-X handler of interrupter 0. Allocation-free.</summary>
    private void OnInterrupt(ref IRQContext context)
    {
        using (_eventLock.AcquireIrqSafe())
        {
            // Both are RW1C; IMAN.IE has to be written back as set.
            _regs.UsbSts = UsbStsEventInterrupt;
            _regs.Iman = ImanInterruptEnable | ImanInterruptPending;
            DrainEvents();
        }
    }

    /// <summary>Consumes every pending event, then moves ERDP past them. The caller holds the event lock.</summary>
    private void DrainEvents()
    {
        bool consumed = false;
        while (_eventRing.TryDequeue(out XhciTrb trb))
        {
            consumed = true;
            switch (trb.Type)
            {
                case XhciTrbType.CommandCompletionEvent:
                    HandleCommandCompletion(trb);
                    break;

                case XhciTrbType.TransferEvent:
                    HandleTransferEvent(trb);
                    break;

                case XhciTrbType.PortStatusChangeEvent:
                    // Port resets during the boot probe raise these too;
                    // only a change after it is a (re)plug. The port itself
                    // is handled on the hot-plug thread.
                    if (_rootPortsProbed)
                    {
                        MarkRootPortDisconnected(trb.PortId);
                        UsbManager.NotifyPortChange();
                    }

                    break;

                case XhciTrbType.HostControllerEvent:
                    Serial.WriteString("[xHCI] Host controller event, completion code ");
                    Serial.WriteNumber((uint)trb.CompletionCode);
                    Serial.WriteString("\n");
                    break;
            }
        }

        if (consumed)
        {
            _regs.Erdp = _eventRing.DequeuePointer | ErdpEventHandlerBusy;
        }
    }

    private void HandleCommandCompletion(XhciTrb trb)
    {
        if (_pendingCommand != 0 && trb.Parameter == _pendingCommand)
        {
            _commandCode = trb.CompletionCode;
            _commandSlotId = trb.SlotId;
            _commandCompleted = true;
            return;
        }

        XhciDevice? device = trb.SlotId < _devices.Length ? _devices[trb.SlotId] : null;
        XhciInterruptPipe? pipe = device?.FindPipeByCommand(trb.Parameter);
        if (device is not null && pipe is not null)
        {
            ContinuePipeRecovery(device, pipe, trb.CompletionCode);
        }
    }

    private void HandleTransferEvent(XhciTrb trb)
    {
        if (trb.SlotId >= _devices.Length || _devices[trb.SlotId] is not XhciDevice device)
        {
            return;
        }

        XhciCompletionCode code = trb.CompletionCode;
        bool succeeded = code is XhciCompletionCode.Success or XhciCompletionCode.ShortPacket;
        if (trb.EndpointId == XhciDevice.ControlEndpointId)
        {
            // The device answered a device-to-host request with less than
            // it was asked for. Not the end of the transfer: the Status
            // Stage runs next and reports it; this only says how much of
            // the data stage was left unfilled.
            if (device == _transferDevice && code == XhciCompletionCode.ShortPacket
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

        XhciBulkPipe? bulkPipe = device.GetBulkPipe(trb.EndpointId);
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

        XhciInterruptPipe? pipe = device.GetPipe(trb.EndpointId);
        if (pipe is null || pipe.State != XhciPipeState.Running || !pipe.TryGetBuffer(trb.Parameter, out ulong buffer))
        {
            return;
        }

        if (succeeded)
        {
            pipe.ConsecutiveErrors = 0;
            pipe.Deliver(buffer, trb.ResidualLength);
            using (_ringLock.AcquireIrqSafe())
            {
                pipe.Queue(buffer);
            }

            _regs.RingDoorbell(device.SlotId, pipe.EndpointId);
            return;
        }

        if (code is not (XhciCompletionCode.Stopped or XhciCompletionCode.StoppedLengthInvalid or XhciCompletionCode.StoppedShortPacket))
        {
            StartPipeRecovery(device, pipe, code, buffer);
        }
    }

    /// <summary>
    /// First step of an interrupt pipe's recovery after a transfer error:
    /// Reset Endpoint. It runs asynchronously, driven by command
    /// completions, because it starts in interrupt context.
    /// </summary>
    private void StartPipeRecovery(XhciDevice device, XhciInterruptPipe pipe, XhciCompletionCode code, ulong failedBuffer)
    {
        // A device that left fails every transfer; there is nothing to recover.
        if (device.IsDisconnected)
        {
            pipe.State = XhciPipeState.Stopped;
            return;
        }

        pipe.ConsecutiveErrors++;
        WritePipePrefix(device, pipe.EndpointId);
        Serial.WriteString("transfer error, completion code ");
        Serial.WriteNumber((uint)code);
        if (pipe.ConsecutiveErrors > MaxPipeRecoveries)
        {
            pipe.State = XhciPipeState.Stopped;
            Serial.WriteString("; giving up\n");
            return;
        }

        Serial.WriteString("; resetting endpoint\n");
        pipe.State = XhciPipeState.ResettingEndpoint;
        pipe.FailedBuffer = failedBuffer;
        pipe.StalledByDevice = code == XhciCompletionCode.StallError;
        QueuePipeCommand(pipe, 0, EndpointCommand(XhciTrbType.ResetEndpointCommand, device.SlotId, pipe.EndpointId));
    }

    private void ContinuePipeRecovery(XhciDevice device, XhciInterruptPipe pipe, XhciCompletionCode code)
    {
        pipe.PendingCommand = 0;
        switch (pipe.State)
        {
            case XhciPipeState.ResettingEndpoint:
                if (code == XhciCompletionCode.ContextStateError)
                {
                    // The endpoint was not halted: only the failed transfer
                    // left the ring, so it alone goes back.
                    pipe.State = XhciPipeState.Running;
                    using (_ringLock.AcquireIrqSafe())
                    {
                        pipe.Queue(pipe.FailedBuffer);
                    }

                    _regs.RingDoorbell(device.SlotId, pipe.EndpointId);
                    return;
                }

                if (code != XhciCompletionCode.Success)
                {
                    StopPipe(device, pipe, code);
                    return;
                }

                pipe.State = XhciPipeState.SettingDequeuePointer;
                ulong dequeuePointer;
                using (_ringLock.AcquireIrqSafe())
                {
                    dequeuePointer = pipe.Ring.EnqueuePointer | (pipe.Ring.CycleState ? DequeueCycleState : 0);
                }

                QueuePipeCommand(pipe, dequeuePointer, EndpointCommand(XhciTrbType.SetTrDequeuePointerCommand, device.SlotId, pipe.EndpointId));
                return;

            case XhciPipeState.SettingDequeuePointer:
                if (code != XhciCompletionCode.Success)
                {
                    StopPipe(device, pipe, code);
                    return;
                }

                // A STALL means the device halted its endpoint as well.
                if (pipe.StalledByDevice)
                {
                    device.SubmitControlTransfer(new UsbSetupPacket(UsbRequestType.Standard | UsbRequestType.Endpoint,
                        (byte)UsbStandardRequest.ClearFeature, UsbDevice.EndpointHaltFeature, pipe.EndpointAddress, 0), []);
                }

                pipe.State = XhciPipeState.Running;
                using (_ringLock.AcquireIrqSafe())
                {
                    pipe.QueueAll();
                }

                _regs.RingDoorbell(device.SlotId, pipe.EndpointId);
                return;
        }
    }

    private void QueuePipeCommand(XhciInterruptPipe pipe, ulong parameter, uint control)
    {
        using (_ringLock.AcquireIrqSafe())
        {
            pipe.PendingCommand = _commandRing.Enqueue(parameter, 0, control);
        }

        _regs.RingDoorbell(0, 0);
    }

    private static void StopPipe(XhciDevice device, XhciInterruptPipe pipe, XhciCompletionCode code)
    {
        pipe.State = XhciPipeState.Stopped;
        WritePipePrefix(device, pipe.EndpointId);
        Serial.WriteString("recovery failed, completion code ");
        Serial.WriteNumber((uint)code);
        Serial.WriteString("\n");
    }

    private static void WritePipePrefix(XhciDevice device, byte endpointId)
    {
        Serial.WriteString("[xHCI] Slot ");
        Serial.WriteNumber((uint)device.SlotId);
        Serial.WriteString(" endpoint ");
        Serial.WriteNumber((uint)endpointId);
        Serial.WriteString(": ");
    }
}
