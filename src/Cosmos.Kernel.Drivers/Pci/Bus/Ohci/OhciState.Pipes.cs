// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// Interrupt pipes: their open into the interrupt tree with every buffer
/// queued, the close of any pipe (the design's missing controller
/// command), and the recovery of a pipe the drain found halted, run on
/// the hot-plug thread.
/// </summary>
internal sealed partial class OhciState
{
    /// <summary>
    /// Adds an interrupt IN endpoint to the device and starts its
    /// transfers: a pooled memory set, a fresh pipe object, the ED built
    /// for the endpoint, then, under the lock, the set's TDs recorded for
    /// the pipe, one TD per buffer queued, the ED linked after the interval
    /// head its period selects and the table entry. Thread context; the
    /// body of <see cref="OhciDevice.OpenInterruptPipeCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="endpoint">An interrupt IN endpoint of the device.</param>
    /// <param name="handler">Receives every report.</param>
    /// <returns>The pipe; null when the endpoint is not interrupt IN, its index is already open, or no DMA memory was left.</returns>
    internal OhciInterruptPipe? OpenInterruptPipe(OhciDevice device, UsbEndpoint endpoint, UsbReportHandler handler)
    {
        if (endpoint.Type != UsbEndpointType.Interrupt || !endpoint.IsIn)
        {
            return null;
        }

        byte index = OhciDevice.EndpointIndex(endpoint);
        if (endpoint.Number == 0 || index > OhciProtocol.MaxEndpointIndex)
        {
            return null;
        }

        bool open;
        using (Lock.Acquire())
        {
            open = device.InterruptPipes[index] is not null || device.BulkPipes[index] is not null;
        }

        if (open)
        {
            _binding.Log($"address {device.Address} endpoint {endpoint.Address}: the endpoint is already open");
            return null;
        }

        OhciPipeMemory? memory = TakePipeMemory(bulk: false);
        if (memory is null)
        {
            _binding.Log($"address {device.Address}: no DMA memory for the endpoint");
            return null;
        }

        int exponent = IntervalExponent(endpoint.Interval);
        OhciInterruptPipe pipe = new(endpoint, handler, memory, index, exponent);
        BuildEndpoint(memory, device.Address, endpoint.Number, OhciProtocol.EndpointDirectionIn, device.Speed, endpoint.MaxPacketSize);

        bool queued;
        using (Lock.Acquire())
        {
            SetOwner(memory, pipe);
            queued = QueueAllBuffers(pipe);
            if (queued)
            {
                pipe.State = OhciPipeState.Running;
                LinkEndpoint(IntervalHead(exponent), memory.Endpoint);
                device.InterruptPipes[index] = pipe;
            }
            else
            {
                SetOwner(memory, null);
                ReturnPipeMemory(memory);
            }
        }

        if (!queued)
        {
            _binding.Log($"address {device.Address} endpoint {endpoint.Address}: no free descriptor for the pipe");
            return null;
        }

        return pipe;
    }

    /// <summary>
    /// Queues one TD per buffer on a pipe whose ED has nothing queued
    /// (every slot but the dummy free): the dummy becomes the first TD,
    /// the next free slots the following ones, the last free slot the
    /// fresh dummy, each TD IN with the toggle from the ED's carry,
    /// rounding on and DI 0 over its buffer, filled first buffer first,
    /// the order the ED runs them in, since the drain takes the queue
    /// order from the fill order. Fewer buffers than the pipe wants are
    /// queued when quarantines left fewer slots. Under the lock (or before
    /// the ED is linked).
    /// </summary>
    /// <returns>False when not even one buffer could be queued.</returns>
    private static bool QueueAllBuffers(OhciInterruptPipe pipe)
    {
        OhciPipeMemory set = pipe.Memory;
        int first = set.DummyIndex;
        if (first < 0)
        {
            return false;
        }

        Span<int> free = stackalloc int[OhciProtocol.InterruptTransferSlots];
        int count = Math.Min(pipe.BufferCount, set.FreeIndices(free));
        if (count < 1)
        {
            return false;
        }

        Span<int> chain = stackalloc int[OhciProtocol.InterruptTransferSlots];
        chain[0] = first;
        for (int i = 1; i < count; i++)
        {
            chain[i] = free[i - 1];
        }

        int dummyIndex = free[count - 1];
        ZeroTransfer(set, dummyIndex);
        for (int i = 0; i < count; i++)
        {
            uint next = i == count - 1 ? set.TdSlots[dummyIndex].PhysicalAddress : set.TdSlots[chain[i + 1]].PhysicalAddress;
            uint start = pipe.BufferAddress(i);
            FillTransfer(set, chain[i],
                TransferControl(OhciProtocol.DirectionPidIn, OhciProtocol.DataToggleFromCarry, rounding: true, OhciProtocol.DelayInterruptImmediate),
                start, start + (uint)pipe.TransferSize - 1, next);
            pipe.TdBuffer[chain[i]] = i;
        }

        pipe.TdBuffer[dummyIndex] = -1;
        PublishTail(set, dummyIndex);
        return true;
    }

