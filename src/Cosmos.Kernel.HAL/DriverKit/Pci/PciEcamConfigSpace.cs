// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.Scheduler;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// PCIe Enhanced Configuration Access Mechanism: a function's 4 KiB of
/// configuration space is memory at the MCFG window's base plus
/// <c>(bus - startBus) &lt;&lt; 20 | device &lt;&lt; 15 | function &lt;&lt; 12</c>,
/// reached through its HHDM alias. Each access is one load or store, so
/// no latch lock is needed; a read-modify-write sequence takes the
/// instance's own IRQ-safe lock through <see cref="AcquireLock"/>. The
/// window is mapped as device memory by whoever creates the instance:
/// the kit's PCI host factory, <see cref="PciHostAccess.ForEcam"/>, maps
/// it bus by bus first. Any context; allocation-free.
/// </summary>
internal sealed unsafe class PciEcamConfigSpace : PciConfigSpace
{
    /// <summary>Shift placing the bus number into ECAM address bits 27:20.</summary>
    private const int EcamBusShift = 20;
    /// <summary>Shift placing the device number into ECAM address bits 19:15.</summary>
    private const int EcamDeviceShift = 15;
    /// <summary>Shift placing the function number into ECAM address bits 14:12.</summary>
    private const int EcamFunctionShift = 12;

    private readonly ulong _virtualBase;
    private readonly byte _startBus;
    private SchedSpinLock _updateLock;

    /// <summary>Creates the mechanism over a mapped window.</summary>
    /// <param name="physicalBase">Physical address of the window: the MCFG entry's base.</param>
    /// <param name="startBus">The first bus the window covers: the MCFG entry's start bus.</param>
    internal PciEcamConfigSpace(ulong physicalBase, byte startBus)
    {
        ulong hhdmOffset = Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;
        _virtualBase = physicalBase + hhdmOffset;
        _startBus = startBus;
    }

    /// <inheritdoc/>
    public override int Size => EcamSize;

    /// <inheritdoc/>
    public override byte Read8(byte bus, byte device, byte function, ushort offset) =>
        Native.MMIO.Read8(AddressOf(bus, device, function, offset));

    /// <inheritdoc/>
    public override ushort Read16(byte bus, byte device, byte function, ushort offset) =>
        Native.MMIO.Read16(AddressOf(bus, device, function, offset));

    /// <inheritdoc/>
    public override uint Read32(byte bus, byte device, byte function, ushort offset) =>
        Native.MMIO.Read32(AddressOf(bus, device, function, offset));

    /// <inheritdoc/>
    public override void Write8(byte bus, byte device, byte function, ushort offset, byte value) =>
        Native.MMIO.Write8(AddressOf(bus, device, function, offset), value);

    /// <inheritdoc/>
    public override void Write16(byte bus, byte device, byte function, ushort offset, ushort value) =>
        Native.MMIO.Write16(AddressOf(bus, device, function, offset), value);

    /// <inheritdoc/>
    public override void Write32(byte bus, byte device, byte function, ushort offset, uint value) =>
        Native.MMIO.Write32(AddressOf(bus, device, function, offset), value);

    /// <inheritdoc/>
    public override IrqLockScope AcquireLock() => _updateLock.AcquireIrqSafe();

    /// <summary>The HHDM alias of a register.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bus"/> is below the window's first bus, or <paramref name="offset"/> is past the function's space.</exception>
    private ulong AddressOf(byte bus, byte device, byte function, ushort offset)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bus, _startBus);
        ThrowIfOutOfRange(offset);
        return _virtualBase
            + ((ulong)(bus - _startBus) << EcamBusShift)
            + ((ulong)(device & MaxDevice) << EcamDeviceShift)
            + ((ulong)(function & MaxFunction) << EcamFunctionShift)
            + offset;
    }
}
