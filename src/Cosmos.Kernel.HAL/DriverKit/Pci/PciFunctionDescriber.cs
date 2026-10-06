// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// The kit's one describe of a PCI function, shared by the host's
/// <see cref="PciHostAccess.TryDescribeFunction"/> and a bridge's
/// <see cref="PciAccess.TryDescribeChild"/>: it reads the header, sizes the
/// base address registers (the six of a type 0 header, the two of a type 1
/// header), places the unassigned registers of a function behind a
/// hot-plug slot inside the bridge's windows when asked, and builds the
/// identity, the six resources, the interrupt sources and the
/// <see cref="PciAccess"/>. Thread context; the callers guard the context
/// and the ranges.
/// </summary>
/// <remarks>
/// Sizing writes all ones to each register and reads the mask back, and
/// placement writes the chosen address and reads it back, so for their
/// duration the function decodes neither memory nor I/O while bus
/// mastering stays as it was. For a type 1 header (a PCI-to-PCI bridge, a
/// root port) only the registers at 0x10 and 0x14 are sized; the bus
/// numbers and the windows from 0x18 on are never written. Interrupts are
/// disabled around the write pairs and the early framebuffer console is
/// paused: the serial log mirrors every byte into the framebuffer, whose
/// BAR may be the one being sized, so nothing in that scope writes to the
/// log.
/// </remarks>
internal static class PciFunctionDescriber
{
    /// <summary>Number of base address registers in a type 0 header, and of the bar and resource slots every description carries.</summary>
    private const int BarCount = 6;
    /// <summary>Number of base address registers in a type 1 header: the two at 0x10 and 0x14.</summary>
    private const int BridgeBarCount = 2;
    /// <summary>Offset of the first base address register.</summary>
    private const ushort BarBaseOffset = 0x10;
    /// <summary>Bytes per base address register.</summary>
    private const int BarSlotSize = 4;
    /// <summary>BAR bit 0: I/O space.</summary>
    private const uint BarIoSpace = 0x1;
    /// <summary>Memory BAR bit 3: prefetchable.</summary>
    private const uint BarPrefetchable = 0x8;
    /// <summary>Shift down to the memory BAR type field (bits 2:1).</summary>
    private const int BarTypeShift = 1;
    /// <summary>Mask of the memory BAR type field after shifting.</summary>
    private const uint BarTypeMask = 0x3;
    /// <summary>Memory BAR type value of a 64-bit register.</summary>
    private const uint BarType64Bit = 0x2;
    /// <summary>Mask of the address bits of a memory BAR.</summary>
    private const uint BarMemoryAddressMask = 0xFFFFFFF0;
    /// <summary>Mask of the address bits of an I/O BAR.</summary>
    private const uint BarIoAddressMask = 0xFFFFFFFC;
    /// <summary>The sizing pattern: every address bit set.</summary>
    private const uint AllOnes = 0xFFFFFFFF;
    /// <summary>Shift placing a BAR's upper half into bits 63:32.</summary>
    private const int UpperHalfShift = 32;
    /// <summary>Number of ports an x64 machine decodes.</summary>
    private const ulong PortSpaceSize = 0x10000;

