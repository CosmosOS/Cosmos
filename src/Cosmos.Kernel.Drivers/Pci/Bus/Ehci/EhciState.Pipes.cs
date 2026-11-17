// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Numerics;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// Interrupt pipes: their open into the periodic schedule behind the
/// interval head their period selects, the ring of qTDs kept queued, the
/// close of any pipe, and the recovery the hot-plug thread runs after a
/// transfer error.
/// </summary>
internal sealed partial class EhciState
{
    /// <summary>
    /// Adds an interrupt IN endpoint to the device and starts its
    /// transfers: a pooled memory set, a fresh pipe object, the queue head
    /// with its split masks from the speed and bInterval and Mult from the
    /// additional transactions, clamped to <see cref="EhciProtocol.QhMultMax"/>,
    /// the qTD ring filled with the overlay pointing at its first qTD, then the
    /// link after the interval head and the table entry under the lock.
    /// Thread context; the body of <see cref="EhciDevice.OpenInterruptPipeCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="endpoint">An interrupt IN endpoint of the device.</param>
    /// <param name="handler">Receives every report.</param>
    /// <returns>The pipe; null when the endpoint is not interrupt IN, its id is already open, no DMA memory was left or the device has no transaction translator.</returns>
    internal EhciInterruptPipe? OpenInterruptPipe(EhciDevice device, UsbEndpoint endpoint, UsbReportHandler handler)
    {
        if (endpoint.Type != UsbEndpointType.Interrupt || !endpoint.IsIn || endpoint.Number == 0)
        {
            return null;
        }

        byte id = EhciDevice.EndpointId(endpoint);
        if (id > EhciProtocol.MaxEndpointId)
        {
            return null;
        }

        bool held;
        using (Lock.Acquire())
        {
            held = device.InterruptPipes[id] is not null || device.BulkPipes[id] is not null;
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

        EhciPipeMemory? memory = TakePipeMemory(bulk: false);
        if (memory is null)
        {
            _binding.Log($"address {device.Address}: no DMA memory for the endpoint");
            return null;
        }

        EhciInterruptPipe pipe = new(device, endpoint, handler, memory, id);
        InterruptSchedule(device.Speed, endpoint.Interval, out int interval, out uint startMask, out uint completeMask);
        uint mult = device.Speed == UsbSpeed.High ? Math.Min((uint)endpoint.AdditionalTransactions + 1, EhciProtocol.QhMultMax) : 1;
        pipe.Interval = interval;

        using (Lock.Acquire())
        {
            WriteQueueHead(ref memory.QueueHead.QueueHead, EhciProtocol.LinkTerminate,
                EndpointCharacteristics(device.Address, endpoint.Number, device.Speed, toggleFromTransfer: false, endpoint.MaxPacketSize, controlEndpoint: false),
                EndpointCapabilities(startMask, completeMask, device.TtHubAddress, device.TtPort, mult),
                memory.Transfers[0].TransferPointer, 0);
            FillInterruptRing(pipe);
            DmaBuffer.WriteBarrier();
            LinkPeriodic(memory.QueueHead, interval);
            device.InterruptPipes[id] = pipe;
            pipe.State = EhciPipeState.Running;
        }

        return pipe;
    }

    /// <summary>
    /// Turns a speed and bInterval into the interval head and the split
    /// masks (EHCI 1.0 section 4.7.2.2, USB 2.0 section 9.6.6). High
    /// speed: bInterval clamped to 1..16, 0 treated as 1; 1, 2 and 3 are
    /// micro-frame periods behind the every-frame head with S-mask 0xFF,
    /// 0x55 or 0x11; 4 and above give head <c>min(bInterval - 4, 7)</c>
    /// with S-mask 0x01. Full and low speed: bInterval in frames, 0
    /// treated as 1, rounded down to a power of two and clamped to 1..128,
    /// S-mask 0x01 and C-mask 0x1C for the split transactions.
    /// </summary>
    private static void InterruptSchedule(UsbSpeed speed, byte interval, out int head, out uint startMask, out uint completeMask)
    {
        if (speed == UsbSpeed.High)
        {
            int value = Math.Clamp(interval == 0 ? 1 : interval, 1, EhciProtocol.HighSpeedMaxInterval);
            completeMask = 0;
            if (value <= EhciProtocol.HighSpeedMicroFrameIntervalLimit)
            {
                head = 0;
                startMask = value switch
                {
                    1 => EhciProtocol.StartMaskEveryMicroFrame,
                    2 => EhciProtocol.StartMaskEveryOtherMicroFrame,
                    _ => EhciProtocol.StartMaskEveryFourthMicroFrame
                };
                return;
            }

            head = Math.Min(value - EhciProtocol.HighSpeedFrameIntervalBase, EhciProtocol.MaxIntervalIndex);
            startMask = EhciProtocol.StartMaskFirstMicroFrame;
            return;
        }

        int frames = Math.Clamp(interval == 0 ? 1 : interval, 1, EhciProtocol.FullSpeedMaxFrames);
        head = Math.Min(BitOperations.Log2((uint)frames), EhciProtocol.MaxIntervalIndex);
        startMask = EhciProtocol.StartMaskFirstMicroFrame;
        completeMask = EhciProtocol.CompleteMaskSplit;
    }

    /// <summary>
    /// Fills the pipe's qTD ring from scratch: slot i carries buffer i and
    /// points at slot i + 1, the last at the dummy slot after them, every
    /// token Active with IOC; the head is slot 0 and the dummy the slot
    /// after the last buffer. The caller issues the barrier and either
    /// links the queue head or points its overlay at slot 0. Under the
    /// lock, the queue head unlinked.
    /// </summary>
    private static void FillInterruptRing(EhciInterruptPipe pipe)
    {
        EhciDescriptorSlot[] slots = pipe.Memory.Transfers;
        int count = pipe.BufferCount;
        for (int i = 0; i <= count; i++)
        {
            slots[i].Clear();
        }

        for (int i = 0; i < count; i++)
        {
            ref EhciTransferDescriptor transfer = ref slots[i].TransferDescriptor;
            FillTransfer(ref transfer, slots[i + 1].TransferPointer, EhciProtocol.LinkTerminate, pipe.BufferAddress(i), pipe.TransferSize);
            transfer.Token = TransferToken(EhciProtocol.PidIn, pipe.TransferSize, dataToggle: false, interruptOnComplete: true);
            pipe.SetBufferOf(i, i);
        }

        pipe.Head = 0;
        pipe.Dummy = count;
    }

    // --- Closing pipes ---

    /// <summary>
    /// Closes a pipe in three steps. 1, under the lock: a closed pipe
    /// returns; an interrupt pipe goes <see cref="EhciPipeState.Closing"/>
    /// so the drain neither re-queues its buffers nor starts a recovery and
    /// a recovery under way leaves it alone; a bulk pipe's claim is awaited
    /// and taken. 2, when the controller runs (whether or not the device is
    /// disconnected or the host binding detaches: the schedule is the
    /// controller's, not the device's): the queue head leaves the schedule,
    /// through the two-frame settle of the periodic list or the async
    /// doorbell. 3, under the lock: the table entry cleared, the pipe
    /// marked closed, its memory returned to the pool, or retired when the
    /// doorbell went unacknowledged or a bulk transfer still held the pipe
    /// at the deadline (its thread quiesces the set later, so the set must
    /// not be another pipe's by then). A halted or vanished controller
    /// fetches nothing, so step 2 is skipped and the set pooled. Thread
    /// context; the body of <see cref="EhciDevice.ClosePipeCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="pipe">A pipe this device opened.</param>
    internal void ClosePipe(EhciDevice device, UsbPipe pipe)
    {
        EhciInterruptPipe? interruptPipe = pipe as EhciInterruptPipe;
        EhciBulkPipe? bulkPipe = pipe as EhciBulkPipe;
        if (interruptPipe is null && bulkPipe is null)
        {
            return;
        }

        byte id = interruptPipe is not null ? interruptPipe.EndpointId : bulkPipe!.EndpointId;
        EhciPipeMemory memory = interruptPipe is not null ? interruptPipe.Memory : bulkPipe!.Memory;

        // 1.
        bool retired;
        bool claimed = false;
        long deadline = DeadlineAfter(EhciProtocol.BulkTimeoutMs);
        while (true)
        {
            using (Lock.Acquire())
            {
                if (pipe.IsClosed)
                {
                    return;
                }

                retired = memory.Retired;
                if (interruptPipe is not null)
                {
                    interruptPipe.State = EhciPipeState.Closing;
                    claimed = true;
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
            // outlives it has already been logged by RunBulkTransfer, and
            // step 3 retires the set that thread will still quiesce.
            if (Stopwatch.GetTimestamp() >= deadline)
            {
                break;
            }

            _binding.Delay(EhciProtocol.PollMicroseconds);
        }

        // 2.
        bool released = true;
        if (!retired && ControllerRuns)
        {
            if (interruptPipe is not null)
            {
                UnlinkPeriodicQueueHead(memory.QueueHead);
            }
            else
            {
                released = UnlinkAsyncQueueHead(memory.QueueHead);
            }
        }
        else
        {
            using (Lock.Acquire())
            {
                RemoveFromList(memory.QueueHead);
            }
        }

        // 3.
        using (Lock.Acquire())
        {
            if (!released || !claimed)
            {
                RetirePipeMemory(memory);
            }

            if (interruptPipe is not null)
            {
                device.InterruptPipes[id] = null;
                interruptPipe.MarkClosed();
                interruptPipe.State = EhciPipeState.Stopped;
            }
            else
            {
                device.BulkPipes[id] = null;
                bulkPipe!.MarkClosed();
                bulkPipe.PendingTransfer = false;
            }

            ReturnPipeMemory(memory);
        }

        _pipesClosed++;
    }

    // --- Recovery ---

    /// <summary>
    /// The hot-plug thread's recovery pass: every interrupt pipe the drain
    /// left in <see cref="EhciPipeState.Recovering"/> is snapshotted under
    /// the lock into the preallocated array (a full array leaves the rest
    /// for the next pass) and recovered outside it, each inside a try/catch
    /// that logs and stops the pipe. Thread context, the hot-plug thread,
    /// never the kit worker: a recovery waits out transfer budgets.
    /// </summary>
    private void RecoverPipes()
    {
        if (!_recoveryRequested)
        {
            return;
        }

        _recoveryRequested = false;
        int count = 0;
        bool more = false;
        using (Lock.Acquire())
        {
            for (int address = 1; address < _devices.Length && !more; address++)
            {
                EhciDevice? device = _devices[address];
                if (device is null)
                {
                    continue;
                }

                EhciInterruptPipe?[] pipes = device.InterruptPipes;
                for (int id = 0; id < pipes.Length; id++)
                {
                    EhciInterruptPipe? pipe = pipes[id];
                    if (pipe is null || pipe.State != EhciPipeState.Recovering)
                    {
                        continue;
                    }

                    if (count == _recoverySnapshot.Length)
                    {
                        more = true;
                        break;
                    }

                    _recoverySnapshot[count++] = pipe;
                }
            }
        }

        if (more)
        {
            _recoveryRequested = true;
        }

        for (int i = 0; i < count; i++)
        {
            EhciInterruptPipe? pipe = _recoverySnapshot[i];
            _recoverySnapshot[i] = null;
            if (pipe is null)
            {
                continue;
            }

            try
            {
                RecoverPipe(pipe);
            }
            catch (Exception exception)
            {
                _binding.Log($"address {pipe.Device.Address} endpoint {pipe.EndpointId}: {exception.Message}");
                StopPipe(pipe);
            }
        }
    }

    /// <summary>
    /// Recovers one interrupt pipe: a disconnected device stops it; a
    /// STALL is cleared on the device with CLEAR_FEATURE(ENDPOINT_HALT)
    /// through the control path first, a failure stopping the pipe; then,
    /// under the lock, provided the pipe is still this device's, still
    /// recovering, not closed and the device still there (else its slots
    /// may belong to another pipe by now), the queue head leaves the
    /// periodic schedule in the same hold as that check (between two holds
    /// a close could pool the set and a reopen relink it, and the unlink
    /// would then take the new pipe off the schedule), the frame settle is
    /// waited outside it, and under the lock, after the same check again,
    /// the ring is rebuilt from the pipe's buffers, the overlay pointed at
    /// its first qTD with the toggle returned to 0 after a cleared halt and
    /// kept otherwise (the device's did not move on a non-STALL error), the
    /// queue head linked again and the pipe running. Thread context.
    /// </summary>
    private void RecoverPipe(EhciInterruptPipe pipe)
    {
        EhciDevice device = pipe.Device;
        if (device.IsDisconnected)
        {
            StopPipe(pipe);
            return;
        }

        bool clearedHalt = false;
        if (pipe.StalledByDevice)
        {
            UsbSetupPacket clearHalt = new(UsbRequestType.HostToDevice | UsbRequestType.Standard | UsbRequestType.Endpoint,
                (byte)UsbStandardRequest.ClearFeature, EhciProtocol.EndpointHaltFeature, pipe.Endpoint.Address, 0);
            if (ControlTransfer(device, clearHalt, Span<byte>.Empty) != UsbTransferStatus.Success)
            {
                _binding.Log($"address {device.Address} endpoint {pipe.EndpointId}: pipe recovery failed");
                StopPipe(pipe);
                return;
            }

            clearedHalt = true;
        }

        using (Lock.Acquire())
        {
            if (!IsRecovering(device, pipe))
            {
                return;
            }

            RemoveFromList(pipe.Memory.QueueHead);
        }

        if (ControllerRuns)
        {
            WaitFrames(EhciProtocol.PeriodicUnlinkFrames);
        }

        using (Lock.Acquire())
        {
            if (!IsRecovering(device, pipe))
            {
                return;
            }

            EhciPipeMemory memory = pipe.Memory;
            FillInterruptRing(pipe);

            ref EhciQueueHead head = ref memory.QueueHead.QueueHead;
            DmaBuffer.ReadBarrier();
            uint token = clearedHalt ? 0 : Volatile.Read(ref head.Token) & EhciProtocol.TokenDataToggle;
            head.CurrentTransfer = 0;
            head.NextTransfer = memory.Transfers[0].TransferPointer;
            head.AlternateNextTransfer = EhciProtocol.LinkTerminate;
            head.Token = token;
            head.Buffer0 = 0;
            head.Buffer1 = 0;
            head.Buffer2 = 0;
            head.Buffer3 = 0;
            head.Buffer4 = 0;
            DmaBuffer.WriteBarrier();

            LinkPeriodic(memory.QueueHead, pipe.Interval);
            pipe.State = EhciPipeState.Running;
        }
    }

    /// <summary>The re-validation before a recovery touches a descriptor: the pipe is still the device's at its id, still recovering, not closed, and the device still there. Under the lock.</summary>
    private static bool IsRecovering(EhciDevice device, EhciInterruptPipe pipe)
    {
        return ReferenceEquals(device.InterruptPipes[pipe.EndpointId], pipe)
            && pipe.State == EhciPipeState.Recovering
            && !pipe.IsClosed
            && !device.IsDisconnected;
    }

    /// <summary>Stops an interrupt pipe for good from thread context, counted and logged; a pipe already stopped, closing or closed is left alone. Thread context.</summary>
    private void StopPipe(EhciInterruptPipe pipe)
    {
        bool stopped = false;
        using (Lock.Acquire())
        {
            if (!pipe.IsClosed && pipe.State is not (EhciPipeState.Stopped or EhciPipeState.Closing))
            {
                pipe.State = EhciPipeState.Stopped;
                _pipesStopped++;
                stopped = true;
            }
        }

        if (stopped)
        {
            _binding.Log($"address {pipe.Device.Address} endpoint {pipe.EndpointId}: pipe stopped");
        }
    }
}