    // --- Closing pipes ---

    /// <summary>
    /// Closes a pipe in three steps. 1, under the lock: a closed pipe
    /// returns; an interrupt pipe goes <see cref="OhciPipeState.Closing"/>
    /// so the drain neither re-queues its buffers nor starts a recovery,
    /// and its descriptor claim is awaited and taken (a recovery may hold
    /// it); a bulk pipe's claim is awaited and taken. 2, when the
    /// controller runs, whether or not the device is connected (the lists
    /// are the controller's, not the device's) and whether or not the host
    /// binding detaches: the ED is taken out of its list with the two-frame
    /// settle, then quiesced so every slot still in flight is delivered or
    /// quarantined. On a controller that stopped or vanished only the
    /// software links are dropped. 3, under the lock: the table entry
    /// cleared, the pipe marked closed, its set's TDs disowned and the set
    /// returned to its pool. Thread context; the body of
    /// <see cref="OhciDevice.ClosePipeCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="pipe">A pipe this device opened.</param>
    internal void ClosePipe(OhciDevice device, UsbPipe pipe)
    {
        OhciInterruptPipe? interruptPipe = pipe as OhciInterruptPipe;
        OhciBulkPipe? bulkPipe = pipe as OhciBulkPipe;
        if (interruptPipe is null && bulkPipe is null)
        {
            return;
        }

        byte index = interruptPipe is not null ? interruptPipe.Index : bulkPipe!.Index;
        OhciPipeMemory memory = interruptPipe is not null ? interruptPipe.Memory : bulkPipe!.Memory;

        // 1.
        long deadline = DeadlineAfter(OhciProtocol.BulkTimeoutMs);
        while (true)
        {
            bool claimed;
            using (Lock.Acquire())
            {
                if (pipe.IsClosed)
                {
                    return;
                }

                if (interruptPipe is not null)
                {
                    interruptPipe.State = OhciPipeState.Closing;
                    claimed = !interruptPipe.Busy;
                    if (claimed)
                    {
                        interruptPipe.Busy = true;
                    }
                }
                else
                {
                    claimed = !bulkPipe!.Busy;
                    if (claimed)
                    {
                        bulkPipe.Busy = true;
                    }
                }
            }

            if (claimed)
            {
                break;
            }

            // Bounded by the holder's own BulkTimeoutMs: a transfer that
            // outlives it has already been logged by RunBulkTransfer.
            if (Stopwatch.GetTimestamp() >= deadline)
            {
                break;
            }

            _binding.Delay(OhciProtocol.PollMicroseconds);
        }

        // 2.
        if (ControllerRuns)
        {
            UnlinkEndpoint(memory.Endpoint);
            QuiesceEndpoint(memory, keepToggle: false, device.Address, pipe.Endpoint.Address);
        }
        else
        {
            using (Lock.Acquire())
            {
                UnlinkEndpointCore(memory.Endpoint);
            }
        }

        // 3.
        using (Lock.Acquire())
        {
            if (interruptPipe is not null)
            {
                if (ReferenceEquals(device.InterruptPipes[index], interruptPipe))
                {
                    device.InterruptPipes[index] = null;
                }

                interruptPipe.MarkClosed();
                interruptPipe.State = OhciPipeState.Stopped;
                interruptPipe.Busy = false;
            }
            else
            {
                if (ReferenceEquals(device.BulkPipes[index], bulkPipe))
                {
                    device.BulkPipes[index] = null;
                }

                bulkPipe!.MarkClosed();
                bulkPipe.PendingIndex = -1;
                bulkPipe.Busy = false;
            }

            SetOwner(memory, null);
            ReturnPipeMemory(memory);
        }

        _pipesClosed++;
    }

    // --- Recovery ---

