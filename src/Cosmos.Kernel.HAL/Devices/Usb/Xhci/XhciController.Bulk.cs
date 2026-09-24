// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

internal sealed unsafe partial class XhciController
{
    /// <summary>
    /// Budget for one bulk TRB. Generous: a flash drive may stall a write
    /// for seconds while it erases, and a USB disk spins up.
    /// </summary>
    private const uint BulkTimeoutMs = 10_000;

    /// <summary>Average TRB length the spec recommends for bulk endpoints (xHCI 1.2 §4.14.1.1).</summary>
    private const ushort BulkAverageTrbLength = 3072;

    /// <summary>Adds a bulk endpoint to the device's slot.</summary>
    internal bool OpenBulkPipe(XhciDevice device, UsbEndpoint endpoint)
    {
        if (endpoint.Type != UsbEndpointType.Bulk)
        {
            Serial.WriteString("[xHCI] Only bulk endpoints can be opened as bulk pipes\n");
            return false;
        }

        byte endpointId = XhciDevice.EndpointId(endpoint);
        if (device.GetBulkPipe(endpointId) is not null)
        {
            return true;
        }

        XhciBulkPipe pipe = new(endpointId, endpoint);
        PrepareEndpointInput(device, endpointId, reinitialize: false);
        WriteBulkEndpoint(device, pipe);

        XhciCompletionCode code = ExecuteCommand(device.InputContextAddress, SlotCommand(XhciTrbType.ConfigureEndpointCommand, device.SlotId), out _);
        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Configure Endpoint", code);
            pipe.Free();
            return false;
        }

        using (_eventLock.AcquireIrqSafe())
        {
            device.AddBulkPipe(pipe);
        }

