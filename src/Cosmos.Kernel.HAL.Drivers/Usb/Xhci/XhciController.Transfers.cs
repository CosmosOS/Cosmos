// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci.Rings;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci;

internal sealed partial class XhciController
{
    /// <summary>Upper bound for a control transfer with a data stage (USB 2.0 §9.2.6.4).</summary>
    private const long TransferTimeoutUs = 5_000_000;

    /// <summary>A Setup Stage TRB carries the 8-byte SETUP packet as immediate data.</summary>
    private const uint SetupPacketLength = 8;

    /// <summary>Set TR Dequeue Pointer parameter bit 0: the dequeue cycle state.</summary>
    private const ulong DequeueCycleState = 1;

    /// <summary>Holds one signal while no synchronous control transfer runs, so a wait on it takes the turn.</summary>
    private readonly DeviceEvent _controlTurn;

    // Synchronous control transfer state, under _eventLock.
    private XhciDevice? _transferDevice;
    private ulong _transferStatusTrb;

    /// <summary>The Data Stage TRB of the device-to-host transfer in flight, 0 when there is none.</summary>
    private ulong _transferDataTrb;

    /// <summary>Bytes of the Data Stage TRB a short packet left unfilled, from its event; 0 when none arrived.</summary>
    private uint _transferResidual;

    private bool _transferCompleted;
    private CompletionCode _transferCode;

    /// <summary>
    /// Runs a control transfer on the default endpoint of <paramref name="device"/>
    /// and waits for it. <paramref name="transferred"/> receives the bytes the
    /// data stage moved on success, 0 otherwise.
    /// </summary>
    internal UsbTransferStatus ControlTransfer(XhciDevice device, UsbSetupPacket setup, Span<byte> data, out int transferred)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(data.Length, (int)setup.Length, nameof(data));
        ArgumentOutOfRangeException.ThrowIfGreaterThan((int)setup.Length, XhciMemory.PageSize, nameof(setup));

        transferred = 0;

        // Refused only once the binding is gone, which a PCI one never is.
        if (!_controlTurn.Wait())
        {
            return UsbTransferStatus.Error;
        }

