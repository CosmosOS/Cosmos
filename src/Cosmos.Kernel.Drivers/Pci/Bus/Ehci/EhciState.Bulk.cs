// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// Bulk pipes: their open into the async schedule, the synchronous IN and
/// OUT transfers through the 64 KiB bounce one five-page qTD at a time,
/// and the endpoint reset that returns the host side to DATA0.
/// </summary>
internal sealed partial class EhciState
{
    /// <summary>
    /// Adds a bulk endpoint to the device: an already open id returns the
    /// existing pipe; otherwise a pooled memory set, a fresh pipe object,
    /// the queue head (DTC clear, so the overlay keeps the toggle across
    /// qTDs; the overlay idling on the set's dummy) and its link after the
    /// async head under the lock. Thread context; the body of
    /// <see cref="EhciDevice.OpenBulkPipeCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="endpoint">A bulk endpoint of the device.</param>
    /// <returns>The pipe; null when the endpoint is not bulk, its id is held by an interrupt pipe, no DMA memory was left or the device has no transaction translator.</returns>
    internal EhciBulkPipe? OpenBulkPipe(EhciDevice device, UsbEndpoint endpoint)
    {
        if (endpoint.Type != UsbEndpointType.Bulk || endpoint.Number == 0)
        {
            return null;
        }

        byte id = EhciDevice.EndpointId(endpoint);
        if (id > EhciProtocol.MaxEndpointId)
        {
            return null;
        }

        bool held = false;
        using (Lock.Acquire())
        {
            EhciBulkPipe? existing = device.BulkPipes[id];
            if (existing is not null)
            {
                return existing;
            }

            held = device.InterruptPipes[id] is not null;
        }

        if (held)
        {
            _binding.Log($"address {device.Address} endpoint {id}: the endpoint is already open");
            return null;
        }

        if (device.Speed is UsbSpeed.Low or UsbSpeed.Full && device.TtHubAddress == 0)
        {
            _binding.Log($"address {device.Address}: no high-speed hub above a full-speed device");
            return null;
        }

        EhciPipeMemory? memory = TakePipeMemory(bulk: true);
        if (memory is null)
        {
            _binding.Log($"address {device.Address}: no DMA memory for the endpoint");
            return null;
        }

        EhciBulkPipe pipe = new(endpoint, memory, id);
        using (Lock.Acquire())
        {
            WriteQueueHead(ref memory.QueueHead.QueueHead, EhciProtocol.LinkTerminate,
                EndpointCharacteristics(device.Address, endpoint.Number, device.Speed, toggleFromTransfer: false, endpoint.MaxPacketSize, controlEndpoint: false),
                ControlCapabilities(device), memory.Transfers[0].TransferPointer, 0);
            pipe.Dummy = 0;
            DmaBuffer.WriteBarrier();
            LinkAsync(memory.QueueHead);
            device.BulkPipes[id] = pipe;
        }

        return pipe;
    }

