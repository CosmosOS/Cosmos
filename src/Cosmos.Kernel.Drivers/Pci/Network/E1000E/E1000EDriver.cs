// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.Drivers.Pci.Network.E1000E;

/// <summary>
/// The driver of the Intel 82574 (E1000E) family over the driver kit: maps
/// BAR0, resets the controller, reads its station address, programs the
/// legacy descriptor rings in DMA memory, connects the function's line when
/// the platform can route it and schedules a periodic drain in any case,
/// then publishes the interface to the ring. Everything it holds for one
/// controller lives on an <see cref="E1000EState"/> in
/// <see cref="DeviceBinding.DriverState"/>. Frames reach the ring through a
/// drain that runs only on the kit worker, so a kernel without one (the
/// scheduler compiled out) is declined rather than left with a controller
/// that transmits and never receives. <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Network)]
public sealed class E1000EDriver : Driver
{
    /// <summary>Intel's PCI vendor id.</summary>
    private const ushort IntelVendorId = 0x8086;

    /// <summary>The 82574L.</summary>
    private const ushort DeviceId82574L = 0x10D3;

    /// <summary>The 82574L, industrial temperature.</summary>
    private const ushort DeviceId82574It = 0x10F6;

    /// <summary>The 82571EB.</summary>
    private const ushort DeviceId82571Eb = 0x105E;

    /// <summary>The 82577LM.</summary>
    private const ushort DeviceId82577Lm = 0x10EA;

    /// <summary>The 82577LC.</summary>
    private const ushort DeviceId82577Lc = 0x10EB;

    /// <summary>The 82578DM.</summary>
    private const ushort DeviceId82578Dm = 0x10EF;

    /// <summary>The base address register holding the controller's registers.</summary>
    private const int RegisterBar = 0;

    /// <summary>Bytes the register window has to span: the 82574 decodes 128 KiB.</summary>
    private const ulong RegisterWindowBytes = 0x20000;

    /// <summary>Polls of a reset or an auto-read: 100 of <see cref="ResetPollMicroseconds"/>, a 10 ms bound.</summary>
    private const int ResetPollCount = 100;

    /// <summary>Delay between two polls of a reset or an auto-read.</summary>
    private const uint ResetPollMicroseconds = 100;

    /// <summary>Polls of one EEPROM word read.</summary>
    private const int EepromPollCount = 1000;

    /// <summary>Delay between two polls of an EEPROM word read.</summary>
    private const uint EepromPollMicroseconds = 10;

    /// <summary>Bytes of a station address.</summary>
    private const int MacAddressBytes = 6;

    /// <summary>EEPROM words holding the station address: words 0 to 2.</summary>
    private const int EepromMacWords = 3;

    /// <summary>Bits per byte, for the register packing of the address.</summary>
    private const int BitsPerByte = 8;

    /// <summary>The interrupt source index of the function's legacy line.</summary>
    private const int LineInterruptIndex = 0;

    /// <summary>Period of the drain the kit runs whether or not the line connected.</summary>
    private const uint DrainPeriodMilliseconds = 50;

    /// <summary>How long the detach hook waits after disabling the receiver, so a frame in flight lands before the buffers are freed.</summary>
    private const uint QuiesceMicroseconds = 100;

    private readonly DeviceMatch[] _matches =
    [
        new PciMatch(vendorId: IntelVendorId, deviceId: DeviceId82574L),
        new PciMatch(vendorId: IntelVendorId, deviceId: DeviceId82574It),
        new PciMatch(vendorId: IntelVendorId, deviceId: DeviceId82571Eb),
        new PciMatch(vendorId: IntelVendorId, deviceId: DeviceId82577Lm),
        new PciMatch(vendorId: IntelVendorId, deviceId: DeviceId82577Lc),
        new PciMatch(vendorId: IntelVendorId, deviceId: DeviceId82578Dm),
    ];

