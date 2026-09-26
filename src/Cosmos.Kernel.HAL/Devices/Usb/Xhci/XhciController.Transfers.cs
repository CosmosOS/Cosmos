// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Drivers.Usb;
using SchedMutex = Cosmos.Kernel.Core.Scheduler.Mutex;

namespace Cosmos.Kernel.HAL.Devices.Usb.Xhci;

internal sealed unsafe partial class XhciController
{
    /// <summary>Upper bound for a control transfer with a data stage (USB 2.0 §9.2.6.4).</summary>
    private const uint TransferTimeoutMs = 5000;

    /// <summary>A Setup Stage TRB carries the 8-byte SETUP packet as immediate data.</summary>
    private const uint SetupPacketLength = 8;

    /// <summary>Set TR Dequeue Pointer parameter bit 0: the dequeue cycle state.</summary>
    private const ulong DequeueCycleState = 1;

    // Synchronous control transfer state, under _eventLock. One at a time,
    // like commands, which _controlMutex enforces.
    private readonly SchedMutex _controlMutex = new();
    private XhciDevice? _transferDevice;
    private ulong _transferStatusTrb;

    /// <summary>The Data Stage TRB of the device-to-host transfer in flight, 0 when there is none.</summary>
    private ulong _transferDataTrb;

    /// <summary>Bytes of the Data Stage TRB a short packet left unfilled, from its event; 0 when none arrived.</summary>
    private uint _transferResidual;

    private bool _transferCompleted;
    private XhciCompletionCode _transferCode;

    /// <summary>
    /// Runs a control transfer on the default endpoint of <paramref name="device"/>
    /// and waits for it. <paramref name="transferred"/> receives the bytes the
    /// data stage moved on success, 0 otherwise.
    /// </summary>
    internal UsbTransferStatus ControlTransfer(XhciDevice device, UsbSetupPacket setup, Span<byte> data, out int transferred)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(data.Length, (int)setup.Length, nameof(data));
        ArgumentOutOfRangeException.ThrowIfGreaterThan((int)setup.Length, XhciDma.PageSize, nameof(setup));

        transferred = 0;
        _controlMutex.Acquire();
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

            Span<byte> buffer = new(device.ControlBuffer, setup.Length);
            if (!setup.IsDeviceToHost)
            {
                data.Slice(0, setup.Length).CopyTo(buffer);
            }

            using (_eventLock.AcquireIrqSafe())
            {
                _transferCompleted = false;
                _transferDevice = device;
                _transferResidual = 0;
                using (_ringLock.AcquireIrqSafe())
                {
                    _transferStatusTrb = EnqueueControlTransfer(device, setup, device.ControlBufferAddress, out ulong dataTrb);
                    _transferDataTrb = setup.IsDeviceToHost ? dataTrb : 0;
                }

                _regs.RingDoorbell(device.SlotId, XhciDevice.ControlEndpointId);
            }

