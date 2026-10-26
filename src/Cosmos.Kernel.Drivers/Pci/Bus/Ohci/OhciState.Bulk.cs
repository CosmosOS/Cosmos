// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// Bulk pipes: their open into the bulk list, the synchronous IN and OUT
/// transfers through the 64 KiB bounce one two-page TD at a time, and the
/// endpoint reset that returns the host side to DATA0.
/// </summary>
internal sealed partial class OhciState
{
    /// <summary>
    /// Adds a bulk endpoint to the device: an already open index returns
    /// the existing pipe; otherwise a pooled memory set, a fresh pipe
    /// object, the ED built with the device's address and the endpoint's
    /// direction and packet size, then, under the lock, the set's TDs
    /// recorded for the pipe, the ED linked after the bulk list head and
    /// the table entry. Thread context; the body of
    /// <see cref="OhciDevice.OpenBulkPipeCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="endpoint">A bulk endpoint of the device.</param>
    /// <returns>The pipe; null when the endpoint is not bulk, its index is held by an interrupt pipe or no DMA memory was left.</returns>
    internal OhciBulkPipe? OpenBulkPipe(OhciDevice device, UsbEndpoint endpoint)
    {
        if (endpoint.Type != UsbEndpointType.Bulk)
        {
            return null;
        }

        byte index = OhciDevice.EndpointIndex(endpoint);
        if (endpoint.Number == 0 || index > OhciProtocol.MaxEndpointIndex)
        {
            return null;
        }

        using (Lock.Acquire())
        {
            OhciBulkPipe? existing = device.BulkPipes[index];
            if (existing is not null)
            {
                return existing;
            }

            if (device.InterruptPipes[index] is not null)
            {
                return null;
            }
        }

        OhciPipeMemory? memory = TakePipeMemory(bulk: true);
        if (memory is null)
        {
            _binding.Log($"address {device.Address}: no DMA memory for the endpoint");
            return null;
        }

        OhciBulkPipe pipe = new(endpoint, memory, index);
        BuildEndpoint(memory, device.Address, endpoint.Number,
            endpoint.IsIn ? OhciProtocol.EndpointDirectionIn : OhciProtocol.EndpointDirectionOut, device.Speed, endpoint.MaxPacketSize);

        using (Lock.Acquire())
        {
            SetOwner(memory, pipe);
            LinkEndpoint(BulkHead, memory.Endpoint);
            device.BulkPipes[index] = pipe;
        }

        return pipe;
    }