    /// <inheritdoc/>
    public override string Name => nameof(E1000EDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    /// <summary>
    /// Brings the controller up and publishes it. Thread context on the kit
    /// worker; a declined or failed result makes the kit release everything
    /// acquired here and quiet the function again.
    /// </summary>
    /// <param name="binding">The function's node and the kit facilities for it.</param>
    /// <returns>Bound with the interface published; declined when the function is not one this driver can operate; failed when the controller did not come up.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. BAR0 is the register window.
        PciAccess pci = binding.Node.Access<PciAccess>();
        PciBar bar0 = pci.Bars[RegisterBar];
        if (!bar0.IsAssigned || bar0.IsIo || bar0.Length < RegisterWindowBytes)
        {
            return ProbeResult.Declined("BAR0 is not a memory window of 128 KiB");
        }

        RegisterWindow registers = binding.MapRegisters(RegisterBar);

        // 2. Decoding and DMA on.
        pci.EnableMemorySpace(true);
        pci.EnableBusMastering(true);

        // 3. Reset, with every cause masked on both sides of it, then the
        //    NVM auto-read the reset starts, before the address registers
        //    are trusted.
        registers.Write32(E1000ERegisters.InterruptMaskClear, E1000ERegisters.AllInterrupts);
        registers.Write32(E1000ERegisters.Control, registers.Read32(E1000ERegisters.Control) | E1000ERegisters.ControlReset);
        if (!WaitForBit(binding, registers, E1000ERegisters.Control, E1000ERegisters.ControlReset, set: false))
        {
            return ProbeResult.Failed("the controller did not leave reset");
        }

        registers.Write32(E1000ERegisters.InterruptMaskClear, E1000ERegisters.AllInterrupts);
        if (!WaitForBit(binding, registers, E1000ERegisters.EepromControl, E1000ERegisters.EepromControlAutoReadDone, set: true))
        {
            return ProbeResult.Failed("the NVM auto-read did not complete");
        }

        // 4. The station address: what firmware programmed, else the EEPROM.
        byte[] address = new byte[MacAddressBytes];
        uint addressLow = registers.Read32(E1000ERegisters.ReceiveAddressLow0);
        uint addressHigh = registers.Read32(E1000ERegisters.ReceiveAddressHigh0);
        if (addressLow != 0 || (addressHigh & E1000ERegisters.ReceiveAddressHighBytesMask) != 0)
        {
            for (int i = 0; i < sizeof(uint); i++)
            {
                address[i] = (byte)(addressLow >> (i * BitsPerByte));
            }

            address[4] = (byte)addressHigh;
            address[5] = (byte)(addressHigh >> BitsPerByte);
        }
        else
        {
            for (int word = 0; word < EepromMacWords; word++)
            {
                if (!TryReadEepromWord(binding, registers, (ushort)word, out ushort value))
                {
                    return ProbeResult.Failed($"EEPROM word {word} did not read");
                }

                address[word * 2] = (byte)value;
                address[word * 2 + 1] = (byte)(value >> BitsPerByte);
            }
        }

        if (IsAllZero(address))
        {
            return ProbeResult.Declined("no MAC address");
        }

        addressLow = address[0] | ((uint)address[1] << BitsPerByte) | ((uint)address[2] << (2 * BitsPerByte)) | ((uint)address[3] << (3 * BitsPerByte));
        addressHigh = address[4] | ((uint)address[5] << BitsPerByte) | E1000ERegisters.ReceiveAddressValid;
        registers.Write32(E1000ERegisters.ReceiveAddressLow0, addressLow);
        registers.Write32(E1000ERegisters.ReceiveAddressHigh0, addressHigh);
        MACAddress macAddress = new(address);

        // 5. The rings and their buffers, in DMA memory; each descriptor
        //    points at its own buffer.
        DmaBuffer receiveRing = binding.AllocateDma(E1000EState.RingBytes, E1000EState.RingAlignment);
        DmaBuffer transmitRing = binding.AllocateDma(E1000EState.RingBytes, E1000EState.RingAlignment);
        DmaBuffer receiveBuffers = binding.AllocateDma(E1000EState.BuffersBytes, E1000EState.BufferAlignment);
        DmaBuffer transmitBuffers = binding.AllocateDma(E1000EState.BuffersBytes, E1000EState.BufferAlignment);
        Span<E1000EReceiveDescriptor> receive = MemoryMarshal.Cast<byte, E1000EReceiveDescriptor>(receiveRing.Span);
        Span<E1000ETransmitDescriptor> transmit = MemoryMarshal.Cast<byte, E1000ETransmitDescriptor>(transmitRing.Span);
        for (int i = 0; i < E1000EState.DescriptorCount; i++)
        {
            receive[i].BufferAddress = receiveBuffers.PhysicalAddress + (ulong)(i * E1000EState.BufferBytes);
            receive[i].Status = 0;
            transmit[i].BufferAddress = transmitBuffers.PhysicalAddress + (ulong)(i * E1000EState.BufferBytes);
            transmit[i].Status = 0;
        }

        DmaBuffer.WriteBarrier();

        // 6. The controller's view of them, the filters and the two engines.
        for (int i = 0; i < E1000ERegisters.MulticastTableEntries; i++)
        {
            registers.Write32(E1000ERegisters.MulticastTableArray + (ulong)(i * E1000ERegisters.MulticastTableEntryBytes), 0);
        }

        registers.Write32(E1000ERegisters.ReceiveDescriptorBaseLow, (uint)receiveRing.PhysicalAddress);
        registers.Write32(E1000ERegisters.ReceiveDescriptorBaseHigh, (uint)(receiveRing.PhysicalAddress >> 32));
        registers.Write32(E1000ERegisters.ReceiveDescriptorLength, E1000EState.RingBytes);
        registers.Write32(E1000ERegisters.ReceiveDescriptorHead, 0);
        registers.Write32(E1000ERegisters.ReceiveDescriptorTail, E1000EState.DescriptorCount - 1);
        registers.Write32(E1000ERegisters.TransmitDescriptorBaseLow, (uint)transmitRing.PhysicalAddress);
        registers.Write32(E1000ERegisters.TransmitDescriptorBaseHigh, (uint)(transmitRing.PhysicalAddress >> 32));
        registers.Write32(E1000ERegisters.TransmitDescriptorLength, E1000EState.RingBytes);
        registers.Write32(E1000ERegisters.TransmitDescriptorHead, 0);
        registers.Write32(E1000ERegisters.TransmitDescriptorTail, 0);
        registers.Write32(E1000ERegisters.TransmitInterPacketGap, E1000ERegisters.InterPacketGap);
        registers.Write32(E1000ERegisters.TransmitControl,
            E1000ERegisters.TransmitControlEnable
            | E1000ERegisters.TransmitControlPadShortPackets
            | (E1000ERegisters.CollisionThreshold << E1000ERegisters.TransmitControlCollisionThresholdShift)
            | (E1000ERegisters.CollisionDistance << E1000ERegisters.TransmitControlCollisionDistanceShift));
        registers.Write32(E1000ERegisters.ReceiveControl,
            E1000ERegisters.ReceiveControlEnable
            | E1000ERegisters.ReceiveControlBroadcastAccept
            | E1000ERegisters.ReceiveControlMulticastPromiscuous
            | E1000ERegisters.ReceiveControlBufferSize2048
            | E1000ERegisters.ReceiveControlStripCrc);

        E1000EState state = new(binding, registers, macAddress, receiveRing, transmitRing, receiveBuffers, transmitBuffers);
        binding.DriverState = state;

        // 7. The drain, which the handler and the timer both schedule.
        //    Scheduling it once tells whether a worker exists to run it:
        //    without one neither the handler nor the timer could ever run
        //    it and the controller would never receive, so the function is
        //    declined. With a worker the run lands after this probe, once
        //    the interface is published, and costs a few register reads.
        WorkItem drain = binding.CreateWorkItem(state.Drain);
        state.DrainWork = drain;
        if (!drain.Schedule())
        {
            Quiet(binding, registers, pci);
            return ProbeResult.Declined("no kit worker to run the drain on");
        }

        // 8. The line when the platform routes it, and the periodic drain in
        //    any case: the drain is idempotent, an edge lost while the line
        //    was masked is recovered within a period, and a line that is
        //    routed but dead still yields a working controller.
        bool hasLine = binding.Node.Interrupts.Count > LineInterruptIndex
            && binding.TryRequestInterrupt(binding.Node.Interrupts[LineInterruptIndex], state.OnInterrupt, out _);
        if (hasLine)
        {
            registers.Write32(E1000ERegisters.InterruptMaskSet,
                E1000ERegisters.InterruptReceiveTimer
                | E1000ERegisters.InterruptLinkStatusChange
                | E1000ERegisters.InterruptReceiveDescriptorMinimumThreshold);
            state.HasLine = true;
        }

        bool polling = binding.TrySchedulePeriodic(DrainPeriodMilliseconds, drain);
        state.IsPolling = polling;
        if (!hasLine && !polling)
        {
            Quiet(binding, registers, pci);
            return ProbeResult.Declined("no interrupt and no timer to poll with");
        }

        // 9. Link up, the lock the transmit path runs under, then the ring.
        registers.Write32(E1000ERegisters.Control, registers.Read32(E1000ERegisters.Control) | E1000ERegisters.ControlSetLinkUp);
        state.LinkUp = (registers.Read32(E1000ERegisters.Status) & E1000ERegisters.StatusLinkUp) != 0;
        state.Lock = binding.CreateLock();
        state.Sink = binding.PublishNetwork(state);

        // 10.
        string line = hasLine ? $"line {pci.InterruptLine}" : "no line";
        string polls = polling ? $"polling every {DrainPeriodMilliseconds} ms" : "no polling";
        binding.Log($"mac {macAddress}, link {(state.LinkUp ? "up" : "down")}, {line}, {polls}");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Quiets the controller on the way out (see <see cref="Quiet"/>) before
    /// the kit frees the buffers. Thread context on the kit worker; nothing
    /// is written when the hardware is gone.
    /// </summary>
    /// <param name="binding">The binding being torn down; its window is still valid.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not E1000EState state || !reason.HardwarePresent)
        {
            return;
        }

        Quiet(binding, state.Registers, binding.Node.Access<PciAccess>());
    }

