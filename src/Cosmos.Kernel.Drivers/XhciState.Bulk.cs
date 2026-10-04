// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Bulk pipes: their open, the synchronous IN and OUT transfers through
/// the 64 KiB bounce one Normal TRB at a time, and the endpoint reset that
/// returns the host side to DATA0.
/// </summary>
public sealed partial class XhciState
{
    /// <summary>
    /// Adds a bulk endpoint to the slot: an already open DCI returns the
    /// existing pipe; otherwise a pooled memory set, a fresh pipe object,
    /// the endpoint's input context (the ring's enqueue pointer and cycle,
    /// average TRB length 3072) and Configure Endpoint under the input
    /// context claim, then the table entry under the lock. Thread context;
    /// the body of
    /// <see cref="XhciSlot.OpenBulkPipeCore"/>.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="endpoint">A bulk endpoint of the device.</param>
    /// <returns>The pipe; null when the endpoint is not bulk, no DMA memory was left or the controller refused.</returns>
    internal XhciBulkPipe? OpenBulkPipe(XhciSlot slot, UsbEndpoint endpoint)
    {
        if (endpoint.Type != UsbEndpointType.Bulk)
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
            XhciBulkPipe? existing = slot.BulkPipes[dci];
            if (existing is not null)
            {
                return existing;
            }

            if (slot.InterruptPipes[dci] is not null)
            {
                return null;
            }
        }

        XhciPipeMemory? memory = TakePipeMemory(bulk: true);
        if (memory is null)
        {
            _binding.Log($"slot {slot.SlotId}: no DMA memory for the endpoint");
            return null;
        }

        XhciBulkPipe pipe = new(endpoint, memory, dci);
        XhciCompletionCode code = XhciCompletionCode.Invalid;
        if (ClaimInput(slot))
        {
            try
            {
                PrepareEndpointInput(slot, dci, dropFlags: 0);
                WriteBulkEndpoint(slot, pipe);
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
            slot.BulkPipes[dci] = pipe;
        }

        return pipe;
    }