    /// <summary>
    /// One recovery pass of the hot-plug thread: every interrupt pipe the
    /// drain left in <see cref="OhciPipeState.Recovering"/> is snapshotted
    /// under the lock into the preallocated arrays (a batch at a time; the
    /// rest asks for another pass) and recovered outside it, each inside a
    /// try/catch that logs and stops the pipe. Thread context, the
    /// hot-plug thread, never the kit worker, whose queue must not wait
    /// behind USB budgets.
    /// </summary>
    private void RecoverPipes()
    {
        if (!_recoveryRequested)
        {
            return;
        }

        _recoveryRequested = false;
        int count = 0;
        using (Lock.Acquire())
        {
            for (int address = 1; address < _devices.Length; address++)
            {
                OhciDevice? device = _devices[address];
                if (device is null)
                {
                    continue;
                }

                OhciInterruptPipe?[] pipes = device.InterruptPipes;
                for (int i = 0; i < pipes.Length; i++)
                {
                    OhciInterruptPipe? pipe = pipes[i];
                    if (pipe is null || pipe.State != OhciPipeState.Recovering)
                    {
                        continue;
                    }

                    if (count == _recoveryPipes.Length)
                    {
                        _recoveryRequested = true;
                        break;
                    }

                    _recoveryDevices[count] = device;
                    _recoveryPipes[count] = pipe;
                    count++;
                }
            }
        }

        for (int i = 0; i < count; i++)
        {
            OhciDevice? device = _recoveryDevices[i];
            OhciInterruptPipe? pipe = _recoveryPipes[i];
            _recoveryDevices[i] = null;
            _recoveryPipes[i] = null;
            if (device is null || pipe is null)
            {
                continue;
            }

            try
            {
                RecoverPipe(device, pipe);
            }
            catch (Exception exception)
            {
                _binding.Log($"address {device.Address} endpoint {pipe.Endpoint.Address}: {exception.Message}");
                using (Lock.Acquire())
                {
                    if (pipe.State == OhciPipeState.Recovering)
                    {
                        StopPipe(pipe);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Recovers one pipe: a disconnected device stops it; a STALL needs
    /// CLEAR_FEATURE(ENDPOINT_HALT) through the device's control path
    /// first (a failure stops the pipe, logged); then, with the pipe still
    /// this device's, still recovering, not closed and the device still
    /// there (a close meanwhile marks it closing, which this re-validation
    /// sees before any descriptor is touched), its descriptors are claimed,
    /// the ED quiesced (the toggle carry cleared after a CLEAR_FEATURE,
    /// kept otherwise, since the device's toggle did not move), every
    /// buffer queued again with fresh generations and the pipe runs.
    /// Thread context, the hot-plug thread.
    /// </summary>
    private void RecoverPipe(OhciDevice device, OhciInterruptPipe pipe)
    {
        if (device.IsDisconnected)
        {
            using (Lock.Acquire())
            {
                if (pipe.State == OhciPipeState.Recovering)
                {
                    StopPipe(pipe);
                }
            }

            return;
        }

        bool clearedHalt = false;
        if (pipe.StalledByDevice)
        {
            UsbSetupPacket clearHalt = new(UsbRequestType.HostToDevice | UsbRequestType.Standard | UsbRequestType.Endpoint,
                (byte)UsbStandardRequest.ClearFeature, OhciProtocol.EndpointHaltFeature, pipe.Endpoint.Address, 0);
            if (ControlTransfer(device, clearHalt, Span<byte>.Empty) != UsbTransferStatus.Success)
            {
                _binding.Log($"address {device.Address} endpoint {pipe.Endpoint.Address}: pipe recovery failed");
                using (Lock.Acquire())
                {
                    if (pipe.State == OhciPipeState.Recovering)
                    {
                        StopPipe(pipe);
                    }
                }

                return;
            }

            clearedHalt = true;
        }

        using (Lock.Acquire())
        {
            if (!IsRecoverable(device, pipe) || pipe.Busy)
            {
                return;
            }

            pipe.Busy = true;
        }

        try
        {
            QuiesceEndpoint(pipe.Memory, keepToggle: !clearedHalt, device.Address, pipe.Endpoint.Address);
            using (Lock.Acquire())
            {
                if (!IsRecoverable(device, pipe))
                {
                    return;
                }

                pipe.StalledByDevice = false;
                if (QueueAllBuffers(pipe))
                {
                    pipe.State = OhciPipeState.Running;
                }
                else
                {
                    StopPipe(pipe);
                }
            }
        }
        finally
        {
            using (Lock.Acquire())
            {
                pipe.Busy = false;
            }
        }
    }

    /// <summary>The re-validation before a recovery touches a descriptor: the pipe is still the device's at its index, still recovering, not closed, and the device is still there. Under the lock.</summary>
    private static bool IsRecoverable(OhciDevice device, OhciInterruptPipe pipe)
    {
        return ReferenceEquals(device.InterruptPipes[pipe.Index], pipe)
            && pipe.State == OhciPipeState.Recovering
            && !pipe.IsClosed
            && !device.IsDisconnected;
    }
}
