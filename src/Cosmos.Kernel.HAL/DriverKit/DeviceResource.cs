// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// One resource of a <see cref="DeviceNode"/>: a memory window, a port range
/// or a RAM-backed window a driver asks the binding to map by index, or an
/// unassigned slot (<see cref="None"/>) a bus keeps so the indices stay
/// what its hardware numbers them. Architecture-neutral by construction: a
/// window is a physical range whatever the architecture, and the kit maps
/// it with the attributes the architecture needs.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public readonly struct DeviceResource
{
    /// <summary>Number of I/O ports an x64 machine decodes.</summary>
    private const uint PortSpaceSize = 0x10000;

    private DeviceResource(DeviceResourceKind kind, ulong baseAddress, ulong physicalBase, ulong length)
    {
        Kind = kind;
        Base = baseAddress;
        PhysicalBase = physicalBase;
        Length = length;
    }

    /// <summary>What <see cref="Base"/> and <see cref="Length"/> mean.</summary>
    public DeviceResourceKind Kind { get; }

    /// <summary>
    /// Physical base address of a memory window, first port of a port range,
    /// or the kernel's own virtual address of a RAM window.
    /// </summary>
    public ulong Base { get; }

    /// <summary>
    /// Physical address of the first byte, for a window of either kind. The
    /// same as <see cref="Base"/> for a memory window; zero for a port range.
    /// </summary>
    public ulong PhysicalBase { get; }

    /// <summary>Length of a window in bytes, or number of ports in a range.</summary>
    public ulong Length { get; }

    /// <summary>True for <see cref="None"/>: a slot nothing is assigned to.</summary>
    public bool IsNone => Kind == DeviceResourceKind.None;

    /// <summary>
    /// An unassigned slot, kept so the indices of the slots after it stay
    /// stable; the binding refuses to map it.
    /// </summary>
    public static DeviceResource None => new(DeviceResourceKind.None, 0, 0, 0);

    /// <summary>A window of device registers or device memory at <paramref name="physicalBase"/>.</summary>
    /// <param name="physicalBase">Physical address of the first byte.</param>
    /// <param name="length">Length in bytes.</param>
    public static DeviceResource MemoryWindow(ulong physicalBase, ulong length) =>
        new(DeviceResourceKind.MemoryWindow, physicalBase, physicalBase, length);

    /// <summary>
    /// A window backed by RAM the kernel already maps, reached through the
    /// mapping the allocator returned; see <see cref="DeviceResourceKind.RamWindow"/>.
    /// </summary>
    /// <param name="virtualBase">The allocator's virtual address of the first byte.</param>
    /// <param name="physicalBase">Physical address of the first byte.</param>
    /// <param name="length">Length in bytes.</param>
    public static DeviceResource RamWindow(ulong virtualBase, ulong physicalBase, ulong length) =>
        new(DeviceResourceKind.RamWindow, virtualBase, physicalBase, length);

    /// <summary>A range of I/O ports starting at <paramref name="basePort"/>.</summary>
    /// <param name="basePort">First port of the range.</param>
    /// <param name="count">Number of ports, at least one; the range ends within the 16-bit port space.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is zero or the range runs past port 0xFFFF.</exception>
    public static DeviceResource PortRange(ushort basePort, ushort count)
    {
        ArgumentOutOfRangeException.ThrowIfZero(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)basePort + count, PortSpaceSize, nameof(count));
        return new DeviceResource(DeviceResourceKind.PortRange, basePort, 0, count);
    }
}
