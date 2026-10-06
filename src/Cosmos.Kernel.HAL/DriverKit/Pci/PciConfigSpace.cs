// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// One way of reaching PCI configuration space: the x86 port mechanism
/// (<see cref="PciPortConfigSpace"/>) or a PCIe ECAM window
/// (<see cref="PciEcamConfigSpace"/>). Both are compiled on both
/// architectures; each PCI host node carries the one its platform node
/// describes, through <see cref="PciHostAccess"/>. Every access is a single
/// locked transaction; a read-modify-write sequence runs under
/// <see cref="AcquireLock"/>, which is a second lock, since the spin lock
/// is not reentrant. Any context; allocation-free.
/// </summary>
internal abstract class PciConfigSpace
{
    /// <summary>Bytes of configuration space per function through the port mechanism.</summary>
    public const int LegacySize = 256;

    /// <summary>Bytes of configuration space per function through ECAM.</summary>
    public const int EcamSize = 4096;

    /// <summary>Highest device number on a bus (5-bit field).</summary>
    public const byte MaxDevice = 31;

    /// <summary>Highest function number of a device (3-bit field).</summary>
    public const byte MaxFunction = 7;

    private static PciPortConfigSpace? s_ports;

    /// <summary>
    /// The one port mechanism: one CONFIG_ADDRESS latch on the machine, one
    /// lock. Created on first use, which is the x64 machine description
    /// publishing its PCI host node (<see cref="PciHostAccess.ForPorts"/>).
    /// </summary>
    public static PciPortConfigSpace Ports => s_ports ??= new PciPortConfigSpace();

    /// <summary>Bytes of configuration space this mechanism reaches per function: 256 or 4096.</summary>
    public abstract int Size { get; }

    /// <summary>Reads one byte of a function's configuration space.</summary>
    /// <param name="bus">The bus.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="offset">The register offset, below <see cref="Size"/>.</param>
    public abstract byte Read8(byte bus, byte device, byte function, ushort offset);

    /// <summary>Reads one word of a function's configuration space.</summary>
    /// <param name="bus">The bus.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="offset">The register offset, even, below <see cref="Size"/>.</param>
    public abstract ushort Read16(byte bus, byte device, byte function, ushort offset);

    /// <summary>Reads one dword of a function's configuration space.</summary>
    /// <param name="bus">The bus.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="offset">The register offset, dword aligned, below <see cref="Size"/>.</param>
    public abstract uint Read32(byte bus, byte device, byte function, ushort offset);

    /// <summary>Writes one byte of a function's configuration space.</summary>
    /// <param name="bus">The bus.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="offset">The register offset, below <see cref="Size"/>.</param>
    /// <param name="value">The value.</param>
    public abstract void Write8(byte bus, byte device, byte function, ushort offset, byte value);

    /// <summary>Writes one word of a function's configuration space.</summary>
    /// <param name="bus">The bus.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="offset">The register offset, even, below <see cref="Size"/>.</param>
    /// <param name="value">The value.</param>
    public abstract void Write16(byte bus, byte device, byte function, ushort offset, ushort value);

    /// <summary>Writes one dword of a function's configuration space.</summary>
    /// <param name="bus">The bus.</param>
    /// <param name="device">The device, 0 to 31.</param>
    /// <param name="function">The function, 0 to 7.</param>
    /// <param name="offset">The register offset, dword aligned, below <see cref="Size"/>.</param>
    /// <param name="value">The value.</param>
    public abstract void Write32(byte bus, byte device, byte function, ushort offset, uint value);

    /// <summary>
    /// Takes the mechanism's read-modify-write lock, IRQ-safe, so a sequence
    /// of accesses that reads a register and writes it back sees no other
    /// such sequence in between. The accesses inside take their own lock;
    /// this one is never held by them. Any context.
    /// </summary>
    public abstract IrqLockScope AcquireLock();

    /// <summary>Refuses an offset the mechanism does not reach.</summary>
    /// <param name="offset">The register offset.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is at or past <see cref="Size"/>.</exception>
    protected void ThrowIfOutOfRange(ushort offset) =>
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((int)offset, Size, nameof(offset));
}
