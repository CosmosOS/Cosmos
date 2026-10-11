// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// Control transfers on a device's default endpoint: the SETUP, DATA and
/// STATUS qTDs queued with the dummy technique on the control queue head,
/// the synchronous wait, the status mapping and the quiesce of a halted
/// or timed-out queue head.
/// </summary>
internal sealed partial class EhciState
{
    /// <summary>
    /// Runs a control transfer on the device's default endpoint and waits
    /// for it: the queue head claimed (Disconnected at once when the device
    /// left, Timeout when another owner kept it past the budget), a halted
    /// queue head quiesced first, the OUT data copied into the bounce page
    /// and the SETUP packet into its slot, the chain queued under the lock
    /// with the SETUP qTD activated last, the completion awaited, the IN
    /// data copied out. Thread context; the body of
    /// <see cref="EhciDevice.ControlTransferCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="setup">The request; its Length is the data stage size.</param>
    /// <param name="data">The OUT data or the IN buffer, at least <see cref="UsbSetupPacket.Length"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the setup's Length, or the Length exceeds one page.</exception>
    internal UsbTransferStatus ControlTransfer(EhciDevice device, UsbSetupPacket setup, Span<byte> data)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(data.Length, setup.Length, nameof(data));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(setup.Length, PageBytes, nameof(setup));

        UsbTransferStatus claim = ClaimControl(device, EhciProtocol.TransferTimeoutMs);
        if (claim != UsbTransferStatus.Success)
        {
            return claim;
        }

