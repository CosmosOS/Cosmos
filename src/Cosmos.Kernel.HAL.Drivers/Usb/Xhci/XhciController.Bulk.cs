// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Usb;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Contexts;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Pipes;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Rings;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci;

internal sealed partial class XhciController
{
    /// <summary>
    /// Budget for one bulk TRB. Generous: a flash drive may stall a write
    /// for seconds while it erases, and a USB disk spins up.
    /// </summary>
    private const long BulkTimeoutUs = 10_000_000;

    /// <summary>Average TRB length the spec recommends for bulk endpoints (xHCI 1.2 §4.14.1.1).</summary>
    private const ushort BulkAverageTrbLength = 3072;

    /// <summary>Adds a bulk endpoint to the device's slot.</summary>
    internal bool OpenBulkPipe(XhciDevice device, UsbEndpointInfo endpoint)
    {
        if (endpoint.Type != UsbEndpointType.Bulk)
        {
            WriteLog($"slot {device.SlotId}: endpoint 0x{endpoint.Address:X2} is not a bulk endpoint");
            return false;
        }

        byte endpointId = XhciDevice.EndpointId(endpoint);
        if (device.GetBulkPipe(endpointId) is not null)
        {
            return true;
        }

        BulkPipe pipe;
        try
        {
            pipe = BulkPipe.Create(endpointId, endpoint, _memory);
        }
        catch (InvalidOperationException exception)
        {
            WriteLog($"slot {device.SlotId}: {exception.Message}");
            return false;
        }

        PrepareEndpointInput(device, endpointId, reinitialize: false);
        WriteBulkEndpoint(device, pipe);

        CompletionCode code = ExecuteCommand(device.InputContextAddress, SlotCommand(TrbType.ConfigureEndpointCommand, device.SlotId), out _);
        if (code != CompletionCode.Success)
        {
            LogCommandFailure("Configure Endpoint", code);
            pipe.Free(_memory);
            return false;
        }

        using (_eventLock.EnterScope())
        {
            device.AddBulkPipe(pipe);
        }

        return true;
    }

