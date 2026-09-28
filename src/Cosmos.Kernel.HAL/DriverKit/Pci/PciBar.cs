// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// One base address register of a PCI function as the host sized it at
/// describe time: what the slot decodes, where, and how much. The six
/// slots are always reported; the upper half of a 64-bit register and a
/// register firmware left unprogrammed are unassigned, with length 0. The
/// resource at the same index on the node is the mappable form.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public readonly struct PciBar
{
    internal PciBar(int index, bool isAssigned, bool isIo, bool is64Bit, bool isPrefetchable, ulong baseAddress, ulong length)
    {
        Index = index;
        IsAssigned = isAssigned;
        IsIo = isIo;
        Is64Bit = is64Bit;
        IsPrefetchable = isPrefetchable;
        Base = baseAddress;
        Length = length;
    }

    /// <summary>The slot, 0 to 5: register offset 0x10 + 4 * Index.</summary>
    public int Index { get; }

    /// <summary>True when the register decodes something: a non-zero base and a non-zero size.</summary>
    public bool IsAssigned { get; }

    /// <summary>True for an I/O port range, false for a memory window.</summary>
    public bool IsIo { get; }

    /// <summary>True for a 64-bit memory register, whose upper half sits in the next slot.</summary>
    public bool Is64Bit { get; }

    /// <summary>True when the memory window is marked prefetchable.</summary>
    public bool IsPrefetchable { get; }

    /// <summary>Physical address of a memory window, or the first port of an I/O range.</summary>
    public ulong Base { get; }

    /// <summary>Bytes of a memory window, or ports of an I/O range; 0 when unassigned.</summary>
    public ulong Length { get; }
}