    /// <summary>Vendor id offset.</summary>
    private const ushort VendorIdOffset = 0x00;
    /// <summary>Device id offset.</summary>
    private const ushort DeviceIdOffset = 0x02;
    /// <summary>Command register offset.</summary>
    private const ushort CommandOffset = 0x04;
    /// <summary>Revision id offset.</summary>
    private const ushort RevisionOffset = 0x08;
    /// <summary>Programming interface offset.</summary>
    private const ushort ProgIfOffset = 0x09;
    /// <summary>Subclass offset.</summary>
    private const ushort SubclassOffset = 0x0A;
    /// <summary>Base class code offset.</summary>
    private const ushort ClassCodeOffset = 0x0B;
    /// <summary>Header type offset.</summary>
    private const ushort HeaderTypeOffset = 0x0E;
    /// <summary>Subsystem vendor id offset of a type 0 header.</summary>
    private const ushort SubsystemVendorIdOffset = 0x2C;
    /// <summary>Subsystem id offset of a type 0 header.</summary>
    private const ushort SubsystemIdOffset = 0x2E;
    /// <summary>Interrupt line offset.</summary>
    private const ushort InterruptLineOffset = 0x3C;
    /// <summary>Interrupt pin offset.</summary>
    private const ushort InterruptPinOffset = 0x3D;
    /// <summary>Header type bits 6:0, the layout; bit 7 flags a multi-function device.</summary>
    private const byte HeaderLayoutMask = 0x7F;
    /// <summary>Header type bit 7: the device implements functions beyond 0.</summary>
    private const byte MultiFunctionBit = 0x80;
    /// <summary>Header layout of a general device.</summary>
    private const byte DeviceHeaderType = 0;
    /// <summary>Header layout of a PCI-to-PCI bridge.</summary>
    private const byte BridgeHeaderType = 1;
    /// <summary>The vendor id an absent function reads as.</summary>
    private const ushort AbsentVendorId = 0xFFFF;
    /// <summary>Command bits 1:0: memory and I/O decode, cleared while the registers are sized or placed.</summary>
    private const ushort CommandDecodeBits = 0x0003;
    /// <summary>Command bit 0: I/O space decode.</summary>
    private const ushort CommandIoSpace = 0x0001;
    /// <summary>Command bit 1: memory space decode.</summary>
    private const ushort CommandMemorySpace = 0x0002;

    /// <summary>
    /// Reads a function's header and builds what its bus driver publishes
    /// for it: its identity, six resources (one per base address register
    /// slot), its interrupt sources (the legacy line first, then one message
    /// source per MSI-X table entry up to
    /// <see cref="PciHostAccess.MaxDescribedMessages"/> when the function has
    /// the capability), and its <see cref="PciAccess"/>. With
    /// <paramref name="windows"/>, the unassigned registers of a type 0
    /// header are placed inside them first. Thread context; the caller
    /// guarded the context and the ranges.
    /// </summary>
    /// <param name="configSpace">The mechanism the function is reached through.</param>
    /// <param name="segment">The host's segment group.</param>
    /// <param name="lastBus">The last bus the host decodes, kept on the access for a bridge's child describe.</param>
    /// <param name="bus">The bus.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="windows">The windows of the hot-plug bridge the function sits behind, or null to place nothing.</param>
    /// <param name="description">What to publish; default when the function is absent.</param>
    /// <returns>False when the vendor id reads 0xFFFF or 0x0000.</returns>
    internal static bool TryDescribe(PciConfigSpace configSpace, ushort segment, byte lastBus, byte bus, byte device, byte function, PciBridgeWindows? windows, out PciFunctionDescription description)
    {
        description = default;
        ushort vendorId = configSpace.Read16(bus, device, function, VendorIdOffset);
        if (vendorId == AbsentVendorId || vendorId == 0)
        {
            return false;
        }

        ushort deviceId = configSpace.Read16(bus, device, function, DeviceIdOffset);
        byte revision = configSpace.Read8(bus, device, function, RevisionOffset);
        byte progIf = configSpace.Read8(bus, device, function, ProgIfOffset);
        byte subclass = configSpace.Read8(bus, device, function, SubclassOffset);
        byte classCode = configSpace.Read8(bus, device, function, ClassCodeOffset);
        byte headerTypeRegister = configSpace.Read8(bus, device, function, HeaderTypeOffset);
        byte headerType = (byte)(headerTypeRegister & HeaderLayoutMask);
        bool isMultiFunction = (headerTypeRegister & MultiFunctionBit) != 0;
        byte interruptLine = configSpace.Read8(bus, device, function, InterruptLineOffset);
        byte interruptPin = configSpace.Read8(bus, device, function, InterruptPinOffset);

        ushort subsystemVendorId = 0;
        ushort subsystemId = 0;
        PciBar[] bars;
        if (headerType == DeviceHeaderType)
        {
            subsystemVendorId = configSpace.Read16(bus, device, function, SubsystemVendorIdOffset);
            subsystemId = configSpace.Read16(bus, device, function, SubsystemIdOffset);
            bars = SizeBars(configSpace, bus, device, function, BarCount);
            if (windows is not null)
            {
                AssignBars(configSpace, bus, device, function, bars, windows);
            }
        }
        else if (headerType == BridgeHeaderType)
        {
            bars = SizeBars(configSpace, bus, device, function, BridgeBarCount);
        }
        else
        {
            bars = new PciBar[BarCount];
            for (int slot = 0; slot < BarCount; slot++)
            {
                bars[slot] = Unassigned(slot);
            }
        }

        PciIdentity identity = new(segment, bus, device, function, vendorId, deviceId, subsystemVendorId, subsystemId,
            classCode, subclass, progIf, revision, headerType);
        DeviceResource[] resources = new DeviceResource[BarCount];
        for (int slot = 0; slot < BarCount; slot++)
        {
            resources[slot] = ResourceOf(bars[slot]);
        }

        PciAccess access = new(configSpace, segment, lastBus, bus, device, function, headerType, bars, interruptLine, interruptPin);
        PciMessageTable? table = access.MessageTable;
        int messageCount = table is null ? 0 : Math.Min(table.EntryCount, PciHostAccess.MaxDescribedMessages);
        InterruptSource[] interrupts = new InterruptSource[1 + messageCount];
        interrupts[0] = new PciLineInterruptSource(access);
        if (table is not null)
        {
            for (int message = 0; message < messageCount; message++)
            {
                interrupts[1 + message] = new PciMessageInterruptSource(table, message);
            }
        }

        description = new PciFunctionDescription(identity, resources, interrupts, access, isMultiFunction);
        return true;
    }