        try
        {
            if (device.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            if (device.ControlEndpointHalted && !RecoverControlEndpoint(device))
            {
                return UsbTransferStatus.Error;
            }

            bool isIn = setup.Direction == UsbDirection.In;
            Span<byte> buffer = device.ControlBuffer[..setup.Length];
            if (!isIn)
            {
                data[..setup.Length].CopyTo(buffer);
            }

            using (_eventLock.EnterScope())
            {
                _transferCompleted = false;
                _transferDevice = device;
                _transferResidual = 0;
                using (_ringLock.EnterScope())
                {
                    _transferStatusTrb = EnqueueControlTransfer(device, setup, device.ControlBufferAddress, out ulong dataTrb);
                    _transferDataTrb = isIn ? dataTrb : 0;
                }

                _registers.RingDoorbell(device.SlotId, XhciDevice.ControlEndpointId);
            }

            CompletionCode code = WaitForControlTransfer(device, out uint residual);
            if (code is CompletionCode.Success or CompletionCode.ShortPacket)
            {
                if (isIn)
                {
                    buffer.CopyTo(data);
                }

                transferred = setup.Length - (int)Math.Min(residual, (uint)setup.Length);
                return UsbTransferStatus.Success;
            }

            // Whatever the transfer ended with, a device that left has
            // nothing to recover: its slot is about to be disabled.
            if (device.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            if (code == CompletionCode.Invalid)
            {
                WriteLog($"slot {device.SlotId}: control transfer timed out");
                return UsbTransferStatus.Timeout;
            }

            // Any error halts the endpoint on the controller side, a STALL
            // included: it has to be reset before the next request can run.
            RecoverControlEndpoint(device);
            return code == CompletionCode.StallError ? UsbTransferStatus.Stall : UsbTransferStatus.Error;
        }
        finally
        {
            _controlTurn.Signal();
        }
    }

    /// <summary>
    /// Queues a host-to-device control transfer without waiting for it.
    /// Safe from interrupt context: it only takes the ring lock.
    /// </summary>
    internal bool SubmitControlTransfer(XhciDevice device, UsbSetupPacket setup, ReadOnlySpan<byte> data)
    {
        if (setup.Direction == UsbDirection.In || setup.Length > XhciMemory.PageSize || data.Length < setup.Length)
        {
            return false;
        }

        using (_ringLock.EnterScope())
        {
            // Checked under the ring lock, which ReleaseDevice takes after
            // marking the device and before freeing its ring.
            if (device.IsDisconnected || device.ControlEndpointHalted)
            {
                return false;
            }

            data[..setup.Length].CopyTo(device.AsyncControlBuffer);
            EnqueueControlTransfer(device, setup, device.AsyncControlBufferAddress, out _);
        }

        _registers.RingDoorbell(device.SlotId, XhciDevice.ControlEndpointId);
        return true;
    }

    /// <summary>
    /// Queues the Setup, optional Data and Status stages of one control
    /// transfer (xHCI 1.2 §4.11.2.2). The caller holds the ring lock.
    /// <paramref name="dataTrb"/> receives the address of the Data Stage
    /// TRB, 0 when the transfer has no data stage.
    /// </summary>
    /// <returns>Address of the Status Stage TRB, the one that interrupts on completion.</returns>
    private static ulong EnqueueControlTransfer(XhciDevice device, UsbSetupPacket setup, ulong dataAddress, out ulong dataTrb)
    {
        ProducerRing ring = device.ControlRing;
        bool hasData = setup.Length != 0;
        bool isIn = setup.Direction == UsbDirection.In;

        uint transferType = !hasData ? 0 : isIn ? Trb.TransferTypeIn : Trb.TransferTypeOut;
        ring.Enqueue(setup.ToUInt64(), SetupPacketLength,
            Trb.TypeField(TrbType.SetupStage) | Trb.ImmediateData | (transferType << Trb.TransferTypeShift));

        dataTrb = 0;
        if (hasData)
        {
            // Interrupt on Short Packet on a device-to-host data stage: a
            // device answering with less than wLength ends the stage early,
            // and only the event this raises for the Data Stage TRB says how
            // much it left unfilled (xHCI 1.2 §4.10.1.1). The controller goes
            // on to the Status Stage either way, whose event still ends the
            // transfer. Linux sets the flag on its data stages for the same
            // reason.
            uint shortPacket = isIn ? Trb.InterruptOnShortPacket : 0;
            dataTrb = ring.Enqueue(dataAddress, setup.Length,
                Trb.TypeField(TrbType.DataStage) | (isIn ? Trb.DirectionIn : 0) | shortPacket);
        }

        // The status stage runs opposite to the data stage, and IN when
        // there is none (USB 2.0 §8.5.3).
        bool statusIn = !hasData || !isIn;
        return ring.Enqueue(0, 0,
            Trb.TypeField(TrbType.StatusStage) | Trb.InterruptOnCompletion | (statusIn ? Trb.DirectionIn : 0));
    }

    /// <returns>
    /// The completion code, or <see cref="CompletionCode.Invalid"/> on
    /// timeout and when <paramref name="device"/> left the bus meanwhile.
    /// <paramref name="residual"/> receives the bytes a short packet left
    /// unfilled in the data stage, 0 when none did.
    /// </returns>
    private CompletionCode WaitForControlTransfer(XhciDevice device, out uint residual)
    {
        for (long waitedUs = 0; ; waitedUs += WaitPollIntervalUs)
        {
            using (_eventLock.EnterScope())
            {
                DrainEvents();
                if (_transferCompleted || device.IsDisconnected || waitedUs >= TransferTimeoutUs)
                {
                    _transferDevice = null;
                    _transferStatusTrb = 0;
                    _transferDataTrb = 0;
                    residual = _transferResidual;
                    return _transferCompleted ? _transferCode : CompletionCode.Invalid;
                }
            }

            _context.Delay(WaitPollInterval);
        }
    }

    /// <summary>
    /// Takes a halted default endpoint back to running: Reset Endpoint, then
    /// move its dequeue pointer past the transfers the halt abandoned
    /// (xHCI 1.2 §4.6.8). The device side needs nothing: a control
    /// endpoint's STALL clears with the next SETUP (USB 2.0 §8.5.3.4).
    /// </summary>
    private bool RecoverControlEndpoint(XhciDevice device)
    {
        CompletionCode code = ExecuteCommand(0,
            EndpointCommand(TrbType.ResetEndpointCommand, device.SlotId, XhciDevice.ControlEndpointId), out _);

        // Context State Error: the endpoint was not halted, nothing to recover.
        if (code == CompletionCode.ContextStateError)
        {
            device.ControlEndpointHalted = false;
            return true;
        }

        if (code != CompletionCode.Success)
        {
            LogCommandFailure("Reset Endpoint", code);
            return false;
        }

        ulong dequeuePointer;
        using (_ringLock.EnterScope())
        {
            dequeuePointer = device.ControlRing.EnqueuePointer | (device.ControlRing.CycleState ? DequeueCycleState : 0);
        }

        code = ExecuteCommand(dequeuePointer,
            EndpointCommand(TrbType.SetTrDequeuePointerCommand, device.SlotId, XhciDevice.ControlEndpointId), out _);
        if (code != CompletionCode.Success)
        {
            LogCommandFailure("Set TR Dequeue Pointer", code);
            return false;
        }

        device.ControlEndpointHalted = false;
        return true;
    }
}