    /// <summary>
    /// Quiets the controller: receiver and transmitter off, every cause
    /// masked, bus mastering off, then a moment for a frame already landing
    /// to finish before the kit frees the buffers the rings point at. Thread
    /// context on the kit worker, from the detach hook and from a probe that
    /// gives the function up after arming the engines.
    /// </summary>
    private static void Quiet(DeviceBinding binding, RegisterWindow registers, PciAccess pci)
    {
        registers.Write32(E1000ERegisters.ReceiveControl, registers.Read32(E1000ERegisters.ReceiveControl) & ~E1000ERegisters.ReceiveControlEnable);
        registers.Write32(E1000ERegisters.TransmitControl, registers.Read32(E1000ERegisters.TransmitControl) & ~E1000ERegisters.TransmitControlEnable);
        registers.Write32(E1000ERegisters.InterruptMaskClear, E1000ERegisters.AllInterrupts);
        pci.EnableBusMastering(false);
        binding.Delay(QuiesceMicroseconds);
    }

    /// <summary>Polls a register until <paramref name="mask"/> reads as <paramref name="set"/>, within the reset bound. Thread context.</summary>
    private static bool WaitForBit(DeviceBinding binding, RegisterWindow registers, ulong register, uint mask, bool set)
    {
        for (int i = 0; i < ResetPollCount; i++)
        {
            binding.Delay(ResetPollMicroseconds);
            bool isSet = (registers.Read32(register) & mask) != 0;
            if (isSet == set)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Reads one EEPROM word through EERD, in the 82574 layout, polling until the done bit. Thread context.</summary>
    private static bool TryReadEepromWord(DeviceBinding binding, RegisterWindow registers, ushort address, out ushort word)
    {
        registers.Write32(E1000ERegisters.EepromRead, ((uint)address << E1000ERegisters.EepromReadAddressShift) | E1000ERegisters.EepromReadStart);
        for (int i = 0; i < EepromPollCount; i++)
        {
            binding.Delay(EepromPollMicroseconds);
            uint value = registers.Read32(E1000ERegisters.EepromRead);
            if ((value & E1000ERegisters.EepromReadDone) != 0)
            {
                word = (ushort)(value >> E1000ERegisters.EepromReadDataShift);
                return true;
            }
        }

        word = 0;
        return false;
    }

    /// <summary>True when every byte is zero. Any context; allocation-free.</summary>
    private static bool IsAllZero(ReadOnlySpan<byte> bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] != 0)
            {
                return false;
            }
        }

        return true;
    }
}