    /// <summary>
    /// Records in <paramref name="windows"/> the registers firmware assigned
    /// to the other functions of a device behind a hot-plug slot, so a
    /// placement for <paramref name="function"/> never lands on a live
    /// register of a function described later in the pass. Function 0 is
    /// read first and functions 1 to 7 only when its header says
    /// multi-function, the host's walk rule; each present function's
    /// registers are sized (two for a type 1 header, six for a type 0
    /// header) under the discipline of <see cref="SizeBars"/>. Thread
    /// context, from <see cref="PciAccess.TryDescribeChild"/> when a
    /// placement pass starts; allocates the sized registers.
    /// </summary>
    /// <param name="configSpace">The mechanism the device is reached through.</param>
    /// <param name="bus">The bridge's secondary bus.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function about to be described, left out.</param>
    /// <param name="windows">The windows of the pass that starts.</param>
    internal static void NoteOtherFunctions(PciConfigSpace configSpace, byte bus, byte device, byte function, PciBridgeWindows windows)
    {
        if (!IsPresent(configSpace, bus, device, 0))
        {
            return;
        }

        bool multiFunction = (configSpace.Read8(bus, device, 0, HeaderTypeOffset) & MultiFunctionBit) != 0;
        byte lastFunction = multiFunction ? PciConfigSpace.MaxFunction : (byte)0;
        for (byte other = 0; other <= lastFunction; other++)
        {
            if (other == function || !IsPresent(configSpace, bus, device, other))
            {
                continue;
            }

            byte layout = (byte)(configSpace.Read8(bus, device, other, HeaderTypeOffset) & HeaderLayoutMask);
            int registerCount = layout == DeviceHeaderType ? BarCount : layout == BridgeHeaderType ? BridgeBarCount : 0;
            if (registerCount == 0)
            {
                continue;
            }

            PciBar[] bars = SizeBars(configSpace, bus, device, other, registerCount);
            for (int slot = 0; slot < BarCount; slot++)
            {
                windows.NoteAssigned(bars[slot]);
            }
        }
    }

    /// <summary>True when the vendor id of the function reads neither 0xFFFF nor 0. Thread context; allocation-free.</summary>
    private static bool IsPresent(PciConfigSpace configSpace, byte bus, byte device, byte function)
    {
        ushort vendorId = configSpace.Read16(bus, device, function, VendorIdOffset);
        return vendorId != AbsentVendorId && vendorId != 0;
    }