    internal UsbTransferStatus BulkIn(XhciDevice device, UsbEndpointInfo endpoint, Span<byte> data, out int transferred)
    {
        transferred = 0;
        BulkPipe? pipe = FindBulkPipe(device, endpoint, UsbDirection.In);
        if (pipe is null)
        {
            return UsbTransferStatus.Error;
        }

        // Counted in before the check, so a release that marked the device
        // gone either sees this transfer and waits, or this sees the mark.
        device.EnterBulkTransfer();
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
                pipe.Buffer[..received].CopyTo(data[transferred..]);
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
            device.ExitBulkTransfer();
        }
    }

    internal UsbTransferStatus BulkOut(XhciDevice device, UsbEndpointInfo endpoint, ReadOnlySpan<byte> data, out int transferred)
    {
        transferred = 0;
        BulkPipe? pipe = FindBulkPipe(device, endpoint, UsbDirection.Out);
        if (pipe is null)
        {
            return UsbTransferStatus.Error;
        }

        device.EnterBulkTransfer();
        try
        {
            if (device.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            while (transferred < data.Length)
            {
                int length = Math.Min(data.Length - transferred, pipe.BufferLength);
                data.Slice(transferred, length).CopyTo(pipe.Buffer);
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
            device.ExitBulkTransfer();
        }
    }

    /// <summary>
    /// Restarts the host side of a bulk endpoint: nothing queued, sequence
    /// number (data toggle) 0. A halted endpoint gets both from Reset
    /// Endpoint; any other keeps its sequence number through Stop Endpoint,
    /// so it is dropped and added back (Linux xhci_endpoint_reset).
    /// </summary>
    internal bool ResetBulkPipe(XhciDevice device, UsbEndpointInfo endpoint)
    {
        BulkPipe? pipe = FindBulkPipe(device, endpoint, endpoint.Direction);
        if (pipe is null)
        {
            return false;
        }

        device.EnterBulkTransfer();
        try
        {
            if (device.IsDisconnected)
            {
                return false;
            }

            if (ContextLayout.GetEndpointState(device.OutputEndpointContext(pipe.EndpointId)) == EndpointState.Halted)
            {
                return StopBulkPipe(device, pipe);
            }

            if (!StopBulkPipe(device, pipe))
            {
                return false;
            }

            PrepareEndpointInput(device, pipe.EndpointId, reinitialize: true);
            WriteBulkEndpoint(device, pipe);
            CompletionCode code = ExecuteCommand(device.InputContextAddress, SlotCommand(TrbType.ConfigureEndpointCommand, device.SlotId), out _);
            if (code != CompletionCode.Success)
            {
                LogCommandFailure("Configure Endpoint (endpoint reset)", code);
                return false;
            }

            return true;
        }
        finally
        {
            device.ExitBulkTransfer();
        }
    }

    private BulkPipe? FindBulkPipe(XhciDevice device, UsbEndpointInfo endpoint, UsbDirection direction)
    {
        BulkPipe? pipe = endpoint.Direction == direction ? device.GetBulkPipe(XhciDevice.EndpointId(endpoint)) : null;
        if (pipe is null)
        {
            WriteLog($"slot {device.SlotId}: bulk transfer on endpoint 0x{endpoint.Address:X2}, which is not an open bulk {(direction == UsbDirection.In ? "IN" : "OUT")} pipe");
        }

        return pipe;
    }

    /// <summary>
    /// Endpoint Context of a bulk endpoint whose ring starts at its current
    /// enqueue pointer, which is where a reinitialized endpoint picks up.
    /// </summary>
    private static void WriteBulkEndpoint(XhciDevice device, BulkPipe pipe) =>
        ContextLayout.WriteEndpoint(device.InputEndpointContext(pipe.EndpointId),
            pipe.Endpoint.Direction == UsbDirection.In ? EndpointType.BulkIn : EndpointType.BulkOut,
            pipe.Endpoint.MaxPacketSize, pipe.Endpoint.MaxBurst, 0,
            pipe.Ring.EnqueuePointer, pipe.Ring.CycleState, BulkAverageTrbLength, 0);

    /// <summary>
    /// Moves <paramref name="length"/> bytes between the pipe's buffer and
    /// the device as one TRB, and waits. The USB core runs one transfer on
    /// the pipe at a time.
    /// </summary>
    private UsbTransferStatus RunBulkTransfer(XhciDevice device, BulkPipe pipe, int length, out int transferred)
    {
        using (_eventLock.EnterScope())
        {
            pipe.Completed = false;
            using (_ringLock.EnterScope())
            {
                pipe.PendingTrb = pipe.Ring.Enqueue(pipe.BufferAddress, (uint)length,
                    Trb.TypeField(TrbType.Normal) | Trb.InterruptOnCompletion | Trb.InterruptOnShortPacket);
            }

            _registers.RingDoorbell(device.SlotId, pipe.EndpointId);
        }

        CompletionCode code = WaitForBulkTransfer(device, pipe, out uint residualLength);
        transferred = code == CompletionCode.Invalid ? 0 : length - (int)Math.Min(residualLength, (uint)length);
        if (code is CompletionCode.Success or CompletionCode.ShortPacket)
        {
            return UsbTransferStatus.Success;
        }

        // Whatever the transfer ended with, a device that left has nothing
        // to recover: its slot is about to be disabled.
        if (device.IsDisconnected)
        {
            return UsbTransferStatus.Disconnected;
        }

        WriteLog(code == CompletionCode.Invalid
            ? $"slot {device.SlotId} endpoint {pipe.EndpointId}: bulk transfer timed out"
            : $"slot {device.SlotId} endpoint {pipe.EndpointId}: bulk transfer failed, completion code {(byte)code}");

        // The TRB is either still queued (timeout) or the error halted the
        // endpoint: either way the ring has to be cleared before the next.
        StopBulkPipe(device, pipe);
        return code switch
        {
            CompletionCode.Invalid => UsbTransferStatus.Timeout,
            CompletionCode.StallError => UsbTransferStatus.Stall,
            _ => UsbTransferStatus.Error
        };
    }

    /// <returns>
    /// The completion code, or <see cref="CompletionCode.Invalid"/> on
    /// timeout and when <paramref name="device"/> left the bus meanwhile.
    /// </returns>
    private CompletionCode WaitForBulkTransfer(XhciDevice device, BulkPipe pipe, out uint residualLength)
    {
        for (long waitedUs = 0; ; waitedUs += WaitPollIntervalUs)
        {
            using (_eventLock.EnterScope())
            {
                DrainEvents();
                if (pipe.Completed || device.IsDisconnected || waitedUs >= BulkTimeoutUs)
                {
                    pipe.PendingTrb = 0;
                    residualLength = pipe.ResidualLength;
                    return pipe.Completed ? pipe.CompletionCode : CompletionCode.Invalid;
                }
            }

            _context.Delay(WaitPollInterval);
        }
    }

    /// <summary>
    /// Leaves a bulk endpoint Stopped with its dequeue pointer past every
    /// abandoned TRB, from whatever state a failed or timed-out transfer
    /// left it in: a Running one is stopped, a Halted one reset (xHCI 1.2
    /// §4.6.8, §4.6.9, §4.6.10).
    /// </summary>
    private bool StopBulkPipe(XhciDevice device, BulkPipe pipe)
    {
        Span<uint> context = device.OutputEndpointContext(pipe.EndpointId);
        CompletionCode code;
        if (ContextLayout.GetEndpointState(context) == EndpointState.Running)
        {
            // Context State Error: it halted or stopped meanwhile, which the
            // state read next tells apart.
            code = ExecuteCommand(0, EndpointCommand(TrbType.StopEndpointCommand, device.SlotId, pipe.EndpointId), out _);
            if (code is not (CompletionCode.Success or CompletionCode.ContextStateError))
            {
                LogCommandFailure("Stop Endpoint", code);
                return false;
            }
        }

        if (ContextLayout.GetEndpointState(context) == EndpointState.Halted)
        {
            code = ExecuteCommand(0, EndpointCommand(TrbType.ResetEndpointCommand, device.SlotId, pipe.EndpointId), out _);
            if (code != CompletionCode.Success)
            {
                LogCommandFailure("Reset Endpoint", code);
                return false;
            }
        }

        ulong dequeuePointer;
        using (_ringLock.EnterScope())
        {
            dequeuePointer = pipe.Ring.EnqueuePointer | (pipe.Ring.CycleState ? DequeueCycleState : 0);
        }

        code = ExecuteCommand(dequeuePointer, EndpointCommand(TrbType.SetTrDequeuePointerCommand, device.SlotId, pipe.EndpointId), out _);
        if (code != CompletionCode.Success)
        {
            LogCommandFailure("Set TR Dequeue Pointer", code);
            return false;
        }

        return true;
    }
}