            XhciCompletionCode code = WaitForControlTransfer(device, out uint residual);
            if (code is XhciCompletionCode.Success or XhciCompletionCode.ShortPacket)
            {
                if (setup.IsDeviceToHost)
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

            if (code == XhciCompletionCode.Invalid)
            {
                Serial.WriteString("[xHCI] Control transfer timed out\n");
                return UsbTransferStatus.Timeout;
            }

            // Any error halts the endpoint on the controller side, a STALL
            // included: it has to be reset before the next request can run.
            RecoverControlEndpoint(device);
            return code == XhciCompletionCode.StallError ? UsbTransferStatus.Stall : UsbTransferStatus.Error;
        }
        finally
        {
            _controlMutex.Release();
        }
    }

    /// <summary>
    /// Queues a host-to-device control transfer without waiting for it.
    /// Safe from interrupt context: it only takes the ring lock.
    /// </summary>
    internal bool SubmitControlTransfer(XhciDevice device, UsbSetupPacket setup, ReadOnlySpan<byte> data)
    {
        if (setup.IsDeviceToHost || setup.Length > XhciDma.PageSize || data.Length < setup.Length)
        {
            return false;
        }

        using (_ringLock.AcquireIrqSafe())
        {
            // Checked under the ring lock, which ReleaseDevice takes after
            // marking the device and before freeing its ring.
            if (device.IsDisconnected || device.ControlEndpointHalted)
            {
                return false;
            }

            data.Slice(0, setup.Length).CopyTo(new Span<byte>(device.AsyncControlBuffer, setup.Length));
            EnqueueControlTransfer(device, setup, device.AsyncControlBufferAddress, out _);
        }

        _regs.RingDoorbell(device.SlotId, XhciDevice.ControlEndpointId);
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
        XhciRing ring = device.ControlRing;
        bool hasData = setup.Length != 0;
        bool isIn = setup.IsDeviceToHost;

        uint transferType = !hasData ? 0 : isIn ? XhciTrb.TransferTypeIn : XhciTrb.TransferTypeOut;
        ring.Enqueue(setup.Pack(), SetupPacketLength,
            XhciTrb.TypeField(XhciTrbType.SetupStage) | XhciTrb.ImmediateData | (transferType << XhciTrb.TransferTypeShift));

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
            uint shortPacket = isIn ? XhciTrb.InterruptOnShortPacket : 0;
            dataTrb = ring.Enqueue(dataAddress, setup.Length,
                XhciTrb.TypeField(XhciTrbType.DataStage) | (isIn ? XhciTrb.DirectionIn : 0) | shortPacket);
        }

        // The status stage runs opposite to the data stage, and IN when
        // there is none (USB 2.0 §8.5.3).
        bool statusIn = !hasData || !isIn;
        return ring.Enqueue(0, 0,
            XhciTrb.TypeField(XhciTrbType.StatusStage) | XhciTrb.InterruptOnCompletion | (statusIn ? XhciTrb.DirectionIn : 0));
    }

    /// <returns>
    /// The completion code, or <see cref="XhciCompletionCode.Invalid"/> on
    /// timeout and when <paramref name="device"/> left the bus meanwhile.
    /// <paramref name="residual"/> receives the bytes a short packet left
    /// unfilled in the data stage, 0 when none did.
    /// </returns>
    private XhciCompletionCode WaitForControlTransfer(XhciDevice device, out uint residual)
    {
        for (uint waitedUs = 0; ; waitedUs += WaitPollIntervalUs)
        {
            using (_eventLock.AcquireIrqSafe())
            {
                DrainEvents();
                if (_transferCompleted || device.IsDisconnected || waitedUs >= TransferTimeoutMs * MicrosecondsPerMillisecond)
                {
                    _transferDevice = null;
                    _transferStatusTrb = 0;
                    _transferDataTrb = 0;
                    residual = _transferResidual;
                    return _transferCompleted ? _transferCode : XhciCompletionCode.Invalid;
                }
            }

            PlatformHAL.Initializer?.DelayMicroseconds(WaitPollIntervalUs);
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
        XhciCompletionCode code = ExecuteCommand(0,
            EndpointCommand(XhciTrbType.ResetEndpointCommand, device.SlotId, XhciDevice.ControlEndpointId), out _);

        // Context State Error: the endpoint was not halted, nothing to recover.
        if (code == XhciCompletionCode.ContextStateError)
        {
            device.ControlEndpointHalted = false;
            return true;
        }

        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Reset Endpoint", code);
            return false;
        }

        ulong dequeuePointer;
        using (_ringLock.AcquireIrqSafe())
        {
            dequeuePointer = device.ControlRing.EnqueuePointer | (device.ControlRing.CycleState ? DequeueCycleState : 0);
        }

        code = ExecuteCommand(dequeuePointer,
            EndpointCommand(XhciTrbType.SetTrDequeuePointerCommand, device.SlotId, XhciDevice.ControlEndpointId), out _);
        if (code != XhciCompletionCode.Success)
        {
            LogCommandFailure("Set TR Dequeue Pointer", code);
            return false;
        }

        device.ControlEndpointHalted = false;
        return true;
    }
}
