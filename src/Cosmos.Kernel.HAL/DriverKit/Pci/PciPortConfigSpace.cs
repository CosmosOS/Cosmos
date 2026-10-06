// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Scheduler;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// PCI Configuration Mechanism #1: CONFIG_ADDRESS at port 0xCF8 selects a
/// dword of a function's 256-byte space and CONFIG_DATA at 0xCFC..0xCFF
/// reads or writes it, through <see cref="PlatformHAL.PortIO"/>. The two
/// port operations share one address latch on the whole machine, so every
/// access pairs them under one static IRQ-safe lock: a thread preempted
/// between them would otherwise resume against whatever register another
/// thread, or a handler, selected meanwhile. There is one instance
/// (<see cref="PciConfigSpace.Ports"/>): one latch, one lock. x64 only in
/// practice; the port I/O implementation of another architecture throws.
/// </summary>
internal sealed class PciPortConfigSpace : PciConfigSpace
{
    /// <summary>CONFIG_ADDRESS I/O port of Configuration Mechanism #1.</summary>
    private const ushort ConfigAddressPort = 0xCF8;
    /// <summary>CONFIG_DATA I/O port of Configuration Mechanism #1 (32-bit window at 0xCFC..0xCFF).</summary>
    private const ushort ConfigDataPort = 0xCFC;
    /// <summary>Enable bit (bit 31) of the CONFIG_ADDRESS value.</summary>
    private const uint ConfigEnableBit = 0x80000000;
    /// <summary>Shift placing the bus number into CONFIG_ADDRESS bits 23:16.</summary>
    private const int ConfigBusShift = 16;
    /// <summary>Shift placing the device number into CONFIG_ADDRESS bits 15:11.</summary>
    private const int ConfigDeviceShift = 11;
    /// <summary>Shift placing the function number into CONFIG_ADDRESS bits 10:8.</summary>
    private const int ConfigFunctionShift = 8;
    /// <summary>Mask aligning a register offset down to its containing dword.</summary>
    private const uint ConfigDwordAlignMask = 0xFC;
    /// <summary>Byte lane of a register within the 32-bit data window.</summary>
    private const int ConfigByteLaneMask = 3;
    /// <summary>Word lane of a register within the 32-bit data window.</summary>
    private const int ConfigWordLaneMask = 2;
    /// <summary>Bits per byte, turning a lane into a shift.</summary>
    private const int BitsPerByte = 8;

    /// <summary>The latch lock: static, because the latch is the machine's.</summary>
    private static SchedSpinLock s_latchLock;

    /// <summary>The read-modify-write lock of <see cref="AcquireLock"/>; static for the same reason.</summary>
    private static SchedSpinLock s_updateLock;

    internal PciPortConfigSpace()
    {
    }

    /// <inheritdoc/>
    public override int Size => LegacySize;

    /// <inheritdoc/>
    public override byte Read8(byte bus, byte device, byte function, ushort offset)
    {
        ThrowIfOutOfRange(offset);
        using (Select(bus, device, function, offset))
        {
            return (byte)(PlatformHAL.PortIO.ReadDWord(ConfigDataPort) >> ((offset & ConfigByteLaneMask) * BitsPerByte));
        }
    }

    /// <inheritdoc/>
    public override ushort Read16(byte bus, byte device, byte function, ushort offset)
    {
        ThrowIfOutOfRange(offset);
        using (Select(bus, device, function, offset))
        {
            return (ushort)(PlatformHAL.PortIO.ReadDWord(ConfigDataPort) >> ((offset & ConfigWordLaneMask) * BitsPerByte));
        }
    }

    /// <inheritdoc/>
    public override uint Read32(byte bus, byte device, byte function, ushort offset)
    {
        ThrowIfOutOfRange(offset);
        using (Select(bus, device, function, offset))
        {
            return PlatformHAL.PortIO.ReadDWord(ConfigDataPort);
        }
    }

    /// <inheritdoc/>
    public override void Write8(byte bus, byte device, byte function, ushort offset, byte value)
    {
        ThrowIfOutOfRange(offset);

        // The data window mirrors the dword at 0xCFC..0xCFF: a byte of
        // offset N lands through port 0xCFC + (N & 3), or it would land at
        // the wrong position in the dword.
        ushort dataPort = (ushort)(ConfigDataPort + (offset & ConfigByteLaneMask));
        using (Select(bus, device, function, offset))
        {
            PlatformHAL.PortIO.WriteByte(dataPort, value);
        }
    }

    /// <inheritdoc/>
    public override void Write16(byte bus, byte device, byte function, ushort offset, ushort value)
    {
        ThrowIfOutOfRange(offset);
        ushort dataPort = (ushort)(ConfigDataPort + (offset & ConfigWordLaneMask));
        using (Select(bus, device, function, offset))
        {
            PlatformHAL.PortIO.WriteWord(dataPort, value);
        }
    }

    /// <inheritdoc/>
    public override void Write32(byte bus, byte device, byte function, ushort offset, uint value)
    {
        ThrowIfOutOfRange(offset);
        using (Select(bus, device, function, offset))
        {
            PlatformHAL.PortIO.WriteDWord(ConfigDataPort, value);
        }
    }

    /// <inheritdoc/>
    public override IrqLockScope AcquireLock() => s_updateLock.AcquireIrqSafe();

    /// <summary>
    /// Takes the latch lock and writes CONFIG_ADDRESS for the dword holding
    /// <paramref name="offset"/>. The caller makes its CONFIG_DATA access
    /// inside the returned scope, so the latch still holds the address
    /// when that access lands.
    /// </summary>
    private static IrqLockScope Select(byte bus, byte device, byte function, ushort offset)
    {
        uint address = ConfigEnableBit
            | ((uint)bus << ConfigBusShift)
            | ((uint)(device & MaxDevice) << ConfigDeviceShift)
            | ((uint)(function & MaxFunction) << ConfigFunctionShift)
            | (offset & ConfigDwordAlignMask);
        IrqLockScope scope = s_latchLock.AcquireIrqSafe();
        PlatformHAL.PortIO.WriteDWord(ConfigAddressPort, address);
        return scope;
    }
}
