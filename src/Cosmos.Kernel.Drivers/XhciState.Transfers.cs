// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Control transfers on a slot's default endpoint: the Setup, Data and
/// Status stages, the synchronous wait, the recovery of a halted control
/// endpoint, and the hand-off of the control ring to an interrupt pipe
/// recovery that waits for its CLEAR_FEATURE.
/// </summary>
public sealed partial class XhciState
{
    /// <summary>A Setup Stage TRB carries the 8-byte SETUP packet as immediate data.</summary>
    private const uint SetupPacketLength = 8;

    /// <summary>ENDPOINT_HALT feature selector (USB 2.0 table 9-6).</summary>
    private const ushort EndpointHaltFeature = 0;

    /// <summary>
    /// Runs a control transfer on the slot's default endpoint and waits for
    /// it: the control ring claimed (Disconnected at once when the device
    /// left, Timeout when another owner kept it past the budget), a halted
    /// endpoint recovered first, the OUT data copied into the bounce page,
    /// the three stages enqueued and the doorbell rung under the lock, the
    /// Status Stage's completion awaited, the IN data copied out. The claim
    /// is released by the hand-off check: an interrupt pipe recovery
    /// waiting for the ring takes it, else the flag clears. Thread context;
    /// the body of <see cref="XhciSlot.ControlTransferCore"/>.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <param name="setup">The request; its Length is the data stage size.</param>
    /// <param name="data">The OUT data or the IN buffer, at least <see cref="UsbSetupPacket.Length"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the setup's Length, or the Length exceeds one page.</exception>
    internal UsbTransferStatus ControlTransfer(XhciSlot slot, UsbSetupPacket setup, Span<byte> data)
    {
        if (data.Length < setup.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(data), data.Length, "The buffer is shorter than the setup's Length.");
        }