    /// <summary>
    /// Reads from a bulk IN pipe and waits: the pipe claimed (Disconnected
    /// at once when the device left, Timeout when a holder kept it past the
    /// budget), then chunks of at most two pages, each one TD, copied out
    /// as they land; a short packet ends the transfer. Thread context; the
    /// body of <see cref="OhciDevice.BulkInCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="pipe">A bulk IN pipe of the device.</param>
    /// <param name="data">Receives the data; its length is the most the transfer reads.</param>
    /// <param name="transferred">Bytes received, set on failure too.</param>
    internal UsbTransferStatus BulkIn(OhciDevice device, UsbPipe pipe, Span<byte> data, out int transferred)
    {
        transferred = 0;
        if (pipe is not OhciBulkPipe bulkPipe || bulkPipe.IsClosed || !bulkPipe.Endpoint.IsIn)
        {
            return UsbTransferStatus.Error;
        }

        UsbTransferStatus claim = ClaimBulk(device, bulkPipe);
        if (claim != UsbTransferStatus.Success)
        {
            return claim;
        }

        try
        {
            while (transferred < data.Length)
            {
                int length = Math.Min(data.Length - transferred, OhciProtocol.BulkChunkBytes);
                UsbTransferStatus status = RunBulkTransfer(device, bulkPipe, length, out int received);
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
    /// at most two pages copied in and sent as one TD each. Thread context;
    /// the body of <see cref="OhciDevice.BulkOutCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="pipe">A bulk OUT pipe of the device.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="transferred">Bytes the device accepted, set on failure too.</param>
    internal UsbTransferStatus BulkOut(OhciDevice device, UsbPipe pipe, ReadOnlySpan<byte> data, out int transferred)
    {
        transferred = 0;
        if (pipe is not OhciBulkPipe bulkPipe || bulkPipe.IsClosed || bulkPipe.Endpoint.IsIn)
        {
            return UsbTransferStatus.Error;
        }

        UsbTransferStatus claim = ClaimBulk(device, bulkPipe);
        if (claim != UsbTransferStatus.Success)
        {
            return claim;
        }

        try
        {
            while (transferred < data.Length)
            {
                int length = Math.Min(data.Length - transferred, OhciProtocol.BulkChunkBytes);
                data.Slice(transferred, length).CopyTo(bulkPipe.Memory.Buffer.Span);
                UsbTransferStatus status = RunBulkTransfer(device, bulkPipe, length, out int sent);
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
    /// Restarts the host side of a bulk endpoint: nothing queued, the
    /// toggle carry cleared to DATA0, through the quiesce. Thread context;
    /// the body of <see cref="OhciDevice.ResetEndpointCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="pipe">A pipe of the device.</param>
    /// <returns>False for an interrupt pipe, a closed pipe or a disconnected device.</returns>
    internal bool ResetEndpoint(OhciDevice device, UsbPipe pipe)
    {
        if (pipe is not OhciBulkPipe bulkPipe || bulkPipe.IsClosed)
        {
            return false;
        }

        if (ClaimBulk(device, bulkPipe) != UsbTransferStatus.Success)
        {
            return false;
        }

        try
        {
            if (device.IsDisconnected)
            {
                return false;
            }

            QuiesceEndpoint(bulkPipe.Memory, keepToggle: false, device.Address, bulkPipe.Endpoint.Address);
            return true;
        }
        finally
        {
            ReleaseBulk(bulkPipe);
        }
    }

    /// <summary>
    /// Moves <paramref name="length"/> bytes between the start of the
    /// bounce and the device as one TD and waits: the completion flag
    /// cleared, the TD queued and BLF set under the lock; the wait; the
    /// pending index cleared and the result read under the lock. A failure
    /// other than a disconnection is logged and followed by the quiesce
    /// with the toggle carry kept (the device's toggle did not move), so
    /// the ED is sane for the next transfer. Thread context; the caller
    /// owns the pipe.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="pipe">The pipe.</param>
    /// <param name="length">Bytes of this TD, at most two pages.</param>
    /// <param name="transferred">Bytes the TD moved; 0 on a timeout.</param>
    private UsbTransferStatus RunBulkTransfer(OhciDevice device, OhciBulkPipe pipe, int length, out int transferred)
    {
        bool queued;
        using (Lock.Acquire())
        {
            Volatile.Write(ref pipe.Completed, false);
            queued = QueueBulkTransfer(pipe, length);
            if (queued)
            {
                SetBulkListFilled();
            }
        }

        if (!queued)
        {
            transferred = 0;
            _binding.Log($"address {device.Address} endpoint {pipe.Endpoint.Address}: no free descriptor for the bulk transfer");
            return UsbTransferStatus.Error;
        }

        bool completed = WaitCompletion(ref pipe.Completed, pipe.Event, device, OhciProtocol.BulkTimeoutMs);
        OhciConditionCode code;
        uint bufferPointer;
        using (Lock.Acquire())
        {
            pipe.PendingIndex = -1;
            code = completed ? pipe.CompletionCode : OhciConditionCode.NotAccessed;
            bufferPointer = pipe.CompletionBufferPointer;
        }

        uint start = (uint)pipe.Memory.Buffer.PhysicalAddress;
        transferred = !completed ? 0 : bufferPointer == 0 ? length : (int)Math.Clamp((long)bufferPointer - start, 0, length);
        if (completed && code == OhciConditionCode.NoError)
        {
            return UsbTransferStatus.Success;
        }

        // Whatever the transfer ended with, a device that left has nothing
        // to recover: its ED is about to be unlinked.
        if (device.IsDisconnected)
        {
            return UsbTransferStatus.Disconnected;
        }

        if (!completed)
        {
            // The wait also ends at once when the host binding begins
            // detaching (the kit sets the flag before the bus marks the
            // devices): no budget elapsed, and the close that follows
            // unlinks and quiesces the ED, so no timeout is logged or
            // counted.
            if (_binding.IsDetaching)
            {
                return UsbTransferStatus.Disconnected;
            }

            _binding.Log($"address {device.Address} endpoint {pipe.Endpoint.Address}: bulk transfer timed out");
        }
        else
        {
            _binding.Log($"address {device.Address} endpoint {pipe.Endpoint.Address}: bulk transfer failed, condition code {(uint)code}");
        }

        // The TD is either still queued (timeout) or the error halted the
        // ED: either way the queue has to be cleared before the next.
        QuiesceEndpoint(pipe.Memory, keepToggle: true, device.Address, pipe.Endpoint.Address);
        if (!completed)
        {
            _timeouts++;
            return UsbTransferStatus.Timeout;
        }

        return code == OhciConditionCode.Stall ? UsbTransferStatus.Stall : UsbTransferStatus.Error;
    }

    /// <summary>
    /// Queues one bulk TD over the first <paramref name="length"/> bytes of
    /// the bounce by the tail pointer method: the dummy becomes the TD
    /// (toggle from the ED's carry, rounding on IN, DI 0), the other slot
    /// the fresh dummy, and the tail moves. Under the lock; the caller sets
    /// BLF.
    /// </summary>
    /// <returns>False when the set has no free slot.</returns>
    private static bool QueueBulkTransfer(OhciBulkPipe pipe, int length)
    {
        OhciPipeMemory set = pipe.Memory;
        int transferIndex = set.DummyIndex;
        if (transferIndex < 0)
        {
            return false;
        }

        Span<int> free = stackalloc int[OhciProtocol.BulkTransferSlots];
        if (set.FreeIndices(free) < 1)
        {
            return false;
        }

        int dummyIndex = free[0];
        ZeroTransfer(set, dummyIndex);

        bool isIn = pipe.Endpoint.IsIn;
        uint start = (uint)set.Buffer.PhysicalAddress;
        FillTransfer(set, transferIndex,
            TransferControl(isIn ? OhciProtocol.DirectionPidIn : OhciProtocol.DirectionPidOut, OhciProtocol.DataToggleFromCarry, rounding: isIn, OhciProtocol.DelayInterruptImmediate),
            start, start + (uint)length - 1, set.TdSlots[dummyIndex].PhysicalAddress);
        pipe.PendingIndex = transferIndex;
        PublishTail(set, dummyIndex);
        return true;
    }
}