        return true;
    }

    internal UsbTransferStatus BulkIn(XhciDevice device, UsbEndpoint endpoint, Span<byte> data, out int transferred)
    {
        transferred = 0;
        XhciBulkPipe? pipe = FindBulkPipe(device, endpoint, isIn: true);
        if (pipe is null)
        {
            return UsbTransferStatus.Error;
        }

        pipe.Mutex.Acquire();
        try
        {
            if (device.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            while (transferred < data.Length)
            {
                int length = Math.Min(data.Length - transferred, pipe.BufferLength);
                UsbTransferStatus status = RunBulkTransfer(device, pipe, length, out int received);
                new ReadOnlySpan<byte>(pipe.Buffer, received).CopyTo(data.Slice(transferred));
                transferred += received;

                // A short packet ends the transfer: the device has sent all it had.
                if (status != UsbTransferStatus.Success || received < length)
                {
                    return status;
                }
            }

            return UsbTransferStatus.Success;
        }
        finally
        {
            pipe.Mutex.Release();
        }
    }

    internal UsbTransferStatus BulkOut(XhciDevice device, UsbEndpoint endpoint, ReadOnlySpan<byte> data, out int transferred)
    {
        transferred = 0;
        XhciBulkPipe? pipe = FindBulkPipe(device, endpoint, isIn: false);
        if (pipe is null)
        {
            return UsbTransferStatus.Error;
        }

        pipe.Mutex.Acquire();
        try
        {
            if (device.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            while (transferred < data.Length)
            {
                int length = Math.Min(data.Length - transferred, pipe.BufferLength);
                data.Slice(transferred, length).CopyTo(new Span<byte>(pipe.Buffer, length));
                UsbTransferStatus status = RunBulkTransfer(device, pipe, length, out int sent);
                transferred += sent;
                if (status != UsbTransferStatus.Success)
                {
                    return status;
                }
            }

            return UsbTransferStatus.Success;
        }
        finally
        {
            pipe.Mutex.Release();
        }
    }

    /// <summary>
    /// Restarts the host side of a bulk endpoint: nothing queued, sequence
    /// number (data toggle) 0. A halted endpoint gets both from Reset
    /// Endpoint; any other keeps its sequence number through Stop Endpoint,
    /// so it is dropped and added back (Linux xhci_endpoint_reset).
    /// </summary>
    internal bool ResetBulkPipe(XhciDevice device, UsbEndpoint endpoint)
    {
        XhciBulkPipe? pipe = FindBulkPipe(device, endpoint, endpoint.IsIn);
        if (pipe is null)
        {
            return false;
        }

        pipe.Mutex.Acquire();
        try
        {
            if (device.IsDisconnected)
            {
                return false;
            }

            if (XhciContext.GetEndpointState(device.OutputEndpointContext(pipe.EndpointId)) == XhciEndpointState.Halted)
            {
                return StopBulkPipe(device, pipe);
            }

            if (!StopBulkPipe(device, pipe))
            {
                return false;
            }

            PrepareEndpointInput(device, pipe.EndpointId, reinitialize: true);
            WriteBulkEndpoint(device, pipe);
            XhciCompletionCode code = ExecuteCommand(device.InputContextAddress, SlotCommand(XhciTrbType.ConfigureEndpointCommand, device.SlotId), out _);
            if (code != XhciCompletionCode.Success)
            {
                LogCommandFailure("Configure Endpoint (endpoint reset)", code);
                return false;
            }

            return true;
        }
        finally
        {
            pipe.Mutex.Release();
        }
    }

    private static XhciBulkPipe? FindBulkPipe(XhciDevice device, UsbEndpoint endpoint, bool isIn)
    {
        XhciBulkPipe? pipe = endpoint.IsIn == isIn ? device.GetBulkPipe(XhciDevice.EndpointId(endpoint)) : null;
        if (pipe is null)
        {
            Serial.WriteString("[xHCI] Bulk transfer on an endpoint that is not an open bulk ");
            Serial.WriteString(isIn ? "IN" : "OUT");
            Serial.WriteString(" pipe\n");
        }

        return pipe;
    }

    /// <summary>
    /// Endpoint Context of a bulk endpoint whose ring starts at its current
    /// enqueue pointer, which is where a reinitialized endpoint picks up.
    /// </summary>
    private static void WriteBulkEndpoint(XhciDevice device, XhciBulkPipe pipe) =>
        XhciContext.WriteEndpoint(device.InputEndpointContext(pipe.EndpointId),
            pipe.Endpoint.IsIn ? XhciEndpointType.BulkIn : XhciEndpointType.BulkOut,
            pipe.Endpoint.MaxPacketSize, pipe.Endpoint.MaxBurst, 0,
            pipe.Ring.EnqueuePointer, pipe.Ring.CycleState, BulkAverageTrbLength, 0);

    /// <summary>
    /// Moves <paramref name="length"/> bytes between the pipe's buffer and
    /// the device as one TRB, and waits. The caller holds the pipe's mutex.
    /// </summary>
    private UsbTransferStatus RunBulkTransfer(XhciDevice device, XhciBulkPipe pipe, int length, out int transferred)
    {
        using (_eventLock.AcquireIrqSafe())
        {
            pipe.Completed = false;
            using (_ringLock.AcquireIrqSafe())
            {
                pipe.PendingTrb = pipe.Ring.Enqueue(pipe.BufferAddress, (uint)length,
                    XhciTrb.TypeField(XhciTrbType.Normal) | XhciTrb.InterruptOnCompletion | XhciTrb.InterruptOnShortPacket);
            }

            _regs.RingDoorbell(device.SlotId, pipe.EndpointId);
        }

        XhciCompletionCode code = WaitForBulkTransfer(device, pipe, out uint residualLength);
        transferred = code == XhciCompletionCode.Invalid ? 0 : length - (int)Math.Min(residualLength, (uint)length);
        if (code is XhciCompletionCode.Success or XhciCompletionCode.ShortPacket)
        {
            return UsbTransferStatus.Success;
        }

        // Whatever the transfer ended with, a device that left has nothing
        // to recover: its slot is about to be disabled.
        if (device.IsDisconnected)
        {
            return UsbTransferStatus.Disconnected;
        }

        WritePipePrefix(device, pipe.EndpointId);
        if (code == XhciCompletionCode.Invalid)
        {
            Serial.WriteString("bulk transfer timed out\n");
        }
        else
        {
            Serial.WriteString("bulk transfer failed, completion code ");
            Serial.WriteNumber((uint)code);
            Serial.WriteString("\n");
        }

        // The TRB is either still queued (timeout) or the error halted the
        // endpoint: either way the ring has to be cleared before the next.
        StopBulkPipe(device, pipe);
        return code switch
        {
            XhciCompletionCode.Invalid => UsbTransferStatus.Timeout,
            XhciCompletionCode.StallError => UsbTransferStatus.Stall,
            _ => UsbTransferStatus.Error
        };
    }

    /// <returns>
    /// The completion code, or <see cref="XhciCompletionCode.Invalid"/> on
    /// timeout and when <paramref name="device"/> left the bus meanwhile.
    /// </returns>
    private XhciCompletionCode WaitForBulkTransfer(XhciDevice device, XhciBulkPipe pipe, out uint residualLength)
    {
        for (uint waitedUs = 0; ; waitedUs += WaitPollIntervalUs)
        {
            using (_eventLock.AcquireIrqSafe())
            {
                DrainEvents();
                if (pipe.Completed || device.IsDisconnected || waitedUs >= BulkTimeoutMs * MicrosecondsPerMillisecond)
                {
                    pipe.PendingTrb = 0;
                    residualLength = pipe.ResidualLength;
                    return pipe.Completed ? pipe.CompletionCode : XhciCompletionCode.Invalid;
                }
            }

            PlatformHAL.Initializer?.DelayMicroseconds(WaitPollIntervalUs);
        }
    }

    /// <summary>
    /// Leaves a bulk endpoint Stopped with its dequeue pointer past every
    /// abandoned TRB, from whatever state a failed or timed-out transfer
    /// left it in: a Running one is stopped, a Halted one reset (xHCI 1.2
    /// §4.6.8, §4.6.9, §4.6.10). The caller holds the pipe's mutex.
    /// </summary>
    private bool StopBulkPipe(XhciDevice device, XhciBulkPipe pipe)
    {
        uint* context = device.OutputEndpointContext(pipe.EndpointId);
        XhciCompletionCode code;
        if (XhciContext.GetEndpointState(context) == XhciEndpointState.Running)
        {
            // Context State Error: it halted or stopped meanwhile, which the
            // state read next tells apart.
            code = ExecuteCommand(0, EndpointCommand(XhciTrbType.StopEndpointCommand, device.SlotId, pipe.EndpointId), out _);
            if (code is not (XhciCompletionCode.Success or XhciCompletionCode.ContextStateError))
            {
                LogCommandFailure("Stop Endpoint", code);
                return false;
            }
        }

        if (XhciContext.GetEndpointState(context) == XhciEndpointState.Halted)
        {
            code = ExecuteCommand(0, EndpointCommand(XhciTrbType.ResetEndpointCommand, device.SlotId, pipe.EndpointId), out _);
            if (code != XhciCompletionCode.Success)
            {
                LogCommandFailure("Reset Endpoint", code);
                return false;
            }
        }

        ulong dequeuePointer;
        using (_ringLock.AcquireIrqSafe())
        {
            dequeuePointer = pipe.Ring.EnqueuePointer | (pipe.Ring.CycleState ? DequeueCycleState : 0);
        }

        code = ExecuteCommand(dequeuePointer, EndpointCommand(XhciTrbType.SetTrDequeuePointerCommand, device.SlotId, pipe.EndpointId), out _);
        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Set TR Dequeue Pointer", code);
            return false;
        }

        return true;
    }
}