        if (setup.Length > PageBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(setup), setup.Length, "The data stage exceeds one page.");
        }

        UsbTransferStatus claim = ClaimControl(slot, XhciProtocol.TransferTimeoutMs);
        if (claim != UsbTransferStatus.Success)
        {
            return claim;
        }

        try
        {
            if (slot.ControlEndpointHalted && !RecoverControlEndpoint(slot))
            {
                return UsbTransferStatus.Error;
            }

            Span<byte> buffer = slot.Memory.ControlBuffer.Span.Slice(0, setup.Length);
            if (!setup.IsDeviceToHost)
            {
                data.Slice(0, setup.Length).CopyTo(buffer);
            }

            using (Lock.Acquire())
            {
                Volatile.Write(ref slot.ControlCompleted, false);
                slot.ControlStatusTrb = EnqueueControlTransfer(slot.Memory.ControlRing, setup, slot.Memory.ControlBuffer.PhysicalAddress);
                RingDoorbell(slot.SlotId, XhciSlot.ControlEndpointId);
            }

            bool completed = WaitCompletion(ref slot.ControlCompleted, slot.ControlEvent, slot, XhciProtocol.TransferTimeoutMs);

            // The address is cleared before the code is read: a stale one
            // would match a later TRB once the 256-entry ring wraps.
            XhciCompletionCode code;
            using (Lock.Acquire())
            {
                slot.ControlStatusTrb = 0;
                code = completed ? slot.ControlCode : XhciCompletionCode.Invalid;
            }

            if (!completed)
            {
                if (slot.IsDisconnected)
                {
                    return UsbTransferStatus.Disconnected;
                }

                _binding.Log("control transfer timed out");
                return UsbTransferStatus.Timeout;
            }

            if (code is XhciCompletionCode.Success or XhciCompletionCode.ShortPacket)
            {
                if (setup.IsDeviceToHost)
                {
                    buffer.CopyTo(data);
                }

                return UsbTransferStatus.Success;
            }

            // Whatever the transfer ended with, a device that left has
            // nothing to recover: its slot is about to be disabled.
            if (slot.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            // Any error halts the endpoint on the controller side, a STALL
            // included: it has to be reset before the next request can run.
            RecoverControlEndpoint(slot);
            return code == XhciCompletionCode.StallError ? UsbTransferStatus.Stall : UsbTransferStatus.Error;
        }
        finally
        {
            using (Lock.Acquire())
            {
                HandOffControlRing(slot);
            }
        }
    }

    /// <summary>
    /// Queues the Setup, optional Data and Status stages of one control
    /// transfer (xHCI 1.2 section 4.11.2.2). Under the lock or in the
    /// handler; the caller rings the doorbell.
    /// </summary>
    /// <param name="ring">The slot's control ring.</param>
    /// <param name="setup">The request.</param>
    /// <param name="dataAddress">The bounce page's physical address; unused without a data stage.</param>
    /// <returns>Address of the Status Stage TRB, the one that interrupts on completion.</returns>
    private static ulong EnqueueControlTransfer(XhciRing ring, UsbSetupPacket setup, ulong dataAddress)
    {
        bool hasData = setup.Length != 0;
        bool isIn = setup.IsDeviceToHost;

        uint transferType = !hasData ? 0 : isIn ? XhciTrb.TransferTypeIn : XhciTrb.TransferTypeOut;
        ring.Enqueue(setup.Pack(), SetupPacketLength,
            XhciTrb.TypeField(XhciTrbType.SetupStage) | XhciTrb.ImmediateData | (transferType << XhciTrb.TransferTypeShift));

        if (hasData)
        {
            ring.Enqueue(dataAddress, setup.Length, XhciTrb.TypeField(XhciTrbType.DataStage) | (isIn ? XhciTrb.DirectionIn : 0));
        }

        // The status stage runs opposite to the data stage, and IN when
        // there is none (USB 2.0 section 8.5.3).
        bool statusIn = !hasData || !isIn;
        return ring.Enqueue(0, 0,
            XhciTrb.TypeField(XhciTrbType.StatusStage) | XhciTrb.InterruptOnCompletion | (statusIn ? XhciTrb.DirectionIn : 0));
    }

    /// <summary>
    /// Takes a halted default endpoint back to running: Reset Endpoint
    /// (Context State Error meaning it was not halted), then Set TR Dequeue
    /// Pointer past the transfers the halt abandoned (xHCI 1.2 section
    /// 4.6.8). The device side needs nothing: a control endpoint's STALL
    /// clears with the next SETUP (USB 2.0 section 8.5.3.4). Thread
    /// context; the caller owns the control ring.
    /// </summary>
    private bool RecoverControlEndpoint(XhciSlot slot)
    {
        XhciCompletionCode code = ExecuteCommand(0,
            EndpointCommand(XhciTrbType.ResetEndpointCommand, slot.SlotId, XhciSlot.ControlEndpointId), out _);
        if (code == XhciCompletionCode.ContextStateError)
        {
            slot.ControlEndpointHalted = false;
            return true;
        }

        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Reset Endpoint", code);
            return false;
        }

        ulong dequeuePointer;
        using (Lock.Acquire())
        {
            XhciRing ring = slot.Memory.ControlRing;
            dequeuePointer = ring.EnqueuePointer | (ring.CycleState ? DequeueCycleState : 0);
        }

        code = ExecuteCommand(dequeuePointer,
            EndpointCommand(XhciTrbType.SetTrDequeuePointerCommand, slot.SlotId, XhciSlot.ControlEndpointId), out _);
        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Set TR Dequeue Pointer", code);
            return false;
        }

        slot.ControlEndpointHalted = false;
        return true;
    }

    /// <summary>
    /// The hand-off check, run by whoever gives up the control ring: the
    /// thread's finally and the completion of a recovery's CLEAR_FEATURE.
    /// The first interrupt pipe waiting in
    /// <see cref="XhciPipeState.SettingDequeuePointer"/> with
    /// <see cref="XhciInterruptPipe.PendingClearHalt"/> takes the ring: its
    /// flag clears, it becomes the recovering pipe, its CLEAR_FEATURE stages
    /// go on the ring with the Status Stage address kept, the doorbell
    /// rings and <see cref="XhciSlot.ControlBusy"/> stays set; with nobody
    /// waiting, on a disconnected slot (its ring is about to be disabled)
    /// or with the control endpoint halted (a CLEAR_FEATURE put on it would
    /// never complete; the waiting pipes keep their flag for the next
    /// thread transfer, which recovers the endpoint first), the flag
    /// clears. Under the lock or in the handler.
    /// </summary>
    private void HandOffControlRing(XhciSlot slot)
    {
        XhciInterruptPipe?[] pipes = slot.InterruptPipes;
        for (int dci = 0; dci < pipes.Length && !slot.IsDisconnected && !slot.ControlEndpointHalted; dci++)
        {
            XhciInterruptPipe? pipe = pipes[dci];
            if (pipe is not null && pipe.PendingClearHalt && pipe.State == XhciPipeState.SettingDequeuePointer)
            {
                pipe.PendingClearHalt = false;
                StartClearHalt(slot, pipe);
                return;
            }
        }

        slot.ControlBusy = false;
    }

    /// <summary>
    /// Puts a recovery's CLEAR_FEATURE(ENDPOINT_HALT, endpoint address) on
    /// the slot's control ring (a Setup Stage, no Data Stage and a Status
    /// Stage that interrupts), records the pipe and the Status Stage
    /// address and rings the doorbell; the caller holds
    /// <see cref="XhciSlot.ControlBusy"/>, which now belongs to the
    /// recovery. Under the lock or in the handler.
    /// </summary>
    private void StartClearHalt(XhciSlot slot, XhciInterruptPipe pipe)
    {
        UsbSetupPacket clearHalt = new(UsbRequestType.HostToDevice | UsbRequestType.Standard | UsbRequestType.Endpoint,
            (byte)UsbStandardRequest.ClearFeature, EndpointHaltFeature, pipe.Endpoint.Address, 0);
        slot.RecoveringPipe = pipe;
        slot.RecoveryStatusTrb = EnqueueControlTransfer(slot.Memory.ControlRing, clearHalt, 0);
        RingDoorbell(slot.SlotId, XhciSlot.ControlEndpointId);
    }
}
