// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Pci;

namespace Cosmos.Kernel.HAL.Drivers.Pci;

/// <summary>
/// The PCI function a driver was offered: the IDs it is matched on, and
/// read access to its configuration space. Config accesses go through the
/// platform's configuration mechanism and are for thread context only.
/// </summary>
internal sealed class PciFunction
{
    /// <summary>
    /// Bytes of configuration space reachable today. Offsets are ushort so
    /// the PCIe extended space (up to 0x1000) fits once ECAM access reaches it.
    /// </summary>
    private const int ConfigSpaceLength = 0x100;

    private readonly PciDevice _device;

    /// <summary>The vendor ID, config offset 0x00.</summary>
    public ushort VendorId => _device.VendorId;

    /// <summary>The device ID, config offset 0x02.</summary>
    public ushort DeviceId => _device.DeviceId;

    /// <summary>The revision ID, config offset 0x08.</summary>
    public byte RevisionId => _device.RevisionId;

    /// <summary>The base class code, config offset 0x0B.</summary>
    public byte BaseClass => _device.ClassCode;

    /// <summary>The subclass code, config offset 0x0A.</summary>
    public byte Subclass => _device.Subclass;

    /// <summary>The programming interface, config offset 0x09.</summary>
    public byte ProgrammingInterface => _device.ProgIf;

    internal PciFunction(PciDevice device)
    {
        _device = device;
    }

    /// <summary>Reads the configuration byte at <paramref name="offset"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is past the configuration space.</exception>
    public byte ReadConfig8(ushort offset)
    {
        ThrowIfOutsideConfigSpace(offset, sizeof(byte));
        return _device.ReadRegister8((byte)offset);
    }

    /// <summary>Reads the 16-bit configuration register at <paramref name="offset"/>, a multiple of 2.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is past the configuration space or misaligned.</exception>
    public ushort ReadConfig16(ushort offset)
    {
        ThrowIfOutsideConfigSpace(offset, sizeof(ushort));
        return _device.ReadRegister16((byte)offset);
    }

    /// <summary>Reads the 32-bit configuration register at <paramref name="offset"/>, a multiple of 4.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is past the configuration space or misaligned.</exception>
    public uint ReadConfig32(ushort offset)
    {
        ThrowIfOutsideConfigSpace(offset, sizeof(uint));
        return _device.ReadRegister32((byte)offset);
    }

    /// <summary>
    /// Finds the first capability with ID <paramref name="id"/> in the
    /// function's capability list.
    /// </summary>
    /// <param name="id">The capability ID, such as 0x11 for MSI-X.</param>
    /// <param name="offset">The capability's config offset when found; 0 otherwise.</param>
    /// <returns>True when the function has the capability.</returns>
    public bool TryFindCapability(byte id, out ushort offset)
    {
        offset = _device.FindCapability(id);
        return offset != 0;
    }

    /// <summary>
    /// Throws unless an access of <paramref name="size"/> bytes at
    /// <paramref name="offset"/> lies inside the configuration space and is
    /// naturally aligned: the configuration mechanisms only carry aligned
    /// accesses within one dword.
    /// </summary>
    internal static void ThrowIfOutsideConfigSpace(ushort offset, int size)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, ConfigSpaceLength - size);
        if ((offset & (size - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "The offset is not a multiple of the access size.");
        }
    }
}