    /// <summary>
    /// Sizes the first <paramref name="registerCount"/> registers of a
    /// header (six for a type 0 header, two for a type 1 header): for each,
    /// the original is read, all ones written, the mask read back and the
    /// original restored; a 64-bit register is sized with both halves
    /// together and its upper slot reported unassigned; a 64-bit register in
    /// the last sized slot is malformed and unassigned; the slots from
    /// <paramref name="registerCount"/> to 5 are unassigned with length 0.
    /// Runs with interrupts disabled, the early framebuffer console paused
    /// and memory and I/O decoding off; Command is restored last. Thread
    /// context.
    /// </summary>
    private static PciBar[] SizeBars(PciConfigSpace configSpace, byte bus, byte device, byte function, int registerCount)
    {
        PciBar[] bars = new PciBar[BarCount];
        for (int slot = registerCount; slot < BarCount; slot++)
        {
            bars[slot] = Unassigned(slot);
        }

        using (InternalCpu.DisableInterruptsScope())
        {
            bool consoleEnabled = EarlyGop.Enabled;
            EarlyGop.Enabled = false;
            ushort command = configSpace.Read16(bus, device, function, CommandOffset);
            configSpace.Write16(bus, device, function, CommandOffset, (ushort)(command & ~CommandDecodeBits));
            try
            {
                int slot = 0;
                while (slot < registerCount)
                {
                    slot += SizeSlot(configSpace, bus, device, function, slot, registerCount, bars);
                }
            }
            finally
            {
                configSpace.Write16(bus, device, function, CommandOffset, command);
                EarlyGop.Enabled = consoleEnabled;
            }
        }

        return bars;
    }

    /// <summary>Sizes the register at <paramref name="slot"/> into <paramref name="bars"/>. Interrupts disabled, from <see cref="SizeBars"/>.</summary>
    /// <returns>How many slots were filled: two for a 64-bit register, one otherwise.</returns>
    private static int SizeSlot(PciConfigSpace configSpace, byte bus, byte device, byte function, int slot, int registerCount, PciBar[] bars)
    {
        ushort offset = BarOffset(slot);
        uint low = configSpace.Read32(bus, device, function, offset);
        bool isIo = (low & BarIoSpace) != 0;
        bool is64Bit = !isIo && ((low >> BarTypeShift) & BarTypeMask) == BarType64Bit;
        bool isPrefetchable = !isIo && (low & BarPrefetchable) != 0;

        if (is64Bit && slot == registerCount - 1)
        {
            bars[slot] = Unassigned(slot);
            return 1;
        }

        if (is64Bit)
        {
            ushort upperOffset = (ushort)(offset + BarSlotSize);
            uint high = configSpace.Read32(bus, device, function, upperOffset);
            configSpace.Write32(bus, device, function, offset, AllOnes);
            configSpace.Write32(bus, device, function, upperOffset, AllOnes);
            uint maskLow = configSpace.Read32(bus, device, function, offset);
            uint maskHigh = configSpace.Read32(bus, device, function, upperOffset);
            configSpace.Write32(bus, device, function, offset, low);
            configSpace.Write32(bus, device, function, upperOffset, high);

            ulong mask = ((ulong)maskHigh << UpperHalfShift) | (maskLow & BarMemoryAddressMask);
            ulong size = mask == 0 ? 0 : ~mask + 1;
            ulong baseAddress = ((ulong)high << UpperHalfShift) | (low & BarMemoryAddressMask);
            bars[slot] = new PciBar(slot, baseAddress != 0 && size != 0, isIo: false, is64Bit: true, isPrefetchable, baseAddress, size);
            bars[slot + 1] = Unassigned(slot + 1);
            return 2;
        }

        configSpace.Write32(bus, device, function, offset, AllOnes);
        uint mask32 = configSpace.Read32(bus, device, function, offset);
        configSpace.Write32(bus, device, function, offset, low);

        ulong base32;
        ulong size32;
        if (isIo)
        {
            base32 = low & BarIoAddressMask;
            size32 = (ushort)(~(mask32 & BarIoAddressMask) + 1);
        }
        else
        {
            base32 = low & BarMemoryAddressMask;
            size32 = ~(mask32 & BarMemoryAddressMask) + 1;
        }

        bars[slot] = new PciBar(slot, base32 != 0 && size32 != 0, isIo, is64Bit: false, isPrefetchable, base32, size32);
        return 1;
    }