    /// <summary>
    /// Reads from a bulk IN pipe and waits: the pipe claimed (Disconnected
    /// at once when the device left, Timeout when a holder kept it past the
    /// budget), then rounds of at most the bounce's length, each in chunks
    /// of five pages, one qTD each, copied out as they land; a short packet
    /// ends the transfer. Thread context; the body of
    /// <see cref="EhciDevice.BulkInCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="pipe">A bulk IN pipe of the device.</param>
    /// <param name="data">Receives the data; its length is the most the transfer reads.</param>
    /// <param name="transferred">Bytes received, set on failure too.</param>
    internal UsbTransferStatus BulkIn(EhciDevice device, UsbPipe pipe, Span<byte> data, out int transferred)
    {
        transferred = 0;
        if (pipe is not EhciBulkPipe bulkPipe || bulkPipe.IsClosed || !bulkPipe.Endpoint.IsIn)
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
                int round = Math.Min(data.Length - transferred, bulkPipe.Memory.BufferLength);
                int chunkStart = 0;
                while (chunkStart < round)
                {
                    int length = Math.Min(round - chunkStart, EhciProtocol.BulkChunkBytes);
                    UsbTransferStatus status = RunBulkTransfer(device, bulkPipe, chunkStart, length, out int received);
                    bulkPipe.Memory.Buffer.Span.Slice(chunkStart, received).CopyTo(data.Slice(transferred));
                    transferred += received;

                    // A short packet ends the transfer: the device has sent all it had.
                    if (status != UsbTransferStatus.Success || received < length)
                    {
                        return status;
                    }

                    chunkStart += length;
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
    /// Writes to a bulk OUT pipe and waits: the pipe claimed, then rounds
    /// of at most the bounce's length copied in and sent in chunks of five
    /// pages, one qTD each. Thread context; the body of
    /// <see cref="EhciDevice.BulkOutCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="pipe">A bulk OUT pipe of the device.</param>
    /// <param name="data">The data to send.</param>
    /// <param name="transferred">Bytes the device accepted, set on failure too.</param>
    internal UsbTransferStatus BulkOut(EhciDevice device, UsbPipe pipe, ReadOnlySpan<byte> data, out int transferred)
    {
        transferred = 0;
        if (pipe is not EhciBulkPipe bulkPipe || bulkPipe.IsClosed || bulkPipe.Endpoint.IsIn)
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
                int round = Math.Min(data.Length - transferred, bulkPipe.Memory.BufferLength);
                data.Slice(transferred, round).CopyTo(bulkPipe.Memory.Buffer.Span);
                int chunkStart = 0;
                while (chunkStart < round)
                {
                    int length = Math.Min(round - chunkStart, EhciProtocol.BulkChunkBytes);
                    UsbTransferStatus status = RunBulkTransfer(device, bulkPipe, chunkStart, length, out int sent);
                    transferred += sent;
                    if (status != UsbTransferStatus.Success)
                    {
                        return status;
                    }

                    chunkStart += length;
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
    /// data toggle 0. The queue head is quiesced with its overlay token
    /// written 0 while unlinked. Thread context; the body of
    /// <see cref="EhciDevice.ResetEndpointCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="pipe">A pipe of the device.</param>
    /// <returns>False for an interrupt pipe, a closed pipe, a disconnected device, a retired set or an unacknowledged doorbell.</returns>
    internal bool ResetEndpoint(EhciDevice device, UsbPipe pipe)
    {
        if (pipe is not EhciBulkPipe bulkPipe || bulkPipe.IsClosed)
        {
            return false;
        }

        if (ClaimBulk(device, bulkPipe) != UsbTransferStatus.Success)
        {
            return false;
        }

        try
        {
            if (device.IsDisconnected || bulkPipe.Memory.Retired)
            {
                return false;
            }

            return QuiesceBulk(bulkPipe, keepToggle: false);
        }
        finally
        {
            ReleaseBulk(bulkPipe);
        }
    }

    /// <summary>
    /// Moves <paramref name="length"/> bytes between the bounce at
    /// <paramref name="offset"/> and the device as one qTD and waits: the
    /// dummy filled as the transfer, the other slot zeroed as the new
    /// dummy, the token written Active last under the lock; the wait; the
    /// result read under the lock. A halt or a timeout other than a
    /// disconnection is logged and followed by the quiesce, which keeps
    /// the toggle (the device's did not move), so the queue head is sane
    /// for the next transfer. Thread context; the caller owns the pipe.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="pipe">The pipe.</param>
    /// <param name="offset">Where in the bounce the chunk starts, a page boundary.</param>
    /// <param name="length">Bytes of this qTD, at most five pages.</param>
    /// <param name="transferred">Bytes the qTD moved; 0 on a timeout.</param>
    private UsbTransferStatus RunBulkTransfer(EhciDevice device, EhciBulkPipe pipe, int offset, int length, out int transferred)
    {
        transferred = 0;
        EhciPipeMemory memory = pipe.Memory;
        if (memory.Retired)
        {
            return UsbTransferStatus.Error;
        }

        using (Lock.Acquire())
        {
            Volatile.Write(ref pipe.Completed, false);
            EhciDescriptorSlot[] slots = memory.Transfers;
            int slot = pipe.Dummy;
            int newDummy = (slot + 1) % slots.Length;
            slots[newDummy].Clear();

            ref EhciTransferDescriptor transfer = ref slots[slot].TransferDescriptor;
            FillTransfer(ref transfer, slots[newDummy].TransferPointer, EhciProtocol.LinkTerminate, memory.Buffer.PhysicalAddress + (ulong)offset, length);
            pipe.TransferSlot = slot;
            pipe.Dummy = newDummy;
            pipe.CompletionToken = 0;
            pipe.PendingTransfer = true;
            DmaBuffer.WriteBarrier();
            Volatile.Write(ref transfer.Token, TransferToken(pipe.Endpoint.IsIn ? EhciProtocol.PidIn : EhciProtocol.PidOut, length, dataToggle: false, interruptOnComplete: true));
        }

        bool completed = WaitCompletion(ref pipe.Completed, pipe.Event, device, EhciProtocol.BulkTimeoutMs);
        uint token;
        using (Lock.Acquire())
        {
            pipe.PendingTransfer = false;
            token = completed ? pipe.CompletionToken : 0;
        }

        if (!completed)
        {
            if (device.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            if (!_binding.IsDetaching)
            {
                _binding.Log($"address {device.Address} endpoint {pipe.EndpointId}: bulk transfer timed out");
                _timeouts++;
            }

            QuiesceBulk(pipe, keepToggle: true);
            return UsbTransferStatus.Timeout;
        }

        transferred = length - Math.Min(RemainingBytes(token), length);
        if ((token & EhciProtocol.TokenHalted) == 0)
        {
            return UsbTransferStatus.Success;
        }

        // Whatever the transfer ended with, a device that left has nothing
        // to recover: its queue head is about to leave the schedule.
        if (device.IsDisconnected)
        {
            return UsbTransferStatus.Disconnected;
        }

        _binding.Log($"address {device.Address} endpoint {pipe.EndpointId}: bulk transfer failed, token status 0x{token & EhciProtocol.TokenStatusMask:x}");
        QuiesceBulk(pipe, keepToggle: true);
        return IsStall(token) ? UsbTransferStatus.Stall : UsbTransferStatus.Error;
    }

    /// <summary>Empties a bulk pipe's queue head through the schedule partial's quiesce; an unacknowledged doorbell retires the set. Thread context; the caller owns the pipe.</summary>
    /// <returns>False when the controller may still hold the queue head.</returns>
    private bool QuiesceBulk(EhciBulkPipe pipe, bool keepToggle)
    {
        EhciPipeMemory memory = pipe.Memory;
        if (QuiesceQueueHead(memory.QueueHead, memory.Transfers, pipe.Dummy, keepToggle))
        {
            return true;
        }

        using (Lock.Acquire())
        {
            RetirePipeMemory(memory);
        }

        return false;
    }
}
