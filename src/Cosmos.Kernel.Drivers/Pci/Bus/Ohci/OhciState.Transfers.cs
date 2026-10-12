// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// Control transfers on a device's default endpoint: the SETUP, DATA and
/// STATUS TDs queued by the tail pointer method, the synchronous wait, and
/// the quiesce of a halted or timed-out control ED.
/// </summary>
internal sealed partial class OhciState
{
    /// <summary>
    /// Runs a control transfer on the device's control ED and waits for it:
    /// the ED claimed (Disconnected at once when the device left, Timeout
    /// when another owner kept it past the budget), a halted ED quiesced
    /// first, the OUT data copied into the bounce page, the SETUP packet
    /// written into its slot, the stages queued and CLF set under the
    /// lock, the completion awaited, the IN data copied out. A STALL or
    /// error leaves the ED halted for the next transfer to quiesce; a
    /// timeout quiesces it now. Thread context; the body of
    /// <see cref="OhciDevice.ControlTransferCore"/>.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="setup">The request; its Length is the data stage size.</param>
    /// <param name="data">The OUT data or the IN buffer, at least <see cref="UsbSetupPacket.Length"/> bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="data"/> is shorter than the setup's Length, or the Length exceeds one page.</exception>
    internal UsbTransferStatus ControlTransfer(OhciDevice device, UsbSetupPacket setup, Span<byte> data)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(data.Length, setup.Length, nameof(data));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(setup.Length, PageBytes, nameof(setup));

        UsbTransferStatus claim = ClaimControl(device, OhciProtocol.TransferTimeoutMs);
        if (claim != UsbTransferStatus.Success)
        {
            return claim;
        }