    /// <summary>
    /// Places every implemented register of a type 0 header that firmware
    /// or the kit did not assign inside <paramref name="windows"/>, above
    /// everything already assigned there, writes it and reads it back; a
    /// register that does not fit stays unassigned, and one that does not
    /// take the value gets its original contents back and stays unassigned,
    /// its space left to the next register (the window's cursor moves only
    /// past a register that took its address). The one place the kit writes
    /// a base address register.
    /// Memory decoding (and I/O decoding when a port range was placed) is
    /// turned on when Command is restored; bus mastering stays as it was.
    /// Runs with interrupts disabled, the early framebuffer console paused
    /// and decoding off. Thread context.
    /// </summary>
    private static void AssignBars(PciConfigSpace configSpace, byte bus, byte device, byte function, PciBar[] bars, PciBridgeWindows windows)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            bool consoleEnabled = EarlyGop.Enabled;
            EarlyGop.Enabled = false;
            ushort command = configSpace.Read16(bus, device, function, CommandOffset);
            configSpace.Write16(bus, device, function, CommandOffset, (ushort)(command & ~CommandDecodeBits));
            bool placedMemory = false;
            bool placedIo = false;
            try
            {
                for (int slot = 0; slot < BarCount; slot++)
                {
                    if (bars[slot].IsAssigned)
                    {
                        windows.NoteAssigned(bars[slot]);
                    }
                }

                for (int slot = 0; slot < BarCount; slot++)
                {
                    PciBar bar = bars[slot];
                    if (bar.IsAssigned || bar.Length == 0)
                    {
                        continue;
                    }

                    if (!windows.TryPlace(bar, out ulong address))
                    {
                        continue;
                    }

                    ushort offset = BarOffset(slot);
                    uint mask = bar.IsIo ? BarIoAddressMask : BarMemoryAddressMask;
                    ushort upperOffset = (ushort)(offset + BarSlotSize);
                    uint upper = (uint)(address >> UpperHalfShift);
                    uint originalLow = configSpace.Read32(bus, device, function, offset);
                    uint originalHigh = bar.Is64Bit ? configSpace.Read32(bus, device, function, upperOffset) : 0;
                    configSpace.Write32(bus, device, function, offset, (uint)address);
                    if (bar.Is64Bit)
                    {
                        configSpace.Write32(bus, device, function, upperOffset, upper);
                    }

                    bool taken = (configSpace.Read32(bus, device, function, offset) & mask) == (uint)address;
                    if (taken && bar.Is64Bit)
                    {
                        taken = configSpace.Read32(bus, device, function, upperOffset) == upper;
                    }

                    if (!taken)
                    {
                        // The register did not take the address: whatever
                        // part it latched must not decode once Command is
                        // restored with decoding on for the others.
                        configSpace.Write32(bus, device, function, offset, originalLow);
                        if (bar.Is64Bit)
                        {
                            configSpace.Write32(bus, device, function, upperOffset, originalHigh);
                        }

                        continue;
                    }

                    bars[slot] = new PciBar(slot, isAssigned: true, bar.IsIo, bar.Is64Bit, bar.IsPrefetchable, address, bar.Length);
                    windows.NoteAssigned(bars[slot]);
                    if (bar.IsIo)
                    {
                        placedIo = true;
                    }
                    else
                    {
                        placedMemory = true;
                    }
                }
            }
            finally
            {
                ushort decode = (ushort)((placedMemory ? CommandMemorySpace : 0) | (placedIo ? CommandIoSpace : 0));
                configSpace.Write16(bus, device, function, CommandOffset, (ushort)(command | decode));
                EarlyGop.Enabled = consoleEnabled;
            }
        }
    }

    /// <summary>The configuration offset of the register at <paramref name="slot"/>.</summary>
    private static ushort BarOffset(int slot) => (ushort)(BarBaseOffset + slot * BarSlotSize);

    /// <summary>A slot with no register behind it: unassigned, length 0.</summary>
    private static PciBar Unassigned(int slot) => new(slot, isAssigned: false, isIo: false, is64Bit: false, isPrefetchable: false, 0, 0);

    /// <summary>
    /// The mappable form of a register: a memory window, a port range
    /// clipped to the port space, or <see cref="DeviceResource.None"/> for
    /// an unassigned register and for an I/O register outside the port
    /// space.
    /// </summary>
    private static DeviceResource ResourceOf(PciBar bar)
    {
        if (!bar.IsAssigned)
        {
            return DeviceResource.None;
        }

        if (!bar.IsIo)
        {
            return DeviceResource.MemoryWindow(bar.Base, bar.Length);
        }

        if (bar.Base == 0 || bar.Base >= PortSpaceSize)
        {
            return DeviceResource.None;
        }

        ulong count = Math.Min(bar.Length, PortSpaceSize - bar.Base);
        return DeviceResource.PortRange((ushort)bar.Base, (ushort)count);
    }
}