    /// <summary>
    /// Reads from a bulk IN pipe and waits: the pipe claimed (Disconnected
    /// at once when the device left, Timeout when a holder kept it past the
    /// budget), then chunks of at most the bounce's length, each one TRB,
    /// copied out as they land; a short packet ends the transfer. Thread
    /// context; the body of <see cref="XhciSlot.BulkInCore"/>.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="pipe">A bulk IN pipe of the slot.</param>
    /// <param name="data">Receives the data; its length is the most the transfer reads.</param>
    /// <param name="transferred">Bytes received, set on failure too.</param>
    internal UsbTransferStatus BulkIn(XhciSlot slot, UsbPipe pipe, Span<byte> data, out int transferred)
    {
        transferred = 0;
        if (pipe is not XhciBulkPipe bulkPipe || bulkPipe.IsClosed || !bulkPipe.Endpoint.IsIn)
        {
            return UsbTransferStatus.Error;
        }

        UsbTransferStatus claim = ClaimBulk(slot, bulkPipe);
        if (claim != UsbTransferStatus.Success)
        {
            return claim;
        }

        try
        {
            while (transferred < data.Length)
            {
                int length = Math.Min(data.Length - transferred, bulkPipe.Memory.BufferLength);
                UsbTransferStatus status = RunBulkTransfer(slot, bulkPipe, length, out int received);
                bulkPipe.Memory.Buffer.Span.Slice(0, received).CopyTo(data.Slice(transferred));
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
            ReleaseBulk(bulkPipe);
        }
    }

    /// <summary>
    /// Writes to a bulk OUT pipe and waits: the pipe claimed, then chunks of
    /// at most the bounce's length copied in and sent as one TRB each.
    /// Thread context; the body of <see cref="XhciSlot.BulkOutCore"/>.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="pipe">A bulk OUT pipe of the slot.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="transferred">Bytes the device accepted, set on failure too.</param>
    internal UsbTransferStatus BulkOut(XhciSlot slot, UsbPipe pipe, ReadOnlySpan<byte> data, out int transferred)
    {
        transferred = 0;
        if (pipe is not XhciBulkPipe bulkPipe || bulkPipe.IsClosed || bulkPipe.Endpoint.IsIn)
        {
            return UsbTransferStatus.Error;
        }

        UsbTransferStatus claim = ClaimBulk(slot, bulkPipe);
        if (claim != UsbTransferStatus.Success)
        {
            return claim;
        }

        try
        {
            while (transferred < data.Length)
            {
                int length = Math.Min(data.Length - transferred, bulkPipe.Memory.BufferLength);
                data.Slice(transferred, length).CopyTo(bulkPipe.Memory.Buffer.Span);
                UsbTransferStatus status = RunBulkTransfer(slot, bulkPipe, length, out int sent);
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
            ReleaseBulk(bulkPipe);
        }
    }

    /// <summary>
    /// Restarts the host side of a bulk endpoint: nothing queued, sequence
    /// number (data toggle) 0. A halted endpoint gets both from the stop
    /// sequence's Reset Endpoint; any other keeps its sequence number
    /// through Stop Endpoint, so it is dropped and added back under the
    /// input context claim (Linux xhci_endpoint_reset). Thread context; the
    /// body of <see cref="XhciSlot.ResetEndpointCore"/>.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="pipe">A pipe of the slot.</param>
    /// <returns>False for an interrupt pipe, a closed pipe, a disconnected device or a failed command.</returns>
    internal bool ResetEndpoint(XhciSlot slot, UsbPipe pipe)
    {
        if (pipe is not XhciBulkPipe bulkPipe || bulkPipe.IsClosed)
        {
            return false;
        }

        if (ClaimBulk(slot, bulkPipe) != UsbTransferStatus.Success)
        {
            return false;
        }

        try
        {
            if (slot.IsDisconnected)
            {
                return false;
            }

            if (XhciContext.GetEndpointState(slot.Memory.OutputEndpoint(bulkPipe.EndpointId)) == XhciEndpointState.Halted)
            {
                return StopEndpoint(slot, bulkPipe.EndpointId, bulkPipe.Memory.Ring);
            }

            if (!StopEndpoint(slot, bulkPipe.EndpointId, bulkPipe.Memory.Ring))
            {
                return false;
            }

            XhciCompletionCode code = XhciCompletionCode.Invalid;
            if (ClaimInput(slot))
            {
                try
                {
                    PrepareEndpointInput(slot, bulkPipe.EndpointId, dropFlags: XhciContext.EndpointFlag(bulkPipe.EndpointId));
                    WriteBulkEndpoint(slot, bulkPipe);
                    code = ExecuteCommand(slot.Memory.InputContext.PhysicalAddress, SlotCommand(XhciTrbType.ConfigureEndpointCommand, slot.SlotId), out _);
                }
                finally
                {
                    ReleaseInput(slot);
                }
            }

            if (code != XhciCompletionCode.Success)
            {
                LogCommandFailure("Configure Endpoint (endpoint reset)", code);
                return false;
            }

            return true;
        }
        finally
        {
            ReleaseBulk(bulkPipe);
        }
    }

    /// <summary>Endpoint Context of a bulk endpoint whose ring starts at its current enqueue pointer, which is where a reinitialized endpoint picks up. Thread context.</summary>
    private static void WriteBulkEndpoint(XhciSlot slot, XhciBulkPipe pipe) =>
        XhciContext.WriteEndpoint(slot.Memory.InputEndpoint(pipe.EndpointId),
            pipe.Endpoint.IsIn ? XhciEndpointType.BulkIn : XhciEndpointType.BulkOut,
            pipe.Endpoint.MaxPacketSize, pipe.Endpoint.MaxBurst, 0,
            pipe.Memory.Ring.EnqueuePointer, pipe.Memory.Ring.CycleState, XhciProtocol.BulkAverageTrbLength, 0);

    /// <summary>
    /// Moves <paramref name="length"/> bytes between the bounce and the
    /// device as one Normal TRB and waits: the completion flag cleared,
    /// the TRB enqueued and the doorbell rung under the lock; the wait;
    /// the pending address cleared and the result read under the lock. A
    /// failure other than a disconnection is logged and followed by the
    /// stop sequence, so the ring is sane for the next transfer. Thread
    /// context; the caller owns the pipe.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="pipe">The pipe.</param>
    /// <param name="length">Bytes of this TRB.</param>
    /// <param name="transferred">Bytes the TRB moved; 0 on a timeout.</param>
    private UsbTransferStatus RunBulkTransfer(XhciSlot slot, XhciBulkPipe pipe, int length, out int transferred)
    {
        using (Lock.Acquire())
        {
            Volatile.Write(ref pipe.Completed, false);
            pipe.PendingTrb = pipe.Memory.Ring.Enqueue(pipe.Memory.Buffer.PhysicalAddress, (uint)length,
                XhciTrb.TypeField(XhciTrbType.Normal) | XhciTrb.InterruptOnCompletion | XhciTrb.InterruptOnShortPacket);
            RingDoorbell(slot.SlotId, pipe.EndpointId);
        }

        bool completed = WaitCompletion(ref pipe.Completed, pipe.Event, slot, XhciProtocol.BulkTimeoutMs);
        XhciCompletionCode code;
        uint residualLength;
        using (Lock.Acquire())
        {
            pipe.PendingTrb = 0;
            code = completed ? pipe.CompletionCode : XhciCompletionCode.Invalid;
            residualLength = pipe.ResidualLength;
        }

        transferred = code == XhciCompletionCode.Invalid ? 0 : length - (int)Math.Min(residualLength, (uint)length);
        if (code is XhciCompletionCode.Success or XhciCompletionCode.ShortPacket)
        {
            return UsbTransferStatus.Success;
        }

        // Whatever the transfer ended with, a device that left has nothing
        // to recover: its slot is about to be disabled.
        if (slot.IsDisconnected)
        {
            return UsbTransferStatus.Disconnected;
        }

        if (code == XhciCompletionCode.Invalid)
        {
            _binding.Log($"slot {slot.SlotId} endpoint {pipe.EndpointId}: bulk transfer timed out");
        }
        else
        {
            _binding.Log($"slot {slot.SlotId} endpoint {pipe.EndpointId}: bulk transfer failed, completion code {(uint)code}");
        }

        // The TRB is either still queued (timeout) or the error halted the
        // endpoint: either way the ring has to be cleared before the next.
        StopEndpoint(slot, pipe.EndpointId, pipe.Memory.Ring);
        return code switch
        {
            XhciCompletionCode.Invalid => UsbTransferStatus.Timeout,
            XhciCompletionCode.StallError => UsbTransferStatus.Stall,
            _ => UsbTransferStatus.Error
        };
    }
}
