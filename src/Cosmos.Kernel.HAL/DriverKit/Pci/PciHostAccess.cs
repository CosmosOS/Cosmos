// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
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
    /// sized by the kit's describer with decoding off and interrupts
    /// disabled: the six of a type 0 header, the two of a type 1 header),
    /// its interrupt sources (the legacy line first, then one message
    /// source per MSI-X table entry up to <see cref="MaxDescribedMessages"/>
    /// when the function has the capability), and its
    /// <see cref="PciAccess"/>. Thread context.
    /// </summary>
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
        if (bus < StartBus || bus > EndBus)
        {
            description = default;
            return false;
        }

        return PciFunctionDescriber.TryDescribe(_configSpace, Segment, EndBus, bus, device, function, null, out description);
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
