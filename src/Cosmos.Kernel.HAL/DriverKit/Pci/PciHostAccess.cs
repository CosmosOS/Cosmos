// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.Pci;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// The access object of a PCI host platform node: the configuration
/// mechanism of one segment's buses (the x86 ports, or an ECAM window the
/// machine description found in MCFG), raw configuration access over it,
/// and <see cref="TryDescribeFunction"/>, which reads a function's header,
/// sizes its base address registers and builds everything a host driver
/// publishes for it. Created by the machine description through the
/// internal factories; the host driver enumerates with it.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class PciHostAccess
{
    /// <summary>Number of base address registers in a type 0 header.</summary>
    private const int BarCount = 6;
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
    /// <summary>The vendor id an absent function reads as.</summary>
    private const ushort AbsentVendorId = 0xFFFF;
    /// <summary>Command bits 1:0: memory and I/O decode, cleared while the registers are sized.</summary>
    private const ushort CommandDecodeBits = 0x0003;
    /// <summary>Shift of the bus number in an ECAM address: 1 MiB of configuration space per bus.</summary>
    private const int EcamBusShift = 20;
    /// <summary>Bytes of ECAM per bus.</summary>
    private const ulong EcamBusSize = 1UL << EcamBusShift;

    /// <summary>
    /// How many message interrupt sources a function's description
    /// carries at most, after its legacy line. A function with a larger
    /// MSI-X table (an NVMe controller advertises up to 2048 entries) is
    /// offered its first 32 messages; a driver that needs more is a later
    /// extension of the description.
    /// </summary>
    internal const int MaxDescribedMessages = 32;

    private readonly PciConfigSpace _configSpace;

    internal PciHostAccess(PciConfigSpace configSpace, ushort segment, byte startBus, byte endBus)
    {
        _configSpace = configSpace;
        Segment = segment;
        StartBus = startBus;
        EndBus = endBus;
    }

    /// <summary>The PCI segment group the host serves.</summary>
    public ushort Segment { get; }

    /// <summary>The first bus the host decodes.</summary>
    public byte StartBus { get; }

    /// <summary>The last bus the host decodes.</summary>
    public byte EndBus { get; }

    /// <summary>The mechanism behind this host.</summary>
    internal PciConfigSpace ConfigSpace => _configSpace;

    /// <summary>A host over the x86 port mechanism, sharing its one latch and lock. Thread context.</summary>
    /// <param name="segment">The segment group.</param>
    /// <param name="startBus">The first bus.</param>
    /// <param name="endBus">The last bus.</param>
    internal static PciHostAccess ForPorts(ushort segment, byte startBus, byte endBus) =>
        new(PciConfigSpace.Ports, segment, startBus, endBus);

    /// <summary>
    /// A host over an ECAM window, mapped here as device memory bus by bus,
    /// from <paramref name="startBus"/> up. The first bus whose megabyte
    /// cannot be mapped ends the host's range: the buses before it are
    /// served, and the rest are logged as unreachable rather than losing
    /// the whole host (the ARM64 mapper can split one 1 GiB block only, so
    /// a window that crosses a block boundary keeps the buses on the near
    /// side). Thread context.
    /// </summary>
    /// <param name="physicalBase">The MCFG entry's base address.</param>
    /// <param name="segment">The segment group.</param>
    /// <param name="startBus">The first bus.</param>
    /// <param name="endBus">The last bus the window covers; <see cref="EndBus"/> is lower when a bus could not be mapped.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="endBus"/> is below <paramref name="startBus"/>.</exception>
    /// <exception cref="InvalidOperationException">Not even the first bus of the window can be mapped.</exception>
    internal static PciHostAccess ForEcam(ulong physicalBase, ushort segment, byte startBus, byte endBus)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(endBus, startBus);
        int mappedBuses = 0;
        while (startBus + mappedBuses <= endBus
            && DeviceMemory.EnsureWindowMapped(physicalBase + ((ulong)mappedBuses << EcamBusShift), EcamBusSize))
        {
            mappedBuses++;
        }

        if (mappedBuses == 0)
        {
            throw new InvalidOperationException("The ECAM window cannot be mapped.");
        }

        byte lastMappedBus = (byte)(startBus + mappedBuses - 1);
        if (lastMappedBus != endBus)
        {
            DriverLog.EcamWindowClamped(physicalBase, (byte)(lastMappedBus + 1), endBus, lastMappedBus);
        }

        return new PciHostAccess(new PciEcamConfigSpace(physicalBase, startBus), segment, startBus, lastMappedBus);
    }

    /// <summary>Reads one byte of a function's configuration space. Any context; allocation-free.</summary>
    /// <param name="bus">The bus, within <see cref="StartBus"/> and <see cref="EndBus"/>.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="offset">The register offset.</param>
    /// <exception cref="ArgumentOutOfRangeException">The bus is outside the host's range, the device or function number is too large, or the offset is past the mechanism's size.</exception>
    public byte ReadConfig8(byte bus, byte device, byte function, ushort offset)
    {
        ThrowIfOutOfRange(bus, device, function, offset, sizeof(byte));
        return _configSpace.Read8(bus, device, function, offset);
    }

    /// <summary>Reads one word of a function's configuration space. Any context; allocation-free.</summary>
    /// <param name="bus">The bus, within <see cref="StartBus"/> and <see cref="EndBus"/>.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="offset">The register offset, even.</param>
    /// <exception cref="ArgumentOutOfRangeException">The bus is outside the host's range, the device or function number is too large, or the offset is odd or past the mechanism's size.</exception>
    public ushort ReadConfig16(byte bus, byte device, byte function, ushort offset)
    {
        ThrowIfOutOfRange(bus, device, function, offset, sizeof(ushort));
        return _configSpace.Read16(bus, device, function, offset);
    }

    /// <summary>Reads one dword of a function's configuration space. Any context; allocation-free.</summary>
    /// <param name="bus">The bus, within <see cref="StartBus"/> and <see cref="EndBus"/>.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="offset">The register offset, dword aligned.</param>
    /// <exception cref="ArgumentOutOfRangeException">The bus is outside the host's range, the device or function number is too large, or the offset is not dword aligned or past the mechanism's size.</exception>
    public uint ReadConfig32(byte bus, byte device, byte function, ushort offset)
    {
        ThrowIfOutOfRange(bus, device, function, offset, sizeof(uint));
        return _configSpace.Read32(bus, device, function, offset);
    }

    /// <summary>Writes one byte of a function's configuration space. Any context; allocation-free.</summary>
    /// <param name="bus">The bus, within <see cref="StartBus"/> and <see cref="EndBus"/>.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="offset">The register offset.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentOutOfRangeException">The bus is outside the host's range, the device or function number is too large, or the offset is past the mechanism's size.</exception>
    public void WriteConfig8(byte bus, byte device, byte function, ushort offset, byte value)
    {
        ThrowIfOutOfRange(bus, device, function, offset, sizeof(byte));
        _configSpace.Write8(bus, device, function, offset, value);
    }

    /// <summary>Writes one word of a function's configuration space. Any context; allocation-free.</summary>
    /// <param name="bus">The bus, within <see cref="StartBus"/> and <see cref="EndBus"/>.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="offset">The register offset, even.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentOutOfRangeException">The bus is outside the host's range, the device or function number is too large, or the offset is odd or past the mechanism's size.</exception>
    public void WriteConfig16(byte bus, byte device, byte function, ushort offset, ushort value)
    {
        ThrowIfOutOfRange(bus, device, function, offset, sizeof(ushort));
        _configSpace.Write16(bus, device, function, offset, value);
    }

    /// <summary>Writes one dword of a function's configuration space. Any context; allocation-free.</summary>
    /// <param name="bus">The bus, within <see cref="StartBus"/> and <see cref="EndBus"/>.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="offset">The register offset, dword aligned.</param>
    /// <param name="value">The value.</param>
    /// <exception cref="ArgumentOutOfRangeException">The bus is outside the host's range, the device or function number is too large, or the offset is not dword aligned or past the mechanism's size.</exception>
    public void WriteConfig32(byte bus, byte device, byte function, ushort offset, uint value)
    {
        ThrowIfOutOfRange(bus, device, function, offset, sizeof(uint));
        _configSpace.Write32(bus, device, function, offset, value);
    }

    /// <summary>
    /// Reads a function's header and builds what a host driver publishes
    /// for it: its identity, six resources (one per base address register,
    /// sized here with decoding off; see the remarks), its interrupt
    /// sources (the legacy line first, then one message source per MSI-X
    /// table entry up to <see cref="MaxDescribedMessages"/> when the
    /// function has the capability), and its <see cref="PciAccess"/>.
    /// Thread context.
    /// </summary>
    /// <remarks>
    /// Sizing writes all ones to each register and reads the mask back, so
    /// for its duration the function decodes neither memory nor I/O while
    /// bus mastering stays as it was (the function the HAL's USB host
    /// controller driver operates keeps its DMA and its MSI-X). Interrupts
    /// are disabled around the
    /// write pairs and the early framebuffer console is paused: the serial
    /// log mirrors every byte into the framebuffer, whose BAR may be the
    /// one being sized, so nothing in that scope writes to the log.
    /// </remarks>
    /// <param name="bus">The bus.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="description">What to publish; default when the function is absent.</param>
    /// <returns>False when <paramref name="bus"/> is outside the host's range or the vendor id reads 0xFFFF or 0x0000.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The device or function number is too large.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public bool TryDescribeFunction(byte bus, byte device, byte function, out PciFunctionDescription description)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(TryDescribeFunction));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(device, PciConfigSpace.MaxDevice);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(function, PciConfigSpace.MaxFunction);
        description = default;
        if (bus < StartBus || bus > EndBus)
        {
            return false;
        }

        ushort vendorId = _configSpace.Read16(bus, device, function, VendorIdOffset);
        if (vendorId == AbsentVendorId || vendorId == 0)
        {
            return false;
        }

        ushort deviceId = _configSpace.Read16(bus, device, function, DeviceIdOffset);
        byte revision = _configSpace.Read8(bus, device, function, RevisionOffset);
        byte progIf = _configSpace.Read8(bus, device, function, ProgIfOffset);
        byte subclass = _configSpace.Read8(bus, device, function, SubclassOffset);
        byte classCode = _configSpace.Read8(bus, device, function, ClassCodeOffset);
        byte headerType = (byte)(_configSpace.Read8(bus, device, function, HeaderTypeOffset) & HeaderLayoutMask);
        byte interruptLine = _configSpace.Read8(bus, device, function, InterruptLineOffset);
        byte interruptPin = _configSpace.Read8(bus, device, function, InterruptPinOffset);

        ushort subsystemVendorId = 0;
        ushort subsystemId = 0;
        PciBar[] bars;
        if (headerType == 0)
        {
            subsystemVendorId = _configSpace.Read16(bus, device, function, SubsystemVendorIdOffset);
            subsystemId = _configSpace.Read16(bus, device, function, SubsystemIdOffset);
            bars = SizeBars(bus, device, function);
        }
        else
        {
            bars = new PciBar[BarCount];
            for (int slot = 0; slot < BarCount; slot++)
            {
                bars[slot] = Unassigned(slot);
            }
        }

        PciIdentity identity = new(Segment, bus, device, function, vendorId, deviceId, subsystemVendorId, subsystemId,
            classCode, subclass, progIf, revision, headerType);
        DeviceResource[] resources = new DeviceResource[BarCount];
        for (int slot = 0; slot < BarCount; slot++)
        {
            resources[slot] = ResourceOf(bars[slot]);
        }

        PciAccess access = new(_configSpace, bus, device, function, headerType, bars, interruptLine, interruptPin);
        PciMessageTable? table = access.MessageTable;
        int messageCount = table is null ? 0 : Math.Min(table.EntryCount, MaxDescribedMessages);
        InterruptSource[] interrupts = new InterruptSource[1 + messageCount];
        interrupts[0] = new PciLineInterruptSource(access);
        if (table is not null)
        {
            for (int message = 0; message < messageCount; message++)
            {
                interrupts[1 + message] = new PciMessageInterruptSource(table, message);
            }
        }

        description = new PciFunctionDescription(identity, resources, interrupts, access);
        return true;
    }

    /// <summary>
    /// Sizes the six registers of a type 0 header: for each, the original
    /// is read, all ones written, the mask read back and the original
    /// restored; a 64-bit register is sized with both halves together and
    /// its upper slot reported unassigned; a 64-bit register in the last
    /// slot is malformed and unassigned. Runs with interrupts disabled,
    /// the early framebuffer console paused and memory and I/O decoding
    /// off; Command is restored last.
    /// </summary>
    private PciBar[] SizeBars(byte bus, byte device, byte function)
    {
        PciBar[] bars = new PciBar[BarCount];
        using (InternalCpu.DisableInterruptsScope())
        {
            bool consoleEnabled = EarlyGop.Enabled;
            EarlyGop.Enabled = false;
            ushort command = _configSpace.Read16(bus, device, function, CommandOffset);
            _configSpace.Write16(bus, device, function, CommandOffset, (ushort)(command & ~CommandDecodeBits));
            try
            {
                int slot = 0;
                while (slot < BarCount)
                {
                    slot += SizeSlot(bus, device, function, slot, bars);
                }
            }
            finally
            {
                _configSpace.Write16(bus, device, function, CommandOffset, command);
                EarlyGop.Enabled = consoleEnabled;
            }
        }

        return bars;
    }

    /// <summary>Sizes the register at <paramref name="slot"/> into <paramref name="bars"/>.</summary>
    /// <returns>How many slots were filled: two for a 64-bit register, one otherwise.</returns>
    private int SizeSlot(byte bus, byte device, byte function, int slot, PciBar[] bars)
    {
        ushort offset = (ushort)(BarBaseOffset + slot * BarSlotSize);
        uint low = _configSpace.Read32(bus, device, function, offset);
        bool isIo = (low & BarIoSpace) != 0;
        bool is64Bit = !isIo && ((low >> BarTypeShift) & BarTypeMask) == BarType64Bit;
        bool isPrefetchable = !isIo && (low & BarPrefetchable) != 0;

        if (is64Bit && slot == BarCount - 1)
        {
            bars[slot] = Unassigned(slot);
            return 1;
        }

        if (is64Bit)
        {
            ushort upperOffset = (ushort)(offset + BarSlotSize);
            uint high = _configSpace.Read32(bus, device, function, upperOffset);
            _configSpace.Write32(bus, device, function, offset, AllOnes);
            _configSpace.Write32(bus, device, function, upperOffset, AllOnes);
            uint maskLow = _configSpace.Read32(bus, device, function, offset);
            uint maskHigh = _configSpace.Read32(bus, device, function, upperOffset);
            _configSpace.Write32(bus, device, function, offset, low);
            _configSpace.Write32(bus, device, function, upperOffset, high);

            ulong mask = ((ulong)maskHigh << UpperHalfShift) | (maskLow & BarMemoryAddressMask);
            ulong size = mask == 0 ? 0 : ~mask + 1;
            ulong baseAddress = ((ulong)high << UpperHalfShift) | (low & BarMemoryAddressMask);
            bars[slot] = new PciBar(slot, baseAddress != 0 && size != 0, isIo: false, is64Bit: true, isPrefetchable, baseAddress, size);
            bars[slot + 1] = Unassigned(slot + 1);
            return 2;
        }

        _configSpace.Write32(bus, device, function, offset, AllOnes);
        uint mask32 = _configSpace.Read32(bus, device, function, offset);
        _configSpace.Write32(bus, device, function, offset, low);

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

    /// <summary>Refuses an access outside the host's buses, a bad device or function number, or a register the mechanism does not reach or the access does not align to.</summary>
    private void ThrowIfOutOfRange(byte bus, byte device, byte function, ushort offset, int size)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bus, StartBus);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(bus, EndBus);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(device, PciConfigSpace.MaxDevice);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(function, PciConfigSpace.MaxFunction);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + size, _configSpace.Size, nameof(offset));
        if ((offset & (size - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "A configuration register is read at its natural alignment.");
        }
    }
}