        try
        {
            EhciDeviceMemory memory = device.Memory;
            if (memory.Retired)
            {
                return UsbTransferStatus.Error;
            }

            if (device.ControlHalted)
            {
                if (!QuiesceControl(device))
                {
                    return UsbTransferStatus.Error;
                }

                device.ControlHalted = false;
            }

            Span<byte> buffer = memory.Buffer.Span.Slice(0, setup.Length);
            if (!setup.IsDeviceToHost && setup.Length != 0)
            {
                data.Slice(0, setup.Length).CopyTo(buffer);
            }

            BinaryPrimitives.WriteUInt64LittleEndian(memory.Setup.Span, setup.Pack());

            using (Lock.Acquire())
            {
                Volatile.Write(ref device.ControlCompleted, false);
                QueueControlChain(device, setup);
            }

            bool completed = WaitCompletion(ref device.ControlCompleted, device.ControlEvent, device, EhciProtocol.TransferTimeoutMs);

            uint status;
            int remaining;
            using (Lock.Acquire())
            {
                status = device.ControlStatus;
                remaining = device.ControlRemaining;
                device.ControlPendingCount = 0;
            }

            if (!completed)
            {
                if (device.IsDisconnected)
                {
                    return UsbTransferStatus.Disconnected;
                }

                // A wait cut short by the host's teardown is not a device
                // timeout; the chain still leaves the schedule.
                if (!_binding.IsDetaching)
                {
                    _binding.Log("control transfer timed out");
                    _timeouts++;
                }

                QuiesceControl(device);
                return UsbTransferStatus.Timeout;
            }

            if ((status & EhciProtocol.TokenHalted) != 0)
            {
                // The queue head stays halted until the next transfer
                // quiesces it. A STALL is a halt with CERR still non-zero:
                // the error bits are sticky across retries, so one left by
                // a retried transaction does not make the halt an error,
                // while a babble or a missed complete-split on an IN is one,
                // which the kit retries instead of clearing a halt the
                // device never set.
                device.ControlHalted = true;
                if (device.IsDisconnected)
                {
                    return UsbTransferStatus.Disconnected;
                }

                return IsStall(status) ? UsbTransferStatus.Stall : UsbTransferStatus.Error;
            }

            if (setup.IsDeviceToHost && setup.Length != 0)
            {
                int received = setup.Length - Math.Min(remaining, setup.Length);
                buffer.Slice(0, received).CopyTo(data);
            }

            return UsbTransferStatus.Success;
        }
        finally
        {
            ReleaseControl(device);
        }
    }

    /// <summary>
    /// Queues the SETUP, optional DATA and STATUS stages on the control
    /// queue head with the dummy technique (EHCI 1.0 section 4.10.2): the
    /// dummy the overlay points at becomes the SETUP qTD, the next slots
    /// take the DATA and STATUS stages, the slot after them is zeroed as
    /// the new dummy the chain ends in; every token but the first is
    /// written Active before the barrier, the SETUP token after it. The
    /// DATA IN stage's alternate next points at the STATUS stage so a
    /// short data stage proceeds to it; the STATUS stage carries IOC and
    /// runs opposite to the data stage, IN when there is none (USB 2.0
    /// section 8.5.3). Under the lock.
    /// </summary>
    private void QueueControlChain(EhciDevice device, UsbSetupPacket setup)
    {
        EhciDeviceMemory memory = device.Memory;
        EhciDescriptorSlot[] slots = memory.Transfers;
        int slotCount = slots.Length;
        bool hasData = setup.Length != 0;
        bool isIn = setup.IsDeviceToHost;
        bool statusIn = !hasData || !isIn;
        int count = hasData ? 3 : 2;

        int setupSlot = memory.DummyIndex;
        int dataSlot = (setupSlot + 1) % slotCount;
        int statusSlot = (setupSlot + count - 1) % slotCount;
        int newDummy = (setupSlot + count) % slotCount;
        slots[newDummy].Clear();

        ref EhciTransferDescriptor status = ref slots[statusSlot].TransferDescriptor;
        FillTransfer(ref status, slots[newDummy].TransferPointer, EhciProtocol.LinkTerminate, 0, 0);
        status.Token = TransferToken(statusIn ? EhciProtocol.PidIn : EhciProtocol.PidOut, 0, dataToggle: true, interruptOnComplete: true);

        if (hasData)
        {
            ref EhciTransferDescriptor dataStage = ref slots[dataSlot].TransferDescriptor;
            uint alternateNext = isIn ? slots[statusSlot].TransferPointer : EhciProtocol.LinkTerminate;
            FillTransfer(ref dataStage, slots[statusSlot].TransferPointer, alternateNext, memory.Buffer.PhysicalAddress, setup.Length);
            dataStage.Token = TransferToken(isIn ? EhciProtocol.PidIn : EhciProtocol.PidOut, setup.Length, dataToggle: true, interruptOnComplete: false);
        }

        ref EhciTransferDescriptor setupStage = ref slots[setupSlot].TransferDescriptor;
        uint next = hasData ? slots[dataSlot].TransferPointer : slots[statusSlot].TransferPointer;
        FillTransfer(ref setupStage, next, EhciProtocol.LinkTerminate, memory.Setup.PhysicalAddress, EhciProtocol.SetupPacketLength);

        device.ControlFirstSlot = setupSlot;
        device.ControlHasData = hasData;
        device.ControlStatus = 0;
        device.ControlRemaining = 0;
        device.ControlPendingCount = count;
        memory.DummyIndex = newDummy;

        DmaBuffer.WriteBarrier();
        Volatile.Write(ref setupStage.Token, TransferToken(EhciProtocol.PidSetup, EhciProtocol.SetupPacketLength, dataToggle: false, interruptOnComplete: false));
    }

    /// <summary>
    /// Empties the device's control queue head after a halt or a timeout:
    /// the quiesce of the schedule partial with the toggle returned to 0
    /// (a DTC queue head takes it from each qTD anyway). Thread context;
    /// the caller owns the control queue head.
    /// </summary>
    /// <returns>False when the controller may still hold the queue head: the set is retired and the device cannot be used.</returns>
    private bool QuiesceControl(EhciDevice device)
    {
        EhciDeviceMemory memory = device.Memory;
        if (QuiesceQueueHead(memory.QueueHead, memory.Transfers, memory.DummyIndex, keepToggle: false))
        {
            return true;
        }

        using (Lock.Acquire())
        {
            RetireDeviceMemory(memory);
        }

        return false;
    }
}