        try
        {
            if (IsControlHalted(device))
            {
                QuiesceEndpoint(device.Memory, keepToggle: false, device.Address, 0);
                using (Lock.Acquire())
                {
                    device.ControlHalted = false;
                }
            }

            Span<byte> buffer = device.Memory.ControlBuffer.Span.Slice(0, setup.Length);
            if (!setup.IsDeviceToHost)
            {
                data.Slice(0, setup.Length).CopyTo(buffer);
            }

            WriteSetupPacket(device.Memory, setup);

            bool queued;
            using (Lock.Acquire())
            {
                Volatile.Write(ref device.ControlCompleted, false);
                device.ControlCode = OhciConditionCode.NotAccessed;
                device.ControlRemaining = 0;
                queued = QueueControlStages(device, setup);
                if (queued)
                {
                    SetControlListFilled();
                }
            }

            if (!queued)
            {
                _binding.Log($"address {device.Address}: no free descriptor for the control transfer");
                return UsbTransferStatus.Error;
            }

            bool completed = WaitCompletion(ref device.ControlCompleted, device.ControlEvent, device, OhciProtocol.TransferTimeoutMs);

            OhciConditionCode code;
            int remaining;
            using (Lock.Acquire())
            {
                device.ControlPending = false;
                device.ControlSetupIndex = -1;
                device.ControlDataIndex = -1;
                device.ControlStatusIndex = -1;
                code = completed ? device.ControlCode : OhciConditionCode.NotAccessed;
                remaining = device.ControlRemaining;
            }

            if (!completed)
            {
                // The wait also ends at once when the host binding begins
                // detaching (the kit sets the flag before the bus marks the
                // devices): no budget elapsed, and the release that follows
                // unlinks and quiesces the ED, so no timeout is logged or
                // counted.
                if (device.IsDisconnected || _binding.IsDetaching)
                {
                    return UsbTransferStatus.Disconnected;
                }

                QuiesceEndpoint(device.Memory, keepToggle: false, device.Address, 0);
                _binding.Log("control transfer timed out");
                _timeouts++;
                return UsbTransferStatus.Timeout;
            }

            if (code == OhciConditionCode.NoError)
            {
                if (setup.IsDeviceToHost)
                {
                    int received = Math.Clamp(setup.Length - remaining, 0, setup.Length);
                    buffer.Slice(0, received).CopyTo(data);
                }

                return UsbTransferStatus.Success;
            }

            // Whatever the transfer ended with, a device that left has
            // nothing to recover: its ED is about to be unlinked.
            if (device.IsDisconnected)
            {
                return UsbTransferStatus.Disconnected;
            }

            // Any error halts the ED, a STALL included (OHCI 1.0a section
            // 4.3.3): the next transfer quiesces it before it queues.
            return code == OhciConditionCode.Stall ? UsbTransferStatus.Stall : UsbTransferStatus.Error;
        }
        finally
        {
            ReleaseControl(device);
        }
    }

    /// <summary>True when the drain marked the control ED halted, or its head carries H. Thread context.</summary>
    private bool IsControlHalted(OhciDevice device)
    {
        using (Lock.Acquire())
        {
            if (device.ControlHalted)
            {
                return true;
            }

            ref OhciEndpointDescriptor ed = ref Ed(device.Memory.Endpoint.Descriptor);
            return (Volatile.Read(ref ed.HeadPointer) & OhciProtocol.EndpointHalted) != 0;
        }
    }

    /// <summary>Writes the 8-byte SETUP packet, little-endian, into the set's SETUP slot. Thread context; the caller owns the ED.</summary>
    private static void WriteSetupPacket(OhciDeviceMemory memory, UsbSetupPacket setup)
    {
        OhciDescriptorPage page = memory.SetupSlot.Page ?? throw new InvalidOperationException("The SETUP slot is empty.");
        BinaryPrimitives.WriteUInt64LittleEndian(page.Bytes(memory.SetupSlot.Index).Slice(0, OhciProtocol.SetupPacketBytes), setup.Pack());
    }

    /// <summary>
    /// Queues the SETUP, optional DATA and STATUS TDs of one control
    /// transfer on the device's ED by the tail pointer method (OHCI 1.0a
    /// sections 4.3.1 and 5.2.8.2): the dummy becomes SETUP (DATA0, DI 6),
    /// DATA takes a free slot (DATA1, rounding on IN, DI 6), STATUS another
    /// (DATA1, zero length, opposite direction or IN without data, DI 0),
    /// the last points at a fresh dummy, and the tail moves. The stages are
    /// filled in that order, the one the ED runs them in, since the drain
    /// takes the queue order from the fill order. Under the lock; the
    /// caller sets CLF.
    /// </summary>
    /// <returns>False when the set has too few free slots.</returns>
    private static bool QueueControlStages(OhciDevice device, UsbSetupPacket setup)
    {
        OhciDeviceMemory set = device.Memory;
        int setupIndex = set.DummyIndex;
        if (setupIndex < 0)
        {
            return false;
        }

        bool hasData = setup.Length != 0;
        bool isIn = setup.IsDeviceToHost;
        Span<int> free = stackalloc int[OhciProtocol.DeviceTransferSlots];
        int count = set.FreeIndices(free);
        if (count < (hasData ? 3 : 2))
        {
            return false;
        }

        int next = 0;
        int dataIndex = hasData ? free[next++] : -1;
        int statusIndex = free[next++];
        int dummyIndex = free[next];
        ZeroTransfer(set, dummyIndex);

        uint statusAddress = set.TdSlots[statusIndex].PhysicalAddress;
        uint setupAddress = set.SetupSlot.PhysicalAddress;
        FillTransfer(set, setupIndex,
            TransferControl(OhciProtocol.DirectionPidSetup, OhciProtocol.DataToggle0, rounding: false, OhciProtocol.DelayInterruptStages),
            setupAddress, setupAddress + OhciProtocol.SetupPacketBytes - 1, hasData ? set.TdSlots[dataIndex].PhysicalAddress : statusAddress);

        if (hasData)
        {
            uint bounce = (uint)set.ControlBuffer.PhysicalAddress;
            FillTransfer(set, dataIndex,
                TransferControl(isIn ? OhciProtocol.DirectionPidIn : OhciProtocol.DirectionPidOut, OhciProtocol.DataToggle1, rounding: isIn, OhciProtocol.DelayInterruptStages),
                bounce, bounce + setup.Length - 1, statusAddress);
        }

        // The status stage runs opposite to the data stage, and IN when
        // there is none (USB 2.0 section 8.5.3).
        uint statusPid = !hasData || !isIn ? OhciProtocol.DirectionPidIn : OhciProtocol.DirectionPidOut;
        FillTransfer(set, statusIndex,
            TransferControl(statusPid, OhciProtocol.DataToggle1, rounding: false, OhciProtocol.DelayInterruptImmediate),
            0, 0, set.TdSlots[dummyIndex].PhysicalAddress);

        device.ControlSetupIndex = setupIndex;
        device.ControlDataIndex = dataIndex;
        device.ControlStatusIndex = statusIndex;
        device.ControlPending = true;
        PublishTail(set, dummyIndex);
        return true;
    }
}
